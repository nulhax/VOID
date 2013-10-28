using UnityEngine;
using System;
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

    // Member Fields
    private EQuality m_quality;
    private ENavButDirection m_navButtonDirection;


    // Member Properties
    public Rect m_titleRect                             { get; set; }
    public Rect m_navAreaRect                           { get; set; }
    public Rect m_subViewAreaRect                       { get; set; }
    public Camera m_renderCamera                        { get; set; }
    public RenderTexture m_renderTex                    { get; set; }
    public Dictionary<string, DUISubView> m_subViews    { get; set; }


    // Member Methods
    private void Update()
    {
        DebugRenderRects();

        // Update the render texture and camera
        m_renderTex.DiscardContents(true, true);
        RenderTexture.active = m_renderTex;
		
        m_renderCamera.Render();

        RenderTexture.active = null;
    }

    public void Initialise(TextAsset _uiXmlDoc)
    {
		m_subViews = new Dictionary<string, DUISubView>();
		
        // Load the XML file for the UI and save the base node for the ui
        m_uiXmlNode = LoadXML(_uiXmlDoc).SelectSingleNode("mainview");

        // Get the screen details
        XmlNode screenNode = m_uiXmlNode.SelectSingleNode("screen");
        m_quality = (EQuality)System.Enum.Parse(typeof(EQuality), screenNode.Attributes["quality"].Value);
        m_dimensions = StringToVector2(screenNode.Attributes["dimensions"].Value);

        // Setup the main view
        SetupMainView();

        // Setup the render texture
        SetupRenderTex();

        // Setup the render camera
        SetupRenderCamera();
    }

    public void AttatchRenderTexture(Material _sharedScreenMat)
    {
        // Set the render text onto the material of the screen
        _sharedScreenMat.SetTexture("_MainTex", m_renderTex); 
    }

    public DUISubView AddSubview(string _subViewName)
    {
        TextAsset ta = (TextAsset)Resources.Load("XMLs/DUI/subviews/" + _subViewName);

        // Create the DUI game object
        GameObject duiGo = new GameObject();
        duiGo.transform.parent = transform;
        duiGo.name = "SubView_" + _subViewName;
        duiGo.layer = gameObject.layer;
        duiGo.transform.localRotation = Quaternion.identity;

        // Place it in the middle
        float x = m_subViewAreaRect.center.x * m_dimensions.x - (m_dimensions.x * 0.5f);
        float y = m_subViewAreaRect.center.y * m_dimensions.y - (m_dimensions.y * 0.5f);
        duiGo.transform.localPosition = new Vector3(x, y);
        
        // Add the DUI component
        DUISubView DUISV = duiGo.AddComponent<DUISubView>();

        // Initialise the DUI Component
        DUISV.Initialise(ta, new Vector2(m_subViewAreaRect.width * m_dimensions.x, m_subViewAreaRect.height * m_dimensions.y));

        // Set the button as beloning to this view object as the partent
        DUISV.m_navButton.transform.parent = transform;

        // Register the button for the event
        DUISV.m_navButton.PressDown += NavButtonPressed;

        // Add to the dictionary
        m_subViews[_subViewName] = DUISV;

        // Reposition the buttons
        RepositionButtons();

        return (DUISV);
    }
	
	public DUIButton FindButtonCollisions(RaycastHit _rh)
    {
		Vector3 offset = new Vector3(_rh.textureCoord.x * m_dimensions.x - m_dimensions.x * 0.5f,
                                     _rh.textureCoord.y * m_dimensions.y - m_dimensions.y * 0.5f,
                                             0.0f);

        offset = transform.rotation * offset;
        Vector3 rayOrigin = transform.position + offset + transform.forward * -1.0f;

        Ray ray = new Ray(rayOrigin, transform.forward);
        RaycastHit hit;
        float rayLength = 2.0f;
		
		DUIButton button = null;
		
        if (Physics.Raycast(ray, out hit, rayLength, 1 << LayerMask.NameToLayer("DUI")))
        {
            button = hit.transform.parent.gameObject.GetComponent<DUIButton>();
			
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * rayLength, Color.green, 0.5f);
        }
        else
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * rayLength, Color.red, 0.5f);
        }
		
		return(button);
    }

    private void SetupMainView()
    {
        // Get the Title info
        m_titleRect = DUIMainView.StringToRect(m_uiXmlNode.SelectSingleNode("title").Attributes["rect"].Value);

        // Get the Navigation Area info
        XmlNode navViewNode = m_uiXmlNode.SelectSingleNode("navarea");
        m_navAreaRect = DUIMainView.StringToRect(navViewNode.Attributes["rect"].Value);
        m_navButtonDirection = (ENavButDirection)System.Enum.Parse(typeof(ENavButDirection), navViewNode.Attributes["butdir"].Value);

        // Get the Subview Area info
        m_subViewAreaRect = DUIMainView.StringToRect(m_uiXmlNode.SelectSingleNode("subviewarea").Attributes["rect"].Value);
		
        // Setup the title
        SetupTitle();
    }

    private void SetupTitle()
    {
        // Get the title node
        XmlNode titleNode = m_uiXmlNode.SelectSingleNode("title");

        string text = titleNode.Attributes["text"].Value;
        Vector3 localPos = new Vector3(m_titleRect.center.x * m_dimensions.x - (m_dimensions.x * 0.5f),
                                      m_titleRect.center.y  * m_dimensions.y - (m_dimensions.y * 0.5f));

        // Add a field for the title
        DUIField duiField = AddField(text);
		
		// Position it to the center of the title area
		duiField.transform.localPosition = localPos;
    }

    private void SetupRenderTex()
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
    }

    private void SetupRenderCamera()
    {
        // Create the camera game object
        GameObject go = new GameObject();
        go.name = "RenderCamera";
        go.transform.parent = transform;
		go.transform.localPosition = new Vector3(0.0f, 0.0f, -1.0f);
        go.transform.localRotation = Quaternion.identity;
        go.layer = gameObject.layer;

        // Get the render camera and set its target as the render texture
        m_renderCamera = go.AddComponent<Camera>();
        m_renderCamera.cullingMask = 1 << gameObject.layer;
        m_renderCamera.orthographic = true;
        m_renderCamera.backgroundColor = Color.black;
        m_renderCamera.nearClipPlane = 0.0f;
        m_renderCamera.farClipPlane = 2.0f;
        m_renderCamera.targetTexture = m_renderTex;
        m_renderCamera.orthographicSize = m_dimensions.y * 0.5f;
    }

    private void RepositionButtons()
    {
        int numSubViews = m_subViews.Count;
        int count = 0;

        foreach (DUISubView subView in m_subViews.Values)
        {
            DUIButton navButton = subView.m_navButton;
           
            // Calculate the position for the nav button to go
            if (m_navButtonDirection == ENavButDirection.Vertical)
            {
                float x = m_navAreaRect.center.x * m_dimensions.x - (m_dimensions.x * 0.5f);
                float y = ((m_navAreaRect.yMax - m_navAreaRect.y) * (count + 0.5f) / numSubViews) * m_dimensions.y - (m_dimensions.y * 0.5f);

                navButton.transform.localPosition = new Vector3(x, y);
            }
            else if (m_navButtonDirection == ENavButDirection.Horizontal)
            {
                float x = (m_navAreaRect.xMax - m_navAreaRect.x) * (count + 0.5f) / numSubViews * m_dimensions.x - (m_dimensions.x * 0.5f);
                float y = m_navAreaRect.center.y * m_dimensions.y - (m_dimensions.y * 0.5f);

                navButton.transform.localPosition = new Vector3(x, y);
            }

            count += 1;
        }
    }

    // Event handler functions
    private void NavButtonPressed(DUIButton _sender)
    {
        foreach(DUISubView subView in m_subViews.Values)
        {
            DUIButton button = subView.m_navButton;

            // If the button belongs to the subview
            if (button == _sender)
            {
                // Toggle its activity
                subView.gameObject.SetActive(!subView.gameObject.activeSelf);
            }
			else
			{
				subView.gameObject.SetActive(false);
			}
        }
    }

    // Debug functions
    private void DebugRenderRects()
    {
        // Render self rect
        DebugDrawRect(new Rect(0.0f, 0.0f, 1.0f, 1.0f), Color.white, 0);

        // Title rect
        DebugDrawRect(m_titleRect, Color.cyan, 0.005f);

        // Nav area rect
        DebugDrawRect(m_navAreaRect, Color.yellow, 0.005f);

        // Subview area rect
        DebugDrawRect(m_subViewAreaRect, Color.red, 0.005f);   
    }
}
