//  Auckland
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
	private CNetworkVar<float> m_AtmosphereDistributionRate;
	private CNetworkVar<float> m_AtmosphereCapacitySupport;
	private CNetworkVar<bool> m_LifeSupportActive;

	// Member Properties
	[AServerOnly]
	public float AtmosphereDistributionRate
	{
		get { return(m_AtmosphereDistributionRate.Get()); }

		[AServerOnly]
		set { m_AtmosphereDistributionRate.Set(value); }
	}

	[AServerOnly]
	public float AtmosphereCapacitySupport
	{
		get { return(m_AtmosphereCapacitySupport.Get()); }

		[AServerOnly]
		set { m_AtmosphereCapacitySupport.Set(value); }
	}

	// Member Methods
	public override void InstanceNetworkVars()
	{
		m_LifeSupportActive = new CNetworkVar<bool>(OnNetworkVarSync, false);
		m_AtmosphereDistributionRate = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_AtmosphereCapacitySupport = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}

	void OnNetworkVarSync(INetworkVar _cVarInstance)
	{
		if(_cVarInstance == m_LifeSupportActive)
		{
			if(m_LifeSupportActive.Get() == true)
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
		m_LifeSupportActive.Set(true);
	}

	[AServerOnly]
	public void DeactivateLifeSupport()
	{
		m_LifeSupportActive.Set(false);
	}
}
