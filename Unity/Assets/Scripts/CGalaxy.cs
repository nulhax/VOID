using UnityEngine;
using System.Collections;

public class CGalaxy : CNetworkMonoBehaviour
{
    ///////////////////////////////////////////////////////////////////////////
    // Objects:

    public struct SGridCellPos
    {
        public int x;
        public int y;
        public int z;

        //public CGridCellPos() { }
        public SGridCellPos(int _x, int _y, int _z) { x = _x; y = _y; z = _z; }

        public static SGridCellPos operator +(SGridCellPos lhs, SGridCellPos rhs) { return new SGridCellPos(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z); }
        public static SGridCellPos operator -(SGridCellPos lhs, SGridCellPos rhs) { return new SGridCellPos(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z); }
        public static bool operator ==(SGridCellPos lhs, SGridCellPos rhs) { return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z; }
        public static bool operator !=(SGridCellPos lhs, SGridCellPos rhs) { return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z; }
    }

    class CGridCellContent
    {
        public CGridCellContent(bool initialValue) { mAlternator = initialValue; }
        public bool mAlternator;   // This is used for culling purposes.
    }

    class CRegisteredObserver
    {
        public GameObject mObserver;
        public float mObservationRadius;   // Bounding sphere.

        public CRegisteredObserver(GameObject observer, float observationRadius) { mObserver = observer; mObservationRadius = observationRadius; }
    }

    class CRegisteredGubbin
    {
        public GameObject mEntity;
        public ushort mNetworkViewID;
        public bool mAlternator;   // This is used for culling purposes.

        public CRegisteredGubbin(GameObject entity, ushort networkViewID, bool alternatorValue) { mEntity = entity; mNetworkViewID = networkViewID; mAlternator = alternatorValue; }
    }

    enum ENoiseLayer : uint
    {
        AsteroidDensity,
        FogDensity,
        MAX
    }

    enum ESkyboxLayer : uint
    {
        Composite,
        Solid,
        MAX
    }

    ///////////////////////////////////////////////////////////////////////////
    // Variables:

    GameObject mGalaxyParent;    // Only the server maintains a reference to the galaxy parent.

    PerlinSimplexNoise[] mNoises = new PerlinSimplexNoise[(uint)ENoiseLayer.MAX];

    string[] mSkyboxFaces = new string[6];
    Texture[][] mSkyboxTextures = new Texture[(uint)ESkyboxLayer.MAX][/*6*/];

    SGridCellPos mCentreCell = new SGridCellPos(0, 0, 0);    // All cells are offset by this cell.
    protected CNetworkVar<int> mCentreCellX;
    protected CNetworkVar<int> mCentreCellY;
    protected CNetworkVar<int> mCentreCellZ;

    System.Collections.Generic.List<GalaxyIE> mGalaxyIEs;   // Is only instantiated when this galaxy is ready to update galaxyIEs
    System.Collections.Generic.List<CRegisteredObserver> mObservers = new System.Collections.Generic.List<CRegisteredObserver>(); // Cells in the grid are loaded and unloaded based on proximity to observers.
    System.Collections.Generic.List<CRegisteredGubbin> mGubbins = new System.Collections.Generic.List<CRegisteredGubbin>();    // Gubbins ("space things") are unloaded based on proximity to cells.
    System.Collections.Generic.Dictionary<SGridCellPos, CGridCellContent> mGrid = new System.Collections.Generic.Dictionary<SGridCellPos, CGridCellContent>();

    protected CNetworkVar<float> mGalaxySize; // (1.3 million kilometres) In metres cubed. Floats can increment up to 16777220.0f (16.7 million).
    float mfGalaxySize = 1391000000.0f;

    protected CNetworkVar<uint> mNumGridSubsets; // Zero is just the one cell.
    uint muiNumGridSubsets = 20;

    protected CNetworkVar<uint> mMaxAsteroidsPerCell;
    uint muiMaxAsteroidsPerCell = 1;

    public const float mfTimeBetweenGridUpdates = 0.2f;
    float mfTimeUntilNextGridUpdate = 0.0f;

    public const uint mNumExtraNeighbourCells = 3;   // Number of extra cells to load in every direction (i.e. load neighbours up to some distance).

    bool mbValidCellValue = false;  // Used for culling cells that are too far away from observers.

    public float mCellDiameter { get { return mfGalaxySize / mNumGridCellsInRow; } }
    public ulong mNumGridCells { get { /*return (uint)Mathf.Pow(8, muiGridSubsets);*/ ulong ul = 1; for (uint ui2 = 0; ui2 < muiNumGridSubsets; ++ui2)ul *= 8u; return ul; } }
    public uint mNumGridCellsInRow { get { /*return (uint)Mathf.Pow(2, muiGridSubsets);*/ uint ui = 1; for (uint ui2 = 0; ui2 < muiNumGridSubsets; ++ui2)ui *= 2; return ui; } }

    ///////////////////////////////////////////////////////////////////////////
    // Functions:

    // Use this for initialization
    void Start()
    {
        // Fog and skybox are controlled by the galaxy.
        RenderSettings.fog = false;
        RenderSettings.skybox = null;

        for(uint ui = 0; ui < (uint)ENoiseLayer.MAX; ++ui)
            mNoises[ui] = new PerlinSimplexNoise();

        // Load skybox textures.
        mSkyboxFaces[0] = "Front";
        mSkyboxFaces[1] = "Back";
        mSkyboxFaces[2] = "Left";
        mSkyboxFaces[3] = "Right";
        mSkyboxFaces[4] = "Up";
        mSkyboxFaces[5] = "Down";

        for (uint uiSkybox = 0; uiSkybox < (uint)ESkyboxLayer.MAX; ++uiSkybox)    // For each skybox...
        {
            mSkyboxTextures[uiSkybox] = new Texture[6]; // Instantiate 6 textures.

            for (uint uiFace = 0; uiFace < 6; ++uiFace)  // For each texture in the skybox...
            {
                mSkyboxTextures[uiSkybox][uiFace] = Resources.Load("Textures/SpaceSkyBox/" + uiSkybox.ToString() + mSkyboxFaces[uiFace]) as Texture;  // Load the texture from file.
                mSkyboxTextures[uiSkybox][uiFace].wrapMode = TextureWrapMode.Clamp;   // Eliminates z-fighting along edges of skybox.
            }
        }

        // Galaxy is ready to update galaxyIEs.
        mGalaxyIEs = new System.Collections.Generic.List<GalaxyIE>();

        if (CNetwork.IsServer)
        {
            // Statistical data sometimes helps spot errors.
            Debug.Log("Galaxy is " + mfGalaxySize.ToString("n0") + " units³ with " + muiNumGridSubsets.ToString("n0") + " grid subsets, thus the " + mNumGridCells.ToString("n0") + " cells are " + (mfGalaxySize / mNumGridCellsInRow).ToString("n0") + " units in diameter and " + mNumGridCellsInRow.ToString("n0") + " cells in a row.");

            mGalaxyParent = CNetwork.Factory.CreateObject((ushort)CGame.ENetworkRegisteredPrefab.GalaxyParent);
        }
    }

    public override void InstanceNetworkVars()
    {
        mCentreCellX = new CNetworkVar<int>(SyncCentreCellX, mCentreCell.x);
        mCentreCellY = new CNetworkVar<int>(SyncCentreCellY, mCentreCell.y);
        mCentreCellZ = new CNetworkVar<int>(SyncCentreCellZ, mCentreCell.z);
        mGalaxySize = new CNetworkVar<float>(SyncGalaxySize, mfGalaxySize);
        mMaxAsteroidsPerCell = new CNetworkVar<uint>(SyncMaxAsteroidsPerCell, muiMaxAsteroidsPerCell);
        mNumGridSubsets = new CNetworkVar<uint>(SyncNumGridSubsets, muiNumGridSubsets);
    }

    // Update is called once per frame
    void Update()
    {
        if (CNetwork.IsServer)
        {
            // Limit processing.
            mfTimeUntilNextGridUpdate -= Time.deltaTime;
            if (mfTimeUntilNextGridUpdate <= 0.0f)
            {
                mfTimeUntilNextGridUpdate = mfTimeBetweenGridUpdates;    // Drop the remainder as it doesn't matter if there is a lag spike.

                mbValidCellValue = !mbValidCellValue;   // Alternate the valid cell value. All cells within proximity of an observer will be updated, while all others will retain the old value making it easier to detect and cull them.

                // Load unloaded grid cells within proximity to observers.
                foreach (CRegisteredObserver observer in mObservers)
                {
                    Vector3 observerPosition = observer.mObserver.transform.position;
                    SGridCellPos occupiedRelativeCell = PointToRelativeCell(observerPosition);
                    int iCellsInARow = 1 /*Centre cell*/ + (int)mNumExtraNeighbourCells * 2 /*Neighbouring cell rows*/ + (Mathf.CeilToInt((observer.mObservationRadius / (mCellDiameter * .5f)) - 1) * 2);

                    for (int x = -((iCellsInARow - 1) / 2); x <= (iCellsInARow - 1) / 2; ++x)
                    {
                        for (int y = -((iCellsInARow - 1) / 2); y <= (iCellsInARow - 1) / 2; ++y)
                        {
                            for (int z = -((iCellsInARow - 1) / 2); z <= (iCellsInARow - 1) / 2; ++z)
                            {
                                // Check if this cell is loaded.
                                SGridCellPos neighbouringRelativeCell = new SGridCellPos(occupiedRelativeCell.x + x, occupiedRelativeCell.y + y, occupiedRelativeCell.z + z);
                                if (RelativeCellWithinProximityOfPoint(neighbouringRelativeCell, observerPosition, observer.mObservationRadius + mCellDiameter * mNumExtraNeighbourCells))
                                {
                                    SGridCellPos neighbouringAbsoluteCell = neighbouringRelativeCell + mCentreCell;
                                    CGridCellContent temp;
                                    if (mGrid.TryGetValue(neighbouringAbsoluteCell, out temp))   // Existing cell...
                                        temp.mAlternator = mbValidCellValue;    // Update alternator to indicate the cell is within proximity of an observer.
                                    else    // Not an existing cell...
                                        LoadAbsoluteCell(neighbouringAbsoluteCell);
                                }
                            }
                        }
                    }
                }

                // Unload cells that are too far from any observers.
                bool restart;
                do
                {
                    restart = false;
                    foreach (System.Collections.Generic.KeyValuePair<SGridCellPos, CGridCellContent> absoluteCell in mGrid) // For every loaded cell...
                    {
                        if (absoluteCell.Value.mAlternator != mbValidCellValue)  // If the cell was not updated to the current alternator value...
                        {
                            // This cell is not within proximity of any observers.
                            UnloadAbsoluteCell(absoluteCell.Key); // Unload the cell.
                            restart = true;
                            break;
                        }
                    }
                } while (restart);

                // Find gubbins that are not within proximity to the cells.
                foreach (CRegisteredGubbin gubbin in mGubbins)
                {
                    foreach (System.Collections.Generic.KeyValuePair<SGridCellPos, CGridCellContent> pair in mGrid)
                    {
                        if (RelativeCellWithinProximityOfPoint(pair.Key - mCentreCell, gubbin.mEntity.transform.position, 0.0f))
                        {
                            gubbin.mAlternator = mbValidCellValue;
                            break;
                        }
                    }
                }

                // Unload gubbins that are not within proximity to the cells.
                do
                {
                    restart = false;
                    foreach (CRegisteredGubbin gubbin in mGubbins)
                    {
                        if (gubbin.mAlternator != mbValidCellValue)
                        {
                            UnloadGubbin(gubbin);
                            restart = true;
                            break;
                        }
                    }
                }
                while (restart);
            }
        }
    }

    public void SyncCentreCellX(INetworkVar sender) { mCentreCell.x = mCentreCellX.Get(); }
    public void SyncCentreCellY(INetworkVar sender) { mCentreCell.y = mCentreCellY.Get(); }
    public void SyncCentreCellZ(INetworkVar sender) { mCentreCell.z = mCentreCellZ.Get(); }
    public void SyncGalaxySize(INetworkVar sender) { mfGalaxySize = mGalaxySize.Get(); }
    public void SyncMaxAsteroidsPerCell(INetworkVar sender) { muiMaxAsteroidsPerCell = mMaxAsteroidsPerCell.Get(); }
    public void SyncNumGridSubsets(INetworkVar sender) { muiNumGridSubsets = mNumGridSubsets.Get(); }

    public void RegisterObserver(GameObject observer, float observationRadius)
    {
        mObservers.Add(new CRegisteredObserver(observer, observationRadius));
    }

    public void DeregisterObserver(GameObject observer)
    {
        foreach (CRegisteredObserver elem in mObservers)
        {
            if (elem.mObserver.GetInstanceID() == observer.GetInstanceID())
            {
                mObservers.Remove(elem);
                break;
            }
        }
    }

    // Returns false if the galaxy is not ready to update galaxyIEs.
    public bool RegisterGalaxyIE(GalaxyIE galaxyIE)
    {
        if (mGalaxyIEs != null) // Whether the galaxy is ready to update galaxyIEs or not is determined by whether the list is instantiated or not.
        {
            mGalaxyIEs.Add(galaxyIE);

            // Provide initial assets.
            UpdateGalaxyIE(mCentreCell, galaxyIE);

            return true;
        }
        else
            return false;
    }

    public void DeregisterGalaxyIE(GalaxyIE galaxyIE) { mGalaxyIEs.Remove(galaxyIE); }

    public Vector3 RelativeCellCentrePoint(SGridCellPos relativeCell)
    {
        return new Vector3(relativeCell.x * mCellDiameter, relativeCell.y * mCellDiameter, relativeCell.z * mCellDiameter);
    }

    public SGridCellPos PointToAbsoluteCell(Vector3 point)
    {
        float cellRadius = mCellDiameter * 0.5f;
        point.x += cellRadius;
        point.y += cellRadius;
        point.z += cellRadius;
        point /= mCellDiameter;
        return new SGridCellPos(Mathf.FloorToInt(point.x) + mCentreCell.x, Mathf.FloorToInt(point.y) + mCentreCell.y, Mathf.FloorToInt(point.z) + mCentreCell.z);
    }

    public SGridCellPos PointToRelativeCell(Vector3 point)
    {
        float cellRadius = mCellDiameter * 0.5f;
        point.x += cellRadius;
        point.y += cellRadius;
        point.z += cellRadius;
        point /= mCellDiameter;
        return new SGridCellPos(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), Mathf.FloorToInt(point.z));
    }

    public bool RelativeCellWithinProximityOfPoint(SGridCellPos relativeCell, Vector3 point, float pointRadius)
    {
        Vector3 cellCentrePos = new Vector3(relativeCell.x * mCellDiameter, relativeCell.y * mCellDiameter, relativeCell.z * mCellDiameter);
        float cellBoundingSphereRadius = mCellDiameter * 0.70710678118654752440084436210485f;
        return (cellCentrePos - point).sqrMagnitude <= cellBoundingSphereRadius * cellBoundingSphereRadius + pointRadius * pointRadius;
    }

    void ShiftGalaxy(SGridCellPos shiftAmount)
    {
        // TODO: Tell network to broadcast message to shift everything.

        mCentreCell += shiftAmount;
    }

    // Set the aesthetic of the galaxy based on the observer's position.
    void UpdateGalaxyIE(SGridCellPos absoluteCell, GalaxyIE galaxyIE)
    {
        for (uint uiFace = 0; uiFace < 6; ++uiFace)  // Apply each texture of a skybox to the material...
            galaxyIE.mGalaxyMaterial.SetTexture("_" + mSkyboxFaces[uiFace] + "Tex1", mSkyboxTextures[(uint)ESkyboxLayer.Composite][uiFace]);

        galaxyIE.mGalaxyMaterial.SetVector("_Tint", Color.grey);
        galaxyIE.mGalaxyMaterial.SetVector("_FogColour", new Color(Random.Range(0.0f, 0.05f), Random.Range(0.0f, 0.05f), Random.Range(0.0f, 0.05f), 1.0f));
        galaxyIE.mGalaxyMaterial.SetFloat("_FogDensity", Random.Range(0.0002f, 0.0002f));
        galaxyIE.mFogStartDistance = Random.Range(10.0f, 100.0f);
        galaxyIE.mFogEndDistance = Random.Range(4000.0f, 5000.0f);  // 5000 is camera's far plane.
    }

    void LoadAbsoluteCell(SGridCellPos absoluteCell)
    {
        mGrid.Add(absoluteCell, new CGridCellContent(mbValidCellValue)); // Create cell with updated alternator to indicate cell is within proximity of observer.
        SGridCellPos relativeCell = absoluteCell - mCentreCell;

        // Load the content for the cell.
        if (false)   // TODO: If the content for the cell is on file...
        {
            // TODO: Load content from SQL.
        }
        else    // This cell is not on file, so it has not been visited...
        {
            // Generate the content in the cell.
            float fCellRadius = mCellDiameter*0.5f;

            // 1) For asteroids.
            uint uiNumAsteroids = (uint)Mathf.RoundToInt(muiMaxAsteroidsPerCell * (0.5f + 0.5f * mNoises[(uint)ENoiseLayer.AsteroidDensity].Generate(absoluteCell.x, absoluteCell.y, absoluteCell.z))) /*Mathf.RoundToInt(PerlinSimplexNoise.(ENoiseLayer.AsteroidDensity, absoluteCell))*/;
            for (uint ui = 0; ui < uiNumAsteroids; ++ui)
            {
                ushort firstAsteroid = (ushort)CGame.ENetworkRegisteredPrefab.Asteroid_FIRST;
                ushort lastAstertoid = (ushort)CGame.ENetworkRegisteredPrefab.Asteroid_LAST;
                ushort randomRange = (ushort)Random.Range(0, (lastAstertoid+1) - firstAsteroid);
                GameObject newAsteroid = CNetwork.Factory.CreateObject((ushort)(firstAsteroid + randomRange));
                newAsteroid.transform.parent = mGalaxyParent.transform;

                // Work out a position where the asteroid fits.
                int iTries = 5;    // To prevent infinite loops.
                do
                {
                    newAsteroid.transform.position = RelativeCellCentrePoint(relativeCell) + new Vector3(Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius));
                    newAsteroid.transform.rotation = Random.rotationUniform;
                    
                    newAsteroid.rigidbody.angularVelocity = Random.onUnitSphere * Random.Range(0.0f, 2.0f);
                    newAsteroid.rigidbody.velocity = Random.onUnitSphere * Random.Range(0.0f, 50.0f);
                    newAsteroid.GetComponent<NetworkedEntity>().UpdateNetworkVars();

                    float uniformScale = Random.Range(10.0f, 100.0f);
                    newAsteroid.transform.localScale = new Vector3(uniformScale, uniformScale, uniformScale);

                } while (--iTries != 0 && false /*newAsteroid.collider.*/);

                CNetworkView newAsteroidNetworkView = newAsteroid.GetComponent<CNetworkView>();
                //newAsteroidNetworkView.SyncTransformPosition();
                //newAsteroidNetworkView.SyncTransformRotation();
                newAsteroidNetworkView.SyncTransformScale();

                mGubbins.Add(new CRegisteredGubbin(newAsteroid, newAsteroidNetworkView.ViewId, mbValidCellValue));  // Add new asteroid to list of gubbins ("space things").
            }
        }
    }

    void UnloadAbsoluteCell(SGridCellPos absoluteCell)
    {
        // Todo: Save stuff to file.
        mGrid.Remove(absoluteCell); // Unload the cell.
    }

    void UnloadGubbin(CRegisteredGubbin gubbin)
    {
        // Todo: Save gubbin to file.
        mGubbins.Remove(gubbin);
        CNetwork.Factory.DestoryObject(gubbin.mNetworkViewID);
    }

    void OnDrawGizmos()/*OnDrawGizmos & OnDrawGizmosSelected*/
    {
        //Debug.LogWarning("88888888888            " + mGrid.Count.ToString());
        foreach (CRegisteredObserver elem in mObservers)
            Gizmos.DrawWireSphere(elem.mObserver.transform.position, elem.mObservationRadius);
        GL.Color(Color.red);
        float fCellDiameter = mCellDiameter;
        float fCellRadius = fCellDiameter * .5f;

        foreach (System.Collections.Generic.KeyValuePair<SGridCellPos, CGridCellContent> pair in mGrid)
        {
            SGridCellPos relativeCell = pair.Key - mCentreCell;

            float x = relativeCell.x * fCellDiameter;
            float y = relativeCell.y * fCellDiameter;
            float z = relativeCell.z * fCellDiameter;

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
}