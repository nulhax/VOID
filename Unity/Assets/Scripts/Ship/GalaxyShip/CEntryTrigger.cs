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
	public CInteriorTrigger m_ReferencedInteriorTrigger = null;


	// Member Properties
	
	
	// Member Methods
	private void OnTriggerEnter(Collider _cOther)
	{
		if(!CNetwork.IsServer)
			return;
		
		Rigidbody rigidBody = _cOther.rigidbody;
		
		// Find rigid body in parnet
		if (rigidBody == null)
		{
			rigidBody = CUtility.FindInParents<Rigidbody>(_cOther.gameObject);
		}
		
		// Notify facility that a actor entered
		if(rigidBody != null)
		{
			CActorLocator actorLocator = rigidBody.GetComponent<CActorLocator>();
			if (actorLocator == null)
				return;

			m_ReferencedInteriorTrigger.NotifyActorEnteredTrigger(actorLocator);
		}
	}
}
