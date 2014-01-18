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
	private EQuality m_Quality = EQuality.VeryHigh;
	
	private GameObject m_Console = null;
    private GameObject m_DUICamera = null;
    private RenderTexture m_RenderTex = null; 
	
	private GameObject m_MainView = null;
	
	private GameObject m_ActiveSubView = null;
	private Dictionary<uint, GameObject> m_SubViews = new Dictionary<uint, GameObject>();
	
	private uint m_ViewIdCount = 0;	
	
	// Member Properties
	
	// Member Methods
	
    // Member Properties
	public GameObject Console 
	{ 
		get { return(m_Console); } 
		set { m_Console = value; } 
	}

	public GameObject DUICamera 
	{ 
		get { return(m_DUICamera); } 
	}
	
	public CDUIMainView MainView
	{
		get { return(m_MainView.GetComponent<CDUIMainView>()); }
	}
	
    // Member Methods
    private void Update()
	{
		// Render using the render camrea
        DUICamera.camera.Render();
    }

    public void Awake()
    {	
		// Test: Create the new thing
		m_MainView = (GameObject)GameObject.Instantiate(Resources.Load ("Prefabs/DUI/Message Root"));
		m_MainView.transform.parent = transform;
		m_MainView.transform.localRotation = Quaternion.identity;
		m_MainView.transform.localPosition = Vector3.zero;

        // Setup the render texture
        SetupRenderTex();

        // Setup the render camera
        SetupDUICamera();
    }
	
    public void AttatchRenderTexture(Material _sharedScreenMat)
    {
        // Set the render text onto the material of the screen
        _sharedScreenMat.SetTexture("_MainTex", m_RenderTex); 
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

    private void SetupDUICamera()
    {
		UIPanel panel = m_MainView.GetComponent<UIPanel>();
		
        // Create the camera game object
		m_DUICamera = new GameObject();
        m_DUICamera.name = "RenderCamera";
        m_DUICamera.transform.parent = transform;
		m_DUICamera.transform.localPosition = new Vector3(0.0f, 0.0f, -1.0f);
        m_DUICamera.transform.localRotation = Quaternion.identity;
        m_DUICamera.layer = gameObject.layer;

        // Get the render camera and set its target as the render texture
        Camera camera = m_DUICamera.AddComponent<Camera>();
        camera.cullingMask = 1 << gameObject.layer;
		camera.clearFlags = CameraClearFlags.Color;
        camera.backgroundColor = Color.clear;
		camera.fieldOfView = 60.0f;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 2.0f;
        camera.targetTexture = m_RenderTex;
		camera.enabled = false;

		// Add the NGUI camera component
		UICamera uiCam = m_DUICamera.AddComponent<UICamera>();
		uiCam.IsDUICamera = true;
		uiCam.DiegeticViewDimensions = new Vector2(panel.width, panel.height);
    }

	public Vector3 DUICameraViewportPos(Vector2 _screenTexCoord)
	{
		UIPanel panel = m_MainView.GetComponent<UIPanel>();
		
		Vector3 offset = new Vector3(_screenTexCoord.x * panel.width,
		                             _screenTexCoord.y * panel.height, 0.0f);
		
		offset = transform.rotation * offset;
		Vector3 rayOrigin = transform.position + offset + transform.forward * -1.0f;
		
		return(rayOrigin);
	}
	
	private void OnSubviewChange(uint _iActiveSubview)
	{
		if(SubviewChanged != null)
			SubviewChanged(_iActiveSubview);
	}
}
