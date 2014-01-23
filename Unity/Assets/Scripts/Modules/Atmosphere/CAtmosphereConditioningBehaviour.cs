//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CAtmosphereConditioningBehaviour.cs
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
public class CAtmosphereConditioningBehaviour : CNetworkMonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	private CNetworkVar<float> m_AtmosphereCapacitySupport = null;
	private CNetworkVar<bool> m_AtmosphereConditioningActive = null;
	
	
	// Member Properties
	public float AtmosphereCapacitySupport
	{
		get { return(m_AtmosphereCapacitySupport.Get()); }
		
		[AServerOnly]
		set { m_AtmosphereCapacitySupport.Set(value); }
	}
	
	public bool IsAtmosphereConditioningActive
	{
		get { return(m_AtmosphereConditioningActive.Get()); }
	}
	
	// Member Methods
	public override void InstanceNetworkVars()
	{
		m_AtmosphereConditioningActive = new CNetworkVar<bool>(OnNetworkVarSync, false);
		m_AtmosphereCapacitySupport = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}
	
	private void OnNetworkVarSync(INetworkVar _VarInstance)
	{
		
	}
	
	public void Start()
	{
		// Register myself as a atmosphere conditioner
		CGameShips.Ship.GetComponent<CShipLifeSupportSystem>().RegisterAtmosphereConditioner(gameObject);
		
		if(CNetwork.IsServer)
			ActivateConditioning();
	}
	
	[AServerOnly]
	public void ActivateConditioning()
	{
		m_AtmosphereConditioningActive.Set(true);
	}
	
	[AServerOnly]
	public void DeactivateConditioning()
	{
		m_AtmosphereConditioningActive.Set(false);
	}
}
