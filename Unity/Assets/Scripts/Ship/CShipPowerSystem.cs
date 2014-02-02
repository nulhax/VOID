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
	private List<GameObject> m_PowerStorageModules = new List<GameObject>();

	private float m_ShipBatteryChargePool = 0.0f; 


	// Member Properties
	
	
	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{

	}
	
	public void OnNetworkVarSync(INetworkVar _VarInstance)
	{
		
	}
	
	public void Update()
	{
		if(CNetwork.IsServer)
		{
			UpdateShipPowerStorage();
			UpdateFacilityPowerConsumption();
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
		if(!m_PowerStorageModules.Contains(_PowerStorage))
		{
			m_PowerStorageModules.Add(_PowerStorage);
		}
	}
	
	public void UnregisterPowerStorage(GameObject _PowerStorage)
	{
		if(m_PowerStorageModules.Contains(_PowerStorage))
		{
			m_PowerStorageModules.Remove(_PowerStorage);
		}
	}


	[AServerOnly]
	public void UpdateShipPowerStorage()
	{
		// Calculate the combined generation of all the generators
		float combinedGeneration = m_PowerGenerators.Sum((pg) => {
			CPowerGenerationBehaviour pgb = pg.GetComponent<CPowerGenerationBehaviour>();
			return(pgb.IsPowerGenerationActive ? pgb.PowerGenerationRate * Time.deltaTime : 0.0f);
		});

		// Select the available storage units
		var availableStorageUnits = 
			from psm in m_PowerStorageModules
			where psm.GetComponent<CPowerStorageBehaviour>().IsBatteryChargeAvailable
			select psm;
		
		// Calculate the even distribution of power to all storage modules
		float evenDistribution = combinedGeneration / availableStorageUnits.ToList().Count;

		// Find the storage modules that are near capacity and fill them up
		foreach(GameObject ps in availableStorageUnits)
		{
			CPowerStorageBehaviour psb = ps.GetComponent<CPowerStorageBehaviour>();

			float newCharge = psb.BatteryCharge + evenDistribution;
			if(newCharge > psb.BatteryCapacity)
			{
				// Top-up the charge and re-evaluate the even distribution
				float chargeAddition = psb.BatteryCapacity - psb.BatteryCharge;
				psb.BatteryCharge = psb.BatteryCapacity;
				evenDistribution -= chargeAddition;
			}
		}

		// Apply the even amount to the rest of storage units
		float totalBatteryCharge = 0.0f;
		foreach(GameObject ps in availableStorageUnits)
		{
			CPowerStorageBehaviour psb = ps.GetComponent<CPowerStorageBehaviour>();

			if(psb.BatteryCharge != psb.BatteryCapacity)
			{
				psb.BatteryCharge += evenDistribution;
			}

			totalBatteryCharge += psb.BatteryCharge;
		}
		
		// Set the battery charge pool
		m_ShipBatteryChargePool = totalBatteryCharge;
	}
	
	[AServerOnly]
	private void UpdateFacilityPowerConsumption()
	{
		// Calculate the combined consumption of all facilities
		float combinedConsumption = gameObject.GetComponent<CShipFacilities>().GetAllFacilities().Sum((f) => {
			CFacilityPower fp = f.GetComponent<CFacilityPower>();
			return(fp.IsPowerActive ? fp.PowerConsumption * Time.deltaTime : 0.0f);
		});

		// Check if the pool of battery charge is insuffcient for the consumption
		if(combinedConsumption > m_ShipBatteryChargePool)
		{
			// Deactivate power to all facilities onboard the ship
			foreach(GameObject facility in gameObject.GetComponent<CShipFacilities>().GetAllFacilities())
			{
				CFacilityPower fp = facility.GetComponent<CFacilityPower>();
				
				if(fp.IsPowerActive)
					fp.InsufficienttPower();
			}
		}
		else
		{
			// Calculate the combined ratio of battery charge vs. capacity of each power generator
			float combinedBatteryChargeRatio = m_PowerStorageModules.Sum((psm) => {
				CPowerStorageBehaviour psb = psm.GetComponent<CPowerStorageBehaviour>();
				return(psb.IsBatteryChargeAvailable ? psb.BatteryCharge / psb.BatteryCapacity : 0.0f);
			});

			// Siphon an average weighing of battery from each power generator
			foreach(GameObject psm in m_PowerStorageModules)
			{
				CPowerStorageBehaviour psb = psm.GetComponent<CPowerStorageBehaviour>();

				// Only count if battery charge is available
				if(psb.IsBatteryChargeAvailable)
				{
					// Calculate the weighing based on the charge of the battery ratio to maximum capacity
					float siphonAmount = psb.BatteryCharge / psb.BatteryCapacity / combinedBatteryChargeRatio * combinedConsumption;

					// Siphon the ammount out of the battery
					if(!Single.IsNaN(siphonAmount))
						psb.BatteryCharge -= siphonAmount;
				}
			}
		}
	}
	
	public void OnGUI()
	{
		string shipPowerOutput = "ShipPowerInfo\n";
		shipPowerOutput += string.Format("\tBatteryChargePool: [{0}]\n", 
		                                 Math.Round(m_ShipBatteryChargePool, 2)); 



		string generatorOutput = "GeneratorInfo\n";
		foreach(GameObject generator in m_PowerGenerators)
		{
			CFacilityInterface fi = CUtility.FindInParents<CFacilityInterface>(generator);
			CPowerGenerationBehaviour pgb = generator.GetComponent<CPowerGenerationBehaviour>();
			
			generatorOutput += string.Format("\t[{0}] Within Facility [{1}] Type [{2}] \n\t\tIsGenerationActive: [{3}] GenRate: [{4}]\n", 
			                                 generator.name,
			                                 fi.FacilityId, 
			                                 fi.FacilityType,
			                                 pgb.IsPowerGenerationActive,
			                                 Math.Round(pgb.PowerGenerationRate, 2));                                  
		}
		
		string storageOutput = "StorageInfo\n";
		foreach(GameObject storage in m_PowerStorageModules)
		{
			CFacilityInterface fi = CUtility.FindInParents<CFacilityInterface>(storage);
			CPowerStorageBehaviour psb = storage.GetComponent<CPowerStorageBehaviour>();
			
			storageOutput += string.Format("\t[{0}] Within Facility [{1}] Type [{2}] \n\t\tIsChargeAvailable: [{3}] Capacity: [{4}] Charge: [{5}]\n", 
		                                   storage.name,
		                                   fi.FacilityId, 
		                                   fi.FacilityType,
		                                   psb.IsBatteryChargeAvailable,
		                                   Math.Round(psb.BatteryCapacity, 2),
		                                   Math.Round(psb.BatteryCharge, 2));	                                  
		}
		
		string facilitiesOutput = "FacilityPowerInfo\n";
		foreach(GameObject facility in gameObject.GetComponent<CShipFacilities>().GetAllFacilities())
		{
			CFacilityInterface fi = facility.GetComponent<CFacilityInterface>();
			CFacilityPower fp = facility.GetComponent<CFacilityPower>();

			facilitiesOutput += string.Format("\tFacility [{0}] Type [{1}] \n\t\tIsActive: [{2}] ConsRate: [{3}]\n", 
			                                  fi.FacilityId, 
			                                  fi.FacilityType,
			                                  fp.IsPowerActive,
			                                  Math.Round(fp.PowerConsumption, 2));
			
		}
		
		float boxWidth = 500;
		float boxHeight = 600;
		GUI.Label(new Rect(Screen.width / 2 - boxWidth, 0.0f, boxWidth, boxHeight),
		          "Power Status'\n" + shipPowerOutput + generatorOutput + storageOutput + facilitiesOutput);
	}
}
