//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CBridgePilotingSystem.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CBridgePilotingSystem : MonoBehaviour 
{
    // Member Types


    // Member Delegates & Events

	
	// Member Fields
	private GameObject m_Cockpit = null;
	
    // Member Properties


    // Member Methods
	public void Start()
	{	
		if(CNetwork.IsServer)
		{
			ServerCreateControlCockpit();
		}
		
		// Get the console script from the children
		CBridgeCockpit cockpit = GetComponentInChildren<CBridgeCockpit>();
		
		// Store the room control console game object
 		m_Cockpit = cockpit.gameObject;
	}
	
	private void ServerCreateControlCockpit()
	{
		Transform cockpitTransform = transform.FindChild("Cockpit");

		CGame.ENetworkRegisteredPrefab eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.Cockpit;
		GameObject newConsoleObject = CNetwork.Factory.CreateObject(eRegisteredPrefab);
	
		newConsoleObject.transform.position = cockpitTransform.position;
		newConsoleObject.transform.rotation = cockpitTransform.rotation;
		newConsoleObject.transform.parent = transform;	
		
		newConsoleObject.GetComponent<CNetworkView>().SyncParent();
		newConsoleObject.GetComponent<CNetworkView>().SyncTransformPosition();
		newConsoleObject.GetComponent<CNetworkView>().SyncTransformRotation();
	}
}

