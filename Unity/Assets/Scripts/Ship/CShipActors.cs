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
	private List<GameObject> m_ActorsCurrentlyOnboard = new List<GameObject>();
	
	// Member Properties
	
	
	// Member Methods
	public void ActorEnteredFacility(GameObject _Actor)
	{
		if(!m_ActorsCurrentlyOnboard.Contains(_Actor))
		{
			m_ActorsCurrentlyOnboard.Add(_Actor);
		}
	}
	
	public void ActorExitedFacility(GameObject _Actor)
	{
		if(m_ActorsCurrentlyOnboard.Contains(_Actor))
		{
			m_ActorsCurrentlyOnboard.Remove(_Actor);
			
			if(CNetwork.IsServer)
			{
				// Transfer the actor to galaxy space
				TransferActorToGalaxySpace(_Actor);
			}
		}
	}
	
	[AServerMethod]
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
		
		// If this actor is the player actor we need to swap the cameras
		if(CGame.PlayerActor == _Actor)
		{
			// Parent the player galaxy camera to the ship
			GameObject galaxyPlayerCamera = gameObject.GetComponent<CShipGalaxySimulatior>().PlayerGalaxyCamera;
			galaxyPlayerCamera.transform.parent = gameObject.transform;
			
			// Update the transform of the player galaxy camera
			galaxyPlayerCamera.transform.localPosition = relativePos;
			galaxyPlayerCamera.transform.localRotation = relativeRot;
		}
		
		// Remove parent
		_Actor.transform.parent = null;
		_Actor.GetComponent<CNetworkView>().SyncParent();
		
		// Resursively set the galaxy layer on the actor
		CUtility.SetLayerRecursively(_Actor, LayerMask.NameToLayer("Galaxy"));
		
	}
}
