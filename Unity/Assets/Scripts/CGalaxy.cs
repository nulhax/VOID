using UnityEngine;
using System.Collections;

public class CGalaxy : MonoBehaviour
{
    ///////////////////////////////////////////////////////////////////////////
    // Objects:

    class CGridCellPos
    {
        public int x;
        public int y;
        public int z;

        public CGridCellPos() { }
        public CGridCellPos(int _x, int _y, int _z) { x = _x; y = _y; z = _z; }

        public static CGridCellPos operator +(CGridCellPos lhs, CGridCellPos rhs) { return new CGridCellPos(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z); }
        public static CGridCellPos operator -(CGridCellPos lhs, CGridCellPos rhs) { return new CGridCellPos(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z); }
        public static bool operator ==(CGridCellPos lhs, CGridCellPos rhs) { return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z; }
        public static bool operator !=(CGridCellPos lhs, CGridCellPos rhs) { return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z; }

        public override bool Equals(object obj){return base.Equals(obj as CGridCellPos);}
        public bool Equals(CGridCellPos rhs){if(rhs == null)return false;return x == rhs.x && y == rhs.y && z == rhs.z;}
        public override int GetHashCode()   {return (x^y^z);}
    }

    class CGridCellContent
    {
        // There is nothing.
    }

    ///////////////////////////////////////////////////////////////////////////
    // Variables:

    CGridCellPos mCentreCell;    // All cells are offset by this cell.
    System.Collections.Generic.Dictionary<int/*gameObject.GetInstanceID()*/, Vector3/*gameObject.GetComponent<Transform>().position*/> mObservers; // Cells in the grid are loaded and unloaded based on proximity to observers.
    System.Collections.Generic.Dictionary<CGridCellPos, CGridCellContent> mGrid;
    public const float mfGalaxySize = 1391000000.0f; // (1.3 million kilometres) In metres cubed. Floats can increment up to 16777220.0f (16.7 million).
    public uint muiGridSubsets = 20; // Zero is just the one cell.
    private bool mbVisualiseGrid_Internal = false;    // Use mbVisualiseGrid.

    public ulong mNumGridCells { get { /*return (uint)Mathf.Pow(8, muiGridSubsets);*/ ulong ul = 1; for (uint ui2 = 0; ui2 < muiGridSubsets; ++ui2)ul *= 8u; return ul; } }
    public uint mNumGridCellsInRow { get { /*return (uint)Mathf.Pow(2, muiGridSubsets);*/ uint ui = 1; for (uint ui2 = 0; ui2 < muiGridSubsets; ++ui2)ui *= 2; return ui; } }

    public bool mbVisualiseGrid
    {
        get {return mbVisualiseGrid_Internal;}

        set
        {
            if(mbVisualiseGrid_Internal != value)
            {
                mbVisualiseGrid_Internal = value;
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



	// Use this for initialization
	void Start()
    {
        
        Debug.Log("Galaxy is " + mfGalaxySize.ToString("n0") + " units³ with " + muiGridSubsets.ToString("n0") + " grid subsets, thus the " + mNumGridCells.ToString("n0") + " cells are " + (mfGalaxySize / mNumGridCellsInRow).ToString("n0") + " units in diameter and " + mNumGridCellsInRow.ToString("n0") + " cells in a row.");

        // Initialise variables.
        mCentreCell = new CGridCellPos(0, 0, 0);
        mObservers = new System.Collections.Generic.Dictionary<int, Vector3>();
        mGrid = new System.Collections.Generic.Dictionary<CGridCellPos, CGridCellContent>();

        // Set debugging.
        mbVisualiseGrid = true;

        // Add test cells.
        mGrid.Add(new CGridCellPos(+1, +1, +1), new CGridCellContent());
        mGrid.Add(new CGridCellPos(+0, +1, +1), new CGridCellContent());
        mGrid.Add(new CGridCellPos(-1, +1, +1), new CGridCellContent());
        mGrid.Add(new CGridCellPos(+1, +0, +1), new CGridCellContent());
        mGrid.Add(new CGridCellPos(+0, +0, +1), new CGridCellContent());
        mGrid.Add(new CGridCellPos(-1, +0, +1), new CGridCellContent());
        mGrid.Add(new CGridCellPos(+1, -1, +1), new CGridCellContent());
        mGrid.Add(new CGridCellPos(+0, -1, +1), new CGridCellContent());
        mGrid.Add(new CGridCellPos(-1, -1, +1), new CGridCellContent());
        mGrid.Add(new CGridCellPos(+1, +1, +0), new CGridCellContent());
        mGrid.Add(new CGridCellPos(+0, +1, +0), new CGridCellContent());
        mGrid.Add(new CGridCellPos(-1, +1, +0), new CGridCellContent());
        mGrid.Add(new CGridCellPos(+1, +0, +0), new CGridCellContent());
        mGrid.Add(new CGridCellPos(+0, +0, +0), new CGridCellContent());
        mGrid.Add(new CGridCellPos(-1, +0, +0), new CGridCellContent());
        mGrid.Add(new CGridCellPos(+1, -1, +0), new CGridCellContent());
        mGrid.Add(new CGridCellPos(+0, -1, +0), new CGridCellContent());
        mGrid.Add(new CGridCellPos(-1, -1, +0), new CGridCellContent());
        mGrid.Add(new CGridCellPos(+1, +1, -1), new CGridCellContent());
        mGrid.Add(new CGridCellPos(+0, +1, -1), new CGridCellContent());
        mGrid.Add(new CGridCellPos(-1, +1, -1), new CGridCellContent());
        mGrid.Add(new CGridCellPos(+1, +0, -1), new CGridCellContent());
        mGrid.Add(new CGridCellPos(+0, +0, -1), new CGridCellContent());
        mGrid.Add(new CGridCellPos(-1, +0, -1), new CGridCellContent());
        mGrid.Add(new CGridCellPos(+1, -1, -1), new CGridCellContent());
        mGrid.Add(new CGridCellPos(+0, -1, -1), new CGridCellContent());
        mGrid.Add(new CGridCellPos(-1, -1, -1), new CGridCellContent());
	}

	// Update is called once per frame
	void Update()
    {
        //  Get a list of all GameObjects in the game.
        //      Occasionally average the position of all observers.
        //          If the average position is outside a certain proximity to the origin of the world:
        //              Translate everything by the offset to the centre of the world (thus making the average position of observers (0,0,0)).
        //      For all observers:
        //          Ensure all grid cells within a radius are loaded.
	}

    void OnDrawGizmosSelected()/*OnDrawGizmos & OnDrawGizmosSelected*/
    {
        if (mbVisualiseGrid_Internal)
        {
            //GL.PushMatrix();
            //GL.LoadProjectionMatrix(Camera.main.projectionMatrix);    // This makes the lines always visible (overlap everything in the scene).
            GL.Color(Color.red);
            float fCellDiameter = mfGalaxySize / mNumGridCellsInRow;
            float fCellRadius = fCellDiameter * .5f;

            foreach (System.Collections.Generic.KeyValuePair<CGridCellPos, CGridCellContent> pair in mGrid)
            {
                CGridCellPos translatedPos = pair.Key - mCentreCell;

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

            //for (float x = fGalaxyMin + fCellRadius; x <= fGalaxyMax; x += fCellDiameter)
            //{
            //    for (float y = fGalaxyMin + fCellRadius; y <= fGalaxyMax; y += fCellDiameter)
            //    {
            //        for (float z = fGalaxyMin + fCellRadius; z <= fGalaxyMax; z += fCellDiameter)
            //        {
            //            GL.Begin(GL.LINES);
            //            GL.Vertex3(x - fCellRadius, y - fCellRadius, z - fCellRadius);
            //            GL.Vertex3(x + fCellRadius, y - fCellRadius, z - fCellRadius);
            //            GL.End();
            //            GL.Begin(GL.LINES);
            //            GL.Vertex3(x - fCellRadius, y + fCellRadius, z - fCellRadius);
            //            GL.Vertex3(x + fCellRadius, y + fCellRadius, z - fCellRadius);
            //            GL.End();
            //            GL.Begin(GL.LINES);
            //            GL.Vertex3(x - fCellRadius, y - fCellRadius, z + fCellRadius);
            //            GL.Vertex3(x + fCellRadius, y - fCellRadius, z + fCellRadius);
            //            GL.End();
            //            GL.Begin(GL.LINES);
            //            GL.Vertex3(x - fCellRadius, y + fCellRadius, z + fCellRadius);
            //            GL.Vertex3(x + fCellRadius, y + fCellRadius, z + fCellRadius);
            //            GL.End();
            //            GL.Begin(GL.LINES);
            //            GL.Vertex3(x - fCellRadius, y - fCellRadius, z - fCellRadius);
            //            GL.Vertex3(x - fCellRadius, y + fCellRadius, z - fCellRadius);
            //            GL.End();
            //            GL.Begin(GL.LINES);
            //            GL.Vertex3(x + fCellRadius, y - fCellRadius, z - fCellRadius);
            //            GL.Vertex3(x + fCellRadius, y + fCellRadius, z - fCellRadius);
            //            GL.End();
            //            GL.Begin(GL.LINES);
            //            GL.Vertex3(x - fCellRadius, y - fCellRadius, z + fCellRadius);
            //            GL.Vertex3(x - fCellRadius, y + fCellRadius, z + fCellRadius);
            //            GL.End();
            //            GL.Begin(GL.LINES);
            //            GL.Vertex3(x + fCellRadius, y - fCellRadius, z + fCellRadius);
            //            GL.Vertex3(x + fCellRadius, y + fCellRadius, z + fCellRadius);
            //            GL.End();
            //            GL.Begin(GL.LINES);
            //            GL.Vertex3(x - fCellRadius, y - fCellRadius, z - fCellRadius);
            //            GL.Vertex3(x - fCellRadius, y - fCellRadius, z + fCellRadius);
            //            GL.End();
            //            GL.Begin(GL.LINES);
            //            GL.Vertex3(x - fCellRadius, y + fCellRadius, z - fCellRadius);
            //            GL.Vertex3(x - fCellRadius, y + fCellRadius, z + fCellRadius);
            //            GL.End();
            //            GL.Begin(GL.LINES);
            //            GL.Vertex3(x + fCellRadius, y - fCellRadius, z - fCellRadius);
            //            GL.Vertex3(x + fCellRadius, y - fCellRadius, z + fCellRadius);
            //            GL.End();
            //            GL.Begin(GL.LINES);
            //            GL.Vertex3(x + fCellRadius, y + fCellRadius, z - fCellRadius);
            //            GL.Vertex3(x + fCellRadius, y + fCellRadius, z + fCellRadius);
            //            GL.End();
            //        }
            //    }
            //}

//            GL.PopMatrix();
        }
    }
}
