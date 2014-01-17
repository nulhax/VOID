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


public class CEntryTrigger : MonoBehaviour 
{
	// Member Types


	// Member Delegates & Events
	
		
	// Member Fields
	
	
	// Member Properties
	
	
	// Member Methods
	[AServerOnly]
	private void OnTriggerEnter(Collider _Other)
	{
		if(_Other.rigidbody != null && _Other.rigidbody.detectCollisions != false && CNetwork.IsServer)
		{
			CActorBoardable dynamicActor = _Other.rigidbody.GetComponent<CActorBoardable>();
			if(dynamicActor != null)
			{			
				dynamicActor.BoardingState = CActorBoardable.EBoardingState.Onboard;
			}
		}
	}
}
