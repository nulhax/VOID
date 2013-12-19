//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CCompoundCameraSystem.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/* Implementation */


public class CCompositeCameraSystem : MonoBehaviour
{

// Member Types


// Member Delegates & Events

	
// Member Fields
	private Camera m_ShipCamera = null;
	private Camera m_GalaxyCamera = null;
	
	private Camera m_ShipDepthCamera = null;
	private Camera m_GalaxyDepthCamera = null;
	
	private RenderTexture m_ShipRenderTex = null;
	private RenderTexture m_GalaxyRenderTex = null;
	
	private RenderTexture m_ShipDepthTex = null;
	private RenderTexture m_GalaxyDepthTex = null;
	
	private Material m_Material = null;
	private Shader m_DepthShaderReplacement = null;
	
	private bool m_IsObserverOutside = false;
	
// Member Properties
	
	Material material 
	{
	    get 
		{
	        if (m_Material == null)
			{
	            m_Material = new Material(Shader.Find("Hidden/CompositeCameraShader"));
			}
			
	        m_Material.hideFlags = HideFlags.HideAndDontSave;
	        return(m_Material);
	    } 
	}
	
// Member Methods
	public void Start()
	{	
		// Create the camera objects
		m_ShipCamera = ((GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Cameras/ShipCamera"))).camera;
		m_GalaxyCamera = ((GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Cameras/GalaxyCamera"))).camera;
		
		// Create the ship depth camera
		GameObject shipDepthCamera = new GameObject("ShipDepthCamera");
		shipDepthCamera.transform.parent = m_ShipCamera.transform;
		shipDepthCamera.transform.localPosition = Vector3.zero;
		shipDepthCamera.transform.localRotation = Quaternion.identity;
		m_ShipDepthCamera = shipDepthCamera.AddComponent<Camera>();
		m_ShipDepthCamera.CopyFrom(m_ShipCamera);
        m_ShipDepthCamera.backgroundColor = Color.clear;
		m_ShipDepthCamera.nearClipPlane = 0.3f;
		m_ShipDepthCamera.farClipPlane = 5000.0f;
		m_ShipDepthCamera.enabled = false;
		
		// Create the galaxy depth camera
		GameObject galaxyDepthCamera = new GameObject("GalaxyDepthCamera");
		galaxyDepthCamera.transform.parent = m_GalaxyCamera.transform;
		galaxyDepthCamera.transform.localPosition = Vector3.zero;
		galaxyDepthCamera.transform.localRotation = Quaternion.identity;
		m_GalaxyDepthCamera = galaxyDepthCamera.AddComponent<Camera>();
		m_GalaxyDepthCamera.CopyFrom(m_GalaxyCamera);
        m_GalaxyDepthCamera.backgroundColor = Color.clear;
		m_GalaxyDepthCamera.nearClipPlane = 0.3f;
		m_GalaxyDepthCamera.farClipPlane = 5000.0f;
		m_GalaxyDepthCamera.enabled = false;
		
		// Find the depth shader replacement shader
		m_DepthShaderReplacement = Shader.Find("Hidden/Camera-DepthTexture");	
	}
	
	public void SetShipViewPerspective(Transform _ShipPerspective)
	{
		m_IsObserverOutside = false;
		
		// Set the perspective of the ship camera
		m_ShipCamera.transform.parent = _ShipPerspective;
		m_ShipCamera.transform.localPosition = Vector3.zero;
		m_ShipCamera.transform.localRotation = Quaternion.identity;
		
		// Unparent the galaxy camera
		m_GalaxyCamera.transform.parent = null;
		
		// Destroy the galaxy observer/shiftable components
        Destroy(m_GalaxyCamera.gameObject.GetComponent<GalaxyObserver>());
        Destroy(m_GalaxyCamera.gameObject.GetComponent<GalaxyShiftable>());
	}
	
	public void SetGalaxyViewPerspective(Transform _GalaxyPerspective)
	{
		m_IsObserverOutside = true;
		
		// Set the perspective of the galaxy camera
		m_GalaxyCamera.transform.parent = _GalaxyPerspective;
		m_GalaxyCamera.transform.localPosition = Vector3.zero;
		m_GalaxyCamera.transform.localRotation = Quaternion.identity;
		
		// Unparent the ship camera
		m_ShipCamera.transform.parent = null;

		// Add the galaxy observer/shiftable components
        m_GalaxyCamera.gameObject.AddComponent<GalaxyObserver>();
        m_GalaxyCamera.gameObject.AddComponent<GalaxyShiftable>();
	}
	
	public void Update()
	{
		UpdateCameraTransforms();
	}
	
	private void UpdateCameraTransforms()
	{	
		Transform shipTransform = CGame.Ship.transform;
		Transform galaxyShipTransform = CGame.GalaxyShip.transform;
		
		if(!m_IsObserverOutside)
		{
			// Update the galaxy camera transform based off the ship camera relative to the ship
			m_GalaxyCamera.transform.position = galaxyShipTransform.rotation * (m_ShipCamera.transform.position - shipTransform.position) + galaxyShipTransform.position;
			m_GalaxyCamera.transform.rotation = galaxyShipTransform.rotation * m_ShipCamera.transform.rotation;
		}
		else
		{
			// Update the ship camera transform based off the galaxy camera relative to the galaxy ship
			m_ShipCamera.transform.position = Quaternion.Inverse(galaxyShipTransform.rotation) * (m_GalaxyCamera.transform.position - galaxyShipTransform.transform.position) + shipTransform.position;
			m_ShipCamera.transform.rotation = Quaternion.Inverse(galaxyShipTransform.rotation) * m_GalaxyCamera.transform.rotation;	
		}
	}
	
	private void OnPreRender() 
    {
		ReleaseTempTextures();

		// Create temporary textures
		m_ShipRenderTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
		m_GalaxyRenderTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
		m_ShipDepthTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
		m_GalaxyDepthTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
	 
		// Assign the depth textures to the depth cameras
		m_ShipDepthCamera.targetTexture = m_ShipDepthTex;
		m_GalaxyDepthCamera.targetTexture = m_GalaxyDepthTex;
		
		// Render the depth textures using the depth cameras
		m_ShipDepthCamera.RenderWithShader(m_DepthShaderReplacement, null);
		m_GalaxyDepthCamera.RenderWithShader(m_DepthShaderReplacement, "RenderType");
		
		// Assign the textures to the main cameras
		m_ShipCamera.targetTexture = m_ShipRenderTex;
		m_GalaxyCamera.targetTexture = m_GalaxyRenderTex;
		
		// Render the other cameras
	   	m_ShipCamera.Render();
	    m_GalaxyCamera.Render();
	}
	
	private void OnRenderImage(RenderTexture _Source, RenderTexture _Destination) 
	{
		material.SetTexture("_ForgroundTex", m_ShipRenderTex);
		material.SetTexture("_BackgroundTex", m_GalaxyRenderTex);
		material.SetTexture("_ForgroundDepth", m_ShipDepthTex);
		material.SetTexture("_BackgroundDepth", m_GalaxyDepthTex);
		
		if(!m_IsObserverOutside)
		{
			material.SetFloat("_ForgroundInBackground", 0.0f);
		}
		else
		{
			material.SetFloat("_ForgroundInBackground", 1.0f);
		}

	    Graphics.Blit(_Source, _Destination, material);
	}
	
	private void ReleaseTempTextures()
	{
		// Cleanup the depth textures
		if(m_ShipDepthTex) 
		{
		    RenderTexture.ReleaseTemporary(m_ShipDepthTex);
		    m_ShipDepthTex = null;
		}
		if(m_GalaxyDepthTex)
		{
			RenderTexture.ReleaseTemporary(m_GalaxyDepthTex);
		    m_GalaxyDepthTex = null;
		}
		if(m_ShipRenderTex)
		{
			RenderTexture.ReleaseTemporary(m_ShipRenderTex);
		    m_ShipRenderTex = null;
		}
		if(m_GalaxyRenderTex)
		{
			RenderTexture.ReleaseTemporary(m_GalaxyRenderTex);
		    m_GalaxyRenderTex = null;
		}
	}
};
