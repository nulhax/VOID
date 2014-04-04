//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPropulsionGeneratorSystem.cs
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
public class CPropulsionGeneratorBehaviour : CNetworkMonoBehaviour 
{

	// Member Types
	
	
	// Member Delegates & Events
	public delegate void NotifyStateChange(CPropulsionGeneratorBehaviour _Self);
	
	public event NotifyStateChange EventPropulsionOutputChanged;
	public event NotifyStateChange EventPropulsionPotentialChanged;

	
	// Member Fields
	private CNetworkVar<float> m_PropulsionPotential = null;
	private CNetworkVar<float> m_fPropulsionOutput = null;
	private CNetworkVar<bool> m_PropulsionGeneratorActive = null;
			

	// Member Properties
	public float PropulsionForce
	{ 
		get { return (m_fPropulsionOutput.Get()); }
		
		[AServerOnly]
		set { m_fPropulsionOutput.Set(value); }
	}

	public float PropulsionPotential
	{ 
		get { return (m_PropulsionPotential.Get()); }
		
		[AServerOnly]
		set 
		{ 
			m_PropulsionPotential.Set(value); 
			
			if(value < PropulsionForce)
				PropulsionForce = value;
		}
	}

	public bool IsPropulsionGeneratorActive
	{
		get { return (m_PropulsionGeneratorActive.Get()); }
	}

	
	// Member Functions
	
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_PropulsionPotential = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_fPropulsionOutput = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_PropulsionGeneratorActive = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, true);
	}
	
	void OnNetworkVarSync(INetworkVar _VarInstance)
	{
		if(m_fPropulsionOutput == _VarInstance)
		{
			if(EventPropulsionOutputChanged != null)
				EventPropulsionOutputChanged(this);
		}
		else if(m_PropulsionPotential == _VarInstance)
		{
			if(EventPropulsionPotentialChanged != null)
				EventPropulsionPotentialChanged(this);
		}
	}
	
	public void Start()
	{
		CGameShips.Ship.GetComponent<CShipPropulsionSystem>().RegisterPropulsionGeneratorSystem(gameObject);
		
		if(CNetwork.IsServer)
		{
			ActivatePropulsionGeneration();
		}
	}
	
	[AServerOnly]
	public void ActivatePropulsionGeneration()
	{
		m_PropulsionGeneratorActive.Set(true);
	}
	
	[AServerOnly]
	public void DeactivatePropulsionGeneration()
	{
		m_PropulsionGeneratorActive.Set(false);
	}
}
