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
        // Check actor is not already contained in this facility
		if(!m_ContainingActors.ContainsKey(_Actor))
		{
			m_ContainingActors.Add(_Actor, 0);

			// Tell actor 
			if(_Actor.GetComponent<CActorLocator>() != null)
				_Actor.GetComponent<CActorLocator>().NotifyEnteredFacility(gameObject);

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
			
			if(EventActorExitedFacility != null)
				EventActorExitedFacility(gameObject, _Actor);
		}
	}


    void Start()
    {
        GetComponent<CNetworkView>().EventPreDestory += OnPreDestroy;
    }


	 void OnPreDestroy(GameObject _cSender)
{
		foreach(GameObject actor in ActorsOnboard)
<<<<<<< HEAD
		{
			if (actor != null)	// During shutdown, lists of GameObject may have null elements (if they got destroyed before this).
			{
				if (actor.GetComponent<CActorLocator>() != null)
					actor.GetComponent<CActorLocator>().NotifyExitedFacility(gameObject);

				if (EventActorExitedFacility != null)
					EventActorExitedFacility(gameObject, actor);
=======
		{
			if (actor != null)	// During shutdown, lists of GameObject may have null elements (if they got destroyed before this).
			{
				if (actor.GetComponent<CActorLocator>() != null)
					actor.GetComponent<CActorLocator>().NotifyExitedFacility(gameObject);

				if (EventActorExitedFacility != null)
					EventActorExitedFacility(gameObject, actor);
>>>>>>> 1e1da5ec43f0ec2e1500fd866717234f237c7aa1
			}
		}
	}
}
