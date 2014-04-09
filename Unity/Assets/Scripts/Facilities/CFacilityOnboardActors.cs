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


	private List<GameObject> m_cContainingActors = new List<GameObject>();


// Member Properties


	public List<GameObject> ActorsOnboard
	{
		get { return(m_cContainingActors); }
	}

	
// Member Methods


	public void OnActorEnteredFacilityTrigger(GameObject _cEnteringActor)
	{
        // Check actor is not already contained in this facility
		if (!m_cContainingActors.Contains(_cEnteringActor))
		{
			m_cContainingActors.Add(_cEnteringActor);

			// Tell actor 
			if (_cEnteringActor.GetComponent<CActorLocator>() != null)
				_cEnteringActor.GetComponent<CActorLocator>().ActorEnteredFacility(gameObject);

			// Fire the actor entered facility event
			if(EventActorEnteredFacility != null)
			{
				EventActorEnteredFacility(gameObject, _cEnteringActor);
			}
		}
	}


	public void OnActorExitedFacilityTrigger(GameObject _cActor)
	{
        // Check actor is contained by this facility
		if (m_cContainingActors.Contains(_cActor))
		{
			m_cContainingActors.Remove(_cActor);

			// Call ActorExitedFacility for the locator
            if (_cActor.GetComponent<CActorLocator>() != null)
            {
                _cActor.GetComponent<CActorLocator>().ActorExitedFacility(gameObject);
            }

			if(EventActorExitedFacility != null)
			{
				EventActorExitedFacility(gameObject, _cActor);
			}
		}
	}


    void Start()
    {
        // Empty
    }


    void Update()
    {
        // Remove consumers that are now null
        m_cContainingActors.RemoveAll(item => item == null); // TODO: This should not have to be called if objects unregister on destory
    }


}
