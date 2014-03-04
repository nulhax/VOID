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
	private List<GameObject> m_ActorsOnboard = new List<GameObject>();


	// Member Properties
	public List<GameObject> ActorsOnboard
	{
		get { return(m_ActorsOnboard); }
	}
	
	// Member Methods
	public void Start()
	{
		// Register the ship to new actor entering/exiting events
		EventActorEnteredFacility += CGameShips.Ship.GetComponent<CShipOnboardActors>().ActorEnteredFacilityTrigger;
		EventActorExitedFacility += CGameShips.Ship.GetComponent<CShipOnboardActors>().ActorExitedFacilityTrigger;
	}

	private void Update()
	{
		// Remove consumers that are now null
		m_ActorsOnboard.RemoveAll(item => item == null);
	}

	[AServerOnly]
	public void OnActorEnteredFacilityTrigger(GameObject _Actor)
	{
		if(!m_ActorsOnboard.Contains(_Actor))
		{
			m_ActorsOnboard.Add(_Actor);

			// Call ActorEnteredFacility for the locator
			if(_Actor.GetComponent<CActorLocator>() != null)
				_Actor.GetComponent<CActorLocator>().ActorEnteredFacility(gameObject);

			// Fire the actor entered facility event
			if(EventActorEnteredFacility != null)
			{
				EventActorEnteredFacility(gameObject, _Actor);
			}
		}
	}

	[AServerOnly]
	public void OnActorExitedFacilityTrigger(GameObject _Actor)
	{
		if(m_ActorsOnboard.Contains(_Actor))
		{
			m_ActorsOnboard.Remove(_Actor);

			// Call ActorExitedFacility for the locator
			if(_Actor.GetComponent<CActorLocator>() != null)
				_Actor.GetComponent<CActorLocator>().ActorExitedFacility(gameObject);

			if(EventActorExitedFacility != null)
			{
				EventActorExitedFacility(gameObject, _Actor);
			}
		}
	}
}
