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
	public CHUDVisor m_Visor = null;
	public CHUDLocator m_ReticleOuter = null;
	
	private Transform m_CachedReticlePanel = null;

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
		// Set the locator on the players aiming position
		m_ReticleOuter.Target = CGamePlayers.SelfActorHead.transform.FindChild("Aim");

		if(CGameCameras.IsOculusRiftActive)
		{
			// Cache the reticle panel
			m_CachedReticlePanel = m_ReticleOuter.transform.parent;
			m_CachedReticlePanel.GetComponent<CHUDPanel>().ContinuouslyUpdateScale = true;
		}
	}

	public void Update()
	{
		if(CGameCameras.IsOculusRiftActive)
		{
			float fovCoefficient = CGameCameras.MainCameraLeft.camera.fieldOfView / CGameCameras.HUDCameraLeft.camera.fieldOfView;

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
	}
}
