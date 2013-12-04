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


public class CShipActors : MonoBehaviour 
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
	
		m_FacilitiesActorsOnboard[_Facility].Add(_Actor);
	}
	
	public void ActorExitedFacility(GameObject _Facility, GameObject _Actor)
	{
		m_FacilitiesActorsOnboard[_Facility].Remove(_Actor);
		
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
		
		// If the actor is not onboard the ship, transfer to galaxy space
		if(!actorOnboardShip)
		{
			TransferActorToGalaxySpace(_Actor);
		}
	}
	
	private void TransferActorToGalaxySpace(GameObject _Actor)
	{	 
		// Get the actors position relative to the ship
		Vector3 relativePos = _Actor.transform.position - transform.position;
		Quaternion relativeRot = _Actor.transform.rotation * Quaternion.Inverse(transform.rotation);
			
		// Temporarily parent the actor to the galaxy ship
		GameObject galaxyShip = gameObject.GetComponent<CShipGalaxySimulatior>().GalaxyShip;
		_Actor.transform.parent = galaxyShip.transform;
		
		// Update the transform of the actor
		_Actor.transform.localPosition = relativePos;
		_Actor.transform.localRotation = relativeRot;
		
		// If this actor is the player actor we need to inform them
		if(CGame.PlayerActor == _Actor)
		{
			_Actor.GetComponent<CPlayerHead>().IsOutsideShip = true;
		}
		
		// Remove parent
		_Actor.transform.parent = null;
		_Actor.GetComponent<CNetworkView>().SyncParent();
		
		// Resursively set the galaxy layer on the actor
		CUtility.SetLayerRecursively(_Actor, LayerMask.NameToLayer("Galaxy"));
	}
}
