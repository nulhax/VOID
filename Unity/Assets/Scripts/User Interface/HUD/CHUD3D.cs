//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CHUDRoot.cs
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


public class CHUD3D : MonoBehaviour
{
	// Member Types
	

	// Member Delegates & Events
	
	
	// Member Fields
	public GameObject m_HUDCamera = null;
	public float m_HUDCameraFOV = 111.0f;
	public CHUDVisor m_Visor = null;
	public CHUDLocator m_ReticleOuter = null;
	
	private Transform m_CachedReticlePanel = null;

	private Transform m_CachedHUDCameraLeft = null;
	private Transform m_CachedHUDCameraRight = null;
		    
	private Transform m_CachedMainCameraLeft = null;
	private Transform m_CachedMainCameraRight = null;

	private static CHUD3D s_Instance = null;
	

	// Member Properties
	public static GameObject HUDCamera 
	{ 
		get { return(s_Instance.m_HUDCamera); } 
	}

	public static CHUDVisor Visor 
	{ 
		get { return(s_Instance.m_Visor); } 
	}
	
	public static CHUD3D Instance
	{
		get { return(s_Instance); }
	}
	
	// Member Methods
	public void Awake()
	{
		s_Instance = this;
	}

	public void Start()
	{
		// Adjust the camera FOV if oculus rift is being used
		if(CGameCameras.IsOculusRiftActive)
		{
			m_HUDCamera.GetComponent<OVRCameraController>().SetVerticalFOV(m_HUDCameraFOV);
		}

		// Set the locator on the players aiming position
		m_ReticleOuter.Target = CGamePlayers.SelfActorHead.transform.FindChild("Aim");

		if(CGameCameras.IsOculusRiftActive)
		{
			// Cache the reticle panel
			m_CachedReticlePanel = m_ReticleOuter.transform.parent;
			m_CachedReticlePanel.GetComponent<CHUDPanel>().ContinuouslyUpdateScale = true;

			// Cache the HUD camera
			m_CachedHUDCameraLeft = m_HUDCamera.transform.FindChild("CameraLeft").transform;
			m_CachedHUDCameraRight = m_HUDCamera.transform.FindChild("CameraRight").transform;

			// Cache the main camera
			m_CachedMainCameraLeft = CGameCameras.MainCamera.transform.FindChild("CameraLeft").transform;
			m_CachedMainCameraRight = CGameCameras.MainCamera.transform.FindChild("CameraRight").transform;
		}
	}

	public void Update()
	{
		if(CGameCameras.IsOculusRiftActive)
		{
			float fovCoefficient = m_CachedMainCameraLeft.camera.fieldOfView / m_CachedHUDCameraLeft.camera.fieldOfView;

			if(CGamePlayers.SelfActor.GetComponent<CPlayerInteractor>().TargetActorObject != null)
			{
				RaycastHit rayHit = CGamePlayers.SelfActor.GetComponent<CPlayerInteractor>().TargetRaycastHit;

				// Update the reticle panel z position to be the same distance to the current target
				if(rayHit.distance < CPlayerInteractor.s_InteractionRange)
				{
					m_CachedReticlePanel.localPosition = Vector3.forward * rayHit.distance * fovCoefficient;
				}
				else
				{
					m_CachedReticlePanel.localPosition = Vector3.forward * CPlayerInteractor.s_InteractionRange * fovCoefficient;
				}
			}
			else
			{
				m_CachedReticlePanel.localPosition = Vector3.forward * CPlayerInteractor.s_InteractionRange * fovCoefficient;
			}
		}
	}

	public void LateUpdate()
	{
		// Update the 3D hud location
		transform.position = CGameCameras.MainCamera.transform.position;
		transform.rotation = CGameCameras.MainCamera.transform.rotation;

		// Update the HUD cameras transform
		if(CGameCameras.IsOculusRiftActive)
		{
			m_CachedHUDCameraLeft.position = m_CachedMainCameraLeft.position;
			m_CachedHUDCameraLeft.rotation = m_CachedMainCameraLeft.rotation;
			m_CachedHUDCameraRight.position = m_CachedMainCameraRight.position;
			m_CachedHUDCameraRight.rotation = m_CachedMainCameraRight.rotation;
		}
	}
}
