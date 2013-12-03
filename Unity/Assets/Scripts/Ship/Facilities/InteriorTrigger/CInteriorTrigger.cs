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
	public event Action<GameObject> ActorEnteredTrigger;
	public event Action<GameObject> ActorExitedTrigger;
	
	public event Action<GameObject> PlayerActorEnteredTrigger;
	public event Action<GameObject> PlayerActorExitedTrigger;
	
		
	// Member Fields
	
	
	// Member Properties
	
	
	// Member Methods
	private void OnTriggerEnter(Collider _Other)
	{
		if(_Other.gameObject.tag == "Player")
		{
			OnPlayerActorEnter(_Other.rigidbody.gameObject);
		}
		
		OnActorEnter(_Other.rigidbody.gameObject);
	}
	
	private void OnTriggerExit(Collider _Other)
	{
		if(_Other.gameObject.tag == "Player")
		{
			OnPlayerActorExit(_Other.rigidbody.gameObject);
		}
		
		OnActorExit(_Other.rigidbody.gameObject);
	}
	
	private void OnPlayerActorEnter(GameObject _PlayerActor)
	{
		if(PlayerActorEnteredTrigger != null)
			PlayerActorEnteredTrigger(_PlayerActor);
	}
	
	private void OnPlayerActorExit(GameObject _PlayerActor)
	{
		if(PlayerActorExitedTrigger != null)
			PlayerActorExitedTrigger(_PlayerActor);
	}
	
	private void OnActorEnter(GameObject _Actor)
	{
		if(ActorEnteredTrigger != null)
			ActorEnteredTrigger(_Actor);
	}
	
	private void OnActorExit(GameObject _Actor)
	{
		if(ActorExitedTrigger != null)
			ActorExitedTrigger(_Actor);
	}
}
