//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CGameHud.cs
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
	
	
	public GameObject m_cHud2d = null;   
	
	
	CHUD3D m_cHud3d = null;
	CHUDVisor m_cHudVisor = null;
	
	static CGameHUD s_cInstance = null;
	static bool s_OnGUIEnabled = false;

	// Member Properties


	public static CGameHUD Instance
	{
		get { return(s_cInstance); }
	}


	public static CHUD3D Hud3D
	{
		get { return(s_cInstance.m_cHud3d); }
	}


    public static CHud2dInterface Hud2dInterface
    {
        get { return (s_cInstance.m_cHud2d.GetComponent<CHud2dInterface>()); }
    }


	public static CHUDVisor Visor
	{
		get { return(s_cInstance.m_cHudVisor); }
	}


	public static bool IsOnGUIEnabled
	{
		get { return(s_OnGUIEnabled); }
	}


	// Member Methods


	public void Awake()
	{	
		s_cInstance = this;

        CNetwork.Connection.EventConnectionAccepted += OnEventConnectionConnect;
        CNetwork.Connection.EventDisconnect += OnEventConnectionDisconnect;
	}


    public void Start()
    {
        m_cHud2d = GameObject.Instantiate(m_cHud2d) as GameObject;
    }


	public void Update()
	{
		if(Input.GetKeyDown(KeyCode.F2))
		{
			s_OnGUIEnabled = !s_OnGUIEnabled;
		}
	}


	public static void SetHUDState(bool _State)
	{
		s_cInstance.m_cHud3d.gameObject.SetActive(_State);
	}


	public static void SetupHUD()
	{
		// Instantiate the 3D HUD
		if (CGameCameras.IsOculusRiftActive)
		{
			s_cInstance.m_cHud3d = ((GameObject)GameObject.Instantiate(Resources.Load("Prefabs/User Interface/HUD/HUD3DOVR"))).GetComponent<CHUD3D>();
		}
		else
		{
			s_cInstance.m_cHud3d = ((GameObject)GameObject.Instantiate(Resources.Load("Prefabs/User Interface/HUD/HUD3D"))).GetComponent<CHUD3D>();
		}

		// Hold onto the visor
		s_cInstance.m_cHudVisor = CGamePlayers.SelfActorHead.transform.GetComponentInChildren<CHUDVisor>();

		// Need to offset for oculus rift
		if (!CGameCameras.IsOculusRiftActive)
		{
			s_cInstance.m_cHudVisor.transform.localPosition = Vector3.zero;
		}
		else
		{
			s_cInstance.m_cHudVisor.transform.localPosition = Vector3.forward * -0.05f;
		}
	}


    void OnEventConnectionConnect()
    {

    }


    void OnEventConnectionDisconnect()
    {
        if (m_cHud3d != null)
        {
            Destroy(s_cInstance.m_cHud3d);
            m_cHudVisor = null;
        }
    }
};
