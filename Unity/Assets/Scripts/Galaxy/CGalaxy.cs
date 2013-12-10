using UnityEngine;
using System.Collections;

public class CGalaxy : CNetworkMonoBehaviour
{
    ///////////////////////////////////////////////////////////////////////////
    // Objects:

    public struct SCellPos
    {
        public int x;
        public int y;
        public int z;

        public SCellPos(int _x, int _y, int _z) { x = _x; y = _y; z = _z; }

        public static SCellPos operator +(SCellPos lhs, SCellPos rhs) { return new SCellPos(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z); }
        public static SCellPos operator -(SCellPos lhs, SCellPos rhs) { return new SCellPos(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z); }
    }

    class CCellContent
    {
        public CCellContent(bool alternatorInitialValue, ECellState state) { mAlternator = alternatorInitialValue; mState = state; }
        public bool mAlternator;   // This is used for culling purposes.
        public ECellState mState;    // Cell loading is drawn out over time. This shows whether the cell is ready or waiting.
    }

    class CRegisteredObserver
    {
        public GameObject mEntity;
        public float mBoundingRadius;   // Bounding sphere.

        public CRegisteredObserver(GameObject entity, float boundingRadius) { mEntity = entity; mBoundingRadius = boundingRadius; }
    }

    class CRegisteredGubbin
    {
        public GameObject mEntity;
        public float mBoundingRadius;   // Bounding sphere.
        public ushort mNetworkViewID;
        public bool mAlternator;   // This is used for culling purposes.
        public bool mAwaitingCull = false;  // Objects get culled over time.

        public CRegisteredGubbin(GameObject entity, float boundingRadius, ushort networkViewID, bool alternatorValue) {mEntity = entity; mBoundingRadius = boundingRadius; mNetworkViewID = networkViewID; mAlternator = alternatorValue; }
    }

    struct SGubbinMeta
    {
        public CGame.ENetworkRegisteredPrefab mPrefabID;
        public SCellPos mParentAbsoluteCell;
        public float mScale;
        public Vector3 mPosition;
        public Quaternion mRotation;
        public Vector3 mLinearVelocity;
        public Vector3 mAngularVelocity;
        public bool mHasNetworkedEntityScript;
        public bool mHasRigidBody;

        public SGubbinMeta(CGame.ENetworkRegisteredPrefab prefabID, SCellPos parentAbsoluteCell, float scale, Vector3 position, Quaternion rotation, Vector3 linearVelocity, Vector3 angularVelocity, bool hasNetworkedEntityScript, bool hasRigidBody)
        {
            mPrefabID = prefabID;
            mParentAbsoluteCell = parentAbsoluteCell;
            mScale = scale;
            mPosition = position;
            mRotation = rotation;
            mLinearVelocity = linearVelocity;
            mAngularVelocity = angularVelocity;
            mHasNetworkedEntityScript = hasNetworkedEntityScript;
            mHasRigidBody = hasRigidBody;
        }
    }

    public enum ECellState : uint
    {
        Loading,
        Loaded,
        Unloading
        // An unloaded cell does not exist.
    }

    public enum ENoiseLayer : uint
    {
        AsteroidDensity,
        FogDensity,
        MAX
    }

    enum ESkybox : uint
    {
        Composite,
        Solid,
        MAX
    }

    ///////////////////////////////////////////////////////////////////////////
    // Variables:

    private static CGalaxy sGalaxy = null;
    public static CGalaxy instance { get { return sGalaxy; } }

    private PerlinSimplexNoise[] mNoises = new PerlinSimplexNoise[(uint)ENoiseLayer.MAX];
    protected CNetworkVar<int>[] mNoiseSeeds = new CNetworkVar<int>[(uint)ENoiseLayer.MAX];

    private Cubemap[] mSkyboxes = new Cubemap[(uint)ESkybox.MAX];

    private SCellPos mCentreCell = new SCellPos(0, 0, 0);    // All cells are offset by this cell.
    protected CNetworkVar<int> mCentreCellX;
    protected CNetworkVar<int> mCentreCellY;
    protected CNetworkVar<int> mCentreCellZ;

    private System.Collections.Generic.List<Transform> mShiftableTransforms = new System.Collections.Generic.List<Transform>();    // When everything moves too far in any direction, the transforms of these registered GameObjects are shifted back.
    private System.Collections.Generic.List<CRegisteredObserver> mObservers = new System.Collections.Generic.List<CRegisteredObserver>(); // Cells are loaded and unloaded based on proximity to observers.
    private System.Collections.Generic.List<GalaxyIE> mGalaxyIEs;   // Is only instantiated when this galaxy is ready to update galaxyIEs.
    private System.Collections.Generic.List<CRegisteredGubbin> mGubbins;    // Gubbins ("space things") are unloaded based on proximity to cells.
    private System.Collections.Generic.List<SGubbinMeta> mGubbinsToLoad;
    private System.Collections.Generic.List<CRegisteredGubbin> mGubbinsToUnload;
    private System.Collections.Generic.Dictionary<SCellPos, CCellContent> mCells;
    private System.Collections.Generic.List<SCellPos> mCellsToLoad;
    private System.Collections.Generic.List<SCellPos> mCellsToUnload;

    private float mfGalaxySize = 1391000000.0f; // (1.3 million kilometres) In metres cubed. Floats can increment up to 16777220.0f (16.7 million).
    protected CNetworkVar<float> mGalaxySize;
    public float galaxySize { get { return mfGalaxySize; } }

    private uint muiNumCellSubsets = 20; // Zero is just the one cell. Also, this is equivalent to the number of bits per axis required to acknowledge each cell (<= 2 for 1 byte, <= 5 for 2 bytes, <= 10 for 4 bytes, <= 21 for 8 bytes).
    protected CNetworkVar<uint> mNumCellSubsets;
    public uint numCellSubsets { get { return muiNumCellSubsets; } }

    private uint muiMaxAsteroidsPerCell = 5;
    protected CNetworkVar<uint> mMaxAsteroidsPerCell;
    public uint maxAsteroidsPerCell { get { return muiMaxAsteroidsPerCell; } }

    public const float mfTimeBetweenQueueCellsToLoadOrUnload =  0.25f;
    private float mfTimeUntilNextQueueCellToLoadOrUnload =      0.0f;
    public const float mfTimeBetweenCellLoads =                 0.1f;
    private float mfTimeUntilNextCellLoad =                     0.0f;
    public const float mfTimeBetweenCellUnloads =               mfTimeBetweenCellLoads;
    private float mfTimeUntilNextCellUnload =                   mfTimeBetweenCellLoads / 2;

    public const float mfTimeBetweenQueueGubbinsToUnload =      mfTimeBetweenQueueCellsToLoadOrUnload;
    private float mfTimeUntilNextQueueGubbinToUnload =          mfTimeBetweenQueueCellsToLoadOrUnload / 2;
    public const float mfTimeBetweenGubbinLoads =               0.05f;
    private float mfTimeUntilNextGubbinLoad =                   0.0f;
    public const float mfTimeBetweenGubbinUnloads =             mfTimeBetweenGubbinLoads;
    private float mfTimeUntilNextGubbinUnload =                 mfTimeBetweenGubbinLoads / 2;

    public const float mfTimeBetweenShiftTests =                0.5f;
    private float mfTimeUntilNextShiftTest =                    0.0f;

    private uint mNumExtraNeighbourCells = 3;   // Number of extra cells to load in every direction (i.e. load neighbours up to some distance).
    public uint numExtraNeighbourCells { get { return mNumExtraNeighbourCells; } }

    private bool mbValidCellValue = false;  // Used for culling cells that are too far away from observers.
    private bool mbValidGubbinValue = false;    // Used for culling gubbins that are too far away from cells.

    public float cellDiameter { get { return mfGalaxySize / numCellsInRow; } }
    public float cellRadius { get { return mfGalaxySize / (numCellsInRow*2u); } }
    public ulong numCells { get { /*return (uint)Mathf.Pow(8, muiNumCellSubsets);*/ ulong ul = 1; for (uint ui2 = 0; ui2 < muiNumCellSubsets; ++ui2)ul *= 8u; return ul; } }
    public uint numCellsInRow { get { /*return (uint)Mathf.Pow(2, muiNumCellSubsets);*/ uint ui = 1; for (uint ui2 = 0; ui2 < muiNumCellSubsets; ++ui2)ui *= 2; return ui; } }

    ///////////////////////////////////////////////////////////////////////////
    // Functions:

    public CGalaxy()
    {
        // Instantiate galaxy noises.
        for (uint ui = 0; ui < (uint)ENoiseLayer.MAX; ++ui)
            mNoises[ui] = new PerlinSimplexNoise();
    }

    void Start()
    {
        sGalaxy = this;

        // Fog and skybox are controlled by the galaxy.
        RenderSettings.fog = false;
        RenderSettings.skybox = null;

        //// Initialise galaxy noises.
        //for(uint ui = 0; ui < (uint)ENoiseLayer.MAX; ++ui)
        //    mNoises[ui] = new PerlinSimplexNoise();

        // Load skyboxes.
        string[] skyboxFaces = new string[6];
        skyboxFaces[0] = "Left";
        skyboxFaces[1] = "Right";
        skyboxFaces[2] = "Down";
        skyboxFaces[3] = "Up";
        skyboxFaces[4] = "Front";
        skyboxFaces[5] = "Back";

        Profiler.BeginSample("Initialise cubemap from 6 textures");
        for (uint uiSkybox = 0; uiSkybox < (uint)ESkybox.MAX; ++uiSkybox)    // For each skybox...
        {
            for (uint uiFace = 0; uiFace < 6; ++uiFace)  // For each face on the skybox...
            {
                Texture2D skyboxFace = Resources.Load("Textures/Galaxy/" + uiSkybox.ToString() + skyboxFaces[uiFace], typeof(Texture2D)) as Texture2D;  // Load the texture from file.
                if (!mSkyboxes[uiSkybox])
                    mSkyboxes[uiSkybox] = new Cubemap(skyboxFace.width, skyboxFace.format, false);
                mSkyboxes[uiSkybox].SetPixels(skyboxFace.GetPixels(), (CubemapFace)uiFace);
                Resources.UnloadAsset(skyboxFace);
            }

            mSkyboxes[uiSkybox].Apply(false, true);
        }
        Profiler.EndSample();

        //Profiler.BeginSample("Load cubemaps");
        //for (uint uiSkybox = 0; uiSkybox < (uint)ESkybox.MAX; ++uiSkybox)    // For each skybox...
        //    mSkyboxes[uiSkybox] = Resources.Load("Textures/Galaxy/" + uiSkybox.ToString() + "Cubemap", typeof(Cubemap)) as Cubemap;  // Load the cubemap texture from file.
        //Profiler.EndSample();

        // Galaxy is ready to update galaxyIEs.
        mGalaxyIEs = new System.Collections.Generic.List<GalaxyIE>();

        // Statistical data sometimes helps spot errors.
        Debug.Log("Galaxy is " + mfGalaxySize.ToString("n0") + " units³ with " + muiNumCellSubsets.ToString("n0") + " cell subsets, thus the " + numCells.ToString("n0") + " cells are " + (mfGalaxySize / numCellsInRow).ToString("n0") + " units in diameter and " + numCellsInRow.ToString("n0") + " cells in a row.");

        if (CNetwork.IsServer)
        {
            mGubbins = new System.Collections.Generic.List<CRegisteredGubbin>();
            mGubbinsToLoad = new System.Collections.Generic.List<SGubbinMeta>();
            mGubbinsToUnload = new System.Collections.Generic.List<CRegisteredGubbin>();
            mCells = new System.Collections.Generic.Dictionary<SCellPos, CCellContent>();
            mCellsToLoad = new System.Collections.Generic.List<SCellPos>();
            mCellsToUnload = new System.Collections.Generic.List<SCellPos>();

            // Seed galaxy noises through the network variable to sync the seed across all clients.
            for(uint ui = 0; ui < (uint)ENoiseLayer.MAX; ++ui)
                mNoiseSeeds[ui].Set(Random.Range(int.MinValue, int.MaxValue));
        }
    }

    void OnDestroy()
    {
        sGalaxy = null;
    }

    public override void InstanceNetworkVars()
    {
        Profiler.BeginSample("InstanceNetworkVars");

        for (uint ui = 0; ui < (uint)ENoiseLayer.MAX; ++ui)
            mNoiseSeeds[ui] = new CNetworkVar<int>(SyncNoiseSeed);

        mCentreCellX = new CNetworkVar<int>(SyncCentreCellX, mCentreCell.x);
        mCentreCellY = new CNetworkVar<int>(SyncCentreCellY, mCentreCell.y);
        mCentreCellZ = new CNetworkVar<int>(SyncCentreCellZ, mCentreCell.z);
        mGalaxySize = new CNetworkVar<float>(SyncGalaxySize, mfGalaxySize);
        mMaxAsteroidsPerCell = new CNetworkVar<uint>(SyncMaxAsteroidsPerCell, muiMaxAsteroidsPerCell);
        mNumCellSubsets = new CNetworkVar<uint>(SyncNumCellSubsets, muiNumCellSubsets);

        Profiler.EndSample();
    }

    void Update()
    {
        if (CNetwork.IsServer)
        {
            // Queue cells to load/unload based on proximity to observers.
            mfTimeUntilNextQueueCellToLoadOrUnload -= Time.deltaTime;
            if (mfTimeUntilNextQueueCellToLoadOrUnload <= 0.0f)  // Running more than once per frame does nothing extra.
            {
                mfTimeUntilNextQueueCellToLoadOrUnload = mfTimeBetweenQueueCellsToLoadOrUnload;    // Drop the remainder as it is not required to run multiple times to make up for lag spikes.

                mbValidCellValue = !mbValidCellValue;   // Alternate the valid cell value. All cells within proximity of an observer will be updated, while all others will retain the old value making it easier to detect and cull them.

                // Queue for loading: unloaded cells within proximity to observers.
                Profiler.BeginSample("Check for cells to queue for loading");
                foreach (CRegisteredObserver observer in mObservers)
                {
                    Vector3 observerPosition = observer.mEntity.transform.position;
                    SCellPos occupiedRelativeCell = PointToRelativeCell(observerPosition);
                    int iCellsInARow = 1 /*Centre cell*/ + (int)mNumExtraNeighbourCells * 2 /*Neighbouring cell rows*/ + (Mathf.CeilToInt((observer.mBoundingRadius / cellRadius) - 1) * 2);    // Centre point plus neighbours per axis.   E.g. 1,3,5,7,9...
                    int iNeighboursPerDirection = (iCellsInARow - 1) / 2;                                                                                                                       // Neighbours per direction.                E.g. 0,2,4,6,8...

                    for (int x = -iNeighboursPerDirection; x <= iNeighboursPerDirection; ++x)
                    {
                        for (int y = -iNeighboursPerDirection; y <= iNeighboursPerDirection; ++y)
                        {
                            for (int z = -iNeighboursPerDirection; z <= iNeighboursPerDirection; ++z)
                            {
                                // Check if this cell is loaded.
                                SCellPos neighbouringRelativeCell = new SCellPos(occupiedRelativeCell.x + x, occupiedRelativeCell.y + y, occupiedRelativeCell.z + z);
                                if (RelativeCellWithinProximityOfPoint(neighbouringRelativeCell, observerPosition, observer.mBoundingRadius + cellDiameter * mNumExtraNeighbourCells))
                                {
                                    SCellPos neighbouringAbsoluteCell = neighbouringRelativeCell + mCentreCell;
                                    CCellContent temp;
                                    if (mCells.TryGetValue(neighbouringAbsoluteCell, out temp))   // If this cell has already been loaded...
                                    {
                                        temp.mAlternator = mbValidCellValue;    // Update alternator to indicate the cell is within proximity to an observer.
                                        if (temp.mState == ECellState.Unloading) // If this cell is waiting to be unloaded...
                                        {
                                            mCellsToUnload.Remove(neighbouringAbsoluteCell);    // Stop it from unloading, as it is now back in proximity to observers.
                                            temp.mState = ECellState.Loaded;    // Reset cell state to 'loaded', as only loaded cells are queued for unloading.
                                        }
                                    }
                                    else    // This cell has not been loaded...
                                    {
                                        mCellsToLoad.Add(neighbouringAbsoluteCell); // Queue cell to load.
                                        mCells.Add(neighbouringAbsoluteCell, new CCellContent(mbValidCellValue, ECellState.Loading));    // Add cell to dictionary of cells as loading.
                                    }
                                }
                            }
                        }
                    }
                }
                Profiler.EndSample();

                // Queue for unloading: cells too far away from observers.
                Profiler.BeginSample("Check for cells to queue for unloading");
                bool restart;
                do
                {
                    restart = false;
                    foreach (System.Collections.Generic.KeyValuePair<SCellPos, CCellContent> absoluteCell in mCells) // For every loaded cell...
                    {
                        if (absoluteCell.Value.mAlternator != mbValidCellValue)  // If the cell was not updated to the current alternator value...
                        {
                            // This cell is not within proximity of any observers.
                            switch (absoluteCell.Value.mState)  // Determine how to unload the cell based on its state.
                            {
                            case ECellState.Loading:    // This cell, which is not within proximity to any observers, is waiting to load.
                                mCellsToLoad.Remove(absoluteCell.Key);    // Deregister this cell for loading, as it is no longer necessary to load.
                                mCells.Remove(absoluteCell.Key); // Remove this cell from the dictionary.
                                restart = true; // Removing an element from a container while it is being iterated breaks the iterator, so the iteration must restart.
                                Debug.Log("Galaxy: Hiccup occured in timing of loading/unloading cells. Performance dent is unavoidable as C# lacks required functionality to handle gracefully");
                                break;

                            case ECellState.Loaded:
                                mCellsToUnload.Add(absoluteCell.Key);   // Register this cell for unloading.
                                absoluteCell.Value.mState = ECellState.Unloading;   // Mark the cell as waiting to unload.
                                break;
                            }

                            if (restart)    // If a restart is required...
                                break;  // The break to stop the loop would have occured within the switch, if the switch didn't use the break keyword itself.
                        }
                    }
                } while (restart);
                Profiler.EndSample();
            }

            // Unload cells over time.
            mfTimeUntilNextCellUnload -= Time.deltaTime;
            if (mfTimeUntilNextCellUnload <= 0.0f)  // Running more than once per frame does nothing extra.
            {
                mfTimeUntilNextCellUnload = mfTimeBetweenCellUnloads;   // Drop the remainder as it is not required to run multiple times to make up for lag spikes.

                if (mCellsToUnload.Count > 0)    // If there are cells to unload...
                {
                    UnloadAbsoluteCell(mCellsToUnload[0]); // Unload the cell.
                    mCellsToUnload.RemoveAt(0); // Cell has been removed.
                }
            }

            // Queue for unloading: Gubbins that are not within proximity to the cells.
            mfTimeUntilNextQueueGubbinToUnload -= Time.deltaTime;
            if (mfTimeUntilNextQueueGubbinToUnload <= 0.0f)  // Running more than once per frame does nothing extra.
            {
                mfTimeUntilNextQueueGubbinToUnload = mfTimeBetweenQueueGubbinsToUnload;   // Drop the remainder as it is not required to run multiple times to make up for lag spikes.

                mbValidGubbinValue = !mbValidGubbinValue;

                // Find gubbins that are not within proximity to the cells.
                Profiler.BeginSample("Find gubbins");

                //foreach (CRegisteredGubbin gubbin in mGubbins)
                //{
                //    foreach (System.Collections.Generic.KeyValuePair<SCellPos, CCellContent> pair in mCells)
                //    {
                //        if (RelativeCellWithinProximityOfPoint(pair.Key - mCentreCell, gubbin.mEntity.transform.position, gubbin.mBoundingRadius))
                //        {
                //            gubbin.mAlternator = mbValidGubbinValue;
                //            break;
                //        }
                //    }
                //}

                foreach (CRegisteredGubbin gubbin in mGubbins)
                {
                    Vector3 gubbinPosition = gubbin.mEntity.transform.position;
                    SCellPos occupiedRelativeCell = PointToRelativeCell(gubbinPosition);
                    int iCellsInARow = 1 + (Mathf.CeilToInt((gubbin.mBoundingRadius / cellRadius) - 1) * 2);    // Centre point plus neighbours per axis.   E.g. 1,3,5,7,9...
                    int iNeighboursPerDirection = (iCellsInARow - 1) / 2;                                       // Neighbours per direction.                E.g. 0,2,4,6,8...

                    // Iterate through all 3 axis, checking the centre cell first.
                    int x = 0;
                    int y = 0;
                    int z = 0;
                    do
                    {
                        do
                        {
                            do
                            {
                                // Check if this cell is loaded.
                                SCellPos neighbouringRelativeCell = new SCellPos(occupiedRelativeCell.x + x, occupiedRelativeCell.y + y, occupiedRelativeCell.z + z);
                                if (RelativeCellWithinProximityOfPoint(neighbouringRelativeCell, gubbinPosition, gubbin.mBoundingRadius))
                                {
                                    if (mCells.ContainsKey(neighbouringRelativeCell + mCentreCell))
                                    {
                                        gubbin.mAlternator = mbValidGubbinValue;
                                        x = y = z = -1;  // Way to break the nested loop.
                                    }
                                }

                                ++z; if (z > iNeighboursPerDirection) z = -iNeighboursPerDirection;
                            } while (z != 0);

                            ++y; if (y > iNeighboursPerDirection) y = -iNeighboursPerDirection;
                        } while (y != 0);

                        ++x; if (x > iNeighboursPerDirection) x = -iNeighboursPerDirection;
                    } while (x != 0);
                }

                Profiler.EndSample();

                // Queue for unloading: Gubbins that are not within proximity to the cells.
                Profiler.BeginSample("Queue gubbins for unloading");
                foreach (CRegisteredGubbin gubbin in mGubbins)
                {
                    if (!gubbin.mAwaitingCull && gubbin.mAlternator != mbValidGubbinValue)  // If this gubbin needs to be culled, and is not already marked for culling...
                    {
                        gubbin.mAwaitingCull = true;    // Mark for culling.
                        mGubbinsToUnload.Add(gubbin);   // Queue for culling.
                    }
                    else if (gubbin.mAwaitingCull && gubbin.mAlternator == mbValidGubbinValue)  // If this gubbin does not need to be culled, but is marked for culling...
                    {
                        gubbin.mAwaitingCull = false;   // Unmark for culling.
                        mGubbinsToUnload.Remove(gubbin);    // Unqueue for culling.
                    }
                }
                Profiler.EndSample();
            }

            // Unload gubbins over time.
            mfTimeUntilNextGubbinUnload -= Time.deltaTime;
            while (mfTimeUntilNextGubbinUnload <= 0.0f) // May unload more than one gubbin per frame.
            {
                mfTimeUntilNextGubbinUnload += mfTimeBetweenGubbinUnloads;  // Preserve the remainder so the right number of gubbins are unloaded over time, regardless of lag spikes.

                if (mGubbinsToUnload.Count > 0)  // If there are gubbins to unload...
                {
                    UnloadGubbin(mGubbinsToUnload[0]);
                    mGubbinsToUnload.RemoveAt(0);
                }
            }

            // Shift the galaxy over time. Note how this is done after cells and gubbins are unloaded, and before cells and gubbins are loaded - this is to minimise the performance impact.
            mfTimeUntilNextShiftTest -= Time.deltaTime;
            if (mfTimeUntilNextShiftTest <= 0.0f)  // Running more than once per frame does nothing extra.
            {
                mfTimeUntilNextShiftTest = mfTimeBetweenShiftTests;   // Drop the remainder as it is not required to run multiple times to make up for lag spikes.

                // Shift the galaxy if the average position of all points is far from the centre of the scene (0,0,0).
                SCellPos relativeCentrePos = PointToRelativeCell(CalculateAverageObserverPosition());
                if (relativeCentrePos.x != 0)
                    mCentreCellX.Set(mCentreCell.x + relativeCentrePos.x);
                if (relativeCentrePos.y != 0)
                    mCentreCellY.Set(mCentreCell.y + relativeCentrePos.y);
                if (relativeCentrePos.z != 0)
                    mCentreCellZ.Set(mCentreCell.z + relativeCentrePos.z);
            }

            // Load queued cells over time.
            mfTimeUntilNextCellLoad -= Time.deltaTime;
            while (mfTimeUntilNextCellLoad <= 0.0f) // May load more than one queued cell per frame.
            {
                mfTimeUntilNextCellLoad += mfTimeBetweenCellLoads;  // Preserve the remainder so the right number of cells are loaded over time, regardless of lag spikes.

                // Time to load a cell.
                if (mCellsToLoad.Count > 0)  // If there are cells to load...
                {
                    LoadAbsoluteCell(mCellsToLoad[0]);  // Load the cell.
                    mCellsToLoad.RemoveAt(0);
                }
            }

            // Load gubbins over time.
            mfTimeUntilNextGubbinLoad -= Time.deltaTime;
            while (mfTimeUntilNextGubbinLoad <= 0)  // May load more than one queued gubbin per frame.
            {
                mfTimeUntilNextGubbinLoad += mfTimeBetweenGubbinLoads;  // Preserve the remainder so the right number of gubbins are loaded over time, regardless of lag spikes.

                if (mGubbinsToLoad.Count > 0)    // If there are gubbins to load...
                {
                    LoadGubbin(mGubbinsToLoad[0]);
                    mGubbinsToLoad.RemoveAt(0);
                }
            }
        }
    }

    private void ShiftEntities(Vector3 translation)
    {
        Profiler.BeginSample("ShiftEntities");

        foreach (Transform transform in mShiftableTransforms)
            transform.position += translation;

        Profiler.EndSample();
    }

    public Vector3 CalculateAverageObserverPosition()
    {
        Profiler.BeginSample("CalculateAverageObserverPosition");

        Vector3 result = new Vector3();
        foreach (CRegisteredObserver observer in mObservers)
            result += observer.mEntity.transform.position;

        Profiler.EndSample();

        return result / mObservers.Count;
    }

    public void SyncNoiseSeed(INetworkVar sender)
    {
        for(uint ui = 0; ui < (uint)ENoiseLayer.MAX; ++ui)
            if(mNoiseSeeds[ui] == sender)
                mNoises[ui].Seed(mNoiseSeeds[ui].Get());
    }

    public void SyncCentreCellX(INetworkVar sender)
    {
        ShiftEntities(new Vector3((mCentreCell.x - mCentreCellX.Get()) * cellDiameter, 0.0f, 0.0f));
        mCentreCell.x = mCentreCellX.Get();
    }
    public void SyncCentreCellY(INetworkVar sender)
    {
        ShiftEntities(new Vector3(0.0f, (mCentreCell.y - mCentreCellY.Get()) * cellDiameter, 0.0f));
        mCentreCell.y = mCentreCellY.Get();
    }
    public void SyncCentreCellZ(INetworkVar sender)
    {
        ShiftEntities(new Vector3(0.0f, 0.0f, (mCentreCell.z - mCentreCellZ.Get()) * cellDiameter));
        mCentreCell.z = mCentreCellZ.Get();
    }
    public void SyncGalaxySize(INetworkVar sender) { mfGalaxySize = mGalaxySize.Get(); }
    public void SyncMaxAsteroidsPerCell(INetworkVar sender) { muiMaxAsteroidsPerCell = mMaxAsteroidsPerCell.Get(); }
    public void SyncNumCellSubsets(INetworkVar sender) { muiNumCellSubsets = mNumCellSubsets.Get(); }

    public void RegisterObserver(GameObject observer, float boundingRadius)
    {
        Profiler.BeginSample("RegisterObserver");
        mObservers.Add(new CRegisteredObserver(observer, boundingRadius));
        Profiler.EndSample();
    }

    public void DeregisterObserver(GameObject observer)
    {
        Profiler.BeginSample("DeregisterObserver");

        foreach (CRegisteredObserver elem in mObservers)
        {
            if (elem.mEntity.GetInstanceID() == observer.GetInstanceID())
            {
                mObservers.Remove(elem);
                break;
            }
        }

        Profiler.EndSample();
    }

    // Returns false if the galaxy is not ready to update galaxyIEs.
    public bool RegisterGalaxyIE(GalaxyIE galaxyIE)
    {
        if (mGalaxyIEs != null) // Whether the galaxy is ready to update galaxyIEs or not is determined by whether the list is instantiated or not.
        {
            Profiler.BeginSample("RegisterGalaxyIE");

            mGalaxyIEs.Add(galaxyIE);

            // Provide initial assets.
            UpdateGalaxyIE(mCentreCell, galaxyIE);

            Profiler.EndSample();

            return true;
        }
        else
            return false;
    }

    public void DeregisterGalaxyIE(GalaxyIE galaxyIE)
    {
        Profiler.BeginSample("DeregisterGalaxyIE");
        mGalaxyIEs.Remove(galaxyIE);
        Profiler.EndSample();
    }

    public void RegisterShiftableEntity(Transform shiftableTransform)
    {
        Profiler.BeginSample("RegisterShiftableEntity");
        mShiftableTransforms.Add(shiftableTransform);
        Profiler.EndSample();
    }

    public void DeregisterShiftableEntity(Transform shiftableTransform)
    {
        Profiler.BeginSample("DeregisterShiftableEntity");
        mShiftableTransforms.Remove(shiftableTransform);
        Profiler.EndSample();
    }

    void LoadAbsoluteCell(SCellPos absoluteCell)
    {
        Profiler.BeginSample("LoadAbsoluteCell");

        Profiler.BeginSample("Push new cell to dictionary");
        // If the cell was queued for loading, it will already have an entry in the cell dictionary, but unlike Add(); the [] operator allows overwriting existing elements in the dictionary.
        mCells[absoluteCell] = new CCellContent(mbValidCellValue, ECellState.Loaded); // Create cell with updated alternator to indicate cell is within proximity of observer.
        Profiler.EndSample();

        SCellPos relativeCell = absoluteCell - mCentreCell;

        // Load the content for the cell.
        //if (false)   // TODO: If the content for the cell is on file...
        //{
        //    // TODO: Load content from SQL.
        //}
        //else    // This cell is not on file, so it has not been visited...
        {
            // Generate the content in the cell.
            float fCellRadius = cellRadius;

            // 1) For asteroids.
            uint uiNumAsteroids = (uint)Mathf.RoundToInt(muiMaxAsteroidsPerCell * (0.5f + 0.5f * mNoises[(uint)ENoiseLayer.AsteroidDensity].Generate(absoluteCell.x, absoluteCell.y, absoluteCell.z))) /*Mathf.RoundToInt(PerlinSimplexNoise.(ENoiseLayer.AsteroidDensity, absoluteCell))*/;
            for (uint ui = 0; ui < uiNumAsteroids; ++ui)
            {
                Profiler.BeginSample("Create asteroid meta and queue for creation");
                
                mGubbinsToLoad.Add(new SGubbinMeta( (CGame.ENetworkRegisteredPrefab)Random.Range((ushort)CGame.ENetworkRegisteredPrefab.Asteroid_FIRST, (ushort)CGame.ENetworkRegisteredPrefab.Asteroid_LAST+1),    // Random asteroid prefab.
                                                    absoluteCell,   // Parent cell.
                                                    Random.Range(10.0f, 150.0f),    // Scale.
                                                    new Vector3(Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius)), // Position within parent cell.
                                                    Random.rotationUniform, // Rotation.
                                                    Random.onUnitSphere * Random.Range(0.0f, 75.0f),    // Linear velocity.
                                                    Random.onUnitSphere * Random.Range(0.0f, 2.0f), // Angular velocity.
                                                    true,   // Has NetworkedEntity script.
                                                    true    // Has a rigid body.
                                                    ));

                Profiler.EndSample();
            }
        }

        Profiler.EndSample();
    }

    void UnloadAbsoluteCell(SCellPos absoluteCell)
    {
        Profiler.BeginSample("UnloadAbsoluteCell");

        // Todo: Save stuff to file.
        mCells.Remove(absoluteCell); // Unload the cell.

        Profiler.EndSample();
    }

    void LoadGubbin(SGubbinMeta gubbin)
    {
        Profiler.BeginSample("LoadGubbin");

        // Create object.
        Profiler.BeginSample("Call to CNetwork.Factory.CreateObject()");
        GameObject gubbinObject = CNetwork.Factory.CreateObject((ushort)gubbin.mPrefabID);
        Profiler.EndSample();

        if (gubbinObject == null)
        {
            Profiler.EndSample();   // LoadGubbin.
            return;
        }

        // Grab components for future use.
        CNetworkView networkView = gubbinObject.GetComponent<CNetworkView>(); System.Diagnostics.Debug.Assert(networkView != null); // Get network view - the object is assumed to have one.
        NetworkedEntity networkedEntity = gubbin.mHasNetworkedEntityScript ? gubbinObject.GetComponent<NetworkedEntity>() : null;   // Get networked entity script IF it has one.
        Rigidbody rigidBody = gubbin.mHasRigidBody ? gubbinObject.GetComponent<Rigidbody>() : null; // Get rigid body IF it has one.

        // Parent object.
        gubbinObject.transform.parent = gameObject.transform;   // Set the object's parent as the galaxy.
        networkView.SyncParent();   // Sync the parent through the network view - the networked entity script does not handle this.

        // Position.
        gubbinObject.transform.position = RelativeCellCentrePoint(gubbin.mParentAbsoluteCell - mCentreCell) + gubbin.mPosition; // Set position.
        if(!networkedEntity || !networkedEntity.Position)   // If the object does not have a networked entity script, or if the networked entity script does not update position...
            networkView.SyncTransformPosition();    // Sync the position through the network view.

        // Rotation.
        gubbinObject.transform.rotation = gubbin.mRotation; // Set rotation
        if (!networkedEntity || !networkedEntity.Angle)  // If the object does not have a networked entity script, or if the networked entity script does not update rotation...
            networkView.SyncTransformRotation();// Sync the rotation through the network view.

        // Scale.
        if (gubbin.mScale != 1.0f)   // Avoid applying scale if possible, as it may invoke expensive computations behind the scenes.
        {
            gubbinObject.transform.localScale *= gubbin.mScale; // Set scale.
            networkView.SyncTransformScale();   // Sync the scale through the network view - the network entity script does not sync this.

            // Mass.
            if (rigidBody != null)  // If the object has a rigid body...
            {
                rigidBody.mass *= gubbin.mScale * gubbin.mScale;    // Update the mass of the object based on its new scale.
                networkView.SyncRigidBodyMass();    // Sync the mass through the network view - the network entity script does not sync this.
            }
        }

        // Linear velocity.
        if (rigidBody != null && gubbin.mLinearVelocity != Vector3.zero)
            rigidBody.velocity = gubbin.mLinearVelocity;

        // Angular velocity.
        if (rigidBody != null && gubbin.mAngularVelocity != Vector3.zero)
            rigidBody.angularVelocity = gubbin.mAngularVelocity;

        // Sync everything the networked entity script handles.
        if (networkedEntity)
            networkedEntity.UpdateNetworkVars();

        Profiler.BeginSample("Push gubbin to list of gubbins");
        mGubbins.Add(new CRegisteredGubbin(gubbinObject, CGalaxy.GetBoundingRadius(gubbinObject), networkView.ViewId, mbValidGubbinValue));
        Profiler.EndSample();

        Profiler.EndSample();   // LoadGubbin.
    }

    void UnloadGubbin(CRegisteredGubbin gubbin)
    {
        Profiler.BeginSample("UnloadGubbin");

        // Todo: Save gubbin to file.
        mGubbins.Remove(gubbin);
        CNetwork.Factory.DestoryObject(gubbin.mNetworkViewID);

        Profiler.EndSample();
    }

    public float SampleNoise(float x, float y, float z, ENoiseLayer noiseLayer)
    {
        return mNoises[(uint)noiseLayer].Generate(x, y, z);
    }

    public Vector3 RelativeCellCentrePoint(SCellPos relativeCell)
    {
        Profiler.BeginSample("RelativeCellCentrePoint");

        Vector3 result = new Vector3(relativeCell.x * cellDiameter, relativeCell.y * cellDiameter, relativeCell.z * cellDiameter);

        Profiler.EndSample();

        return result;
    }

    public SCellPos PointToAbsoluteCell(Vector3 point)
    {
        Profiler.BeginSample("PointToAbsoluteCell");

        point.x += cellRadius;
        point.y += cellRadius;
        point.z += cellRadius;
        point /= cellDiameter;
        SCellPos result = new SCellPos(Mathf.FloorToInt(point.x) + mCentreCell.x, Mathf.FloorToInt(point.y) + mCentreCell.y, Mathf.FloorToInt(point.z) + mCentreCell.z);

        Profiler.EndSample();

        return result;
    }

    public SCellPos PointToRelativeCell(Vector3 point)
    {
        Profiler.BeginSample("PointToRelativeCell");

        point.x += cellRadius;
        point.y += cellRadius;
        point.z += cellRadius;
        point /= cellDiameter;
        SCellPos result = new SCellPos(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), Mathf.FloorToInt(point.z));

        Profiler.EndSample();

        return result;
    }

    public bool RelativeCellWithinProximityOfPoint(SCellPos relativeCell, Vector3 point, float pointRadius)
    {
        Profiler.BeginSample("RelativeCellWithinProximityOfPoint");

        Vector3 cellCentrePos = new Vector3(relativeCell.x * cellDiameter, relativeCell.y * cellDiameter, relativeCell.z * cellDiameter);
        float cellBoundingSphereRadius = cellDiameter * 0.86602540378443864676372317075294f;
        bool result = (cellCentrePos - point).sqrMagnitude <= cellBoundingSphereRadius * cellBoundingSphereRadius + pointRadius * pointRadius;

        Profiler.EndSample();

        return result;
    }

    // Set the aesthetic of the galaxy based on the observer's position.
    void UpdateGalaxyIE(SCellPos absoluteCell, GalaxyIE galaxyIE)
    {
        Profiler.BeginSample("UpdateGalaxyIE");

        // Skybox.
        galaxyIE.mSkyboxMaterial.SetTexture("_Skybox1", mSkyboxes[(uint)ESkybox.Composite]);
        galaxyIE.mSkyboxMaterial.SetVector("_Tint", Color.grey);

        // Fog.
        Shader.SetGlobalFloat("void_FogStartDistance", 20.0f);
        Shader.SetGlobalFloat("void_FogEndDistance", 40.0f);
        Shader.SetGlobalFloat("void_FogDensity", 0.01f);

        Profiler.EndSample();
    }

    void OnDrawGizmos()/*OnDrawGizmos & OnDrawGizmosSelected*/
    {
        Profiler.BeginSample("OnDrawGizmos");

        if (CNetwork.IsServer)
        {
            foreach (CRegisteredObserver elem in mObservers)
                Gizmos.DrawWireSphere(elem.mEntity.transform.position, elem.mBoundingRadius);

            foreach (CRegisteredGubbin elem in mGubbins)
                Gizmos.DrawWireSphere(elem.mEntity.transform.position, elem.mBoundingRadius);

            float fCellDiameter = cellDiameter;
            float fCellRadius = fCellDiameter * .5f;

            foreach (System.Collections.Generic.KeyValuePair<SCellPos, CCellContent> pair in mCells)
            {
                SCellPos relativeCell = pair.Key - mCentreCell;

                float x = relativeCell.x * fCellDiameter;
                float y = relativeCell.y * fCellDiameter;
                float z = relativeCell.z * fCellDiameter;

                // Set colour based on whether it is loading, loaded, or unloading.
                GL.Color(pair.Value.mState == ECellState.Loading ? Color.yellow : pair.Value.mState == ECellState.Loaded ? Color.green : /*Else unloading*/Color.red);

                GL.Begin(GL.LINES);
                GL.Vertex3(x - fCellRadius, y - fCellRadius, z - fCellRadius);
                GL.Vertex3(x + fCellRadius, y - fCellRadius, z - fCellRadius);
                GL.End();
                GL.Begin(GL.LINES);
                GL.Vertex3(x - fCellRadius, y + fCellRadius, z - fCellRadius);
                GL.Vertex3(x + fCellRadius, y + fCellRadius, z - fCellRadius);
                GL.End();
                GL.Begin(GL.LINES);
                GL.Vertex3(x - fCellRadius, y - fCellRadius, z + fCellRadius);
                GL.Vertex3(x + fCellRadius, y - fCellRadius, z + fCellRadius);
                GL.End();
                GL.Begin(GL.LINES);
                GL.Vertex3(x - fCellRadius, y + fCellRadius, z + fCellRadius);
                GL.Vertex3(x + fCellRadius, y + fCellRadius, z + fCellRadius);
                GL.End();
                GL.Begin(GL.LINES);
                GL.Vertex3(x - fCellRadius, y - fCellRadius, z - fCellRadius);
                GL.Vertex3(x - fCellRadius, y + fCellRadius, z - fCellRadius);
                GL.End();
                GL.Begin(GL.LINES);
                GL.Vertex3(x + fCellRadius, y - fCellRadius, z - fCellRadius);
                GL.Vertex3(x + fCellRadius, y + fCellRadius, z - fCellRadius);
                GL.End();
                GL.Begin(GL.LINES);
                GL.Vertex3(x - fCellRadius, y - fCellRadius, z + fCellRadius);
                GL.Vertex3(x - fCellRadius, y + fCellRadius, z + fCellRadius);
                GL.End();
                GL.Begin(GL.LINES);
                GL.Vertex3(x + fCellRadius, y - fCellRadius, z + fCellRadius);
                GL.Vertex3(x + fCellRadius, y + fCellRadius, z + fCellRadius);
                GL.End();
                GL.Begin(GL.LINES);
                GL.Vertex3(x - fCellRadius, y - fCellRadius, z - fCellRadius);
                GL.Vertex3(x - fCellRadius, y - fCellRadius, z + fCellRadius);
                GL.End();
                GL.Begin(GL.LINES);
                GL.Vertex3(x - fCellRadius, y + fCellRadius, z - fCellRadius);
                GL.Vertex3(x - fCellRadius, y + fCellRadius, z + fCellRadius);
                GL.End();
                GL.Begin(GL.LINES);
                GL.Vertex3(x + fCellRadius, y - fCellRadius, z - fCellRadius);
                GL.Vertex3(x + fCellRadius, y - fCellRadius, z + fCellRadius);
                GL.End();
                GL.Begin(GL.LINES);
                GL.Vertex3(x + fCellRadius, y + fCellRadius, z - fCellRadius);
                GL.Vertex3(x + fCellRadius, y + fCellRadius, z + fCellRadius);
                GL.End();
            }

        }

        Profiler.EndSample();
    }

    public static float GetBoundingRadius(GameObject _gameObject)
    {
        float result = 1.0f;

        // Depending on the type of model; it may use a collider, mesh renderer, animator, or something else.
        Collider collider = _gameObject.GetComponent<Collider>();
        if (collider)
            result = collider.bounds.extents.magnitude;
        else
        {
            MeshRenderer meshRenderer = _gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer)
                result = meshRenderer.bounds.extents.magnitude;
            else
            {
                bool gotSomethingFromAnimator = false;
                Animator anim = _gameObject.GetComponent<Animator>();
                if (anim)
                {
                    gotSomethingFromAnimator = anim.renderer || anim.collider || anim.rigidbody;
                    if (anim.renderer) result = anim.renderer.bounds.extents.magnitude;
                    else if (anim.collider) result = anim.collider.bounds.extents.magnitude;
                    else if (anim.rigidbody) result = anim.rigidbody.collider.bounds.extents.magnitude;
                }

                if (!gotSomethingFromAnimator)
                    Debug.LogWarning("Galaxy→GetBoundingRadius(): No RigidBody, Collider, MeshRenderer, or Animator on " + _gameObject.name + ". Bounding radius set to 1");
            }
        }

        return result;
    }
}