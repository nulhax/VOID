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
	
	
	// Member Properties
	
	
	// Member Methods
	[AServerOnly]
	private void OnTriggerEnter(Collider _Other)
	{
		if(!CNetwork.IsServer)
			return;

		if(_Other.rigidbody != null)
		{
			transform.parent.GetComponent<CFacilityOnboardActors>().OnActorEnteredFacilityTrigger(_Other.rigidbody.gameObject);
		}
	}

	[AServerOnly]
	private void OnTriggerExit(Collider _Other)
	{
		if(!CNetwork.IsServer)
			return;

		if(_Other.rigidbody != null)
		{
			transform.parent.GetComponent<CFacilityOnboardActors>().OnActorExitedFacilityTrigger(_Other.rigidbody.gameObject);
		}
	}
}
