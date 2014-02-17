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
	public UICamera m_HUDCamera = null;

	public UISprite m_VisorOverlay = null;
	public UISprite m_Reticle = null;

	private static CHUDRoot s_Instance = null;

	// Member Properties
	public static UICamera HUDCamera 
	{ 
		get { return(s_Instance.m_HUDCamera); } 
	}

	public static UISprite VisorOverlay 
	{ 
		get { return(s_Instance.m_VisorOverlay); } 
	}
	
	// Member Methods
	public void Awake()
	{
		s_Instance = this;
	}
	
	public void Start()
	{

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
