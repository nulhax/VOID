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
	
	private static CHUD3D s_Instance = null;

	private Transform m_CachedTransform = null;
	private Transform m_CachedPlayerHead = null;

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
		m_CachedTransform = transform;

		// Save transform of the head
        if (CGamePlayers.SelfActor != null)
        {
            m_CachedPlayerHead = CGamePlayers.SelfActor.GetComponent<CPlayerHead>().ActorHead.transform;
        }

		// Adjust the camera FOV if oculus rift is being used
		if(CGameCameras.IsOculusRiftActive)
		{
			m_HUDCamera.GetComponent<OVRCameraController>().SetVerticalFOV(70.0f);
		}
	}

	public void LateUpdate()
	{
        if (m_CachedPlayerHead != null)
        {
            // Update the 3D hud location
            m_CachedTransform.position = m_CachedPlayerHead.position;
            m_CachedTransform.rotation = m_CachedPlayerHead.rotation;
        } 
        else
        {
            if(CGamePlayers.SelfActor != null)
            {
                m_CachedPlayerHead = CGamePlayers.SelfActor.GetComponent<CPlayerHead>().ActorHead.transform;
            }
        }
	}
}
