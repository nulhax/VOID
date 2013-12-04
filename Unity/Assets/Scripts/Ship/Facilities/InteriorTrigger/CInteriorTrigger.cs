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
	public delegate void FacilityActorInteriorTriggerHandler(GameObject _Facility, GameObject _Actor);
	
	public event FacilityActorInteriorTriggerHandler ActorEnteredTrigger;
	public event FacilityActorInteriorTriggerHandler ActorExitedTrigger;
	
	public event FacilityActorInteriorTriggerHandler PlayerActorEnteredTrigger;
	public event FacilityActorInteriorTriggerHandler PlayerActorExitedTrigger;
		
	// Member Fields
	
	
	// Member Properties
	
	
	// Member Methods
	private void OnTriggerEnter(Collider _Other)
	{
		OnActorEnter(transform.parent.gameObject, _Other.rigidbody.gameObject);
	}
	
	private void OnTriggerExit(Collider _Other)
	{
		OnActorExit(transform.parent.gameObject, _Other.rigidbody.gameObject);
	}
	
	private void OnActorEnter(GameObject _Facility, GameObject _Actor)
	{
		if(ActorEnteredTrigger != null)
		{
			if(_Actor.tag == "Player")
				PlayerActorEnteredTrigger(_Facility, _Actor);
			else
				ActorEnteredTrigger(_Facility, _Actor);
		}
	}
	
	private void OnActorExit(GameObject _Facility, GameObject _Actor)
	{
		if(ActorExitedTrigger != null)
			{
			if(_Actor.tag == "Player")
				PlayerActorExitedTrigger(_Facility, _Actor);
			else
				ActorExitedTrigger(_Facility, _Actor);
		}
	}
}
