//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPowerStorageBehaviour.cs
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
public class CPowerStorageBehaviour : CNetworkMonoBehaviour 
{
	
	// Member Types
	
	
	// Member Delegates & Events
	public delegate void NotifyStateChange(CPowerStorageBehaviour _Self);
	
	public event NotifyStateChange EventBatteryChargeChanged;
	public event NotifyStateChange EventBatteryCapacityChanged;


	// Member Fields
	CNetworkVar<float> m_BatteryCharge = null;
	CNetworkVar<float> m_BatteryCapacity = null;
	CNetworkVar<bool> m_BatteryChargeAvailable = null;
	
	
	// Member Properties
	public float BatteryCharge
	{ 
		get { return (m_BatteryCharge.Get()); }
		
		[AServerOnly]
		set { m_BatteryCharge.Set(value); }
	}
	
	public float BatteryCapacity
	{ 
		get { return (m_BatteryCapacity.Get()); }
		
		[AServerOnly]
		set 
		{ 
			m_BatteryCapacity.Set(value); 

			if(value > BatteryCharge)
				BatteryCharge = value;
		}
	}
	
	public bool IsBatteryChargeAvailable
	{
		get { return (m_BatteryChargeAvailable.Get() && BatteryCharge != 0.0f); }
	}
	
	// Member Functions
	
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_BatteryCharge = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_BatteryCapacity = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_BatteryChargeAvailable = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, false);
	}
	
	void OnNetworkVarSync(INetworkVar _VarInstance)
	{
		if(_VarInstance == m_BatteryCapacity)
		{
			if(EventBatteryCapacityChanged != null)
				EventBatteryCapacityChanged(this);
		}
		else if(_VarInstance == m_BatteryCharge)
		{
			if(EventBatteryChargeChanged != null)
				EventBatteryChargeChanged(this);
		}
	}
	
	public void Start()
	{
		CGameShips.Ship.GetComponent<CShipPowerSystem>().RegisterPowerStorage(gameObject);
		
		if(CNetwork.IsServer)
		{
			ActivateBatteryChargeAvailability();
		}
	}
	
	[AServerOnly]
	public void ActivateBatteryChargeAvailability()
	{
		m_BatteryChargeAvailable.Set(true);
	}
	
	[AServerOnly]
	public void DeactivateBatteryChargeAvailability()
	{
		m_BatteryChargeAvailable.Set(false);
	}
}
