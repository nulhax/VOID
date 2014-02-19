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


public class CHUDRoot : MonoBehaviour
{
	// Member Types

	
	// Member Delegates & Events
	
	
	// Member Fields
	public GameObject m_HUDCamera2D = null;
	public GameObject m_HUDCamera3D = null;

	public CHUDVisor m_Visor = null;
	public UISprite m_Reticle = null;

	private UICamera m_CachedHUDCamerera2D = null;
	private UICamera m_CachedHUDCamerera3D = null;

	private static CHUDRoot s_Instance = null;

	// Member Properties
	public static UICamera HUDCamerera2D 
	{ 
		get { return(s_Instance.m_CachedHUDCamerera2D); } 
	}

	public static UICamera HUDCamerera3D 
	{ 
		get { return(s_Instance.m_CachedHUDCamerera2D); } 
	}

	public static CHUDVisor Visor 
	{ 
		get { return(s_Instance.m_Visor); } 
	}
	
	// Member Methods
	public void Awake()
	{
		s_Instance = this;
	}
	
	public void Start()
	{
		m_CachedHUDCamerera2D = m_HUDCamera2D.GetComponent<UICamera>();
		m_CachedHUDCamerera3D = m_HUDCamera2D.GetComponent<UICamera>();	
	}

	public static void UpdateReticleTarget(Transform _Target)
	{
		// Set the target for the reticle
		//s_Instance.m_Reticle.GetComponent<UIFollowTarget>().target = _Target;
	}

	public static void UpdateHUDGameCamera(Camera _PlayerHeadCamera)
	{
		// Set each of the follow target game cameras to the current camera on the players head
		foreach(UIFollowTarget ft in s_Instance.GetComponentsInChildren<UIFollowTarget>())
		{
			ft.gameCamera = _PlayerHeadCamera;
		}
	}
}
