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

	private bool m_IsObserverOutside = false;
	
// Member Properties

	
// Member Methods
	public void LateUpdate()
	{
		if(!CNetwork.Connection.IsConnected ||
		    CNetwork.Connection.IsDownloadingInitialGameData)
		{
			return;
		}

		// Update the transforms of the cameras
		UpdateCameraTransforms();
	}

	public void Start()
	{	
		CNetwork.Connection.EventConnectionAccepted += new CNetworkConnection.OnConnect(OnConnect);
		CNetwork.Connection.EventDisconnect += new CNetworkConnection.OnDisconnect(OnDisconnect);
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

	public void SetDefaultViewPerspective()
	{
		// Ensure player actor exists
		if (CGamePlayers.SelfActor != null)
		{
			SetShipViewPerspective(CGamePlayers.SelfActor.GetComponent<CPlayerHead>().ActorHead.transform);
		}
	}
	
	private void UpdateCameraTransforms()
	{		
		if(!m_IsObserverOutside)
		{
			// Transfer the galaxy camera based off the ship camera
			CGameShips.ShipGalaxySimulator.TransferFromSimulationToGalaxy(m_ShipCamera.transform.position, m_ShipCamera.transform.rotation, m_GalaxyCamera.transform);
		}
		else
		{
			// Transfer the ship camera based off the galaxy camera
			CGameShips.ShipGalaxySimulator.TransferFromGalaxyToSimulation(m_GalaxyCamera.transform.position, m_GalaxyCamera.transform.rotation, m_ShipCamera.transform);	
		}
	}

	private void OnConnect()
	{
		// Instantiate the galaxy camera (must be first!)
		m_GalaxyCamera = ((GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Cameras/GalaxyCamera"))).camera;
		
		// Instantiate the ship camera
		m_ShipCamera = ((GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Cameras/ShipCamera"))).camera;
	}

	private void OnDisconnect()
	{
		if(m_GalaxyCamera != null)
		{
			Destroy(m_GalaxyCamera.gameObject);
		}

		if(m_ShipCamera != null)
		{
			Destroy(m_ShipCamera.gameObject);
		}
	}
};
