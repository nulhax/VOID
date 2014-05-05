//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFacilityOnboardActors.cs
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


public class CFacilityOnboardActors : MonoBehaviour 
{

// Member Types
	
	
// Member Delegates & Events
	public delegate void FacilityActorEnterExit(GameObject _Facility, GameObject _Actor);
	
	public event FacilityActorEnterExit EventActorEnteredFacility;
	public event FacilityActorEnterExit EventActorExitedFacility;


// Member Fields
	Dictionary<GameObject, int> m_ContainingActors = new Dictionary<GameObject, int>();


// Member Properties
    [AServerOnly]
	public List<GameObject> ActorsOnboard
	{
		get { return(new List<GameObject>(m_ContainingActors.Keys)); }
	}


// Member Methods
    [AServerOnly]
	public void OnActorEnteredFacilityTrigger(GameObject _Actor)
	{
        if(!CNetwork.IsServer)
            Debug.LogError("This is a server only function");

        // Check actor is not already contained in this facility
		if(!m_ContainingActors.ContainsKey(_Actor))
		{
			m_ContainingActors.Add(_Actor, 0);

			// Tell actor 
			if(_Actor.GetComponent<CActorLocator>() != null)
				_Actor.GetComponent<CActorLocator>().NotifyEnteredFacility(gameObject);

            //if (_Actor.GetComponent<CActorBoardable>() != null)
            //    _Actor.GetComponent<CActorBoardable>().NotifyExitedFacility(gameObject);

			// Fire the actor entered facility event
			if(EventActorEnteredFacility != null)
				EventActorEnteredFacility(gameObject, _Actor);
		}

		// Increment the count to the containing actor
		m_ContainingActors[_Actor] += 1;
	}


    [AServerOnly]
	public void OnActorExitedFacilityTrigger(GameObject _Actor)
	{
        if(!CNetwork.IsServer)
            Debug.LogError("This is a server only function");

        // Make sure actor is contained within this facility
		if(!m_ContainingActors.ContainsKey(_Actor))
			return;

		// Decrement the count to the containing actor
		m_ContainingActors[_Actor] -= 1;

		// If count is zero, remove the actor
		if(m_ContainingActors[_Actor] == 0)
		{
			m_ContainingActors.Remove(_Actor);
			
			// Call ActorExitedFacility for the locator
			if(_Actor.GetComponent<CActorLocator>() != null)
				_Actor.GetComponent<CActorLocator>().NotifyExitedFacility(gameObject);

            //if (_Actor.GetComponent<CActorBoardable>() != null)
            //    _Actor.GetComponent<CActorBoardable>().NotifyExitedFacility(gameObject);
			
			if(EventActorExitedFacility != null)
				EventActorExitedFacility(gameObject, _Actor);
		}
	}
}
