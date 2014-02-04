//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPartBehaviour.cs
//  Description :   --------------------------
//
//  Author  	:  Scott Emery
//  Mail    	:  scott.ipod@gmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CPartBehaviour : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


	public delegate void NotifyBroken();
	public event NotifyBroken EventModuleBroken;


// Member Properties
	

	// Min&Max range to randomize between for breaking module
	float fMinTime = 100.0f;
	float fMaxTime = 200.0f;
	

	public float TimeBetweenEvents
	{
		get { return(Random.Range(fMinTime, fMaxTime)); }
	}
	

	public float TimeUntilNextEvent
	{
		get { return(m_fTimeUntilNextEvent); }
		set { m_fTimeUntilNextEvent = value; }
	}
	

// Member Functions


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
		m_bIsFunctional = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, true);
    }


// Member Methods


	void Start()
	{
		TimeUntilNextEvent = Time.time + TimeBetweenEvents;
	}


	void OnDestroy()
	{
		// Empty
	}


	void Update()
	{
		// On server
		if(CNetwork.IsServer)
		{
			
			// Get random time to break
			if(Time.time >= TimeUntilNextEvent)
			{
				TimeUntilNextEvent = Time.time + TimeBetweenEvents;
				
				// Do stuff.
				// Break Module by sending bool over network to clients
				m_bIsFunctional.Set(false);
			}
		}
	}
	

	void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
		if(_cVarInstance == m_bIsFunctional)
		{
			if(m_bIsFunctional.Get() == true)
			{
				// Do nothing to the module, it's still working
			}
			else
			{
				// Module is broken
				Debug.Log ("Part Has Broken");
				MeshRenderer[] MeshRendererArray = gameObject.GetComponentsInChildren<MeshRenderer>();
				
				foreach(MeshRenderer eachthingy in MeshRendererArray)
				{
					eachthingy.material.color = Color.white;
				}
				
				if(EventModuleBroken != null)
				{
					EventModuleBroken();
				}
			}
			// has the module been replaced
			// Set m_bIsFunctional to true
		}
	}
	

// Member Fields


	CNetworkVar<bool> m_bIsFunctional;
	float m_fTimeBetweenEvents;
	float m_fTimeUntilNextEvent;


};