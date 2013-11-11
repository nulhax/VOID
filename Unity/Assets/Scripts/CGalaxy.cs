using UnityEngine;
using System.Collections;

public class CGalaxy : MonoBehaviour
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

    enum ENoiseLayer
    {
        AsteroidDensity
    }

    ///////////////////////////////////////////////////////////////////////////
    // Variables:

    GameObject mGalaxyParent = null;
    SGridCellPos mCentreCell = new SGridCellPos(13, 0, 0);    // All cells are offset by this cell.
    System.Collections.Generic.List<CRegisteredObserver> mObservers = new System.Collections.Generic.List<CRegisteredObserver>(); // Cells in the grid are loaded and unloaded based on proximity to observers.
    System.Collections.Generic.List<CRegisteredGubbin> mGubbins = new System.Collections.Generic.List<CRegisteredGubbin>();    // Gubbins ("space things") are unloaded based on proximity to cells.
    System.Collections.Generic.Dictionary<SGridCellPos, CGridCellContent> mGrid = new System.Collections.Generic.Dictionary<SGridCellPos, CGridCellContent>();
    public const float mfGalaxySize = 1391000000.0f; // (1.3 million kilometres) In metres cubed. Floats can increment up to 16777220.0f (16.7 million).
    public const uint muiMaxAsteroidsPerCell = 1;
    public const float mfTimeBetweenGridUpdates = 0.2f;
    float mfTimeUntilNextProcess = 0.0f;
    public const uint muiGridSubsets = 20; // Zero is just the one cell.
    public const uint mNumExtraNeighbourCells = 3;   // Number of extra cells to load in every direction (i.e. load neighbours up to some distance).
    bool mbVisualDebug_Internal = false;    // Use mbVisualiseGrid.
    bool mbValidCellValue = false;  // Used for culling cells that are too far away from observers.

    public float mCellDiameter { get { return mfGalaxySize / mNumGridCellsInRow; } }
    public ulong mNumGridCells { get { /*return (uint)Mathf.Pow(8, muiGridSubsets);*/ ulong ul = 1; for (uint ui2 = 0; ui2 < muiGridSubsets; ++ui2)ul *= 8u; return ul; } }
    public uint mNumGridCellsInRow { get { /*return (uint)Mathf.Pow(2, muiGridSubsets);*/ uint ui = 1; for (uint ui2 = 0; ui2 < muiGridSubsets; ++ui2)ui *= 2; return ui; } }

    public bool mbVisualDebug
    {
        get { return mbVisualDebug_Internal; }

        set
        {
            if(mbVisualDebug_Internal != value)
            {
                mbVisualDebug_Internal = value;
                if(value)
                {
                    // Begin grid visualisation.
                    // http://answers.unity3d.com/questions/138917/how-to-draw-3d-text-from-code.html
                    // http://docs.unity3d.com/Documentation/ScriptReference/TextMesh.html
                }
                else    // !value
                {
                    // End grid visualisation.
                }
            }
            // Else the value is the same, so nothing needs to be done.
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    // Functions:

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

    // Returns a real from 0 to 1 inclusive.
    float SampleNoise(ENoiseLayer layer, SGridCellPos absoluteCell)
    {
        // X, Y, and Z coordinates in 'absoluteCell' specify the sample location of the noise.
        return Random.value;
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
            int noiseSample = Mathf.RoundToInt(SampleNoise(ENoiseLayer.AsteroidDensity, absoluteCell));
            for (int i = 0; i < muiMaxAsteroidsPerCell * noiseSample; ++i)
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

                    float uniformScale = Random.RandomRange(10.0f, 100.0f);
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

	// Use this for initialization
	void Start()
    {
        Debug.Log("Galaxy is " + mfGalaxySize.ToString("n0") + " units³ with " + muiGridSubsets.ToString("n0") + " grid subsets, thus the " + mNumGridCells.ToString("n0") + " cells are " + (mfGalaxySize / mNumGridCellsInRow).ToString("n0") + " units in diameter and " + mNumGridCellsInRow.ToString("n0") + " cells in a row.");

        //mGalaxyParent = GameObject.Instantiate(Resources.Load("Prefabs/GalaxyParent")) as GameObject;
        mGalaxyParent = CNetwork.Factory.CreateObject((ushort)CGame.ENetworkRegisteredPrefab.GalaxyParent);

        // Set debugging.
        mbVisualDebug = true;
	}

	// Update is called once per frame
	void Update()
    {
        // Limit processing.
        mfTimeUntilNextProcess -= Time.deltaTime;
        if (mfTimeUntilNextProcess <= 0.0f)
        {
            mfTimeUntilNextProcess = mfTimeBetweenGridUpdates;    // Drop the remainder as it doesn't matter if there is a lag spike.

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
            while(restart);
        }
	}

    void OnDrawGizmos()/*OnDrawGizmos & OnDrawGizmosSelected*/
    {
        //Debug.LogWarning("88888888888            " + mGrid.Count.ToString());
        if (mbVisualDebug_Internal)
        {
            foreach (CRegisteredObserver elem in mObservers)
                Gizmos.DrawWireSphere(elem.mObserver.transform.position, elem.mObservationRadius);
            GL.Color(Color.red);
            float fCellDiameter = mfGalaxySize / mNumGridCellsInRow;
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
}
