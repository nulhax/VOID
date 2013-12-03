//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFacilityGravity.cs
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


public class CFacilityGravity : MonoBehaviour 
{
	// Member Types


	// Member Delegates & Events

	
	// Member Fields
	private List<GameObject> m_ActorsInsideTrigger = new List<GameObject>();
	private Vector3 m_FacilityGravityAcceleration = new Vector3(0.0f, -9.81f, 0.0f);
	
	// Member Properties
	
	
	// Member Methods
	public void Awake()
	{
		// Register the actors entering/exiting the trigger zone
		CInteriorTrigger facilityInteriorTrigger = GetComponentInChildren<CInteriorTrigger>();
		
		if(facilityInteriorTrigger == null)
			Debug.LogError("CFacilityGravity, no interior trigger to use for gravity application!");
		
		facilityInteriorTrigger.ActorEnteredTrigger += new Action<GameObject>(ActorEnteredGravityZone);
		facilityInteriorTrigger.ActorExitedTrigger += new Action<GameObject>(ActorExitedGravityZone);
	}
	
	
	public void Update()
	{
		m_ActorsInsideTrigger.RemoveAll((item) => item == null);
		
		// Apply the gravity to the actor every frame (so we can modify it if we want later)
		foreach(GameObject actor in m_ActorsInsideTrigger)
		{	
			actor.GetComponent<CDynamicActor>().GravityAcceleration = m_FacilityGravityAcceleration;
		}
	}
	
	private void ActorEnteredGravityZone(GameObject _Actor)
	{
		// Only add to the list if there is a dynamic actor
		if(_Actor.GetComponent<CDynamicActor>() == null)
			return;
		
		m_ActorsInsideTrigger.Add(_Actor);
	}
	
	private void ActorExitedGravityZone(GameObject _Actor)
	{
		if(!m_ActorsInsideTrigger.Contains(_Actor))
			return;
		
		_Actor.GetComponent<CDynamicActor>().GravityAcceleration = Vector3.zero;
			
		m_ActorsInsideTrigger.Remove(_Actor);
	}
}
