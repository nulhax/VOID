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
	CFacilityOnboardActors m_CachedOnboardActors = null;

	
	// Member Properties
	
	
	// Member Methods
	private void Awake()
	{
		m_CachedOnboardActors = transform.parent.GetComponent<CFacilityOnboardActors>();
	}


	private void OnTriggerEnter(Collider _Other)
	{
		Rigidbody rb = _Other.rigidbody;

		if(rb == null)
			rb = CUtility.FindInParents<Rigidbody>(_Other.gameObject);

		if(rb != null)
		{
			m_CachedOnboardActors.OnActorEnteredFacilityTrigger(rb.gameObject);
		}
	}


	private void OnTriggerExit(Collider _Other)
	{
		Rigidbody rb = _Other.rigidbody;
		
		if(rb == null)
			rb = CUtility.FindInParents<Rigidbody>(_Other.gameObject);
		
		if(rb != null)
		{
			m_CachedOnboardActors.OnActorExitedFacilityTrigger(rb.gameObject);
		}
	}
}
