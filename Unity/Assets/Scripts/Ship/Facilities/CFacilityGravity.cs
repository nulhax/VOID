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
	public delegate void OnGravitySourceCreate(GameObject _GravitySource);
	public delegate void OnGravitySourceDestroy(GameObject _GravitySource);
	public event OnGravitySourceCreate  EventOnGravitySourceCreate;
	public event OnGravitySourceDestroy EventOnGravitySourceDestroy;

	
	// Member Fields
	private List<GameObject> m_GravitySources;
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
		
		facilityInteriorTrigger.ActorEnteredTrigger += ActorEnteredGravityZone;
		facilityInteriorTrigger.ActorExitedTrigger += ActorExitedGravityZone;
	}
	
	public void AddGravitySource(GameObject _Source)
	{
		m_GravitySources.Add(_Source);
		
		UpdateGravity();
	}
	
	public void RemoveGravitySource(GameObject _Source)
	{
		m_GravitySources.Remove(_Source);
		
		UpdateGravity();
	}
	
	private void UpdateGravity()
	{
		float fLargestGravitySource = 0.0f;
		
		foreach (GameObject GravitySource in m_GravitySources)
		{
			float TempGravitySourceOutput = GravitySource.GetComponent<CGravityGeneration>().GetGravityOutput();
			
			// Less than because gravity is a negative force on the y axis
			if (TempGravitySourceOutput < fLargestGravitySource)
			{
				fLargestGravitySource = TempGravitySourceOutput;
			}
		}
		
		m_FacilityGravityAcceleration.y = fLargestGravitySource;
	}
	
	public void Update()
	{
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
