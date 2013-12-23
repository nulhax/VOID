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
	public delegate void FacilityActorEnterExit(GameObject _Sender, GameObject _Actor);
	
	public event FacilityActorEnterExit ActorEnteredFacility;
	public event FacilityActorEnterExit ActorExitedFacility;
	
	// Member Fields
	private List<GameObject> m_ActorsOnboard = new List<GameObject>();
	
	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		ActorEnteredFacility += new FacilityActorEnterExit(CGame.Ship.GetComponent<CShipOnboardActors>().ActorEnteredFacilityTrigger);
		ActorExitedFacility += new FacilityActorEnterExit(CGame.Ship.GetComponent<CShipOnboardActors>().ActorExitedFacilityTrigger);
	}

	public void ActorEnteredFacilityTrigger(GameObject _Actor)
	{
		if(!m_ActorsOnboard.Contains(_Actor))
		{
            // Check this is a player actor
            if (_Actor.GetComponent<CPlayerLocator>() != null)
            {
                // Notify player actor has entered this facility
                _Actor.GetComponent<CPlayerLocator>().SetContainingFacility(gameObject);
            }

			m_ActorsOnboard.Add(_Actor);
			
			OnActorEnter(_Actor);
		}
	}
	
	public void ActorExitedFacilityTrigger(GameObject _Actor)
	{
		if(m_ActorsOnboard.Contains(_Actor))
		{
			m_ActorsOnboard.Remove(_Actor);
			
			OnActorExit(_Actor);
		}
	}
	
	private void OnActorEnter(GameObject _Actor)
	{
		if(ActorEnteredFacility != null)
		{
			ActorEnteredFacility(gameObject, _Actor);
		}
	}
	
	private void OnActorExit(GameObject _Actor)
	{
		if(ActorExitedFacility != null)
		{
			ActorExitedFacility(gameObject, _Actor);
		}
	}
}
