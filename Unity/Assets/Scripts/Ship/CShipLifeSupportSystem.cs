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


public class CShipLifeSupportSystem : CNetworkMonoBehaviour 
{
	// Member Types


	// Member Delegates & Events

	
	// Member Fields
	private List<GameObject> m_AtmosphereGenerators = new List<GameObject>();
	private List<GameObject> m_AtmosphereConditioners = new List<GameObject>();

	private CNetworkVar<float> m_ShipAtmosphericQuality = null;

	// Member Properties
	public float ShipAtmosphericQuality
	{
		get { return(m_ShipAtmosphericQuality.Get()); }
		
		[AServerOnly]
		set { m_ShipAtmosphericQuality.Set(value); }
	}

	
	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_ShipAtmosphericQuality = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}
	
	public void OnNetworkVarSync(INetworkVar _VarInstance)
	{
		
	}

	public void Update()
	{
		if(CNetwork.IsServer)
		{
			UpdateFacilityAtmosphereRefilling();
			UpdateFacilityAtmosphereQuality();
		}
	}

	public void RegisterAtmosphereGenerator(GameObject _AtmosphereGenerator)
	{
		if(!m_AtmosphereGenerators.Contains(_AtmosphereGenerator))
		{
			m_AtmosphereGenerators.Add(_AtmosphereGenerator);
		}
	}
	
	public void UnregisterAtmosphereGenerator(GameObject _AtmosphereGenerator)
	{
		if(m_AtmosphereGenerators.Contains(_AtmosphereGenerator))
		{
			m_AtmosphereGenerators.Remove(_AtmosphereGenerator);
		}
	}

	public void RegisterAtmosphereConditioner(GameObject _AtmosphereConditioner)
	{
		if(!m_AtmosphereConditioners.Contains(_AtmosphereConditioner))
		{
			m_AtmosphereConditioners.Add(_AtmosphereConditioner);
		}
	}

	public void UnregisterAtmosphereConditioner(GameObject _AtmosphereConditioner)
	{
		if(m_AtmosphereConditioners.Contains(_AtmosphereConditioner))
		{
			m_AtmosphereConditioners.Remove(_AtmosphereConditioner);
		}
	}

	[AServerOnly]
	private void UpdateFacilityAtmosphereRefilling()
	{
		// Reset all facility refilling
		foreach(GameObject facility in gameObject.GetComponent<CShipFacilities>().Facilities)
		{
			facility.GetComponent<CFacilityAtmosphere>().AtmosphereRefillRate = 0.0f;
		}

		// Get the facilities that are requiring a refill of atmosphere (i.e, have a leak, hull breach, consumer)
		var facilitiesRequiringRefilling = 
			from facility in gameObject.GetComponent<CShipFacilities>().Facilities
			where facility.GetComponent<CFacilityAtmosphere>().RequiresAtmosphereRefill
			select facility;
		
		// If there are facilities requiring this, calculate the refilling output
		int numFacilitiesRequiringRefill = facilitiesRequiringRefilling.ToArray().Length;
		if(numFacilitiesRequiringRefill != 0)
		{
			// Get the combined output of all atmosphere distributors
			float combinedOutput = 0.0f;
			foreach(GameObject ag in m_AtmosphereGenerators)
			{
				CAtmosphereGeneratorBehaviour agb = ag.GetComponent<CAtmosphereGeneratorBehaviour>();

				if(agb.IsAtmosphereGenerationActive)
				{
					combinedOutput += agb.AtmosphereGenerationRate;
				}
			}
			
			// Calculate the output for each facility evenly
			float evenDistribution = combinedOutput / numFacilitiesRequiringRefill;
			
			// Apply this refilling value to the facilities that need it
			foreach(GameObject facility in facilitiesRequiringRefilling)
			{
				facility.GetComponent<CFacilityAtmosphere>().AtmosphereRefillRate = evenDistribution;
			}
		}
	}

	[AServerOnly]
	private void UpdateFacilityAtmosphereQuality()
	{
		// Calculate the combined quantity of atmosphere between all facilities
		float combinedVolume = 0.0f;
		foreach(GameObject facility in gameObject.GetComponent<CShipFacilities>().Facilities)
		{
			combinedVolume += facility.GetComponent<CFacilityAtmosphere>().AtmosphereVolume;
		}

		// Calculate the combined quality capacity of all the life support system
		float combinedCapacitySupport = 0.0f;
		foreach(GameObject conditioner in m_AtmosphereConditioners)
		{
			CAtmosphereConditioningBehaviour acb = conditioner.GetComponent<CAtmosphereConditioningBehaviour>();
			
			if(acb.IsAtmosphereConditioningActive)
			{
				combinedCapacitySupport += acb.AtmosphereCapacitySupport;
			}
		}

		// Apply the global quality value for all facilities
		float atmosphereQuality = combinedCapacitySupport / combinedVolume;

		// Cap the quality to 120%
		if(atmosphereQuality > 1.2f) 
			atmosphereQuality = 1.2f;

		ShipAtmosphericQuality = atmosphereQuality;
	}

	public void OnGUI()
	{
        return;
		string shipLifeSupportOutput = "ShipLifeSupportInfo\n";
		shipLifeSupportOutput += string.Format("\tQuality: [{0}%]\n", 
		                                       Math.Round(ShipAtmosphericQuality * 100.0f, 1)); 

		string generatorOutput = "GeneratorInfo\n";
		foreach(GameObject generator in m_AtmosphereGenerators)
		{
			CFacilityInterface fi = CUtility.FindInParents<CFacilityInterface>(generator);
			CAtmosphereGeneratorBehaviour agb = generator.GetComponent<CAtmosphereGeneratorBehaviour>();

			generatorOutput += string.Format("\t[{0}] Within Facility [{1}] Type [{2}] \n\t\tIsGenerationActive: [{3}] GenRate: [{4}]\n", 
			                                 generator.name,  
			                                 fi.FacilityId, 
			                                 fi.FacilityType,
			                                 agb.IsAtmosphereGenerationActive,
			                                 Math.Round(agb.AtmosphereGenerationRate, 2));                                  
		}

		string conditionerOutput = "ConditionerInfo\n";
		foreach(GameObject conditioner in m_AtmosphereConditioners)
		{
			CFacilityInterface fi = CUtility.FindInParents<CFacilityInterface>(conditioner);
			CAtmosphereConditioningBehaviour acb = conditioner.GetComponent<CAtmosphereConditioningBehaviour>();
			
			conditionerOutput += string.Format("\t[{0}] Within Facility [{1}] Type [{2}] \n\t\tIsConditioningActive: [{3}] SupportCap: [{4}]\n", 
			                                   conditioner.name,
			                                   fi.FacilityId, 
			                                   fi.FacilityType,
			                                   acb.IsAtmosphereConditioningActive,
			                                   Math.Round(acb.AtmosphereCapacitySupport, 2));	                                  
		}

		string facilitiesOutput = "FacilityAtmosphereInfo\n";
		foreach(GameObject facility in gameObject.GetComponent<CShipFacilities>().Facilities)
		{
			CFacilityInterface fi = facility.GetComponent<CFacilityInterface>();
			CFacilityAtmosphere fa = facility.GetComponent<CFacilityAtmosphere>();

			facilitiesOutput += string.Format("\tFacility [{0}] Type [{1}] \n\t\tTotalVol: [{2}] Quantity: [{3}] RefilRate: [{4}] ConsRate: [{5}]\n", 
			                                  fi.FacilityId, 
			                                  fi.FacilityType,
			                                  Math.Round(fa.AtmosphereVolume, 2),
			                                  Math.Round(fa.AtmosphereQuantity, 2),
			                                  Math.Round(fa.AtmosphereRefillRate, 2),
			                                  Math.Round(fa.AtmosphereConsumeRate, 2));

		}
		
		float boxWidth = 500;
		float boxHeight = 600;
		GUI.Label(new Rect(Screen.width / 2, 0.0f, boxWidth, boxHeight),
		          "Atmosphere Status'\n" + shipLifeSupportOutput + generatorOutput + conditionerOutput + facilitiesOutput);
	}
}
