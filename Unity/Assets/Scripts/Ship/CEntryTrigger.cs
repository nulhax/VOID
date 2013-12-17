﻿//  Auckland
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


public class CEntryTrigger : MonoBehaviour 
{
	// Member Types


	// Member Delegates & Events
	
		
	// Member Fields
	
	
	// Member Properties
	
	
	// Member Methods
	private void OnTriggerEnter(Collider _Other)
	{
		if(_Other.rigidbody != null && CNetwork.IsServer)
		{
			CDynamicActor dynamicActor = _Other.rigidbody.GetComponent<CDynamicActor>();
			if(dynamicActor != null)
			{
				// Transfer to dynamic actor to ship space
				dynamicActor.TransferActorToShipSpace();
								
				// Set the boarding state
				dynamicActor.BoardingState = CDynamicActor.EBoardingState.Boarding;
			}
		}
	}
}
