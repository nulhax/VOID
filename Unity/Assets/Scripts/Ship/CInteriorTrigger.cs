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
		
	// Member Fields
	
	
	// Member Properties
	
	
	// Member Methods
	private void OnTriggerEnter(Collider _Other)
	{
		if(_Other.rigidbody != null)
		{
			OnActorEnter(transform.parent.gameObject, _Other.rigidbody.gameObject);
		}
	}
	
	private void OnTriggerExit(Collider _Other)
	{
		if(_Other.rigidbody != null)
		{
			OnActorExit(transform.parent.gameObject, _Other.rigidbody.gameObject);
		}
	}
	
	private void OnActorEnter(GameObject _Facility, GameObject _Actor)
	{
		if(ActorEnteredTrigger != null)
		{
			ActorEnteredTrigger(_Facility, _Actor);
		}
	}
	
	private void OnActorExit(GameObject _Facility, GameObject _Actor)
	{
		if(ActorExitedTrigger != null)
		{
			ActorExitedTrigger(_Facility, _Actor);
		}
	}
}
