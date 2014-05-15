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
	private CFacilityOnboardActors m_cFacilityOnboardActors = null;

	
	// Member Properties

	
	// Member Methods
	public void SetParentFacility(GameObject _Facility)
	{
		m_cFacilityOnboardActors = _Facility.GetComponent<CFacilityOnboardActors>();
	}


    [AServerOnly]
	void OnTriggerEnter(Collider _cOther)
	{
        if(!CNetwork.IsServer)
            return;

		Rigidbody rigidBody = _cOther.rigidbody;

        // Find rigid body in parnet
        if (rigidBody == null)
        {
            rigidBody = CUtility.FindInParents<Rigidbody>(_cOther.gameObject);
        }

        // Notify facility that a actor entered
		if(rigidBody != null)
		{
			m_cFacilityOnboardActors.OnActorEnteredFacilityTrigger(rigidBody.gameObject);
		}
	}


    [AServerOnly]
	void OnTriggerExit(Collider _cOther)
	{
        if (!CNetwork.IsServer)
            return;

		Rigidbody rigidBody = _cOther.rigidbody;

        // Find rigid body in parent
        if (rigidBody == null)
        {
            rigidBody = CUtility.FindInParents<Rigidbody>(_cOther.gameObject);
        }
		
        // Notify facility that a actor left
		if(rigidBody != null)
		{
			m_cFacilityOnboardActors.OnActorExitedFacilityTrigger(rigidBody.gameObject);
		}
	}
}
