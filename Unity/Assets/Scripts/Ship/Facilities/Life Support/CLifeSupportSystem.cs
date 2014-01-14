﻿//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLifeSupportDistribution.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CLifeSupportSystem : CNetworkMonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	

	// Member Fields
	private CNetworkVar<float> m_AtmosphereGenerationRate;
	private CNetworkVar<float> m_AtmosphereCapacitySupport;
	private CNetworkVar<bool> m_AtmosphereGenerationActive;


	// Member Properties
	public float AtmosphereGenerationRate
	{
		get { return(m_AtmosphereGenerationRate.Get()); }

		[AServerOnly]
		set { m_AtmosphereGenerationRate.Set(value); }
	}
	
	public float AtmosphereCapacitySupport
	{
		get { return(m_AtmosphereCapacitySupport.Get()); }

		[AServerOnly]
		set { m_AtmosphereCapacitySupport.Set(value); }
	}
	
	public bool AtmosphereGenerationActive
	{
		get { return(m_AtmosphereGenerationActive.Get()); }
		
		[AServerOnly]
		set { m_AtmosphereGenerationActive.Set(value); }
	}

	// Member Methods
	public override void InstanceNetworkVars()
	{
		m_AtmosphereGenerationActive = new CNetworkVar<bool>(OnNetworkVarSync, false);
		m_AtmosphereGenerationRate = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_AtmosphereCapacitySupport = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}

	void OnNetworkVarSync(INetworkVar _cVarInstance)
	{
		if(_cVarInstance == m_AtmosphereGenerationActive)
		{
			if(m_AtmosphereGenerationActive.Get() == true)
			{	
				CGame.Ship.GetComponent<CShipLifeSupportSystem>().RegisterLifeSupportSystem(gameObject);
			}
			else
			{
				CGame.Ship.GetComponent<CShipLifeSupportSystem>().UnregisterLifeSupportSystem(gameObject);
			}
		}
	}

	public void Start()
	{
		if(CNetwork.IsServer)
			ActivateLifeSupport();
	}

	[AServerOnly]
	public void ActivateLifeSupport()
	{
		m_AtmosphereGenerationActive.Set(true);
	}

	[AServerOnly]
	public void DeactivateLifeSupport()
	{
		m_AtmosphereGenerationActive.Set(false);
	}
}