//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShipInhabitants.cs
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


public class CShipOnboardActors : MonoBehaviour 
{
	// Member Types


	// Member Delegates & Events


	// Member Fields
	private Dictionary<GameObject, List<GameObject>> m_FacilitiesActorsOnboard = new Dictionary<GameObject, List<GameObject>>();
	
	// Member Properties
	
	
	// Member Methods
	public void ActorEnteredFacility(GameObject _Facility, GameObject _Actor)
	{
		if(!m_FacilitiesActorsOnboard.ContainsKey(_Facility))
		{
			m_FacilitiesActorsOnboard.Add(_Facility, new List<GameObject>());
		}
	
		// If the actor is not onboard the ship invoke the callback for entering
		if(!IsActorOnboardShip(_Actor) && CNetwork.IsServer)
		{
			// Only on dynamic actor script
			if(_Actor.GetComponent<CDynamicActor>() != null)	
			{
				_Actor.GetComponent<CDynamicActor>().IsOnboardShip = true;
				
				Debug.Log("Actor Entered ship");
			}
		}
		
		m_FacilitiesActorsOnboard[_Facility].Add(_Actor);
	}
	
	public void ActorExitedFacility(GameObject _Facility, GameObject _Actor)
	{
		m_FacilitiesActorsOnboard[_Facility].Remove(_Actor);
		
		// If the actor is not onboard the ship invoke the callback for exiting
		if(!IsActorOnboardShip(_Actor) && CNetwork.IsServer)
		{
			// Only on dynamic actor script
			if(_Actor.GetComponent<CDynamicActor>() != null)	
			{
				_Actor.GetComponent<CDynamicActor>().IsOnboardShip = false;
				
				Debug.Log("Actor Exited ship");
			}
		}
	}
	
	public bool IsActorOnboardShip(GameObject _Actor)
	{
		// Check if this actor is onboard any other facility
		bool actorOnboardShip = false;
		foreach(List<GameObject> actorsOnboardFacility in m_FacilitiesActorsOnboard.Values)
		{
			if(actorsOnboardFacility.Contains(_Actor))
			{
				actorOnboardShip = true;
				break;
			}
		}
		
		return(actorOnboardShip);
	}
}
