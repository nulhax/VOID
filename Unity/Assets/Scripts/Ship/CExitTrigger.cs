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


public class CExitTrigger : MonoBehaviour 
{
	// Member Types


	// Member Delegates & Events
	
		
	// Member Fields
	
	
	// Member Properties
	
	
	// Member Methods
	[AServerMethod]
	private void OnTriggerExit(Collider _Other)
	{
		if(_Other.rigidbody != null && _Other.rigidbody.detectCollisions != false && CNetwork.IsServer)
		{
			CActorBoardable dynamicActor = _Other.rigidbody.GetComponent<CActorBoardable>();
			if(dynamicActor != null)
			{
				// Ensure the actor is not onboard any other facility before disembarking
				if(!CGame.Ship.GetComponent<CShipOnboardActors>().IsActorOnboardShip(dynamicActor.gameObject))
				{
					// Set the disembarking state
					dynamicActor.BoardingState = CActorBoardable.EBoardingState.Offboard;
				}
			}
		}
	}
}
