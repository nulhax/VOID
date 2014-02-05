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
	
	private Vector3 m_FacilityGravityAcceleration = new Vector3(0.0f, -9.8f, 0.0f);
	
	// Member Properties
	public Vector3 FacilityGravityAcceleration
	{
		get { return (m_FacilityGravityAcceleration); }
	}

    public bool IsGravityEnabled
    {
        get { return (m_cGravityEnabled.Get()); }
    }
	
	
	// Member Methods
	public void Start()
	{
		// Register the actors entering/exiting the trigger zone
		GetComponent<CFacilityOnboardActors>().EventActorEnteredFacility += new CFacilityOnboardActors.FacilityActorEnterExit(ActorEnteredGravityTrigger);
		GetComponent<CFacilityOnboardActors>().EventActorExitedFacility += new CFacilityOnboardActors.FacilityActorEnterExit(ActorExitedGravityTrigger);
	}

	[AServerOnly]
	private void ActorEnteredGravityTrigger(GameObject _Facility, GameObject _Actor)
	{
		CActorGravity ag = _Actor.GetComponent<CActorGravity>();
		if(ag != null)
			ag.ActorEnteredGravityTrigger(gameObject);
	}

	[AServerOnly]
	private void ActorExitedGravityTrigger(GameObject _Facility, GameObject _Actor)
	{
		CActorGravity ag = _Actor.GetComponent<CActorGravity>();
		if(ag != null)
			ag.ActorExitedGravityTrigger(gameObject);
	}

    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
		m_cGravityEnabled = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, true);
    }

    void OnNetworkVarSync(INetworkVar _SyncedVar)
    {

    }
}
