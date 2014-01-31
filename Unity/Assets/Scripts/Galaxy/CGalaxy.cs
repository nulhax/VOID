//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLASSNAME.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        public CNetworkViewId mNetworkViewID;
        public bool mAlternator;   // This is used for culling purposes.
        public bool mAwaitingCull = false;  // Objects get culled over time.

        public CRegisteredGubbin(GameObject entity, float boundingRadius, CNetworkViewId networkViewID, bool alternatorValue) { mEntity = entity; mBoundingRadius = boundingRadius; mNetworkViewID = networkViewID; mAlternator = alternatorValue; }
    }

    public class SGubbinMeta
    {
        public CGameRegistrator.ENetworkPrefab mPrefabID;
        public SCellPos mParentAbsoluteCell;
        public float mScale;
        public Vector3 mPosition;
        public Quaternion mRotation;
        public Vector3 mLinearVelocity;
        public Vector3 mAngularVelocity;
        public float mMassToHealthScalar;
        public bool mHasNetworkedEntityScript;
        public bool mHasRigidBody;

        public SGubbinMeta(CGameRegistrator.ENetworkPrefab prefabID, SCellPos parentAbsoluteCell, float scale, Vector3 position, Quaternion rotation, Vector3 linearVelocity, Vector3 angularVelocity, float massToHealthScalar, bool hasNetworkedEntityScript, bool hasRigidBody)
        {
            mPrefabID = prefabID;
            mParentAbsoluteCell = parentAbsoluteCell;
            mScale = scale;
            mPosition = position;
            mRotation = rotation;
            mLinearVelocity = linearVelocity;
            mAngularVelocity = angularVelocity;
            mMassToHealthScalar = massToHealthScalar;
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
        SparseAsteroidCount,
        AsteroidClusterCount,
        DebrisDensity,
        FogDensity,
        AsteroidResourceAmount,
        EnemyShipCount,
        MAX
    }

    ///////////////////////////////////////////////////////////////////////////
    // Variables:

    private static CGalaxy sGalaxy = null;
    public static CGalaxy instance { get { return sGalaxy; } }

    private PerlinSimplexNoise[] mNoises = new PerlinSimplexNoise[(uint)ENoiseLayer.MAX];
    protected CNetworkVar<int>[] mNoiseSeeds = new CNetworkVar<int>[(uint)ENoiseLayer.MAX];

    private SCellPos mCentreCell = new SCellPos(0, 0, 0);    // All cells are offset by this cell.
    protected CNetworkVar<int> mCentreCellX;
    protected CNetworkVar<int> mCentreCellY;
    protected CNetworkVar<int> mCentreCellZ;
    public SCellPos centreCell { get { return mCentreCell; } }

    private List<GalaxyShiftable> mShiftableEntities = new List<GalaxyShiftable>();    // When everything moves too far in any direction, the transforms of these registered GameObjects are shifted back.
    private List<CRegisteredObserver> mObservers = new List<CRegisteredObserver>(); // Cells are loaded and unloaded based on proximity to observers.
    private List<CRegisteredGubbin> mGubbins;    // Gubbins ("space things") are unloaded based on proximity to cells.
    private List<SGubbinMeta> mGubbinsToLoad;
    private List<CRegisteredGubbin> mGubbinsToUnload;
    private Dictionary<SCellPos, CCellContent> mCells;
    private List<SCellPos> mCellsToLoad;
    private List<SCellPos> mCellsToUnload;

    private float mfGalaxySize = 1391000000.0f; // (1.3 million kilometres) In metres cubed. Floats can increment up to 16777220.0f (16.7 million).
    protected CNetworkVar<float> mGalaxySize;
    public float galaxySize { get { return mfGalaxySize; } }
    public float galaxyRadius { get { return galaxySize * 0.5f; } }

    private uint muiNumCellSubsets = 20; // Zero is just the one cell. Also, this is equivalent to the number of bits per axis required to acknowledge each cell (<= 2 for 1 byte, <= 5 for 2 bytes, <= 10 for 4 bytes, <= 21 for 8 bytes).
    protected CNetworkVar<uint> mNumCellSubsets;
    public uint numCellSubsets { get { return muiNumCellSubsets; } }

<<<<<<< HEAD
    public const float mfTimeBetweenUpdateCellLoadUnloadQueues = 0.15f;
    private float mfTimeUntilNextUpdateCellLoadUnloadQueues = 0.0f;

    public const float mfTimeBetweenCellLoads = 0.05f;
    private float mfTimeUntilNextCellLoad = 0.0f;

    public const float mfTimeBetweenCellUnloads = mfTimeBetweenCellLoads;
    private float mfTimeUntilNextCellUnload = mfTimeBetweenCellLoads / 2;

    public const float mfTimeBetweenUpdateGubbinUnloadQueue = mfTimeBetweenUpdateCellLoadUnloadQueues;
    private float mfTimeUntilNextUpdateGubbinUnloadQueue = mfTimeBetweenUpdateCellLoadUnloadQueues / 2;

    public const float mfTimeBetweenGubbinLoads = 0.01f;
    private float mfTimeUntilNextGubbinLoad = 0.0f;

    public const float mfTimeBetweenGubbinUnloads = mfTimeBetweenGubbinLoads;
    private float mfTimeUntilNextGubbinUnload = mfTimeBetweenGubbinLoads / 2;

    public const float mfTimeBetweenShiftTests = 0.5f;
    private float mfTimeUntilNextShiftTest = 0.0f;
=======
    public const float mfTimeBetweenUpdateCellLoadUnloadQueues = 0.15f;
    private float mfTimeUntilNextUpdateCellLoadUnloadQueues = 0.0f;

    public const float mfTimeBetweenCellLoads = 0.05f;
    private float mfTimeUntilNextCellLoad = 0.0f;

    public const float mfTimeBetweenCellUnloads = mfTimeBetweenCellLoads;
    private float mfTimeUntilNextCellUnload = mfTimeBetweenCellLoads / 2;

    public const float mfTimeBetweenUpdateGubbinUnloadQueue = mfTimeBetweenUpdateCellLoadUnloadQueues;
    private float mfTimeUntilNextUpdateGubbinUnloadQueue = mfTimeBetweenUpdateCellLoadUnloadQueues / 2;

    public const float mfTimeBetweenGubbinLoads = 0.1f;
    private float mfTimeUntilNextGubbinLoad = 0.0f;

    public const float mfTimeBetweenGubbinUnloads = mfTimeBetweenGubbinLoads;
    private float mfTimeUntilNextGubbinUnload = mfTimeBetweenGubbinLoads / 2;

    public const float mfTimeBetweenShiftTests = 0.5f;
    private float mfTimeUntilNextShiftTest = 0.0f;
>>>>>>> dddd13219fbce1556ae52b8f7d00cfdbf4a8d4d0

    private uint mNumExtraNeighbourCells = 3;   // Number of extra cells to load in every direction (i.e. load neighbours up to some distance).
    public uint numExtraNeighbourCells { get { return mNumExtraNeighbourCells; } }

    private bool mbValidCellValue = false;  // Used for culling cells that are too far away from observers.
    private bool mbValidGubbinValue = false;    // Used for culling gubbins that are too far away from cells.

    public float cellDiameter { get { return mfGalaxySize / numCellsInRow; } }
    public float cellRadius { get { return mfGalaxySize / (numCellsInRow * 2u); } }
    public ulong numCells { get { /*return (uint)Mathf.Pow(8, muiNumCellSubsets);*/ ulong ul = 1; for (uint ui2 = 0; ui2 < muiNumCellSubsets; ++ui2)ul *= 8u; return ul; } }
    public uint numCellsInRow { get { /*return (uint)Mathf.Pow(2, muiNumCellSubsets);*/ uint ui = 1; for (uint ui2 = 0; ui2 < muiNumCellSubsets; ++ui2)ui *= 2; return ui; } }

    ///////////////////////////////////////////////////////////////////////////
    // Functions:

    public CGalaxy()
    {
		sGalaxy = this;

        // Instantiate galaxy noises.
        for (uint ui = 0; ui < (uint)ENoiseLayer.MAX; ++ui)
            mNoises[ui] = new PerlinSimplexNoise();
    }

    void Start()
    {
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
            for (uint ui = 0; ui < (uint)ENoiseLayer.MAX; ++ui)
                mNoiseSeeds[ui].Set(Random.Range(int.MinValue, int.MaxValue));

            gameObject.AddComponent<DungeonMaster>();
            new DynamicEvent_RogueAsteroid();
            new DifficultyModifier_DifficultyChoice();
            gameObject.AddComponent<DifficultyModifier_RandomFluctuation>();
            new DifficultyModifier_ShipDamage();
            gameObject.AddComponent<DifficultyModifier_TotalDistanceTravelled>();
            new DifficultyModifier_TotalShipWorth();
        }
    }

    void OnDestroy()
    {
        sGalaxy = null;
    }

    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        Profiler.BeginSample("InstanceNetworkVars");

        for (uint ui = 0; ui < (uint)ENoiseLayer.MAX; ++ui)
            mNoiseSeeds[ui]= _cRegistrar.CreateNetworkVar<int>(SyncNoiseSeed);

        mCentreCellX= _cRegistrar.CreateNetworkVar<int>(SyncCentreCellX, mCentreCell.x);
        mCentreCellY= _cRegistrar.CreateNetworkVar<int>(SyncCentreCellY, mCentreCell.y);
        mCentreCellZ= _cRegistrar.CreateNetworkVar<int>(SyncCentreCellZ, mCentreCell.z);
        mGalaxySize = _cRegistrar.CreateNetworkVar<float>(SyncGalaxySize, mfGalaxySize);
        mNumCellSubsets = _cRegistrar.CreateNetworkVar<uint>(SyncNumCellSubsets, muiNumCellSubsets);

        Profiler.EndSample();
    }

    void Update()
    {
        if (CNetwork.IsServer)
        {
            mfTimeUntilNextUpdateCellLoadUnloadQueues   -= Time.deltaTime;
            mfTimeUntilNextCellUnload                   -= Time.deltaTime;
            mfTimeUntilNextUpdateGubbinUnloadQueue      -= Time.deltaTime;
            mfTimeUntilNextGubbinUnload                 -= Time.deltaTime;
            mfTimeUntilNextShiftTest                    -= Time.deltaTime;
            mfTimeUntilNextCellLoad                     -= Time.deltaTime;
            mfTimeUntilNextGubbinLoad                   -= Time.deltaTime;

            if (mfTimeUntilNextUpdateCellLoadUnloadQueues <= 0.0f)
            { UpdateCellLoadingUnloadingQueues(); mfTimeUntilNextUpdateCellLoadUnloadQueues = mfTimeBetweenUpdateCellLoadUnloadQueues; }

            if (mfTimeUntilNextCellUnload <= 0.0f)
            { UnloadQueuedCell(); mfTimeUntilNextCellUnload = mfTimeBetweenCellUnloads; }

            if (mfTimeUntilNextUpdateGubbinUnloadQueue <= 0.0f)
            { UpdateGubbinUnloadingQueue(); mfTimeUntilNextUpdateGubbinUnloadQueue = mfTimeBetweenUpdateGubbinUnloadQueue; }

            while (mfTimeUntilNextGubbinUnload <= 0.0f)
            { UnloadQueuedGubbin(); mfTimeUntilNextGubbinUnload += mfTimeBetweenGubbinUnloads; }

            if (mfTimeUntilNextShiftTest <= 0.0f)
            { ShiftGalaxy(); mfTimeUntilNextShiftTest = mfTimeBetweenShiftTests; }

            while (mfTimeUntilNextCellLoad <= 0.0f)
            { LoadQueuedCell(); mfTimeUntilNextCellLoad += mfTimeBetweenCellLoads; }

            while (mfTimeUntilNextGubbinLoad <= 0.0f)
            { LoadQueuedGubbin(); mfTimeUntilNextGubbinLoad += mfTimeBetweenGubbinLoads; }
        }
    }

    private void UpdateCellLoadingUnloadingQueues()
    {
        Profiler.BeginSample("UpdateCellLoadingUnloadingQueues");

        UpdateCellLoadingQueue();
        UpdateCellUnloadingQueue();

        Profiler.EndSample();
    }

    private void UpdateCellLoadingQueue()
    {
        Profiler.BeginSample("UpdateCellLoadingQueue");
            
        mbValidCellValue = !mbValidCellValue;   // Alternate the valid cell value. All cells within proximity of an observer will be updated, while all others will retain the old value making it easier to detect and cull them.;

        // Queue for loading: unloaded cells within proximity to observers.
        foreach (CRegisteredObserver observer in mObservers)
        {
            Vector3 observerPosition = observer.mEntity.transform.position;
            SCellPos occupiedRelativeCell = RelativePointToRelativeCell(observerPosition);
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
    }

    private void UpdateCellUnloadingQueue()
    {
        Profiler.BeginSample("UpdateCellUnloadingQueue");

        // Queue for unloading: cells too far away from observers.
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

    private void UnloadQueuedCell()
    {
        Profiler.BeginSample("UnloadQueuedCell");

        if (mCellsToUnload.Count > 0)    // If there are cells to unload...
        {
            UnloadAbsoluteCell(mCellsToUnload[0]); // Unload the cell.
            mCellsToUnload.RemoveAt(0); // Cell has been removed.
        }

        Profiler.EndSample();
    }

    private void UpdateGubbinUnloadingQueue()
    {
        Profiler.BeginSample("UpdateGubbinUnloadingQueue");

        mbValidGubbinValue = !mbValidGubbinValue;

        MarkGubbinsToPreserve();
        QueueGubbinsForUnloading();

        Profiler.EndSample();
    }

    private void MarkGubbinsToPreserve()
    {
        Profiler.BeginSample("MarkGubbinsToPreserve");

        // Find gubbins that are not within proximity to the cells.

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
            SCellPos occupiedRelativeCell = RelativePointToRelativeCell(gubbinPosition);
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
    }

    private void QueueGubbinsForUnloading()
    {
        Profiler.BeginSample("QueueGubbinsForUnloading");

        // Queue for unloading: Gubbins that are not within proximity to the cells.
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

    private void UnloadQueuedGubbin()
    {
        Profiler.BeginSample("UnloadQueuedGubbin");

        if (mGubbinsToUnload.Count > 0)  // If there are gubbins to unload...
        {
            UnloadGubbin(mGubbinsToUnload[0]);
            mGubbinsToUnload.RemoveAt(0);
        }

        Profiler.EndSample();
    }

    private void ShiftGalaxy()
    {
        Profiler.BeginSample("ShiftGalaxy");

        // Shift the galaxy if the average position of all points is far from the centre of the scene (0,0,0).
        SCellPos relativeCentrePos = RelativePointToRelativeCell(CalculateAverageObserverPosition());
        if (relativeCentrePos.x != 0)
            mCentreCellX.Set(mCentreCell.x + relativeCentrePos.x);
        if (relativeCentrePos.y != 0)
            mCentreCellY.Set(mCentreCell.y + relativeCentrePos.y);
        if (relativeCentrePos.z != 0)
            mCentreCellZ.Set(mCentreCell.z + relativeCentrePos.z);

        Profiler.EndSample();
    }

    private void LoadQueuedCell()
    {
        Profiler.BeginSample("LoadQueuedCell");

        if (mCellsToLoad.Count > 0)  // If there are cells to load...
        {
            LoadAbsoluteCell(mCellsToLoad[0]);  // Load the cell.
            mCellsToLoad.RemoveAt(0);
        }

        Profiler.EndSample();
    }

    private void LoadQueuedGubbin()
    {
        Profiler.BeginSample("LoadQueuedGubbin");

        if (mGubbinsToLoad.Count > 0)    // If there are gubbins to load...
        {
            for (uint uiTry = 0; uiTry < 3; ++uiTry)   // Try a couple times to place the gubbin.
            {
                if (LoadGubbin(mGubbinsToLoad[0]))
                    break;
                else
                    mGubbinsToLoad[0].mPosition = new Vector3(Random.Range(-cellRadius, +cellRadius), Random.Range(-cellRadius, +cellRadius), Random.Range(-cellRadius, +cellRadius));
            }
            mGubbinsToLoad.RemoveAt(0);
        }

        Profiler.EndSample();
    }

    private void ShiftEntities(Vector3 translation)
    {
        Profiler.BeginSample("ShiftEntities");

		foreach (GalaxyShiftable shiftableEntity in mShiftableEntities)
			shiftableEntity.Shift(translation);

        Profiler.EndSample();
    }

    private Vector3 CalculateAverageObserverPosition()
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
        for (uint ui = 0; ui < (uint)ENoiseLayer.MAX; ++ui)
            if (mNoiseSeeds[ui] == sender)
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
    public void SyncGalaxySize(INetworkVar sender)
    {
        mfGalaxySize = mGalaxySize.Get();
    }
    public void SyncNumCellSubsets(INetworkVar sender)
    {
        muiNumCellSubsets = mNumCellSubsets.Get();
    }

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

    public void RegisterShiftableEntity(GalaxyShiftable shiftableEntity)
    {
        Profiler.BeginSample("RegisterShiftableEntity");
        mShiftableEntities.Add(shiftableEntity);
        Profiler.EndSample();
    }

    public void DeregisterShiftableEntity(GalaxyShiftable shiftableEntity)
    {
        Profiler.BeginSample("DeregisterShiftableEntity");
        mShiftableEntities.Remove(shiftableEntity);
        Profiler.EndSample();
    }

    void LoadAbsoluteCell(SCellPos absoluteCell)
    {
        Profiler.BeginSample("LoadAbsoluteCell");

        Profiler.BeginSample("Push new cell to dictionary");
        // If the cell was queued for loading, it will already have an entry in the cell dictionary, but unlike Add(); the [] operator allows overwriting existing elements in the dictionary.
        mCells[absoluteCell] = new CCellContent(mbValidCellValue, ECellState.Loaded); // Create cell with updated alternator to indicate cell is within proximity of observer.
        Profiler.EndSample();

        // Load the content for the cell.
        //if (false)   // TODO: If the content for the cell is on file...
        //{
        //    // TODO: Load content from SQL.
        //}
        //else    // This cell is not on file, so it has not been visited...
        {
            // Generate the content in the cell.
            LoadEnemyShips(absoluteCell);
            LoadAsteroidClusters(absoluteCell);
            LoadSparseAsteroids(absoluteCell);
            LoadDebris(absoluteCell);
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

    public void DeregisterGubbin(GalaxyGubbin gubbinToDeregister)
    {
        for (int ui = 0; ui < mGubbins.Count; ++ui)
        {
            if (mGubbins[ui].mEntity == gubbinToDeregister.gameObject)
            {
                if (mGubbins[ui].mAwaitingCull)
                    mGubbinsToUnload.Remove(mGubbins[ui]);

                mGubbins.RemoveAt(ui);
                break;
            }
        }

        gubbinToDeregister.registeredWithGalaxy = false;
    }

    public bool LoadGubbin(SGubbinMeta gubbin)
    {
        Profiler.BeginSample("LoadGubbin");

        // Create object.
        Profiler.BeginSample("Call to CNetwork.Factory.CreateObject()");
        GameObject gubbinObject = CNetwork.Factory.CreateObject((ushort)gubbin.mPrefabID);
        Profiler.EndSample();

        if (gubbinObject == null)
        {
            Profiler.EndSample();   // LoadGubbin.
            return false;
        }

        Vector3 gubbinPosition = RelativeCellToRelativePoint(gubbin.mParentAbsoluteCell - mCentreCell) + gubbin.mPosition;
        
        // Check if the new gubbin has room to spawn.
        if (Physics.CheckSphere(gubbinPosition, GetBoundingRadius(gubbinObject) * gubbin.mScale))
        {
            CNetwork.Factory.DestoryObject(gubbinObject);
            return false;
        }

        gubbinObject.AddComponent<GalaxyGubbin>();

        // Grab components for future use.
        CNetworkView networkView = gubbinObject.GetComponent<CNetworkView>(); System.Diagnostics.Debug.Assert(networkView != null); // Get network view - the object is assumed to have one.
        NetworkedEntity networkedEntity = gubbin.mHasNetworkedEntityScript ? gubbinObject.GetComponent<NetworkedEntity>() : null;   // Get networked entity script IF it has one.
        Rigidbody rigidBody = gubbin.mHasRigidBody ? gubbinObject.GetComponent<Rigidbody>() : null; // Get rigid body IF it has one.

        // Parent object.
        gubbinObject.GetComponent<CNetworkView>().SetParent(gameObject.GetComponent<CNetworkView>().ViewId);   // Set the object's parent as the galaxy.

        // Position.
        gubbinObject.transform.position = gubbinPosition; // Set position.
        if (!networkedEntity || !networkedEntity.Position)   // If the object does not have a networked entity script, or if the networked entity script does not update position...
            networkView.SyncTransformPosition();    // Sync the position through the network view.

        // Rotation.
        gubbinObject.transform.rotation = gubbin.mRotation; // Set rotation.
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
        if (rigidBody != null && gubbin.mLinearVelocity != null)
            rigidBody.velocity = gubbin.mLinearVelocity;

        // Angular velocity.
        if (rigidBody != null && gubbin.mAngularVelocity != null)
            rigidBody.angularVelocity = gubbin.mAngularVelocity;

        // Sync everything the networked entity script handles.
        if (networkedEntity)
            networkedEntity.UpdateNetworkVars();

<<<<<<< HEAD
        // Health.
        if (gubbin.mMassToHealthScalar > 0.0f && rigidBody != null)
        {
            CActorHealth health = gubbinObject.GetComponent<CActorHealth>();
            health.health = rigidBody.mass * gubbin.mMassToHealthScalar;
        }
=======
        // Health.
        //if (gubbin.mMassToHealthScalar > 0.0f && rigidBody != null)
        //{
        //    CActorHealth health = gubbinObject.GetComponent<CActorHealth>();
        //    health.health = rigidBody.mass * gubbin.mMassToHealthScalar;
        //}
>>>>>>> dddd13219fbce1556ae52b8f7d00cfdbf4a8d4d0

        Profiler.BeginSample("Push gubbin to list of gubbins");
        mGubbins.Add(new CRegisteredGubbin(gubbinObject, CGalaxy.GetBoundingRadius(gubbinObject), networkView.ViewId, mbValidGubbinValue));
        Profiler.EndSample();

        Profiler.EndSample();   // LoadGubbin.

        return true;
    }

    void UnloadGubbin(CRegisteredGubbin gubbin)
    {
        Profiler.BeginSample("UnloadGubbin");

        // Todo: Save gubbin to file.

        gubbin.mEntity.GetComponent<GalaxyGubbin>().registeredWithGalaxy = false;
        mGubbins.Remove(gubbin);
        CNetwork.Factory.DestoryObject(gubbin.mNetworkViewID);

        Profiler.EndSample();
    }

    public Vector3 AbsoluteCellNoiseSamplePoint(SCellPos absoluteCell, float sampleScale)
    {
        return new Vector3((sampleScale * absoluteCell.x * cellDiameter / cellRadius), (sampleScale * absoluteCell.y * cellDiameter / cellRadius), (sampleScale * absoluteCell.z * cellDiameter / cellRadius));
    }

    public float SampleNoise(float x, float y, float z, ENoiseLayer noiseLayer)
    {
        return 0.5f + 0.5f * mNoises[(uint)noiseLayer].Generate(x, y, z);
    }

    public float SampleNoise(SCellPos absoluteCell, float sampleScale, ENoiseLayer noiseLayer)
    {
        Vector3 samplePoint = AbsoluteCellNoiseSamplePoint(absoluteCell, sampleScale);
        return 0.5f + 0.5f * mNoises[(uint)noiseLayer].Generate(samplePoint.x, samplePoint.y, samplePoint.z);
    }

    public Vector3 RelativeCellToRelativePoint(SCellPos relativeCell)
    {
        Profiler.BeginSample("RelativeCellToRelativePoint");
        Vector3 result = new Vector3(relativeCell.x * cellDiameter, relativeCell.y * cellDiameter, relativeCell.z * cellDiameter);
        Profiler.EndSample();

        return result;
    }

    public Vector3 AbsoluteCellToAbsolutePoint(SCellPos absoluteCell)
    {
        Profiler.BeginSample("AbsoluteCellToAbsolutePoint");
        Vector3 result = new Vector3(absoluteCell.x * cellDiameter, absoluteCell.y * cellDiameter, absoluteCell.z * cellDiameter);
        Profiler.EndSample();

        return result;
    }

    public SCellPos RelativePointToRelativeCell(Vector3 relativePoint)
    {
        Profiler.BeginSample("RelativePointToRelativeCell");

        relativePoint.x += cellRadius;
        relativePoint.y += cellRadius;
        relativePoint.z += cellRadius;
        relativePoint /= cellDiameter;
        SCellPos result = new SCellPos(Mathf.FloorToInt(relativePoint.x), Mathf.FloorToInt(relativePoint.y), Mathf.FloorToInt(relativePoint.z));

        Profiler.EndSample();

        return result;
    }

    public SCellPos AbsolutePointToAbsoluteCell(Vector3 absolutePoint)
    {
        Profiler.BeginSample("AbsolutePointToAbsoluteCell");

        absolutePoint.x += cellRadius;
        absolutePoint.y += cellRadius;
        absolutePoint.z += cellRadius;
        absolutePoint /= cellDiameter;
        SCellPos result = new SCellPos(Mathf.FloorToInt(absolutePoint.x), Mathf.FloorToInt(absolutePoint.y), Mathf.FloorToInt(absolutePoint.z));

        Profiler.EndSample();

        return result;
    }

    public SCellPos RelativePointToAbsoluteCell(Vector3 relativePoint)
    {
        Profiler.BeginSample("RelativePointToAbsoluteCell");

        relativePoint.x += cellRadius;
        relativePoint.y += cellRadius;
        relativePoint.z += cellRadius;
        relativePoint /= cellDiameter;
        SCellPos result = new SCellPos(Mathf.FloorToInt(relativePoint.x) + mCentreCell.x, Mathf.FloorToInt(relativePoint.y) + mCentreCell.y, Mathf.FloorToInt(relativePoint.z) + mCentreCell.z);

        Profiler.EndSample();

        return result;
    }

    public SCellPos AbsolutePointToRelativeCell(Vector3 absolutePoint)
    {
        Profiler.BeginSample("AbsolutePointToRelativeCell");

        absolutePoint.x += cellRadius;
        absolutePoint.y += cellRadius;
        absolutePoint.z += cellRadius;
        absolutePoint /= cellDiameter;
        SCellPos result = new SCellPos(Mathf.FloorToInt(absolutePoint.x) - mCentreCell.x, Mathf.FloorToInt(absolutePoint.y) - mCentreCell.y, Mathf.FloorToInt(absolutePoint.z) - mCentreCell.z);

        Profiler.EndSample();

        return result;
    }

    public Vector3 RelativePointToAbsolutePoint(Vector3 relativePoint) { return relativePoint + AbsoluteCellToAbsolutePoint(mCentreCell); }
    public Vector3 AbsolutePointToRelativePoint(Vector3 absolutePoint) { return absolutePoint - AbsoluteCellToAbsolutePoint(mCentreCell); }

    public bool RelativeCellWithinProximityOfPoint(SCellPos relativeCell, Vector3 point, float pointRadius)
    {
        Profiler.BeginSample("RelativeCellWithinProximityOfPoint");

        Vector3 cellCentrePos = new Vector3(relativeCell.x * cellDiameter, relativeCell.y * cellDiameter, relativeCell.z * cellDiameter);
        float cellBoundingSphereRadius = cellDiameter * 0.86602540378443864676372317075294f;
        bool result = (cellCentrePos - point).sqrMagnitude <= cellBoundingSphereRadius * cellBoundingSphereRadius + pointRadius * pointRadius;

        Profiler.EndSample();

        return result;
    }

    public uint SparseAsteroidCount(SCellPos absoluteCell) { return (uint)Mathf.RoundToInt(4/*maxAsteroids*/ * SampleNoise_SparseAsteroid(absoluteCell)); }
    public uint AsteroidClusterCount(SCellPos absoluteCell) { return (uint)Mathf.RoundToInt(1/*maxClusters*/ * SampleNoise_AsteroidCluster(absoluteCell)); }
    public float DebrisDensity(SCellPos absoluteCell) { return SampleNoise_DebrisDensity(absoluteCell); }
    public float FogDensity(SCellPos absoluteCell) { return SampleNoise_FogDensity(absoluteCell); }
    public float ResourceAmount(SCellPos absoluteCell) { return 800 * SampleNoise_ResourceAmount(absoluteCell); }
    public uint EnemyShipCount(SCellPos absoluteCell) { return (uint)Mathf.RoundToInt(1/*maxEnemyShips*/ * SampleNoise_EnemyShipDensity(absoluteCell)); }

    public float SampleNoise_SparseAsteroid(SCellPos absoluteCell)
    {
        float sample = SampleNoise(absoluteCell, 0.01f, ENoiseLayer.SparseAsteroidCount);
        float start = 0.5f, end = 0.9f;
        sample = (sample - start) / (end - start);
        return sample < 0.0f ? 0.0f : sample > 1.0f ? 1.0f : sample;
    }
    
    public float SampleNoise_AsteroidCluster(SCellPos absoluteCell)
    {
        float sample = SampleNoise(absoluteCell, 0.1f, ENoiseLayer.AsteroidClusterCount);
        float start = 0.8f, end = 0.9f;
        sample = (sample - start) / (end - start);
        return sample < 0.0f ? 0.0f : sample > 1.0f ? 1.0f : sample;
    }

    public float SampleNoise_DebrisDensity(SCellPos absoluteCell)
    {
        float sample = SampleNoise(absoluteCell, 0.25f, ENoiseLayer.DebrisDensity);
        float start = 0.0f, end = 1.0f;
        sample = (sample - start) / (end - start);
        return sample < 0.0f ? 0.0f : sample > 1.0f ? 1.0f : sample;
    }

    public float SampleNoise_FogDensity(SCellPos absoluteCell)
    {
        float sample = SampleNoise(absoluteCell, 0.000001f, ENoiseLayer.FogDensity);
        float start = 0.4f, end = 0.8f;
        sample = (sample - start) / (end - start);
        return sample < 0.0f ? 0.0f : sample > 1.0f ? 1.0f : sample;
    }

    public float SampleNoise_ResourceAmount(SCellPos absoluteCell)
    {
        float sample = SampleNoise(absoluteCell, 0.01f, ENoiseLayer.AsteroidResourceAmount);
        float start = 0.75f, end = 0.9f;
        sample = (sample - start) / (end - start);
        return sample < 0.0f ? 0.0f : sample > 1.0f ? 1.0f : sample;
    }

    public float SampleNoise_EnemyShipDensity(SCellPos absoluteCell)
    {
        float sample = SampleNoise(absoluteCell, 0.001f, ENoiseLayer.EnemyShipCount);
        ////float start = 0.85f, end = 0.95f;
		//float start = 0.0f, end = 0.001f;
		float start = 1.1f, end = 1.2f;
        sample = (sample - start) / (end - start);
        return sample < 0.0f ? 0.0f : sample > 1.0f ? 1.0f : sample;
    }

    private void LoadSparseAsteroids(SCellPos absoluteCell)
    {
        float fCellRadius = cellRadius;

        uint uiNumAsteroids = SparseAsteroidCount(absoluteCell);
        for (uint ui = 0; ui < uiNumAsteroids; ++ui)
        {
            Profiler.BeginSample("Create asteroid meta and queue for creation");

            mGubbinsToLoad.Add(new SGubbinMeta((CGameRegistrator.ENetworkPrefab)Random.Range((ushort)CGameRegistrator.ENetworkPrefab.Asteroid_FIRST, (ushort)CGameRegistrator.ENetworkPrefab.Asteroid_LAST + 1),    // Random asteroid prefab.
                                                absoluteCell,   // Parent cell.
                                                Random.Range(10.0f, 150.0f),    // Scale.
                                                new Vector3(Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius)), // Position within parent cell.
                                                Random.rotationUniform, // Rotation.
                                                Vector3.zero/*Random.onUnitSphere * Random.Range(0.0f, 75.0f)*/,    // Linear velocity.
                                                Vector3.zero/*Random.onUnitSphere * Random.Range(0.0f, 2.0f)*/, // Angular velocity.
                                                0.5f,   // Mass to health scalar. Zero if there is no health script.
                                                true,   // Has NetworkedEntity script.
                                                true    // Has a rigid body.
                                                ));

            Profiler.EndSample();
        }
    }

<<<<<<< HEAD
    private void LoadAsteroidClusters(SCellPos absoluteCell)
    {
        float fCellRadius = cellRadius;

        uint uiNumAsteroidClusters = AsteroidClusterCount(absoluteCell);
        for (uint uiCluster = 0; uiCluster < uiNumAsteroidClusters; ++uiCluster)
        {
            Profiler.BeginSample("Create asteroid clusters");

            uint uiNumAsteroidsInCluster = (uint)Random.Range(6, 21);
            for (uint uiAsteroid = 0; uiAsteroid < uiNumAsteroidsInCluster; ++uiAsteroid)
            {
                Vector3 clusterCentre = new Vector3(Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius));

                mGubbinsToLoad.Add(new SGubbinMeta((CGameRegistrator.ENetworkPrefab)Random.Range((ushort)CGameRegistrator.ENetworkPrefab.Asteroid_FIRST, (ushort)CGameRegistrator.ENetworkPrefab.Asteroid_LAST + 1),    // Random asteroid prefab.
                                                    absoluteCell,   // Parent cell.
                                                    Random.Range(10.0f, 150.0f),    // Scale.
                                                    clusterCentre + Random.onUnitSphere * Random.Range(0.0f, fCellRadius * 0.25f), // Position within parent cell.
                                                    Random.rotationUniform, // Rotation.
                                                    Vector3.zero/*Random.onUnitSphere * Random.Range(0.0f, 75.0f)*/,    // Linear velocity.
                                                    Vector3.zero/*Random.onUnitSphere * Random.Range(0.0f, 2.0f)*/, // Angular velocity.
                                                    0.5f,   // Mass to health scalar. Zero if there is no health script.
                                                    true,   // Has NetworkedEntity script.
                                                    true    // Has a rigid body.
                                                    ));
            }

            Profiler.EndSample();
        }
    }

    private void LoadDebris(SCellPos absoluteCell)
    {

    }

    private void LoadEnemyShips(SCellPos absoluteCell)
    {
		float fCellRadius = cellRadius;
        uint uiNumEnemyShips = EnemyShipCount(absoluteCell);

        for (uint ui = 0; ui < uiNumEnemyShips; ++ui)
        {
            mGubbinsToLoad.Add(new SGubbinMeta(CGameRegistrator.ENetworkPrefab.EnemyShip,    // Enemy ship prefab.
                                                absoluteCell,   // Parent cell.
                                                1.0f,    // Scale.
                                                new Vector3(Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius)), // Position within parent cell.
                                                Random.rotationUniform, // Rotation.
                                                Vector3.zero/*Random.onUnitSphere * Random.Range(0.0f, 75.0f)*/,    // Linear velocity.
                                                Vector3.zero/*Random.onUnitSphere * Random.Range(0.0f, 2.0f)*/, // Angular velocity.
                                                0.0f,   // Mass to health scalar. Zero if there is no health script.
                                                true,   // Has NetworkedEntity script.
                                                true    // Has a rigid body.
                                                ));
        }
=======
        Profiler.EndSample();
    }

    public uint SparseAsteroidCount(SCellPos absoluteCell) { return (uint)Mathf.RoundToInt(4/*maxAsteroids*/ * SampleNoise_SparseAsteroid(absoluteCell)); }
    public uint AsteroidClusterCount(SCellPos absoluteCell) { return (uint)Mathf.RoundToInt(1/*maxClusters*/ * SampleNoise_AsteroidCluster(absoluteCell)); }
    public float DebrisDensity(SCellPos absoluteCell) { return SampleNoise_DebrisDensity(absoluteCell); }
    public float FogDensity(SCellPos absoluteCell) { return SampleNoise_FogDensity(absoluteCell); }
    public float ResourceAmount(SCellPos absoluteCell) { return 800 * SampleNoise_ResourceAmount(absoluteCell); }
    public uint EnemyShipCount(SCellPos absoluteCell) { return (uint)Mathf.RoundToInt(1/*maxEnemyShips*/ * SampleNoise_EnemyShipDensity(absoluteCell)); }

    public float SampleNoise_SparseAsteroid(SCellPos absoluteCell)
    {
        float sample = SampleNoise(absoluteCell, 0.01f, ENoiseLayer.SparseAsteroidCount);
        float start = 0.5f, end = 0.9f;
        sample = (sample - start) / (end - start);
        return sample < 0.0f ? 0.0f : sample > 1.0f ? 1.0f : sample;
    }
    
    public float SampleNoise_AsteroidCluster(SCellPos absoluteCell)
    {
        float sample = SampleNoise(absoluteCell, 0.1f, ENoiseLayer.AsteroidClusterCount);
        float start = 0.8f, end = 0.9f;
        sample = (sample - start) / (end - start);
        return sample < 0.0f ? 0.0f : sample > 1.0f ? 1.0f : sample;
    }

    public float SampleNoise_DebrisDensity(SCellPos absoluteCell)
    {
        float sample = SampleNoise(absoluteCell, 0.25f, ENoiseLayer.DebrisDensity);
        float start = 0.0f, end = 1.0f;
        sample = (sample - start) / (end - start);
        return sample < 0.0f ? 0.0f : sample > 1.0f ? 1.0f : sample;
    }

    public float SampleNoise_FogDensity(SCellPos absoluteCell)
    {
        float sample = SampleNoise(absoluteCell, 0.000001f, ENoiseLayer.FogDensity);
        float start = 0.4f, end = 0.8f;
        sample = (sample - start) / (end - start);
        return sample < 0.0f ? 0.0f : sample > 1.0f ? 1.0f : sample;
    }

    public float SampleNoise_ResourceAmount(SCellPos absoluteCell)
    {
        float sample = SampleNoise(absoluteCell, 0.01f, ENoiseLayer.AsteroidResourceAmount);
        float start = 0.75f, end = 0.9f;
        sample = (sample - start) / (end - start);
        return sample < 0.0f ? 0.0f : sample > 1.0f ? 1.0f : sample;
    }

    public float SampleNoise_EnemyShipDensity(SCellPos absoluteCell)
    {
        float sample = SampleNoise(absoluteCell, 0.001f, ENoiseLayer.EnemyShipCount);
        ////float start = 0.85f, end = 0.95f;
		//float start = 0.0f, end = 0.001f;
		float start = 1.1f, end = 1.2f;
        sample = (sample - start) / (end - start);
        return sample < 0.0f ? 0.0f : sample > 1.0f ? 1.0f : sample;
    }

    private void LoadSparseAsteroids(SCellPos absoluteCell)
    {
        float fCellRadius = cellRadius;

        uint uiNumAsteroids = SparseAsteroidCount(absoluteCell);
        for (uint ui = 0; ui < uiNumAsteroids; ++ui)
        {
            Profiler.BeginSample("Create asteroid meta and queue for creation");

            mGubbinsToLoad.Add(new SGubbinMeta((CGameRegistrator.ENetworkPrefab)Random.Range((ushort)CGameRegistrator.ENetworkPrefab.Asteroid_FIRST, (ushort)CGameRegistrator.ENetworkPrefab.Asteroid_LAST + 1),    // Random asteroid prefab.
                                                absoluteCell,   // Parent cell.
                                                /*Random.Range(10.0f, 150.0f)*/1.0f,    // Scale.
                                                new Vector3(Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius)), // Position within parent cell.
                                                Random.rotationUniform, // Rotation.
                                                Vector3.zero/*Random.onUnitSphere * Random.Range(0.0f, 75.0f)*/,    // Linear velocity.
                                                Vector3.zero/*Random.onUnitSphere * Random.Range(0.0f, 2.0f)*/, // Angular velocity.
                                                -1.0f,   // Mass to health scalar. Zero if there is no health script.
                                                true,   // Has NetworkedEntity script.
                                                false    // Has a rigid body.
                                                ));

            Profiler.EndSample();
        }
    }

    private void LoadAsteroidClusters(SCellPos absoluteCell)
    {
        float fCellRadius = cellRadius;

        uint uiNumAsteroidClusters = AsteroidClusterCount(absoluteCell);
        for (uint uiCluster = 0; uiCluster < uiNumAsteroidClusters; ++uiCluster)
        {
            Profiler.BeginSample("Create asteroid clusters");

            uint uiNumAsteroidsInCluster = (uint)Random.Range(6, 21);
            for (uint uiAsteroid = 0; uiAsteroid < uiNumAsteroidsInCluster; ++uiAsteroid)
            {
                Vector3 clusterCentre = new Vector3(Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius));

                mGubbinsToLoad.Add(new SGubbinMeta((CGameRegistrator.ENetworkPrefab)Random.Range((ushort)CGameRegistrator.ENetworkPrefab.Asteroid_FIRST, (ushort)CGameRegistrator.ENetworkPrefab.Asteroid_LAST + 1),    // Random asteroid prefab.
                                                    absoluteCell,   // Parent cell.
                                                    1.0f/*Random.Range(10.0f, 150.0f)*/,    // Scale.
                                                    clusterCentre + Random.onUnitSphere * Random.Range(0.0f, fCellRadius * 0.25f), // Position within parent cell.
                                                    Random.rotationUniform, // Rotation.
                                                    Vector3.zero/*Random.onUnitSphere * Random.Range(0.0f, 75.0f)*/,    // Linear velocity.
                                                    Vector3.zero/*Random.onUnitSphere * Random.Range(0.0f, 2.0f)*/, // Angular velocity.
                                                    0.5f,   // Mass to health scalar. Zero if there is no health script.
                                                    true,   // Has NetworkedEntity script.
                                                    true    // Has a rigid body.
                                                    ));
            }

            Profiler.EndSample();
        }
    }

    private void LoadDebris(SCellPos absoluteCell)
    {

    }

    private void LoadEnemyShips(SCellPos absoluteCell)
    {
		float fCellRadius = cellRadius;
        uint uiNumEnemyShips = EnemyShipCount(absoluteCell);

        for (uint ui = 0; ui < uiNumEnemyShips; ++ui)
        {
            mGubbinsToLoad.Add(new SGubbinMeta(CGameRegistrator.ENetworkPrefab.EnemyShip,    // Enemy ship prefab.
                                                absoluteCell,   // Parent cell.
                                                1.0f,    // Scale.
                                                new Vector3(Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius)), // Position within parent cell.
                                                Random.rotationUniform, // Rotation.
                                                Vector3.zero/*Random.onUnitSphere * Random.Range(0.0f, 75.0f)*/,    // Linear velocity.
                                                Vector3.zero/*Random.onUnitSphere * Random.Range(0.0f, 2.0f)*/, // Angular velocity.
                                                0.0f,   // Mass to health scalar. Zero if there is no health script.
                                                true,   // Has NetworkedEntity script.
                                                true    // Has a rigid body.
                                                ));
        }
>>>>>>> dddd13219fbce1556ae52b8f7d00cfdbf4a8d4d0
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

                float noiseValue = SampleNoise_SparseAsteroid(pair.Key);
                Gizmos.color = new Color(1.0f, 1.0f, 1.0f, noiseValue);
                Gizmos.DrawSphere(new Vector3(x, y, z), cellRadius * 0.5f);
            }
        }

        Profiler.EndSample();
    }

    public static float GetBoundingRadius(GameObject gameObject)
    {
        float result = 1.0f;

        // Depending on the type of model; it may use a collider, mesh renderer, animator, or something else.
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider)
            result = collider.bounds.extents.magnitude;
        else
        {
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer)
                result = meshRenderer.bounds.extents.magnitude;
            else
            {
                bool gotSomethingFromAnimator = false;
                Animator anim = gameObject.GetComponent<Animator>();
                if (anim)
                {
                    gotSomethingFromAnimator = anim.renderer || anim.collider || anim.rigidbody;
                    if (anim.renderer) result = anim.renderer.bounds.extents.magnitude;
                    else if (anim.collider) result = anim.collider.bounds.extents.magnitude;
                    else if (anim.rigidbody) result = anim.rigidbody.collider.bounds.extents.magnitude;
                }

                if (!gotSomethingFromAnimator)
                    Debug.LogWarning("Galaxy→GetBoundingRadius(): No RigidBody, Collider, MeshRenderer, or Animator on " + gameObject.name + ". Bounding radius set to 1");
            }
        }

        return result;
    }
}