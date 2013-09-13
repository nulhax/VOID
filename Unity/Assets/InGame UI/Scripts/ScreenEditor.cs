using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ScreenEditor : MonoBehaviour 
{
    public enum Quality
    { 
        Excelent,
        Good,
        Average,
        Bad,
    }

    // Member Variables
    public float m_Width;
    public float m_Height;
    public Quality m_Quality;

    private Mesh m_ScreenMesh;
    private Camera m_ScreenRenderCamera;

    // Member Methods
    void OnEnable()
    {
        if (GetComponent<MeshFilter>() == null)
            return;
        
        m_ScreenMesh = GetComponent<MeshFilter>().sharedMesh;
        m_ScreenRenderCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (m_ScreenMesh.bounds.extents.x != m_Width || m_ScreenMesh.bounds.extents.y != m_Height)
        {
            // Update the screen meshes verts
            Vector3[] verts = m_ScreenMesh.vertices;
            verts[0] = new Vector3(-m_Width * 0.5f, -m_Height * 0.5f);
            verts[1] = new Vector3(m_Width * 0.5f, verts[0].y);
            verts[2] = new Vector3(verts[0].x, m_Height * 0.5f);
            verts[3] = new Vector3(verts[1].x, verts[2].y);
            verts[4] = verts[0];
            verts[5] = verts[1];
            verts[6] = verts[2];
            verts[7] = verts[3];

            m_ScreenMesh.vertices = verts;
            m_ScreenMesh.RecalculateBounds();

            // Reset the camera ortho size
            float smallerDimension = (m_Width > m_Height) ? m_Height : m_Width;
            m_ScreenRenderCamera.orthographicSize = smallerDimension * 0.5f;
        }
    }
}
