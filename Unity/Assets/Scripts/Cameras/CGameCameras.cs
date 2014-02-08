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


public class CGameCameras : MonoBehaviour
{

// Member Types


// Member Delegates & Events

	
// Member Fields
	private static Camera s_ShipCamera = null;
	private static Camera s_GalaxyCamera = null;

	private static bool s_IsObserverOutside = false;
	private static CGameCameras s_Instance = null;

// Member Properties
	public static CGameCameras Instance
	{
		get
		{
			return(s_Instance);
		}
	}

	public static Camera GalaxyCamera
	{
		get
		{
			return(s_GalaxyCamera);
		}
	}

	public static Camera PlayersHeadCamera
	{
		get
		{
			if(!s_IsObserverOutside)
				return(s_ShipCamera);
			else
				return(s_GalaxyCamera);
		}
	}
	
// Member Methods
	public void Start()
	{	
		s_Instance = this;
		
		CNetwork.Connection.EventConnectionAccepted += new CNetworkConnection.OnConnect(OnConnect);
		CNetwork.Connection.EventDisconnect += new CNetworkConnection.OnDisconnect(OnDisconnect);
	}

	public void LateUpdate()
	{
		// Make sure the client has the most up to date information
		if(!CNetwork.Connection.IsDownloadingInitialGameData &&
		   CNetwork.Connection.IsConnected)
		{
			// Update the transforms of the cameras
			UpdateCameraTransforms();
		}
	}

	public static void SetPlayersViewPerspectiveToShip(Transform _PlayerHead)
	{
		s_IsObserverOutside = false;
		
		// Set the perspective of the ship camera
		s_ShipCamera.transform.parent = _PlayerHead;
		s_ShipCamera.transform.localPosition = Vector3.zero;
		s_ShipCamera.transform.localRotation = Quaternion.identity;
		
		// Unparent the galaxy camera
		s_GalaxyCamera.transform.parent = null;
	}
	
	public static void SetPlayersViewPerspectiveToGalaxy(Transform _PlayerHead)
	{
		s_IsObserverOutside = true;
		
		// Set the perspective of the galaxy camera
		s_GalaxyCamera.transform.parent = _PlayerHead;
		s_GalaxyCamera.transform.localPosition = Vector3.zero;
		s_GalaxyCamera.transform.localRotation = Quaternion.identity;
		
		// Unparent the ship camera
		s_ShipCamera.transform.parent = null;
	}

	public static void SetDefaultViewPerspective()
	{
		// Ensure player actor exists
		if (CGamePlayers.SelfActor != null)
		{
			SetPlayersViewPerspectiveToShip(CGamePlayers.SelfActor.GetComponent<CPlayerHead>().ActorHead.transform);
		}
	}
	
	private void UpdateCameraTransforms()
	{		
		if(!s_IsObserverOutside)
		{
			// Transfer the galaxy camera based off the ship camera
			CGameShips.ShipGalaxySimulator.TransferFromSimulationToGalaxy(s_ShipCamera.transform.position, s_ShipCamera.transform.rotation, s_GalaxyCamera.transform);
		}
		else
		{
			// Transfer the ship camera based off the galaxy camera
			CGameShips.ShipGalaxySimulator.TransferFromGalaxyToSimulation(s_GalaxyCamera.transform.position, s_GalaxyCamera.transform.rotation, s_ShipCamera.transform);	
		}
	}

	private void OnConnect()
	{
		// Instantiate the galaxy camera (must be first!)
		s_GalaxyCamera = ((GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Cameras/GalaxyCamera"))).camera;
		
		// Instantiate the ship camera
		s_ShipCamera = ((GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Cameras/ShipCamera"))).camera;
	}

	private void OnDisconnect()
	{
		if(s_GalaxyCamera != null)
		{
			Destroy(s_GalaxyCamera.gameObject);
		}

		if(s_ShipCamera != null)
		{
			Destroy(s_ShipCamera.gameObject);
		}
	}
};
