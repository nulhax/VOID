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
	public delegate void FacilityActorEnterExit(GameObject _Facility, CActorLocator _ActorLocator);
	
	public event FacilityActorEnterExit EventActorEnteredFacility;
	public event FacilityActorEnterExit EventActorExitedFacility;


// Member Fields
    Dictionary<GameObject, List<Collider>> m_OnboardActors = new Dictionary<GameObject, List<Collider>>();

    Dictionary<GameObject, Dictionary<Collider, GameObject>> m_DebugTriggers = new Dictionary<GameObject, Dictionary<Collider,GameObject>>();


// Member Properties
    [AServerOnly]
	public List<GameObject> ActorsOnboard
	{
		get { return(new List<GameObject>(m_OnboardActors.Keys)); }
	}


// Member Methods
    [AServerOnly]
	public void OnActorEnteredFacilityTrigger(CActorLocator _ActorLocator, Collider _InteriorTrigger)
	{
        // Check actor is not already contained in this facility
		if(!m_OnboardActors.ContainsKey(_ActorLocator.gameObject))
		{
            m_OnboardActors.Add(_ActorLocator.gameObject, new List<Collider>());
            //m_DebugTriggers.Add(_ActorLocator.gameObject, new Dictionary<Collider, GameObject>());

			// Tell actor 
			_ActorLocator.NotifyEnteredFacility(gameObject);

			// Fire the actor entered facility event
			if(EventActorEnteredFacility != null)
				EventActorEnteredFacility(gameObject, _ActorLocator);
		}

		// Increment the count to the containing actor
		if(!m_OnboardActors[_ActorLocator.gameObject].Contains(_InteriorTrigger))
        	m_OnboardActors[_ActorLocator.gameObject].Add(_InteriorTrigger);

//		if(!m_DebugTriggers[_ActorLocator.gameObject].ContainsKey(_InteriorTrigger))
//		{
//			GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
//			cube.transform.transform.position = _InteriorTrigger.transform.position + Vector3.up * 2.0f;
//			cube.transform.localScale = Vector3.one * 4;
//			Destroy(cube.collider);
//			cube.renderer.material.color = Color.green;
//			cube.transform.parent = _InteriorTrigger.transform;
//        	m_DebugTriggers[_ActorLocator.gameObject].Add(_InteriorTrigger, cube);
//		}
//        
//        Debug.Log("Trigger Entered: " + _InteriorTrigger.name + " " + _InteriorTrigger.GetComponent<CTileInterface>().m_GridPosition + " - Facility: " + gameObject.name);
	}

    [AServerOnly]
    public void OnActorExitedFacilityTrigger(CActorLocator _ActorLocator, Collider _InteriorTrigger)
	{
		if(!m_OnboardActors.ContainsKey(_ActorLocator.gameObject))
			return;

		// Decrement the count to the containing actor
		m_OnboardActors[_ActorLocator.gameObject].Remove(_InteriorTrigger);
        
//		Destroy(m_DebugTriggers[_ActorLocator.gameObject][_InteriorTrigger]);
//		m_DebugTriggers[_ActorLocator.gameObject].Remove(_InteriorTrigger);

		// If count is zero, remove the actor
		if(m_OnboardActors[_ActorLocator.gameObject].Count == 0)
		{
			m_OnboardActors.Remove(_ActorLocator.gameObject);
			//m_DebugTriggers.Remove(_ActorLocator.gameObject);
			
			// Call ActorExitedFacility for the locator
			_ActorLocator.NotifyExitedFacility(gameObject);
			
			if(EventActorExitedFacility != null)
				EventActorExitedFacility(gameObject, _ActorLocator);
		}

       // Debug.Log("Trigger Exited: " + _InteriorTrigger.name + " " + _InteriorTrigger.GetComponent<CTileInterface>().m_GridPosition + " - Facility: " + gameObject.name);
	}


    void Start()
    {
        GetComponent<CNetworkView>().EventPreDestory += OnPreDestroy;
    }


	void OnPreDestroy(GameObject _cSender)
    {
		Dictionary<GameObject, List<Collider>> tempDic = new Dictionary<GameObject, List<Collider>>(m_OnboardActors);
		foreach(KeyValuePair<GameObject, List<Collider>> onboardActor in tempDic)
        {
			List<Collider> tempList = new List<Collider>(onboardActor.Value);
			foreach(Collider trigger in tempList)
			{
				OnActorExitedFacilityTrigger(onboardActor.Key.GetComponent<CActorLocator>(), trigger);
			}
		}
	}
}
