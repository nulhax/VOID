using UnityEngine;
using UnityEditor;
using System.Collections;

public class ScreenWizard : ScriptableWizard
{
    // Member Variables
    public string m_MonitorName = "Default";
    public GameObject m_Monitor = null;
    public float m_Width = 0.5f;
    public float m_Height = 0.5f;

    private GameObject m_MonitorInstance = null;

    // Member Methods
    [MenuItem("In-Game UI/Monitor...")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard("In Game UI - Monitor", typeof(ScreenWizard));
    }

    void OnWizardCreate()
    {
        m_MonitorInstance = new GameObject();
        m_MonitorInstance.name = m_MonitorName;   
            
        GameObject MonitorMesh = (GameObject)Instantiate(m_Monitor);
        MonitorMesh.transform.parent = m_MonitorInstance.transform;
        MonitorMesh.transform.localPosition = Vector3.zero;
        MonitorMesh.name = m_MonitorName + "_Model";

        // Create the screen
        CreateScreen();

        // Destroy the created instance of the monitor
        DestroyImmediate(m_MonitorInstance);
    }

    void OnWizardUpdate()
    {
        if (m_Monitor != null && m_MonitorName == "Default")
        {
            m_MonitorName = m_Monitor.name;
        }

        helpString = "Monitor Wizard";
    }

    void CreateScreen()
    {
        // Create the screen
        GameObject screen = new GameObject(m_MonitorInstance.name + "_Screen");

        // Set to the center
        screen.transform.parent = m_MonitorInstance.transform;
        screen.transform.localPosition = Vector3.zero;
        screen.transform.localRotation = Quaternion.identity;

        // Create the mesh that will be the screen
        Mesh mesh = new Mesh();
        mesh.name = screen.name + "_mesh";
        CreateScreenMesh(ref mesh);

        // Add the mesh filter and renderer to the screen object and create the material
        MeshFilter meshFilter = screen.AddComponent<MeshFilter>();
        MeshRenderer meshRender = screen.AddComponent<MeshRenderer>();
        Material material = new Material(Shader.Find("Diffuse"));
        material.name = screen.name + "_mat";

        // Add the script for InGameUI and editor for the monitor
        ScreenEditor sE = screen.gameObject.AddComponent<ScreenEditor>();
        sE.m_ScreenWidth = m_Width;
        sE.m_ScreenHeight = m_Height;
        screen.gameObject.AddComponent<ScreenUI>();

        // Create the screen camera object
        CreateScreenCamera(screen.gameObject);

        // Save the generated mesh into the prefab
        GameObject monitorPrefab = PrefabUtility.CreatePrefab("Assets/InGame UI/Monitors/" + m_MonitorInstance.name + ".prefab", m_MonitorInstance);
        AssetDatabase.AddObjectToAsset(mesh, monitorPrefab);
        AssetDatabase.AddObjectToAsset(material, monitorPrefab);
        AssetDatabase.SaveAssets();

        // Set the mesh filter and the material to use
        meshFilter.mesh = mesh;
        meshRender.material = material;

        // Replace the prefab
        PrefabUtility.ReplacePrefab(m_MonitorInstance, monitorPrefab, ReplacePrefabOptions.Default);
    }

    void CreateScreenMesh(ref Mesh _Mesh)
    {
        int hCount2 = 2;
        int vCount2 = 2;
        int numTriangles = 6;
        int numVertices = hCount2 * vCount2;

        Vector3[] vertices = new Vector3[numVertices];
        Vector2[] uvs = new Vector2[numVertices];
        int[] triangles = new int[numTriangles];

        int index = 0;
        float uvFactorX = 1.0f;
        float uvFactorY = 1.0f;
        for (float y = 0.0f; y < hCount2; y++)
        {
            for (float x = 0.0f; x < vCount2; x++)
            {
                vertices[index] = new Vector3(x * m_Width - m_Width / 2.0f, y * m_Height - m_Height / 2.0f);
                uvs[index++] = new Vector2(x * uvFactorX, y * uvFactorY);
            }
        }

        index = 0;
        for (int y = 0; y < 1; y++)
        {
            for (int x = 0; x < 1; x++)
            {
                triangles[index] = (y * hCount2) + x;
                triangles[index + 1] = ((y + 1) * hCount2) + x;
                triangles[index + 2] = (y * hCount2) + x + 1;

                triangles[index + 3] = ((y + 1) * hCount2) + x;
                triangles[index + 4] = ((y + 1) * hCount2) + x + 1;
                triangles[index + 5] = (y * hCount2) + x + 1;
                index += 6;
            }
        }

        _Mesh.vertices = vertices;
        _Mesh.uv = uvs;
        _Mesh.triangles = triangles;
        _Mesh.RecalculateNormals();
        _Mesh.RecalculateBounds();
    }

    void CreateScreenCamera(GameObject _ScreenObj)
    {
        GameObject screenCamera = new GameObject();
        screenCamera.name = _ScreenObj.name + "_RenderCamera";
        screenCamera.transform.parent = _ScreenObj.transform;
        screenCamera.transform.localPosition = new Vector3(0.0f, 0.0f, -1.0f);
        screenCamera.transform.localRotation = Quaternion.identity;

        Camera RenderCamera = screenCamera.AddComponent<Camera>();
        RenderCamera.cullingMask = 1 << LayerMask.NameToLayer("UI");
        RenderCamera.orthographic = true;
        RenderCamera.orthographicSize = m_Height * 0.5f;
        RenderCamera.backgroundColor = Color.black;
    }
}