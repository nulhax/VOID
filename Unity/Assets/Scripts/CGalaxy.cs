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

    enum ENoiseLayer
    {
        AsteroidDensity
    }

    ///////////////////////////////////////////////////////////////////////////
    // Variables:

    SGridCellPos mCentreCell = new SGridCellPos(0, 0, 0);    // All cells are offset by this cell.
    System.Collections.Generic.List<CRegisteredObserver> mObservers = new System.Collections.Generic.List<CRegisteredObserver>(); // Cells in the grid are loaded and unloaded based on proximity to observers.
    System.Collections.Generic.Dictionary<SGridCellPos, CGridCellContent> mGrid = new System.Collections.Generic.Dictionary<SGridCellPos, CGridCellContent>();
    const float mfGalaxySize = 1391000000.0f; // (1.3 million kilometres) In metres cubed. Floats can increment up to 16777220.0f (16.7 million).
    const uint muiMaxAsteroidsPerCell = 1;
    const float mfTimeBetweenProcesses = 1.0f;
    float mfTimeUntilNextProcess = 0.0f;
    uint muiGridSubsets = 20; // Zero is just the one cell.
    uint mNumExtraNeighbourCells = 3;   // Number of extra cells to load in every direction (i.e. load neighbours up to some distance).
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

    public Vector3 TransformedCellCentrePoint(SGridCellPos transformedCell)
    {
        return new Vector3(transformedCell.x * mCellDiameter, transformedCell.y * mCellDiameter, transformedCell.z * mCellDiameter);
    }

    public SGridCellPos PointToCell(Vector3 point)
    {
        float cellRadius = mCellDiameter * 0.5f;
        point.x += cellRadius;
        point.y += cellRadius;
        point.z += cellRadius;
        point /= mCellDiameter;
        return new SGridCellPos(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), Mathf.FloorToInt(point.z));
    }

    public SGridCellPos PointToTransformedCell(Vector3 point)
    {
        float cellRadius = mCellDiameter * 0.5f;
        point.x += cellRadius;
        point.y += cellRadius;
        point.z += cellRadius;
        point /= mCellDiameter;
        return new SGridCellPos(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), Mathf.FloorToInt(point.z)) - mCentreCell;
    }

    public bool TransformedCellWithinProximityOfPoint(SGridCellPos transformedCell, Vector3 point, float pointRadius)
    {
        Vector3 cellCentrePos = new Vector3(transformedCell.x * mCellDiameter, transformedCell.y * mCellDiameter, transformedCell.z * mCellDiameter);
        float cellBoundingSphereRadius = mCellDiameter * 0.70710678118654752440084436210485f;
        return (cellCentrePos - point).sqrMagnitude <= cellBoundingSphereRadius * cellBoundingSphereRadius + pointRadius * pointRadius;
    }



    // Returns a real from 0 to 1 inclusive.
    float SampleNoise(ENoiseLayer layer, SGridCellPos cell)
    {
        // X, Y, and Z coordinates in 'cell' specify the sample location of the noise.
        return Random.value;
    }

    void LoadCell(SGridCellPos cell)
    {
        mGrid.Add(cell, new CGridCellContent(mbValidCellValue)); // Create cell with updated alternator to indicate cell is within proximity of observer.

        // Load the content for the cell.
        if (false)   // TODO: If the content for the cell is on file...
        {
            // TODO: Load content from SQL.
        }
        else    // This celll is not on file, so it has not been visited...
        {
            // Generate the content in the cell.
            float fCellRadius = mCellDiameter*0.5f;

            // 1) For asteroids.
            int noiseSample = Mathf.RoundToInt(SampleNoise(ENoiseLayer.AsteroidDensity, cell));
            for (int i = 0; i < muiMaxAsteroidsPerCell * noiseSample; ++i)
            {
                ushort firstAsteroid = (ushort)CGame.ENetworkRegisteredPrefab.Asteroid_FIRST;
                ushort lastAstertoid = (ushort)CGame.ENetworkRegisteredPrefab.Asteroid_LAST;
                ushort randomRange = (ushort)Random.Range(0, (lastAstertoid+1) - firstAsteroid);
                GameObject newAsteroid = CNetwork.Factory.CreateObject((ushort)(firstAsteroid + randomRange));

                // Work out a position where the asteroid fits.
                int iTries = 5;    // To prevent infinite loops.
                do
                {
                    newAsteroid.transform.position = TransformedCellCentrePoint(cell) + new Vector3(Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius));
                    newAsteroid.transform.rotation = Random.rotationUniform;
                    
                    newAsteroid.rigidbody.angularVelocity = Random.onUnitSphere * Random.Range(0.0f, 2.0f);
                    newAsteroid.rigidbody.velocity = Random.onUnitSphere * Random.Range(0.0f, 50.0f);

                    float uniformScale = Random.RandomRange(10.0f, 100.0f);
                    newAsteroid.transform.localScale = new Vector3(uniformScale, uniformScale, uniformScale);

                    newAsteroid.GetComponent<CNetworkView>().SyncTransformScale();

                } while (--iTries != 0 && false /*newAsteroid.collider.*/);
            }
        }
    }

    void UnloadCell(SGridCellPos cell)
    {
        mGrid.Remove(cell); // Unload the cell.
    }

	// Use this for initialization
	void Start()
    {
        Debug.Log("Galaxy is " + mfGalaxySize.ToString("n0") + " units³ with " + muiGridSubsets.ToString("n0") + " grid subsets, thus the " + mNumGridCells.ToString("n0") + " cells are " + (mfGalaxySize / mNumGridCellsInRow).ToString("n0") + " units in diameter and " + mNumGridCellsInRow.ToString("n0") + " cells in a row.");

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
            mfTimeUntilNextProcess = mfTimeBetweenProcesses;    // Drop the remainder as it doesn't matter if there is a lag spike.

            mbValidCellValue = !mbValidCellValue;   // Alternate the valid cell value. All cells within proximity of an observer will be updated, while all others will retain the old value making it easier to detect and cull them.

            // Load unloaded grid cells within proximity to observers.
            foreach (CRegisteredObserver observer in mObservers)
            {
                Vector3 observerPosition = observer.mObserver.transform.position;
                SGridCellPos occupiedCell = PointToTransformedCell(observerPosition);
                int iCellsInARow = 1 /*Centre cell*/ + (int)mNumExtraNeighbourCells * 2 /*Neighbouring cell rows*/ + (Mathf.CeilToInt((observer.mObservationRadius / (mCellDiameter * .5f)) - 1) * 2);

                for (int x = -((iCellsInARow - 1) / 2); x <= (iCellsInARow - 1) / 2; ++x)
                {
                    for (int y = -((iCellsInARow - 1) / 2); y <= (iCellsInARow - 1) / 2; ++y)
                    {
                        for (int z = -((iCellsInARow - 1) / 2); z <= (iCellsInARow - 1) / 2; ++z)
                        {
                            // Check if this cell is loaded.
                            SGridCellPos cellPos = new SGridCellPos(occupiedCell.x + x - mCentreCell.x, occupiedCell.y + y - mCentreCell.y, occupiedCell.z + z - mCentreCell.z);
                            if (TransformedCellWithinProximityOfPoint(cellPos, observerPosition, observer.mObservationRadius + mCellDiameter * mNumExtraNeighbourCells))
                            {
                                CGridCellContent temp;
                                if (mGrid.TryGetValue(cellPos, out temp))   // Existing cell...
                                    temp.mAlternator = mbValidCellValue;    // Update alternator to indicate the cell is within proximity of an observer.
                                else    // Not an existing cell...
                                    LoadCell(cellPos);
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
                foreach (System.Collections.Generic.KeyValuePair<SGridCellPos, CGridCellContent> cell in mGrid) // For every loaded cell...
                {
                    if (cell.Value.mAlternator != mbValidCellValue)  // If the cell was not updated to the current alternator value...
                    {
                        // This cell is not within proximity of any observers.
                        UnloadCell(cell.Key); // Unload the cell.
                        restart = true;
                        break;
                    }
                }
            } while (restart);
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
                SGridCellPos translatedPos = pair.Key - mCentreCell;

                float x = translatedPos.x * fCellDiameter;
                float y = translatedPos.y * fCellDiameter;
                float z = translatedPos.z * fCellDiameter;

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
