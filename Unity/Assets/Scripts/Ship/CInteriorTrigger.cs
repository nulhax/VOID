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
        if (!CNetwork.IsServer)
            return;

		Rigidbody cRigidbody = _cOther.rigidbody;

        // Find rigid body in parnet
        if (cRigidbody == null)
        {
            cRigidbody = CUtility.FindInParents<Rigidbody>(_cOther.gameObject);
        }

        // Notify facility that a actor entered
		if(cRigidbody != null)
		{
			m_cFacilityOnboardActors.OnActorEnteredFacilityTrigger(cRigidbody.gameObject);
		}
	}


    [AServerOnly]
	void OnTriggerExit(Collider _cOther)
	{
        if (!CNetwork.IsServer)
            return;

		Rigidbody cRigidBody = _cOther.rigidbody;

        // Find rigid body in parent
        if (cRigidBody == null)
        {
            cRigidBody = CUtility.FindInParents<Rigidbody>(_cOther.gameObject);
        }
		
        // Notify facility that a actor left
		if(cRigidBody != null)
		{
			m_cFacilityOnboardActors.OnActorExitedFacilityTrigger(cRigidBody.gameObject);
		}
	}
}
