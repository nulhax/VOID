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
	[AServerOnly]
	private void OnTriggerExit(Collider _Other)
	{
		if(_Other.rigidbody != null && CNetwork.IsServer)
		{
			GameObject actor = _Other.rigidbody.gameObject;
			CActorBoardable boardableActor = actor.GetComponent<CActorBoardable>();
			if(boardableActor != null)
			{
				// Ensure the actor is not onboard any other facility before disembarking
				bool isWithinOtherFacility = false;
				if(actor.GetComponent<CActorLocator>() != null)
					isWithinOtherFacility = _Other.rigidbody.GetComponent<CActorLocator>().CurrentFacility != null;
				else
					isWithinOtherFacility = CGameShips.Ship.GetComponent<CShipOnboardActors>().IsActorOnboardShip(actor);

				// If not onboard within another facility, disembark the actor
				if(!isWithinOtherFacility)
				{
					// Set the disembarking state
					boardableActor.DisembarkActor();
				}
			}
		}
	}
}
