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

    private CNetworkVar<bool> m_GravityEnabled = null;
	
	private Vector3 m_FacilityGravityAcceleration = new Vector3(0.0f, -9.8f, 0.0f);


	// Member Properties

	public Vector3 FacilityGravityAcceleration
	{
		get { return (m_FacilityGravityAcceleration); }
	}

    public bool IsGravityEnabled
    {
        get { return (m_GravityEnabled.Get()); }
    }
	
	
	// Member Methods
	
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_GravityEnabled = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, true);
	}
	
	void OnNetworkVarSync(INetworkVar _SyncedVar)
	{
		
	}

	public void Start()
	{
		// Register the actors entering/exiting the trigger zone
		GetComponent<CFacilityOnboardActors>().EventActorEnteredFacility += ActorEnteredGravityTrigger;
		GetComponent<CFacilityOnboardActors>().EventActorExitedFacility += ActorExitedGravityTrigger;

		// Register when facility power events
		GetComponent<CFacilityPower>().EventFacilityPowerDeactivated += DisableGravity;
		GetComponent<CFacilityPower>().EventFacilityPowerActivated += EnableGravity;
	}

	[AServerOnly]
	private void ActorEnteredGravityTrigger(GameObject _Facility, GameObject _Actor)
	{
		CActorGravity ag = _Actor.GetComponent<CActorGravity>();
		if(ag != null)
		{
			ag.ActorEnteredGravityTrigger(gameObject);
		}
	}

	[AServerOnly]
	private void ActorExitedGravityTrigger(GameObject _Facility, GameObject _Actor)
	{
		CActorGravity ag = _Actor.GetComponent<CActorGravity>();
		if(ag != null)
		{
			ag.ActorExitedGravityTrigger(gameObject);
		}
	}

	private void DisableGravity(GameObject _Facility)
	{
		if(CNetwork.IsServer)
		{
			m_GravityEnabled.Value = false;

			// Get all of the actors inside and disable their gravity
			CFacilityOnboardActors foa = GetComponent<CFacilityOnboardActors>();
			foreach(GameObject actor in foa.ActorsOnboard)
			{
				ActorExitedGravityTrigger(gameObject, actor);
			}
		}
	}

	private void EnableGravity(GameObject _Facility)
	{
		if(CNetwork.IsServer)
		{
			m_GravityEnabled.Value = true;

			// Get all of the actors inside and disable their gravity
			CFacilityOnboardActors foa = GetComponent<CFacilityOnboardActors>();
			foreach(GameObject actor in foa.ActorsOnboard)
			{
				ActorEnteredGravityTrigger(gameObject, actor);
			}
		}
	}
}
