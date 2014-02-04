﻿//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPowerGenerationBehaviour.cs
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
public class CPowerGenerationBehaviour : CNetworkMonoBehaviour 
{
	
// Member Types


// Member Delegates & Events
	public delegate void NotifyStateChange(CPowerGenerationBehaviour _Self);

	public event NotifyStateChange EventGenerationRateChanged;

// Member Fields
	private CNetworkVar<float> m_PowerGenerationRate = null;
	private CNetworkVar<bool> m_PowerGenerationActive = null;


// Member Properties
	public float PowerGenerationRate
	{ 
		get { return (m_PowerGenerationRate.Get()); }

		[AServerOnly]
		set { m_PowerGenerationRate.Set(value); }
	}

	public bool IsPowerGenerationActive
	{
		get { return (m_PowerGenerationActive.Get()); }
	}

// Member Functions

	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_PowerGenerationRate = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_PowerGenerationActive = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, false);
	}
	
	void OnNetworkVarSync(INetworkVar _VarInstance)
	{
		if(m_PowerGenerationRate == _VarInstance)
		{
			if(EventGenerationRateChanged != null)
				EventGenerationRateChanged(this);
		}
	}

	public void Start()
	{
		CGameShips.Ship.GetComponent<CShipPowerSystem>().RegisterPowerGenerator(gameObject);

		if(CNetwork.IsServer)
		{
			ActivatePowerGeneration();
		}
	}
	
	[AServerOnly]
	public void ActivatePowerGeneration()
	{
		m_PowerGenerationActive.Set(true);
	}
	
	[AServerOnly]
	public void DeactivatePowerGeneration()
	{
		m_PowerGenerationActive.Set(false);
	}
}
