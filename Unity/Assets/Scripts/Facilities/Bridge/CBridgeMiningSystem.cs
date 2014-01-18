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
using System.Linq;


/* Implementation */


public class CBridgeMiningSystem : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	private List<GameObject> m_MiningTurretCockpits = new List<GameObject>();
	
	// Member Properties
	
	
	// Member Methods
	public void Start()
	{	
		if(CNetwork.IsServer)
		{
			ServerCreateMiningTurretCockpits();
		}

		// Add the cockpits to a list
		foreach(CTurretCockpitBehaviour turretCockpit in gameObject.GetComponentsInChildren<CTurretCockpitBehaviour>())
		{
			m_MiningTurretCockpits.Add(turretCockpit.gameObject);
		}

		// DEBUG. Create turrets on all nodes
		if(CNetwork.IsServer)
		{
			foreach(GameObject turretNode in gameObject.GetComponent<CFacilityTurrets>().GetAllFreeTurretNodes())
			{
				gameObject.GetComponent<CFacilityTurrets>().CreateTurret(turretNode.GetComponent<CTurretNodeInterface>().TurretNodeId, CTurretInterface.ETurretType.Laser);
			}
		}
	}
	
	private void ServerCreateMiningTurretCockpits()
	{
		/*
		List<Transform> turretCockpitNodes = new List<Transform>();
		for(int i = 0; i < transform.childCount; ++i)
		{
			if(transform.GetChild(i).name == "TurretCockpitNode")
			{
				turretCockpitNodes.Add(transform.GetChild(i));
			}
		}

		foreach(Transform t in turretCockpitNodes)
		{
			CGameResourceLoader.ENetworkRegisteredPrefab eRegisteredPrefab = CGameResourceLoader.ENetworkRegisteredPrefab.TurretCockpit;
			GameObject newCockpitObject = CNetwork.Factory.CreateObject(eRegisteredPrefab);
				
			newCockpitObject.transform.position = t.position;
			newCockpitObject.transform.rotation = t.rotation;
			newCockpitObject.transform.parent = transform;	
				
			newCockpitObject.GetComponent<CNetworkView>().SyncParent();
			newCockpitObject.GetComponent<CNetworkView>().SyncTransformPosition();
			newCockpitObject.GetComponent<CNetworkView>().SyncTransformRotation();
		}
		*/
	}
}

