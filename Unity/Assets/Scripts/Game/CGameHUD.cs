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


public class CGameHUD : MonoBehaviour
{
	
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	private CHUD3D m_HUD3D = null;
	private CHUDVisor m_Visor = null;

	private static CGameHUD s_Instance = null;
	
	
	// Member Properties
	public static CGameHUD Instance
	{
		get { return(s_Instance); }
	}

	public static CHUD3D HUD3D
	{
		get { return(s_Instance.m_HUD3D); }
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
	
	public static void SetupHUD()
	{
		// Instantiate the 3D HUD
		if(CGameCameras.IsOculusRiftActive)
		{
			s_Instance.m_HUD3D = ((GameObject)GameObject.Instantiate(Resources.Load("Prefabs/User Interface/HUD/HUD3DOVR"))).GetComponent<CHUD3D>();
		}
		else
		{
			s_Instance.m_HUD3D = ((GameObject)GameObject.Instantiate(Resources.Load("Prefabs/User Interface/HUD/HUD3D"))).GetComponent<CHUD3D>();
		}

		// Hold onto the visor
		s_Instance.m_Visor = CGamePlayers.SelfActorHead.transform.GetComponentInChildren<CHUDVisor>();

		// Need to offset for oculus rift
		if(!CGameCameras.IsOculusRiftActive)
		{
			s_Instance.m_Visor.transform.localPosition = Vector3.zero;
		}
		else
		{
			s_Instance.m_Visor.transform.localPosition = Vector3.forward * -0.05f;
		}
	}
};
