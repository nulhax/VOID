//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CInteriorTrigger.cs
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


public class CInteriorTrigger : MonoBehaviour 
{

	// Member Types


	// Member Delegates & Events
	

	// Member Fields
	public CFacilityOnboardActors m_FacilityOnboardActors = null;

	
	// Member Properties

	
	// Member Methods
	public void SetParentFacility(GameObject _Facility)
	{
		m_FacilityOnboardActors = _Facility.GetComponent<CFacilityOnboardActors>();
	}


    [AServerOnly]
	private void OnTriggerEnter(Collider _Collider)
	{
		if(!CNetwork.IsServer)
            return;

		Rigidbody rigidBody = _Collider.rigidbody;

        // Find rigid body in parnet
        if (rigidBody == null)
        {
            rigidBody = CUtility.FindInParents<Rigidbody>(_Collider.gameObject);
        }

        // Notify facility that a actor entered
		if(rigidBody != null)
		{
			CActorLocator actorLocator = rigidBody.GetComponent<CActorLocator>();
			if (actorLocator == null)
				return;
		
			NotifyActorEnteredTrigger(actorLocator);
		}
	}


    [AServerOnly]
	private void OnTriggerExit(Collider _Collider)
	{
		if (!CNetwork.IsServer)
            return;

		Rigidbody rigidBody = _Collider.rigidbody;

        // Find rigid body in parent
        if (rigidBody == null)
        {
            rigidBody = CUtility.FindInParents<Rigidbody>(_Collider.gameObject);
        }
		
        // Notify facility that a actor left
		if(rigidBody != null)
		{
			CActorLocator actorLocator = rigidBody.GetComponent<CActorLocator>();
			if (actorLocator == null)
				return;

			m_FacilityOnboardActors.OnActorExitedFacilityTrigger(actorLocator, collider);
		}
	}

	[AServerOnly]
	public void NotifyActorEnteredTrigger(CActorLocator _ActorLocator)
	{
		m_FacilityOnboardActors.OnActorEnteredFacilityTrigger(_ActorLocator, collider);
	}
}
