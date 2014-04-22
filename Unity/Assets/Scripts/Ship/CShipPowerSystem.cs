//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShipAtmosphere.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

/* Implementation */


public class CShipPowerSystem : CNetworkMonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	private List<GameObject> m_PowerGenerators = new List<GameObject>();
	private List<GameObject> m_PowerStorages = new List<GameObject>();
	
	private float m_ShipCurrentCharge = 0.0f; 

	private float m_ShipChargeCapacityPotential = 0.0f; 
	private float m_ShipCurrentChargeCapacity = 0.0f; 

	private float m_ShipGenerationRatePotential = 0.0f;
	private float m_ShipCurentGenerationRate = 0.0f;

	private float m_ShipCurrentConsumptionRate = 0.0f;

	// Member Properties
	public float ShipGenerationRatePotential
	{
		get { return (m_ShipGenerationRatePotential); } 
	}

	public float ShipCurentGenerationRate
	{
		get { return (m_ShipCurentGenerationRate); } 
	}

    public float ShipCurrentCharge
    {
		get { return (m_ShipCurrentCharge); } 
    }

	public float ShipChargeCapacityPotential
	{
		get { return (m_ShipChargeCapacityPotential); } 
	}

	public float ShipCurrentChargeCapacity
	{
		get { return (m_ShipCurrentChargeCapacity); } 
	}

	public float ShipCurrentConsumptionRate
	{
		get { return (m_ShipCurrentConsumptionRate); } 
	}

	
	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{

	}
	
	public void OnNetworkVarSync(INetworkVar _VarInstance)
	{
		
	}
	
	public void Update()
	{
		// Update the power variables
		UpdateGenerationVariables();
		UpdateChargeVariables();
		UpdateConsumptionVariables();

		if(CNetwork.IsServer)
		{
			UpdatePowerGenerationAndConsumption();
		}
	}
	
	public void RegisterPowerGenerator(GameObject _PowerGenerator)
	{
		if(!m_PowerGenerators.Contains(_PowerGenerator))
		{
			m_PowerGenerators.Add(_PowerGenerator);
		}
	}

	public void UnregisterPowerGenerator(GameObject _PowerGenerator)
	{
		if(m_PowerGenerators.Contains(_PowerGenerator))
		{
			m_PowerGenerators.Remove(_PowerGenerator);
		}
	}

	public void RegisterPowerStorage(GameObject _PowerStorage)
	{
		if(!m_PowerStorages.Contains(_PowerStorage))
		{
			m_PowerStorages.Add(_PowerStorage);
		}
	}
	
	public void UnregisterPowerStorage(GameObject _PowerStorage)
	{
		if(m_PowerStorages.Contains(_PowerStorage))
		{
			m_PowerStorages.Remove(_PowerStorage);
		}
	}

	private void UpdateGenerationVariables()
	{
		m_ShipCurentGenerationRate = 0.0f;
		m_ShipGenerationRatePotential = 0.0f;
		foreach(GameObject pg in m_PowerGenerators)
		{
			CPowerGenerationBehaviour pgb = pg.GetComponent<CPowerGenerationBehaviour>();
			if(pgb.IsPowerGenerationActive)
			{
				m_ShipCurentGenerationRate += pgb.PowerGenerationRate;
			}
			m_ShipGenerationRatePotential += pgb.PowerGenerationRatePotential;
		}
	}

	private void UpdateChargeVariables()
	{
		m_ShipCurrentCharge = 0.0f;
		m_ShipCurrentChargeCapacity = 0.0f;
		m_ShipChargeCapacityPotential = 0.0f;
		foreach(GameObject ps in m_PowerStorages)
		{
			CPowerStorageBehaviour psb = ps.GetComponent<CPowerStorageBehaviour>();
			
			if(psb.IsBatteryChargeAvailable)
			{
				m_ShipCurrentCharge += psb.BatteryCharge;
				m_ShipCurrentChargeCapacity += psb.BatteryCapacity;
			}
			
			m_ShipChargeCapacityPotential += psb.BatteryCapacity;
		}
	}

	private void UpdateConsumptionVariables()
	{
		// Calculate the combined consumption of all facilities
		m_ShipCurrentConsumptionRate = 0.0f;
		foreach(GameObject facility in gameObject.GetComponent<CShipFacilities>().Facilities)
		{
			CFacilityPower fp = facility.GetComponent<CFacilityPower>();
			
			if(fp.IsPowerActive)
				m_ShipCurrentConsumptionRate += fp.PowerConsumptionRate;
		}
	}

	[AServerOnly]
	private void UpdatePowerGenerationAndConsumption()
	{
		// Calculate the actual generation for this frame
		float combinedGeneration = m_ShipCurentGenerationRate * Time.deltaTime;

		// Calculate the combined consumption of all facilities
		float combinedConsumption = m_ShipCurrentConsumptionRate * Time.deltaTime;

		// If the current charge + combined generation of the ship is less than this frames consumption, there is insufficient power
		if(combinedConsumption > (m_ShipCurrentCharge + combinedGeneration))
		{
			// Deactivate power to all facilities onboard the ship
			foreach(GameObject facility in gameObject.GetComponent<CShipFacilities>().Facilities)
			{
				CFacilityPower fp = facility.GetComponent<CFacilityPower>();
				
				if(fp.IsPowerActive)
					fp.InsufficienttPower();
			}
		}

		// Select the available storage units
		var availableStorageUnits = 
			from psm in m_PowerStorages
			where psm.GetComponent<CPowerStorageBehaviour>().IsBatteryChargeAvailable
			select psm.GetComponent<CPowerStorageBehaviour>();

		// Distribute the charge and consumption to the storage units
		DistributeChargeAndConsumption(combinedGeneration, combinedConsumption, availableStorageUnits.ToList());
	}

	private void DistributeChargeAndConsumption(float _ChargeAmount, float _ConsumptionAmount, List<CPowerStorageBehaviour> _AvailabeStorageUnits)
	{
		// Calculate the even distribution of power to all available storage modules
		float evenDistribution = _ChargeAmount / _AvailabeStorageUnits.Count;
		
		// Calculate the combined ratio of battery charge vs. capacity of each power generator
		float combinedChargeRatio = _AvailabeStorageUnits.Sum((psb) => {
			float value = psb.BatteryCharge / psb.BatteryCapacity;
			return(psb.IsBatteryChargeAvailable ? value : 0.0f);
		});
		
		// Apply the new ammount of power to each
		float leftOverCharge = 0.0f;
		foreach(CPowerStorageBehaviour psb in _AvailabeStorageUnits)
		{
			// Calculate the siphon amount based on the charge ratio of the battery / combined charge ratio of the ship
			float chargeRatio = psb.BatteryCharge / psb.BatteryCapacity;
			float siphonAmount = chargeRatio / combinedChargeRatio * _ConsumptionAmount;
			float finalAmount = evenDistribution + (float.IsNaN(siphonAmount) ? 0.0f : (siphonAmount * -1.0f));

			// Make sure to count the left over charge if near capacity
			float finalCharge = psb.BatteryCharge + finalAmount;

			if(finalCharge > psb.BatteryCapacity)
			{
				// Cap the charge at the storage capacity and get the left over amount
				leftOverCharge += finalCharge - psb.BatteryCapacity;
				psb.BatteryCharge = psb.BatteryCapacity;
			}
			else if(finalCharge < 0.0f || float.IsNaN(finalCharge))
			{
				// Cap the charge at zero
				psb.BatteryCharge = 0.0f;
			}
			else
			{
				// Apply the final amount to the storage charge
				psb.BatteryCharge = finalCharge;
			}
		}

		// If there is any charge or consumption left over, distribute them again
		if(!Mathf.Approximately(leftOverCharge, 0.0f))
		{
			// Select the storage units that are not fully charged yet
			var availableStorageUnits = 
				from psm in _AvailabeStorageUnits
				where !psm.GetComponent<CPowerStorageBehaviour>().IsBatteryAtCapacity
				select psm.GetComponent<CPowerStorageBehaviour>();

			// Make sure there are some available
			if(availableStorageUnits.Count() != 0)
			{
				DistributeChargeAndConsumption(leftOverCharge, 0.0f, availableStorageUnits.ToList());
			}
		}
	}

    /////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////

    //float fModulePowerConsumption;

    //[AServerOnly]
    //void UpdateModulePowerConsumption()
    //{
    //    fModulePowerConsumption = (float)CModuleInterface.GetAllModules().Sum((m) =>
    //    {
    //        CModulePower mp = m.GetComponent<CModulePower>();
    //        return (mp.IsPowerActive ? mp.PowerConsumption * Time.deltaTime : 0.0);
    //    });

    //    float TOTAL = combinedConsumption + fModulePowerConsumption;

    //    Debug.Log("TOTAL: " + TOTAL.ToString());
    //}

    /////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////
	

}
