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


public class CLifeSupportAtmosphereDistribution : CNetworkMonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	

	// Member Fields
	private float m_AtmosphereDistributionRate = 0.0f;

	private CNetworkVar<bool> m_DistributionActive = null;

	// Member Properties
	public float AtmosphereDistributionRate
	{
		get { return(m_AtmosphereDistributionRate); }
		set { m_AtmosphereDistributionRate = value; }
	}

	// Member Methods
	public override void InstanceNetworkVars()
	{
		m_DistributionActive = new CNetworkVar<bool>(OnNetworkVarSync);
	}

	void OnNetworkVarSync(INetworkVar _cVarInstance)
	{
		if(_cVarInstance == m_DistributionActive)
		{
			if(m_DistributionActive.Get() == true)
			{	
				CGame.Ship.GetComponent<CShipAtmosphereSystem>().RegisterAtmosphereDistributor(gameObject);
			}
			else
			{
				CGame.Ship.GetComponent<CShipAtmosphereSystem>().UnregisterAtmosphereDistributor(gameObject);
			}
		}
	}

	public void Start()
	{
		if(CNetwork.IsServer)
			ActivateDistribution();
	}

	[AServerMethod]
	public void ActivateDistribution()
	{
		m_DistributionActive.Set(true);
	}

	[AServerMethod]
	public void DeactivateDistribution()
	{
		m_DistributionActive.Set(false);
	}
}
