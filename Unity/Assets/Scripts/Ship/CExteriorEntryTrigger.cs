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


public class CExteriorEntryTrigger : MonoBehaviour 
{
	// Member Types


	// Member Delegates & Events
	
		
	// Member Fields
	
	
	// Member Properties
	
	
	// Member Methods
	private void OnTriggerEnter(Collider _Other)
	{
		if(_Other.rigidbody != null)
		{
			CDynamicActor dynamicActor = _Other.rigidbody.GetComponent<CDynamicActor>();
			if(dynamicActor != null)
			{
				dynamicActor.IsOnboardShip = true;
			}
		}
	}
	
	private void OnTriggerExit(Collider _Other)
	{
		if(_Other.rigidbody != null)
		{
			CDynamicActor dynamicActor = _Other.rigidbody.GetComponent<CDynamicActor>();
			if(dynamicActor != null)
			{
				if(!CGame.Ship.GetComponent<CShipOnboardActors>().IsActorOnboardShip(_Other.rigidbody.gameObject))
				{
					dynamicActor.IsOnboardShip = false;
				}
			}
		}
	}
}
