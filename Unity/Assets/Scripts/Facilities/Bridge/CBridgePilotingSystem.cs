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
		if (CNetwork.IsServer)
		{
			m_Cockpit = GetComponent<CFacilityModules>().FindModulesByType(CModuleInterface.EType.PilotCockpit)[0];
		}

		//if(CNetwork.IsServer)
		//{
			//ServerCreateControlCockpit();
		//}
		
		// Get the console script from the children
		//CBridgeCockpit cockpit = GetComponentInChildren<CBridgeCockpit>();
		
		// Store the room control console game object
 		//m_Cockpit = cockpit.gameObject;
	}
	
	private void ServerCreateControlCockpit()
	{
		/*
		Transform cockpitTransform = transform.FindChild("PilotCockpitNode");

		CGameResourceLoader.ENetworkRegisteredPrefab eRegisteredPrefab = CGameResourceLoader.ENetworkRegisteredPrefab.BridgeCockpit;
		GameObject newCockpitObject = CNetwork.Factory.CreateObject(eRegisteredPrefab);
	
		newCockpitObject.transform.position = cockpitTransform.position;
		newCockpitObject.transform.rotation = cockpitTransform.rotation;
		newCockpitObject.transform.parent = transform;	
		
		newCockpitObject.GetComponent<CNetworkView>().SyncParent();
		newCockpitObject.GetComponent<CNetworkView>().SyncTransformPosition();
		newCockpitObject.GetComponent<CNetworkView>().SyncTransformRotation();
		*/
	}
}

