//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CActorMotor.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CDUI : MonoBehaviour
{
	// Member delegates
	public event Action<uint> SubviewChanged;
	
    // Member Fields
    private EQuality m_Quality = EQuality.INVALID;
	
	private GameObject m_Console = null;
    private GameObject m_RenderCamera = null;
    private RenderTexture m_RenderTex = null; 
	
	private GameObject m_MainView = null;
	
	private GameObject m_ActiveSubView = null;
	private Dictionary<uint, GameObject> m_SubViews = new Dictionary<uint, GameObject>();
	
	private uint m_ViewIdCount = 0;	
	
	// Member Properties
	
	// Member Methods
	
    // Member Properties
	public Camera RenderCamera 
	{ 
		get { return(m_RenderCamera.camera); } 
	}
	
	public CDUIMainView MainView
	{
		get { return(m_MainView.GetComponent<CDUIMainView>()); }
	}
	
	public GameObject Console
	{
		get { return(m_Console); }
	}

	
    // Member Methods
    private void Update()
	{
		// Render using the render camrea
        RenderCamera.Render();
    }

    public void Initialise(EQuality _Quality, ELayoutStyle _Layout, Vector2 _Dimensions, GameObject _Console)
    {
		m_Console = _Console;
        m_Quality = _Quality;
		
        // Setup the main view
        SetupMainView(_Layout, _Dimensions);

        // Setup the render texture
        SetupRenderTex();

        // Setup the render camera
        SetupRenderCamera();
    }

    public CDUISubView AddSubView()
    {
        // Create the DUI game object
        GameObject subView = new GameObject();
        subView.transform.parent = transform;
        subView.name = "SubView " + m_SubViews.Count.ToString();
        subView.layer = gameObject.layer;
        subView.transform.localRotation = Quaternion.identity;
		
		// Add the DUI component
        CDUISubView duiSubView = subView.AddComponent<CDUISubView>();
		duiSubView.ViewID = ++m_ViewIdCount;
		
		// Get the MainView component and setup the subview
		CDUIMainView duiMainView = m_MainView.GetComponent<CDUIMainView>();
        CDUIButton navButton = duiMainView.SetupSubViewAndNavButton(duiSubView);
		
		// Register the button for the event
        navButton.PressDown += NavButtonPressed;
		
		// Add to the dictionaries
        m_SubViews[duiSubView.ViewID] = subView;
		
		// Set as the active subview on the server
		if(CNetwork.IsServer)
		{
			OnSubviewChange(duiSubView.ViewID);
		}
		
        return(duiSubView);
    }
	
	public CDUISubView GetSubView(uint _SubViewId)
	{
		GameObject subView = null;
			
		if(m_SubViews.ContainsKey(_SubViewId))
		{ 	
			subView = m_SubViews[_SubViewId];
		}
		else
		{
			Debug.LogError("GetSubView, id sent in doesn't exsist!");
		}
		
		return(subView.GetComponent<CDUISubView>());
	}
	
	public void SetActiveSubView(GameObject _SubView)
	{
		// Reposition all of the subviews out of view of the camera
		foreach(GameObject subView in m_SubViews.Values)
		{
			float x = MainView.m_SubViewAreaRect.center.x * MainView.Dimensions.x - (MainView.Dimensions.x * 0.5f);
        	float y = MainView.m_SubViewAreaRect.center.y * MainView.Dimensions.y - (MainView.Dimensions.y * 0.5f);
			
			subView.transform.localPosition = new Vector3(x, y, -1.5f);
		}
		
		// Move this subview back into view
		if(m_SubViews.ContainsValue(_SubView))
		{
			m_ActiveSubView = _SubView;
			
			m_ActiveSubView.transform.localPosition = new Vector3(m_ActiveSubView.transform.localPosition.x, m_ActiveSubView.transform.localPosition.y, 0.0f);
		}
		else
		{
			Debug.Log("SetActiveSubView, subview doesn't belong to this DUI!!");
		}
	}
	
    public void AttatchRenderTexture(Material _sharedScreenMat)
    {
        // Set the render text onto the material of the screen
        _sharedScreenMat.SetTexture("_MainTex", m_RenderTex); 
    }
	
	private void NavButtonPressed(CDUIButton _sender)
    {
		// Call the subview changed event
		OnSubviewChange(MainView.GetSubviewFromNavButton(_sender).GetComponent<CDUISubView>().ViewID);
    }
	
	private void SetupMainView(ELayoutStyle _Layout, Vector2 _Dimensions)
    {
//		// Create the DUI game object
//        m_MainView = new GameObject();
//        m_MainView.transform.parent = transform;
//        m_MainView.name = "MainView";
//        m_MainView.layer = gameObject.layer;
//        m_MainView.transform.localRotation = Quaternion.identity;
//		m_MainView.transform.localPosition = Vector3.zero;
//		
//        // Add the DUI component
//        CDUIMainView duiMainView = m_MainView.AddComponent<CDUIMainView>();
//
//        // Initialise the DUI Component
//        duiMainView.Initialise(_Layout, _Dimensions);
//		duiMainView.ViewID = 0;

		// Test: Create the new thing
		m_MainView = (GameObject)GameObject.Instantiate(Resources.Load ("Prefabs/DUI/Message Root"));
		m_MainView.transform.parent = transform;
		m_MainView.transform.localRotation = Quaternion.identity;
		m_MainView.transform.localPosition = Vector3.zero;
    }
	
    private void SetupRenderTex()
    {
		UIPanel panel = m_MainView.GetComponent<UIPanel>();
		
        // Figure out the pixels density for the screen based on quality setting
        float pd = 0.0f;
        switch (m_Quality)
        {
		case EQuality.VeryHigh: pd = 1000f; break;
		case EQuality.High: pd = 600.0f; break;
		case EQuality.Medium: pd = 350.0f; break;
		case EQuality.Low: pd = 200.0f; break;
		case EQuality.VeryLow: pd = 100.0f; break;
        default:break;
        }

		int width = (int)(panel.width * pd);
		int height = (int)(panel.height * pd);

        // Create a new render texture
        m_RenderTex = new RenderTexture(width, height, 16);
        m_RenderTex.name = name + " RT";
        m_RenderTex.Create();
    }

    private void SetupRenderCamera()
    {
		UIPanel panel = m_MainView.GetComponent<UIPanel>();
		
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
		camera.clearFlags = CameraClearFlags.Color;
        camera.orthographic = true;
        camera.backgroundColor = Color.clear;
        camera.nearClipPlane = 0.0f;
        camera.farClipPlane = 2.0f;
        camera.targetTexture = m_RenderTex;
		camera.orthographicSize = panel.height * 0.5f;
		camera.enabled = false;

		// Add the NGUI camera component
		m_RenderCamera.AddComponent<UICamera>();
    }
	
	public GameObject FindDUIElementCollisions(float _texCoordU, float _texCoordV)
    {
		UIPanel panel = m_MainView.GetComponent<UIPanel>();

		Vector3 offset = new Vector3(_texCoordU * panel.width - panel.width * 0.5f,
		                             _texCoordV * panel.height - panel.height * 0.5f, 0.0f);

        offset = transform.rotation * offset;
        Vector3 rayOrigin = transform.position + offset + transform.forward * -1.0f;

        Ray ray = new Ray(rayOrigin, transform.forward);

		GameObject element = null;
		
//        if (Physics.Raycast(ray, out hit, rayLength, 1 << LayerMask.NameToLayer("DUI")))
//        {
//            element = hit.collider.gameObject;
//			
//            Debug.DrawLine(ray.origin, ray.origin + ray.direction * rayLength, Color.green, 0.5f);
//        }
//        else
//        {
//            Debug.DrawLine(ray.origin, ray.origin + ray.direction * rayLength, Color.red, 0.5f);
//        }
		
		return(element);
	}
	
	private void OnSubviewChange(uint _iActiveSubview)
	{
		if(SubviewChanged != null)
			SubviewChanged(_iActiveSubview);
	}
}
