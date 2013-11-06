﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;

public class CDUI : MonoBehaviour
{
    // Member Fields
    private EQuality m_Quality = EQuality.INVALID;
	
	private GameObject m_Console = null;
    private GameObject m_RenderCamera = null;
    private RenderTexture m_RenderTex = null; 
	
	private GameObject m_MainView = null;
	
	
	private Dictionary<uint, GameObject> m_SubViews = new Dictionary<uint, GameObject>();
	
	private uint m_ViewIdCount = 0;
	
    // Member Properties
	public Camera RenderCamera 
	{ 
		get
		{ 
			return(m_RenderCamera.camera); 
		} 
	}

    // Member Methods
    private void Update()
	{
        // Update the render texture
        m_RenderTex.DiscardContents(true, true);
        RenderTexture.active = m_RenderTex;
		
		// Render using the render camrea
        RenderCamera.Render();

        RenderTexture.active = null;
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
        duiMainView.SetupSubView(duiSubView);
		
		// Add to the dictionaries
        m_SubViews[duiSubView.ViewID] = subView;

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
	
    public void AttatchRenderTexture(Material _sharedScreenMat)
    {
        // Set the render text onto the material of the screen
        _sharedScreenMat.SetTexture("_MainTex", m_RenderTex); 
    }
	
	private void SetupMainView(ELayoutStyle _Layout, Vector2 _Dimensions)
    {
		// Create the DUI game object
        m_MainView = new GameObject();
        m_MainView.transform.parent = transform;
        m_MainView.name = "MainView";
        m_MainView.layer = gameObject.layer;
        m_MainView.transform.localRotation = Quaternion.identity;
		m_MainView.transform.localPosition = Vector3.zero;
		
        // Add the DUI component
        CDUIMainView duiMainView = m_MainView.AddComponent<CDUIMainView>();

        // Initialise the DUI Component
        duiMainView.Initialise(_Layout, _Dimensions);
		duiMainView.ViewID = ++m_ViewIdCount;
    }
	
    private void SetupRenderTex()
    {
		CDUIMainView duiMainView = m_MainView.GetComponent<CDUIMainView>();
		
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

        int width = (int)(duiMainView.Dimensions.x * ppm);
        int height = (int)(duiMainView.Dimensions.y * ppm);

        // Create a new render texture
        m_RenderTex = new RenderTexture(width, height, 16);
        m_RenderTex.name = name + " RT";
        m_RenderTex.Create();
    }

    private void SetupRenderCamera()
    {
		CDUIMainView duiMainView = m_MainView.GetComponent<CDUIMainView>();
		
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
		camera.clearFlags = CameraClearFlags.SolidColor;
        camera.orthographic = true;
        camera.backgroundColor = Color.black;
        camera.nearClipPlane = 0.0f;
        camera.farClipPlane = 2.0f;
        camera.targetTexture = m_RenderTex;
        camera.orthographicSize = duiMainView.Dimensions.y * 0.5f;
    }
	
	public GameObject FindDUIElementCollisions(RaycastHit _rh)
    {
		CDUIView duiMainView = m_MainView.GetComponent<CDUIMainView>();
		
		Vector3 offset = new Vector3(_rh.textureCoord.x * duiMainView.Dimensions.x - duiMainView.Dimensions.x * 0.5f,
                                     _rh.textureCoord.y * duiMainView.Dimensions.y - duiMainView.Dimensions.y * 0.5f, 0.0f);

        offset = transform.rotation * offset;
        Vector3 rayOrigin = transform.position + offset + transform.forward * -1.0f;

        Ray ray = new Ray(rayOrigin, transform.forward);
        RaycastHit hit;
        float rayLength = 2.0f;
		
		GameObject element = null;
		
        if (Physics.Raycast(ray, out hit, rayLength, 1 << LayerMask.NameToLayer("DUI")))
        {
            element = hit.transform.parent.gameObject;
			
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * rayLength, Color.green, 0.5f);
        }
        else
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * rayLength, Color.red, 0.5f);
        }
		
		return(element);
	}
}
