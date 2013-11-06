using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;

public class CDUIMainView : CDUIView
{
    // Member Fields
    private EQuality m_Quality = EQuality.INVALID;
    private ELayoutStyle m_Layout = ELayoutStyle.INVALID;
	
    private GameObject m_RenderCamera = null;
    private RenderTexture m_RenderTex = null; 
	
	private Rect m_TitleRect = new Rect();
    private Rect m_NavAreaRect = new Rect();
    private Rect m_SubViewAreaRect = new Rect();
	
	private uint m_SubViewIdCount = 0;
	private Dictionary<uint, GameObject> m_SubViews = new Dictionary<uint, GameObject>();
	
	private GameObject m_ActiveSubView = null;
	
    // Member Properties
	public Camera RenderCamera { get{ return(m_RenderCamera.camera); } }

    // Member Methods
    private void Update()
    {
		// Draw debug information
        DebugRenderRects();

        // Update the render texture
        m_RenderTex.DiscardContents(true, true);
        RenderTexture.active = m_RenderTex;
		
		// Render using the render camrea
        RenderCamera.Render();

        RenderTexture.active = null;
    }

    public void Initialise(EQuality _Quality, ELayoutStyle _Layout, Vector2 _Dimensions)
    {
        m_Quality = _Quality;
        m_Dimensions = _Dimensions;
		m_Layout = _Layout;
		
        // Setup the main view
        SetupMainView();

        // Setup the render texture
        SetupRenderTex();

        // Setup the render camera
        SetupRenderCamera();
    }
	
    public void AddTitle(string _TitleText)
    {
        Vector3 localPos = new Vector3(m_TitleRect.center.x * m_Dimensions.x - (m_Dimensions.x * 0.5f),
                                       m_TitleRect.center.y  * m_Dimensions.y - (m_Dimensions.y * 0.5f));

        // Add a field for the title
        CDUIField duiField = AddField(_TitleText);
		
		// Position it to the center of the title area
		duiField.transform.localPosition = localPos;
    }
	
    public void AttatchRenderTexture(Material _sharedScreenMat)
    {
        // Set the render text onto the material of the screen
        _sharedScreenMat.SetTexture("_MainTex", m_RenderTex); 
    }

    public GameObject AddSubView()
    {
        // Create the DUI game object
        GameObject duiSubView = new GameObject();
        duiSubView.transform.parent = transform;
        duiSubView.name = "SubView";
        duiSubView.layer = gameObject.layer;
        duiSubView.transform.localRotation = Quaternion.identity;

        // Place it in the middle
        float x = m_SubViewAreaRect.center.x * m_Dimensions.x - (m_Dimensions.x * 0.5f);
        float y = m_SubViewAreaRect.center.y * m_Dimensions.y - (m_Dimensions.y * 0.5f);
        duiSubView.transform.localPosition = new Vector3(x, y);
		
        // Add the DUI component
        CDUISubView subView = duiSubView.AddComponent<CDUISubView>();
		subView.ViewID = ++m_SubViewIdCount;
		
        // Initialise the DUI Component
        subView.Initialise(new Vector2(m_SubViewAreaRect.width * m_Dimensions.x, m_SubViewAreaRect.height * m_Dimensions.y));

        // Set the button as beloning to this view object as the partent
        subView.m_NavButton.transform.parent = transform;

        // Register the button for the event
        subView.m_NavButton.PressDown += NavButtonPressed;
		
		// Add to the dictionary
        m_SubViews[subView.ViewID] = duiSubView;
		
		// Set this as the active subview
		SetActiveSubView(subView.ViewID);
		
        // Reposition the buttons
        RepositionButtons();

        return(duiSubView);
    }
	
	public GameObject GetSubView(uint _SubViewId)
	{
		GameObject subView = null;
			
		if(m_SubViews.ContainsKey(_SubViewId))
		{
			subView = m_SubViews[_SubViewId];
			subView.gameObject.SetActive(true);
		}
		else
		{
			Debug.LogError("GetSubView, id sent in doesn't exsist!");
		}
		
		return(subView);
	}
	
	public void SetActiveSubView(uint _SubViewId)
	{
		if(m_ActiveSubView != null)
			m_ActiveSubView.SetActive(false);
		
		m_ActiveSubView = GetSubView(_SubViewId);
	}
	
	public GameObject FindButtonCollisions(RaycastHit _rh)
    {
		Vector3 offset = new Vector3(_rh.textureCoord.x * m_Dimensions.x - m_Dimensions.x * 0.5f,
                                     _rh.textureCoord.y * m_Dimensions.y - m_Dimensions.y * 0.5f, 0.0f);

        offset = transform.rotation * offset;
        Vector3 rayOrigin = transform.position + offset + transform.forward * -1.0f;

        Ray ray = new Ray(rayOrigin, transform.forward);
        RaycastHit hit;
        float rayLength = 2.0f;
		
		GameObject button = null;
		
        if (Physics.Raycast(ray, out hit, rayLength, 1 << LayerMask.NameToLayer("DUI")))
        {
            button = hit.transform.parent.gameObject;
			
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
		 // Setup the rects according to layout
		switch (m_Layout) 
		{
		case ELayoutStyle.Layout_1: 
			m_TitleRect.Set(0.0f, 0.8f, 1.0f, 0.2f); 
			m_NavAreaRect.Set(0.0f, 0.0f, 0.2f, 0.8f); 
			m_SubViewAreaRect.Set(0.2f, 0.0f, 0.8f, 0.8f); 
			break;
			
		case ELayoutStyle.Layout_2: 
			m_TitleRect.Set(0.0f, 0.9f, 1.0f, 0.1f);
			m_NavAreaRect.Set(0.0f, 0.0f, 1.0f, 0.2f);
			m_SubViewAreaRect.Set(0.0f, 0.2f, 1.0f, 0.7f);
			break;
			
		default:
			Debug.LogError("Invalid Layout for console provided!");	
			break;
		}
    }

    private void SetupRenderTex()
    {
        // Figure out the pixels per meter for the screen based on quality setting
        float ppm = 0.0f;
        switch (m_Quality)
        {
            case EQuality.VeryHigh: ppm = 500; break;
            case EQuality.High: ppm = 400; break;
            case EQuality.Medium: ppm = 300; break;
            case EQuality.Low: ppm = 200; break;
            case EQuality.VeryLow: ppm = 100; break;
            
            default:break;
        }

        int width = (int)(m_Dimensions.x * ppm);
        int height = (int)(m_Dimensions.y * ppm);

        // Create a new render texture
        m_RenderTex = new RenderTexture(width, height, 16);
        m_RenderTex.name = name + " RT";
        m_RenderTex.Create();
    }

    private void SetupRenderCamera()
    {
        // Create the camera game object
        m_RenderCamera = new GameObject();
        m_RenderCamera.name = "RenderCamera";
        m_RenderCamera.transform.parent = transform;
		m_RenderCamera.transform.localPosition = new Vector3(0.0f, 0.0f, -1.0f);
        m_RenderCamera.transform.localRotation = Quaternion.identity;
        m_RenderCamera.layer = gameObject.layer;

        // Get the render camera and set its target as the render texture
        Camera camera = m_RenderCamera.AddComponent<Camera>();
        camera.cullingMask = 1 << gameObject.layer;
        camera.orthographic = true;
        camera.backgroundColor = Color.black;
        camera.nearClipPlane = 0.0f;
        camera.farClipPlane = 2.0f;
        camera.targetTexture = m_RenderTex;
        camera.orthographicSize = m_Dimensions.y * 0.5f;
    }
	
    private void RepositionButtons()
    {
        int numSubViews = m_SubViews.Count;
        int count = 0;

        foreach (GameObject subView in m_SubViews.Values)
        {
            CDUIButton navButton = subView.GetComponent<CDUISubView>().m_NavButton;
           
            // Calculate the position for the nav button to go
            if (m_NavAreaRect.width * Dimensions.x < m_NavAreaRect.height * Dimensions.y)
            {
                float x = m_NavAreaRect.center.x * m_Dimensions.x - (m_Dimensions.x * 0.5f);
                float y = ((m_NavAreaRect.yMax - m_NavAreaRect.y) * (count + 0.5f) / numSubViews) * m_Dimensions.y - (m_Dimensions.y * 0.5f);

                navButton.transform.localPosition = new Vector3(x, y);
            }
            else
            {
                float x = (m_NavAreaRect.xMax - m_NavAreaRect.x) * (count + 0.5f) / numSubViews * m_Dimensions.x - (m_Dimensions.x * 0.5f);
                float y = m_NavAreaRect.center.y * m_Dimensions.y - (m_Dimensions.y * 0.5f);

                navButton.transform.localPosition = new Vector3(x, y);
            }

            count += 1;
        }
    }

    private void NavButtonPressed(CDUIButton _sender)
    {
        foreach(GameObject subView in m_SubViews.Values)
        {
            CDUIButton button = subView.GetComponent<CDUISubView>().m_NavButton;

            // If the button belongs to the subview
            if (button == _sender)
            {
                SetActiveSubView(subView.GetComponent<CDUISubView>().ViewID);
				break;
			}
        }
    }

    // Debug functions
    private void DebugRenderRects()
    {
        // Render self rect
        DebugDrawRect(new Rect(0.0f, 0.0f, 1.0f, 1.0f), Color.white, 0);

        // Title rect
        DebugDrawRect(m_TitleRect, Color.cyan, 0.005f);

        // Nav area rect
        DebugDrawRect(m_NavAreaRect, Color.yellow, 0.005f);

        // Subview area rect
        DebugDrawRect(m_SubViewAreaRect, Color.red, 0.005f);   
    }
}
