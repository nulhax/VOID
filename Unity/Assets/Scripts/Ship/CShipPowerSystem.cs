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

	private float m_ShipBatteryChargePool = 0.0f; 


	// Member Properties
	
	
	// Member Methods
	public override void InstanceNetworkVars()
	{

	}
	
	public void OnNetworkVarSync(INetworkVar _VarInstance)
	{
		
	}
	
	public void Update()
	{
		if(CNetwork.IsServer)
		{
			UpdateShipPowerPool();
			UpdateFacilityPowerConsumption();
		}
	}
	
	public void RegisterPowerGeneratorSystem(GameObject _PowerGeneratorSystem)
	{
		if(!m_PowerGenerators.Contains(_PowerGeneratorSystem))
		{
			m_PowerGenerators.Add(_PowerGeneratorSystem);
		}
	}

	public void UnregisterPowerGeneratorSystem(GameObject _PowerGeneratorSystem)
	{
		if(m_PowerGenerators.Contains(_PowerGeneratorSystem))
		{
			m_PowerGenerators.Remove(_PowerGeneratorSystem);
		}
	}

	[AServerOnly]
	public void UpdateShipPowerPool()
	{
		// Get the combined power battery charge from each power generator
		float totalBatteryCharge = m_PowerGenerators.Sum((pg) => {
			CPowerGeneratorSystem pgs = pg.GetComponent<CPowerGeneratorSystem>();
			return(pgs.IsBatteryChargeAvailable ? pgs.BatteryCharge : 0.0f);
		});
		
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
					fp.DeactivatePower();
			}
		}
		else
		{
			// Calculate the combined ratio of battery charge vs. capacity of each power generator
			float combinedBatteryChargeRatio = m_PowerGenerators.Sum((pg) => {
				CPowerGeneratorSystem pgs = pg.GetComponent<CPowerGeneratorSystem>();
				return(pgs.IsBatteryChargeAvailable ? pgs.BatteryCharge / pgs.BatteryCapacity : 0.0f);
			});

			// Siphon an average weighing of battery from each power generator
			foreach(GameObject pg in m_PowerGenerators)
			{
				CPowerGeneratorSystem pgs = pg.GetComponent<CPowerGeneratorSystem>();

				// Only count if battery charge is available
				if(pgs.IsBatteryChargeAvailable)
				{
					// Calculate the weighing based on the charge of the battery ratio to maximum capacity
					float siphonAmount = pgs.BatteryCharge / pgs.BatteryCapacity / combinedBatteryChargeRatio * combinedConsumption;

					// Siphon the ammount out of the battery
					if(!Single.IsNaN(siphonAmount))
						pgs.BatteryCharge -= siphonAmount;
				}
			}
		}
	}
	
	public void OnGUI()
	{
		string shipPowerOutput = "ShipPowerInfo\n";
		shipPowerOutput += string.Format("\tBatteryChargePool: [{0}]\n", 
		                                 Math.Round(m_ShipBatteryChargePool, 2)); 



		string powerGeneratorOutput = "PowerGeneratorInfo\n";
		foreach(GameObject pg in m_PowerGenerators)
		{
			CFacilityInterface fi = pg.GetComponent<CFacilityInterface>();
			CPowerGeneratorSystem pgs = pg.GetComponent<CPowerGeneratorSystem>();

			powerGeneratorOutput += string.Format("\tFacility [{0}] Type [{1}] \n\t\tIsGenActive: [{2}] GenRate: [{3}] \n\t\tIsBatteryChargAvail: [{4}] BatteryCap: [{5}] BatteryCharge: [{6}]\n", 
			                                      fi.FacilityId, 
			                                      fi.FacilityType,
			                                      pgs.IsPowerGenerationActive,
			                                      Math.Round(pgs.PowerGenerationRate, 2),
			                                      pgs.IsBatteryChargeAvailable,
			                                      Math.Round(pgs.BatteryCapacity, 2),
			                                      Math.Round(pgs.BatteryCharge, 2));	                                  
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
		          "Power Status'\n" + shipPowerOutput + powerGeneratorOutput + facilitiesOutput);
	}
}
