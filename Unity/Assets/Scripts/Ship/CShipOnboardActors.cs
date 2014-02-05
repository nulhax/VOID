//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShipOnboardActors.cs
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
	[AServerOnly]
	public void ActorEnteredFacilityTrigger(GameObject _Facility, GameObject _Actor)
	{
		if(!m_FacilitiesActorsOnboard.ContainsKey(_Facility))
		{
			m_FacilitiesActorsOnboard.Add(_Facility, new List<GameObject>());
		}

		if(!m_FacilitiesActorsOnboard[_Facility].Contains(_Actor))
		{
			m_FacilitiesActorsOnboard[_Facility].Add(_Actor);
		}
	}

	[AServerOnly]
	public void ActorExitedFacilityTrigger(GameObject _Facility, GameObject _Actor)
	{
		if(m_FacilitiesActorsOnboard[_Facility].Contains(_Actor))
		{
			m_FacilitiesActorsOnboard[_Facility].Remove(_Actor);
		}
	}

	[AServerOnly]
	public bool IsActorOnboardShip(GameObject _Actor)
	{
		bool actorOnboardShip = false;

		// Check if this actor is onboard any other facility
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

	[AServerOnly]
	public GameObject GetContainingFacility(GameObject _Actor)
	{
		foreach(KeyValuePair<GameObject, List<GameObject>> facilityActorPairs in m_FacilitiesActorsOnboard)
		{
			if(facilityActorPairs.Value.Exists((actor) => actor == _Actor))
			{
				return(facilityActorPairs.Key);
			}
		}

		return(null);
	}
}
