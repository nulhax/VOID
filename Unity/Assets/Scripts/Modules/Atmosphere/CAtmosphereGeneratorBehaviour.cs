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


[RequireComponent(typeof(CModuleInterface))]
public class CAtmosphereGeneratorBehaviour : CNetworkMonoBehaviour 
{
	// Member Types
	public delegate void NotifyStateChange(CAtmosphereGeneratorBehaviour _Self);
	
	public event NotifyStateChange EventGenerationRateChanged;

	
	// Member Delegates & Events
	

	// Member Fields
	private CNetworkVar<float> m_AtmosphereGenerationRate = null;
	private CNetworkVar<bool> m_AtmosphereGenerationActive = null;


	// Member Properties
	public float AtmosphereGenerationRate
	{
		get { return(m_AtmosphereGenerationRate.Get()); }

		[AServerOnly]
		set { m_AtmosphereGenerationRate.Set(value); }
	}
	
	public bool IsAtmosphereGenerationActive
	{
		get { return(m_AtmosphereGenerationActive.Get()); }
	}

	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_AtmosphereGenerationActive = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, false);
		m_AtmosphereGenerationRate = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}

	private void OnNetworkVarSync(INetworkVar _VarInstance)
	{
		if(m_AtmosphereGenerationRate == _VarInstance)
		{
			if(EventGenerationRateChanged != null)
				EventGenerationRateChanged(this);
		}
	}

	public void Start()
	{
		if(CNetwork.IsServer)
			ActivateGeneration();
	}

	[AServerOnly]
	public void ActivateGeneration()
	{
		m_AtmosphereGenerationActive.Set(true);

		// Register self with ship atmosphere
		CGameShips.Ship.GetComponent<CShipAtmosphere>().RegisterAtmosphereGenerator(gameObject);
	}

	[AServerOnly]
	public void DeactivateGeneration()
	{
		m_AtmosphereGenerationActive.Set(false);

		// Unregister self with ship atmosphere
		CGameShips.Ship.GetComponent<CShipAtmosphere>().UnregisterAtmosphereGenerator(gameObject);
	}
}
