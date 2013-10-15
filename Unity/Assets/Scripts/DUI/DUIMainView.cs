using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;

public class DUIMainView : DUIView
{
    public enum ENavButDirection
    {
        Horizontal,
        Vertical
    }

    private XmlNode m_uiXmlNode;

    public Vector2 m_dimensions         { get; set; }
    
    public Rect m_titleRect             { get; set; }
    public Rect m_navAreaRect           { get; set; }
    public Rect m_subViewAreaRect       { get; set; }

    private EQuality m_quality;
    private ENavButDirection m_navButtonDirection;
    
    private Camera m_renderCamera;
    private RenderTexture m_renderTex;

    // Member Methods
    public void Initialise(TextAsset _uiXmlDoc, Material _sharedScreenMat)
    {
        // Load the XML file for the UI and save the base node for the ui
        m_uiXmlNode = LoadXML(_uiXmlDoc).SelectSingleNode("dui");

        // Get the UI quality
        m_quality = (EQuality)System.Enum.Parse(typeof(EQuality), m_uiXmlNode.Attributes["quality"].Value);

        // Get the UI dimensions
        m_dimensions = StringToVector2(m_uiXmlNode.Attributes["dimensions"].Value);

        // Setup the main view
        SetupMainView();

        // Setup the render texture
        SetupRenderTex(_sharedScreenMat);

        // Setup the render camera
        SetupRenderCamera();
    }

    private void Update()
    {
        DebugRenderRects();

        // Update the render texture and camera
        m_renderTex.DiscardContents(true, true);
        RenderTexture.active = m_renderTex;

        m_renderCamera.Render();

        RenderTexture.active = null;
    }

    private void SetupMainView()
    {
        XmlNode mainViewNode = m_uiXmlNode.SelectSingleNode("mainview");

        // Get the Title info
        m_titleRect = DUIMainView.StringToRect(mainViewNode.SelectSingleNode("title").Attributes["rect"].Value);

        // Get the Navigation Area info
        m_navAreaRect = DUIMainView.StringToRect(mainViewNode.SelectSingleNode("navarea").Attributes["rect"].Value);
        m_navButtonDirection = (ENavButDirection)System.Enum.Parse(typeof(ENavButDirection), mainViewNode.SelectSingleNode("navarea").Attributes["butdir"].Value);

        // Get the Subview Area info
        m_subViewAreaRect = DUIMainView.StringToRect(mainViewNode.SelectSingleNode("subviewarea").Attributes["rect"].Value);

        // Setup the title
        SetupTitle();

        // Setup the navigation buttons
        SetupNavButtons();
    }

    private void SetupTitle()
    {
        // Get the title node
        XmlNode titleNode = m_uiXmlNode.SelectSingleNode("mainview").SelectSingleNode("title");

        string text = titleNode.Attributes["text"].Value;
        Vector3 localPos = new Vector3(m_titleRect.center.x * m_dimensions.x - (m_dimensions.x * 0.5f),
                                      m_titleRect.center.y  * m_dimensions.y - (m_dimensions.y * 0.5f));

        // Create the title
        CreateTitle(text, localPos);
    }

    private void SetupNavButtons()
    {
        // Get the count to space the nav buttons out evenly
        XmlNodeList subViews = m_uiXmlNode.SelectNodes("subview");
        int subViewCount = subViews.Count;

        // Iterate through the subviews and create the nav buttons
        for (int i = 0; i < subViewCount; ++i)
        {
            XmlNode navButtonNode = subViews[i].SelectSingleNode("navbutton");

            string prefabPath = "Assets/Resources/Prefabs/DUI/Buttons/" + navButtonNode.Attributes["prefab"].Value + ".prefab";
            string text = navButtonNode.Attributes["text"].Value;
            Vector3 localPos = Vector3.zero;

            if (m_navButtonDirection == ENavButDirection.Vertical)
                localPos = new Vector3(m_navAreaRect.center.x * m_dimensions.x - (m_dimensions.x * 0.5f),
                                      (m_navAreaRect.yMax - m_navAreaRect.y) * (i + 0.5f)/subViewCount * m_dimensions.y - (m_dimensions.y * 0.5f));
            else
                localPos = new Vector3((m_navAreaRect.xMax - m_navAreaRect.x) * (i + 0.5f) / subViewCount * m_dimensions.x - (m_dimensions.x * 0.5f),
                                      (m_navAreaRect.center.y * m_dimensions.y - (m_dimensions.y * 0.5f)));

            // Create the navigation button
            CreateNavButton(prefabPath, text, localPos);
        }
    }

    private void SetupRenderTex(Material _sharedScreenMat)
    {
        // Figure out the pixels per meter for the screen based on quality setting
        float ppm = 0.0f;
        switch (m_quality)
        {
            case EQuality.VeryHigh: ppm = 500; break;
            case EQuality.High: ppm = 400; break;
            case EQuality.Medium: ppm = 300; break;
            case EQuality.Low: ppm = 200; break;
            case EQuality.VeryLow: ppm = 100; break;
            
            default:break;
        }

        int width = (int)(m_dimensions.x * ppm);
        int height = (int)(m_dimensions.y * ppm);

        // Create a new render texture
        m_renderTex = new RenderTexture(width, height, 16);
        m_renderTex.name = name + " RT";
        m_renderTex.Create();

        // Set it onto the material of the screen
        _sharedScreenMat.SetTexture("_MainTex", m_renderTex);
    }

    private void SetupRenderCamera()
    {
        // Create the camera game object
        GameObject go = new GameObject();
        go.name = transform.name + "_RenderCamera";
        go.transform.parent = transform;
        go.transform.localPosition = new Vector3(0.0f, 0.0f, -1.0f);
        go.transform.localRotation = Quaternion.identity;
        go.layer = LayerMask.NameToLayer("DUI");

        // Get the render camera and set its target as the render texture
        m_renderCamera = go.AddComponent<Camera>();
        m_renderCamera.cullingMask = 1 << LayerMask.NameToLayer("DUI");
        m_renderCamera.orthographic = true;
        m_renderCamera.backgroundColor = Color.black;
        m_renderCamera.nearClipPlane = 0.0f;
        m_renderCamera.farClipPlane = 2.0f;
        m_renderCamera.targetTexture = m_renderTex;
        m_renderCamera.orthographicSize = m_dimensions.y * 0.5f;
    }

    private void CreateTitle(string _text, Vector3 _localPos)
    {
        GameObject titleGo = new GameObject();

        // Set the default values
        titleGo.layer = gameObject.layer;
        titleGo.transform.parent = transform;
        titleGo.transform.localPosition = _localPos;
        titleGo.transform.localRotation = Quaternion.identity;

        MeshRenderer mr = titleGo.AddComponent<MeshRenderer>();
        mr.material = (Material)Resources.Load("Fonts/Arial", typeof(Material));

        TextMesh tm = titleGo.AddComponent<TextMesh>();
        tm.fontSize = 0;
        tm.characterSize = 0.05f;
        tm.font = (Font)Resources.Load("Fonts/Arial", typeof(Font));
        tm.anchor = TextAnchor.MiddleCenter;
        tm.text = _text;
    }

    private void CreateNavButton(string _prefabPath, string _text, Vector3 _localPos)
    {
        GameObject buttonGo = (GameObject)Instantiate(Resources.LoadAssetAtPath(_prefabPath, typeof(GameObject)));

        // Set the default values
        buttonGo.layer = gameObject.layer;
        buttonGo.transform.parent = transform;
        buttonGo.transform.localPosition = _localPos;
        buttonGo.transform.localRotation = Quaternion.identity;

        buttonGo.GetComponentInChildren<TextMesh>().text = _text;
    }

    public void CheckButtonCollisions(RaycastHit _rh)
    {
        Vector3 offset = new Vector3(_rh.textureCoord.x * m_dimensions.x - m_dimensions.x * 0.5f,
                                     _rh.textureCoord.y * m_dimensions.y - m_dimensions.y * 0.5f,
                                             0.0f);

        offset = transform.rotation * offset;
        Vector3 rayOrigin = transform.position + offset + transform.forward * -1.0f;

        Ray ray = new Ray(rayOrigin, transform.forward);
        RaycastHit hit;
        float rayLength = 2.0f;

        if (Physics.Raycast(ray, out hit, rayLength, 1 << LayerMask.NameToLayer("DUI")))
        {
            DUIButton bUI = hit.transform.parent.gameObject.GetComponent<DUIButton>();
            if (bUI)
            {
                Debug.Log("Button Hit: " + hit.transform.parent.name);
                bUI.ButtonPressed();
            }
            else
            {
                Debug.Log("Button Hit: " + hit.transform.name);
            }

            Debug.DrawLine(ray.origin, ray.origin + ray.direction * rayLength, Color.green, 0.5f);
        }
        else
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * rayLength, Color.red, 0.5f);
        }
    }

    // Debug Functions
    private void DebugRenderRects()
    {
        // Test for rendering title, nav and content areas
        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.zero;

        start = new Vector3(m_titleRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_titleRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_titleRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_titleRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);

        start = new Vector3(m_titleRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_titleRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_titleRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_titleRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);

        start = new Vector3(m_titleRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_titleRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_titleRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_titleRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);

        start = new Vector3(m_titleRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_titleRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_titleRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_titleRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);






        start = new Vector3(m_navAreaRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_navAreaRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_navAreaRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_navAreaRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);

        start = new Vector3(m_navAreaRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_navAreaRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_navAreaRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_navAreaRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);

        start = new Vector3(m_navAreaRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_navAreaRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_navAreaRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_navAreaRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);

        start = new Vector3(m_navAreaRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_navAreaRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_navAreaRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_navAreaRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);





        start = new Vector3(m_subViewAreaRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_subViewAreaRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_subViewAreaRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_subViewAreaRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);

        start = new Vector3(m_subViewAreaRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_subViewAreaRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_subViewAreaRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_subViewAreaRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);

        start = new Vector3(m_subViewAreaRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_subViewAreaRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_subViewAreaRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_subViewAreaRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);

        start = new Vector3(m_subViewAreaRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_subViewAreaRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_subViewAreaRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_subViewAreaRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);
    }
}
