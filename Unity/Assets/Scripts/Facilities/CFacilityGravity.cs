//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFacilityGravity.cs
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


public class CFacilityGravity : CNetworkMonoBehaviour
{
	// Member Types


	// Member Delegates & Events

	
	// Member Fields

    private CNetworkVar<bool> m_cGravityEnabled = null;

	private List<GameObject> m_ActorsInsideGravityTrigger = new List<GameObject>();
	private Vector3 m_FacilityGravityAcceleration = new Vector3(0.0f, -9.8f, 0.0f);
	
	// Member Properties


    public bool IsGravityEnabled
    {
        get { return (m_cGravityEnabled.Get()); }
    }
	
	
	// Member Methods
	public void Start()
	{
		// Register the actors entering/exiting the trigger zone
		GetComponent<CFacilityOnboardActors>().ActorEnteredFacility += new CFacilityOnboardActors.FacilityActorEnterExit(ActorEnteredGravityTrigger);
		GetComponent<CFacilityOnboardActors>().ActorExitedFacility += new CFacilityOnboardActors.FacilityActorEnterExit(ActorExitedGravityTrigger);
	}
	
	public void Update()
	{
		if(CNetwork.IsServer)
		{
			// Remove actors that dont exist anymore
			m_ActorsInsideGravityTrigger.RemoveAll((item) => item == null);
			
			// Apply the gravity to the actor every frame (so we can modify it if we want later)
			foreach(GameObject actor in m_ActorsInsideGravityTrigger)
			{	
				actor.GetComponent<CActorGravity>().GravityAcceleration = m_FacilityGravityAcceleration;
			}
		}
	}

	[AServerOnly]
	private void ActorEnteredGravityTrigger(GameObject _Facility, GameObject _Actor)
	{
		// Only add to the list if there is a gravity component
		if(_Actor.GetComponent<CActorGravity>() == null)
			return;
		
		m_ActorsInsideGravityTrigger.Add(_Actor);
	}

	[AServerOnly]
	private void ActorExitedGravityTrigger(GameObject _Facility, GameObject _Actor)
	{
		if(!m_ActorsInsideGravityTrigger.Contains(_Actor))
			return;

		_Actor.GetComponent<CActorGravity>().GravityAcceleration = Vector3.zero;
			
		m_ActorsInsideGravityTrigger.Remove(_Actor);
	}

    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_cGravityEnabled = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, true);
    }

    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {

    }
}
