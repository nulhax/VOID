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


public class CPropulsionGeneratorSystem : CNetworkMonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	const float m_kfMaximumPropulsion = 15.0f;

	CNetworkVar<float> m_fPropulsionOutput = null;
	CNetworkVar<bool> m_PropulsionGeneratorActive = null;
			
	// Member Properties
	public float PropulsionForce
	{ 
		get { return (m_fPropulsionOutput.Get()); }
		
		[AServerOnly]
		set { m_fPropulsionOutput.Set(value); }
	}

	public bool IsPropulsionGeneratorActive
	{
		get { return (m_PropulsionGeneratorActive.Get()); }
	}

	
	// Member Functions
	
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_fPropulsionOutput = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_PropulsionGeneratorActive = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, true);
	}
	
	void OnNetworkVarSync(INetworkVar _VarInstance)
	{
		
	}
	
	public void Start()
	{
		CGameShips.Ship.GetComponent<CShipPropulsionSystem>().RegisterPropulsionGeneratorSystem(gameObject);
		
		if(CNetwork.IsServer)
		{
			ActivatePropulsionGeneration();
		}
	}
	
	public void Update()
	{
		if(CNetwork.IsServer)
		{
			UpdatePropulsion();
		}
	}
	
	public void UpdatePropulsion()
	{
		if (IsPropulsionGeneratorActive) 
		{
			m_fPropulsionOutput.Set(m_kfMaximumPropulsion);
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
