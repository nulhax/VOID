using UnityEngine;
using System.Collections;

public class CGalaxy : MonoBehaviour
{
    ///////////////////////////////////////////////////////////////////////////
    // Objects:

    ///////////////////////////////////////////////////////////////////////////
    // Variables:

    public const float mfGalaxySize = 2000.0f; // In metres cubed. Floats can increment up to 16777220.0f (16.7 million).
    public uint muiGridSubsets = 3; // Zero is just the one cell.
    private bool mbVisualiseGrid_Internal = false;    // Use mbVisualiseGrid.

    public uint mNumGridCells { get { return (uint)Mathf.Pow(8, muiGridSubsets); } }
    public uint mNumGridCellsInRow { get { return (uint)Mathf.Pow(2, muiGridSubsets); } }

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
        mbVisualiseGrid = true;
        Debug.Log("Galaxy is " + mfGalaxySize.ToString() + " units³ with " + muiGridSubsets.ToString() + " grid subsets, thus the " + mNumGridCells.ToString() + " cells are " + (mfGalaxySize / mNumGridCellsInRow).ToString() + " units in diameter.");
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
            float fGalaxyMin = -(mfGalaxySize * .5f);
            float fGalaxyMax = +(mfGalaxySize * .5f);
            float fCellDiameter = mfGalaxySize / mNumGridCellsInRow;
            float fCellRadius = fCellDiameter * .5f;
            for (float x = fGalaxyMin + fCellRadius; x <= fGalaxyMax; x += fCellDiameter)
            {
                for (float y = fGalaxyMin + fCellRadius; y <= fGalaxyMax; y += fCellDiameter)
                {
                    for (float z = fGalaxyMin + fCellRadius; z <= fGalaxyMax; z += fCellDiameter)
                    {
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

//            GL.PopMatrix();
        }
    }
}
