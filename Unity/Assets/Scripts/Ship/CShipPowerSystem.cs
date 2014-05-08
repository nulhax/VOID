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


// Member Properties


    public float MaxGenerationRate
    {
        get { return (m_fMaxGenerationRate.Value); }
    }


	public float TotalGenerationRate
	{
        get { return (m_fCurrentGenerationRate.Value); } 
	}


	public float TotalCapacity
	{
        get { return (m_fCurrentCapacity.Value); } 
	}


    public float TotalCharge
    {
        get { return (m_fCurrentCharge.Value); }
    }


    public float TotalConsumptionRate
    {
        get { return (m_fCurrentConsumptionRate.Value); }
    }

	
// Member Methods


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
        m_fMaxGenerationRate    = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fCurrentConsumptionRate = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fCurrentGenerationRate  = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fMaxCapacity          = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fCurrentCapacity        = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fCurrentCharge          = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}


    public void ChangeMaxGenerationRate(float _fValue)
    {
        m_fMaxGenerationRate.Value += _fValue;

        Debug.Log(string.Format("Ship power generation max change({0}) max generation({1})", _fValue, m_fMaxGenerationRate.Value));
    }


    public void ChangeGenerationRate(float _fValue)
    {
        m_fCurrentGenerationRate.Value += _fValue;

        Debug.Log(string.Format("Ship power generation total change({0}) total generation({1})", _fValue, m_fCurrentGenerationRate.Value));
    }


    public void ChangeConsumptionRate(float _fValue)
    {
        m_fCurrentConsumptionRate.Value += _fValue;

        Debug.Log(string.Format("Ship power consumption total change({0}) total consumption({1})", _fValue, m_fCurrentConsumptionRate.Value));
    }


    public void ChangeMaxCapacity(float _fValue)
    {
        m_fMaxCapacity.Value += _fValue;

        Debug.Log(string.Format("Ship power capacity max change({0}) max capacity({1})", _fValue, m_fMaxCapacity.Value));
    }


    public void ChangeCapacity(float _fValue)
    {
        m_fCurrentCapacity.Value += _fValue;

        Debug.Log(string.Format("Ship power capacity total change({0}) total capacity({1})", _fValue, m_fCurrentCapacity.Value));
    }


	void Update()
	{
        // Empty
	}


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        // Empty
    }
	

    /*
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
			CPowerGeneratorInterface pgb = pg.GetComponent<CPowerGeneratorInterface>();
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
			CPowerBatteryInterface psb = ps.GetComponent<CPowerBatteryInterface>();
			
			if(psb.IsBatteryChargeAvailable)
			{
				m_ShipCurrentCharge += psb.ChargedAmount;
				m_ShipCurrentChargeCapacity += psb.ChargeCapacity;
			}
			
			m_ShipChargeCapacityPotential += psb.ChargeCapacity;
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

                if (fp.IsPowerActive)
                    fp.SetPowerActive(false);
			}
		}

		// Select the available storage units
		var availableStorageUnits = 
			from psm in m_PowerStorages
			where psm.GetComponent<CPowerBatteryInterface>().IsBatteryChargeAvailable
			select psm.GetComponent<CPowerBatteryInterface>();

		// Distribute the charge and consumption to the storage units
		DistributeChargeAndConsumption(combinedGeneration, combinedConsumption, availableStorageUnits.ToList());
	}

	private void DistributeChargeAndConsumption(float _ChargeAmount, float _ConsumptionAmount, List<CPowerBatteryInterface> _AvailabeStorageUnits)
	{
		// Calculate the even distribution of power to all available storage modules
		float evenDistribution = _ChargeAmount / _AvailabeStorageUnits.Count;
		
		// Calculate the combined ratio of battery charge vs. capacity of each power generator
		float combinedChargeRatio = _AvailabeStorageUnits.Sum((psb) => {
			float value = psb.ChargedAmount / psb.ChargeCapacity;
			return(psb.IsBatteryChargeAvailable ? value : 0.0f);
		});
		
		// Apply the new ammount of power to each
		float leftOverCharge = 0.0f;
		foreach(CPowerBatteryInterface psb in _AvailabeStorageUnits)
		{
			// Calculate the siphon amount based on the charge ratio of the battery / combined charge ratio of the ship
			float chargeRatio = psb.ChargedAmount / psb.ChargeCapacity;
			float siphonAmount = chargeRatio / combinedChargeRatio * _ConsumptionAmount;
			float finalAmount = evenDistribution + (float.IsNaN(siphonAmount) ? 0.0f : (siphonAmount * -1.0f));

			// Make sure to count the left over charge if near capacity
			float finalCharge = psb.ChargedAmount + finalAmount;

			if(finalCharge > psb.ChargeCapacity)
			{
				// Cap the charge at the storage capacity and get the left over amount
				leftOverCharge += finalCharge - psb.ChargeCapacity;
				psb.ChargedAmount = psb.ChargeCapacity;
			}
			else if(finalCharge < 0.0f || float.IsNaN(finalCharge))
			{
				// Cap the charge at zero
				psb.ChargedAmount = 0.0f;
			}
			else
			{
				// Apply the final amount to the storage charge
				psb.ChargedAmount = finalCharge;
			}
		}

		// If there is any charge or consumption left over, distribute them again
		if(!Mathf.Approximately(leftOverCharge, 0.0f))
		{
			// Select the storage units that are not fully charged yet
			var availableStorageUnits = 
				from psm in _AvailabeStorageUnits
				where !psm.GetComponent<CPowerBatteryInterface>().IsFullyCharged
				select psm.GetComponent<CPowerBatteryInterface>();

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
	
	public void OnGUI()
	{
        return;
		string shipPowerOutput = "ShipPowerInfo\n";
		shipPowerOutput += string.Format("\tBatteryChargePool: [{0}]\n", 
		                                 Math.Round(m_ShipCurrentCharge, 2)); 



		string generatorOutput = "GeneratorInfo\n";
		foreach(GameObject generator in m_PowerGenerators)
		{
			CFacilityInterface fi = CUtility.FindInParents<CFacilityInterface>(generator);
			CPowerGeneratorInterface pgb = generator.GetComponent<CPowerGeneratorInterface>();
			
			generatorOutput += string.Format("\t[{0}] Within Facility [{1}] Type [{2}] \n\t\tIsGenerationActive: [{3}] GenRate: [{4}]\n", 
			                                 generator.name,
			                                 fi.FacilityId, 
			                                 fi.FacilityType,
			                                 pgb.IsPowerGenerationActive,
			                                 Math.Round(pgb.PowerGenerationRate, 2));                                  
		}
		
		string storageOutput = "StorageInfo\n";
		foreach(GameObject storage in m_PowerStorages)
		{
			CFacilityInterface fi = CUtility.FindInParents<CFacilityInterface>(storage);
			CPowerBatteryInterface psb = storage.GetComponent<CPowerBatteryInterface>();
			
			storageOutput += string.Format("\t[{0}] Within Facility [{1}] Type [{2}] \n\t\tIsChargeAvailable: [{3}] Capacity: [{4}] Charge: [{5}]\n", 
		                                   storage.name,
		                                   fi.FacilityId, 
		                                   fi.FacilityType,
		                                   psb.IsBatteryChargeAvailable,
		                                   Math.Round(psb.ChargeCapacity, 2),
		                                   Math.Round(psb.ChargedAmount, 2));	                                  
		}
		
		string facilitiesOutput = "FacilityPowerInfo\n";
		foreach(GameObject facility in gameObject.GetComponent<CShipFacilities>().Facilities)
		{
			CFacilityInterface fi = facility.GetComponent<CFacilityInterface>();
			CFacilityPower fp = facility.GetComponent<CFacilityPower>();

			facilitiesOutput += string.Format("\tFacility [{0}] Type [{1}] \n\t\tIsActive: [{2}] ConsRate: [{3}]\n", 
			                                  fi.FacilityId, 
			                                  fi.FacilityType,
			                                  fp.IsPowerActive,
			                                  Math.Round(fp.PowerConsumptionRate, 2));
			
		}
		
		float boxWidth = 500;
		float boxHeight = 600;
		GUI.Label(new Rect(Screen.width / 2 - boxWidth, 0.0f, boxWidth, boxHeight),
		          "Power Status'\n" + shipPowerOutput + generatorOutput + storageOutput + facilitiesOutput);
	}
     * 
     * 
     * 
     */


// Member Fields


    CNetworkVar<float> m_fMaxGenerationRate = null;
    CNetworkVar<float> m_fCurrentConsumptionRate = null;
    CNetworkVar<float> m_fCurrentGenerationRate = null;
    CNetworkVar<float> m_fMaxCapacity = null;
    CNetworkVar<float> m_fCurrentCapacity = null;
    CNetworkVar<float> m_fCurrentCharge = null;


}



