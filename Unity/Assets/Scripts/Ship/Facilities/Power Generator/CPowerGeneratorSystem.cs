//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPowerGeneratorSystem.cs
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


public class CPowerGeneratorSystem : CNetworkMonoBehaviour 
{
	
// Member Types


// Member Delegates & Events


// Member Fields
	CNetworkVar<float> m_fBatteryCharge = null;
	CNetworkVar<float> m_fBatteryCapacity = null;
	CNetworkVar<float> m_fPowerGenerationRate = null;
	CNetworkVar<bool> m_PowerGenerationActive = null;
	CNetworkVar<bool> m_BatteryChargeAvailable = null;


// Member Properties
	public float BatteryCharge
	{ 
		get { return (m_fBatteryCharge.Get()); }
		
		[AServerOnly]
		set { m_fBatteryCharge.Set(value); }
	}
	
	public float BatteryCapacity
	{ 
		get { return (m_fBatteryCapacity.Get()); }
		
		[AServerOnly]
		set { m_fBatteryCapacity.Set(value); }
	}

	public float PowerGenerationRate
	{ 
		get { return (m_fPowerGenerationRate.Get()); }

		[AServerOnly]
		set { m_fPowerGenerationRate.Set(value); }
	}

	public bool IsPowerGenerationActive
	{
		get { return (m_PowerGenerationActive.Get()); }
	}

	public bool IsBatteryChargeAvailable
	{
		get { return (m_BatteryChargeAvailable.Get() && BatteryCharge != 0.0f); }
	}

// Member Functions

	public override void InstanceNetworkVars()
	{
		m_fBatteryCharge = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_fBatteryCapacity = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_fPowerGenerationRate = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_PowerGenerationActive = new CNetworkVar<bool>(OnNetworkVarSync, false);
		m_BatteryChargeAvailable = new CNetworkVar<bool>(OnNetworkVarSync, false);
	}
	
	void OnNetworkVarSync(INetworkVar _VarInstance)
	{

	}

	public void Start()
	{
		CGame.Ship.GetComponent<CShipPowerSystem>().RegisterPowerGeneratorSystem(gameObject);

		if(CNetwork.IsServer)
		{
			ActivatePowerGeneration();
			ActivateBatteryChargeAvailability();
		}
	}

	public void Update()
	{
		if(CNetwork.IsServer)
		{
			UpdateBatteryCharge();
		}
	}

	public void UpdateBatteryCharge()
	{
		if(IsPowerGenerationActive)
		{
			// Calculate the new charge
			float newCharge = BatteryCharge + PowerGenerationRate * Time.deltaTime;
			
			// Clamp atmosphere
			if(newCharge > BatteryCapacity)
			{
				newCharge = BatteryCapacity;
			}

			// Set the new battery charge
			m_fBatteryCharge.Set(newCharge);
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
