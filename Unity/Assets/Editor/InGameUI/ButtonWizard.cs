using UnityEngine;
using UnityEditor;
using System.Collections;

public class ButtonWizard : ScriptableWizard
{
    // Member Variables
    public string m_ButtonName = "Default";
    public Texture m_ButtonTexture = null;
    public float m_Width = 0.4f;
    public float m_Height = 0.3f;

    private GameObject m_ButtonInstance = null;
    private GameObject m_ButtonBackInstance = null;
    private GameObject m_ButtonTextInstance = null;

    private Mesh m_ButtonBackMesh = null;
    private Material m_ButtonBackMat = null;

    // Member Methods
    [MenuItem("In-Game UI/Button...")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<ButtonWizard>("In Game UI - Button");
    }

    void OnWizardCreate()
    {
        // Create the button
        CreateButton();

        // Destroy the created instance of the button
        DestroyImmediate(m_ButtonInstance);
    }

    void OnWizardUpdate()
    {
        if (m_ButtonTexture != null && m_ButtonName == "Default")
        {
            m_ButtonName = m_ButtonTexture.name;
        }

        helpString = "Button Wizard";
    }

    void CreateButton()
    {
        // Create the button
        m_ButtonInstance = new GameObject(m_ButtonName);
        m_ButtonInstance.layer = LayerMask.NameToLayer("UI");

        // Create the button background
        CreateButtonContents();

        // Save the generated mesh into the prefab
        GameObject buttonPrefab = PrefabUtility.CreatePrefab("Assets/InGame UI/Buttons/" + m_ButtonInstance.name + ".prefab", m_ButtonInstance);
        AssetDatabase.AddObjectToAsset(m_ButtonBackMesh, buttonPrefab);
        AssetDatabase.AddObjectToAsset(m_ButtonBackMat, buttonPrefab);
        AssetDatabase.SaveAssets();

        // Set the mesh filter and the material to use on the object
        m_ButtonBackInstance.GetComponent<MeshFilter>().mesh = m_ButtonBackMesh;
        m_ButtonBackInstance.GetComponent<MeshRenderer>().material = m_ButtonBackMat;

        // Replace the prefab to update changes
        PrefabUtility.ReplacePrefab(m_ButtonInstance, buttonPrefab, ReplacePrefabOptions.Default);
    }

    void CreateButtonContents()
    {
        // Add a new game object with the a mesh and rendering
        m_ButtonBackInstance = new GameObject(m_ButtonInstance.name + "_background");
        m_ButtonBackInstance.transform.parent = m_ButtonInstance.transform;
        m_ButtonBackInstance.layer = LayerMask.NameToLayer("UI");

        // Create the mesh that will be the screen
        CreateButtonMesh();

        // Add the mesh filter and renderer to the screen object
        m_ButtonBackInstance.AddComponent<MeshFilter>();
        m_ButtonBackInstance.AddComponent<MeshRenderer>();

        // Create the material to use
        m_ButtonBackMat = new Material(Shader.Find("Transparent/Diffuse"));
        m_ButtonBackMat.SetTexture("_MainTex", m_ButtonTexture);
        m_ButtonBackMat.name = m_ButtonBackInstance.name + "_mat";

        // Add a new game object with the 3D text component
        m_ButtonTextInstance = new GameObject(m_ButtonInstance.name + "_text");
        m_ButtonTextInstance.transform.parent = m_ButtonInstance.transform;
        m_ButtonTextInstance.layer = LayerMask.NameToLayer("UI");

        // Configure the text mesh
        TextMesh tm = m_ButtonTextInstance.AddComponent<TextMesh>();
        tm.anchor = TextAnchor.MiddleCenter;
        tm.text = "Text";

        // Add the script for InGameUI and editor for the monitor
        m_ButtonInstance.AddComponent<ButtonUI>();
        ButtonEditor bE = m_ButtonInstance.AddComponent<ButtonEditor>();
        bE.m_ButtonWidth = m_Width;
        bE.m_ButtonHeight = m_Height;
    }

    void CreateButtonMesh()
    {
        m_ButtonBackMesh = new Mesh();
        m_ButtonBackMesh.name = m_ButtonBackInstance.name + "_mesh";

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

        m_ButtonBackMesh.vertices = vertices;
        m_ButtonBackMesh.uv = uvs;
        m_ButtonBackMesh.triangles = triangles;
        m_ButtonBackMesh.RecalculateNormals();
        m_ButtonBackMesh.RecalculateBounds();
    }
}
