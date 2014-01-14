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
	private List<GameObject> m_LifeSupportSystems = new List<GameObject>();
	private CNetworkVar<float> m_fShipAtmosphericQuality = null;

	// Member Properties
	public float ShipAtmosphericQuality
	{
		get { return(m_fShipAtmosphericQuality.Get()); }
		
		[AServerOnly]
		set { m_fShipAtmosphericQuality.Set(value); }
	}

	
	// Member Methods
	public override void InstanceNetworkVars()
	{
		m_fShipAtmosphericQuality = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}
	
	public void OnNetworkVarSync(INetworkVar _cVarInstance)
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

	public void RegisterLifeSupportSystem(GameObject _LifeSupportDistributor)
	{
		if(!m_LifeSupportSystems.Contains(_LifeSupportDistributor))
		{
			m_LifeSupportSystems.Add(_LifeSupportDistributor);
		}
	}

	public void UnregisterLifeSupportSystem(GameObject _LifeSupportDistributor)
	{
		if(m_LifeSupportSystems.Contains(_LifeSupportDistributor))
		{
			m_LifeSupportSystems.Remove(_LifeSupportDistributor);
		}
	}

	[AServerOnly]
	private void UpdateFacilityAtmosphereRefilling()
	{
		// Reset all facility refilling
		foreach(GameObject facility in gameObject.GetComponent<CShipFacilities>().GetAllFacilities())
		{
			facility.GetComponent<CFacilityAtmosphere>().AtmosphereRefillRate = 0.0f;
		}

		// Get the facilities that are requiring a refill of atmosphere (i.e, have a leak, hull breach, consumer)
		var facilitiesRequiringRefilling = 
			from facility in gameObject.GetComponent<CShipFacilities>().GetAllFacilities()
			where facility.GetComponent<CFacilityAtmosphere>().RequiresAtmosphereRefill
			select facility;
		
		// If there are facilities requiring this, calculate the refilling output
		int numFacilitiesRequiringRefill = facilitiesRequiringRefilling.ToArray().Length;
		if(numFacilitiesRequiringRefill != 0)
		{
			// Get the combined output of all atmosphere distributors
			float combinedOutput = 0.0f;
			foreach(GameObject dist in m_LifeSupportSystems)
			{
				combinedOutput += dist.GetComponent<CLifeSupportSystem>().AtmosphereDistributionRate;
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
		foreach(GameObject facility in gameObject.GetComponent<CShipFacilities>().GetAllFacilities())
		{
			combinedVolume += facility.GetComponent<CFacilityAtmosphere>().AtmosphereVolume;
		}

		// Calculate the combined quality capacity of all the life support system
		float combinedCapacitySupport = 0.0f;
		foreach(GameObject facility in m_LifeSupportSystems)
		{
			combinedCapacitySupport += facility.GetComponent<CLifeSupportSystem>().AtmosphereCapacitySupport;
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
		string shipLifeSupportOutput = "ShipLifeSupportInfo\n";
		shipLifeSupportOutput += string.Format("\tAtmosQual: {0}%\n", 
		                                       Math.Round(ShipAtmosphericQuality * 100.0f, 1)); 

		string lifeSupportOutput = "LifeSupportInfo\n";
		foreach(GameObject ls in m_LifeSupportSystems)
		{
			lifeSupportOutput += string.Format("\tFacility [{0}] Type [{1}] AtmosDist: {2} AtmosSupp: {3}\n", 
			                                  ls.GetComponent<CFacilityInterface>().FacilityId, 
			                                  ls.GetComponent<CFacilityInterface>().FacilityType,
			                                  Math.Round(ls.GetComponent<CLifeSupportSystem>().AtmosphereDistributionRate, 2),
			                                  Math.Round(ls.GetComponent<CLifeSupportSystem>().AtmosphereCapacitySupport, 2));	                                  
		}

		string facilitiesOutput = "FacilityAtmosphereInfo\n";
		foreach(GameObject facility in gameObject.GetComponent<CShipFacilities>().GetAllFacilities())
		{
			facilitiesOutput += string.Format("\tFacility [{0}] Type [{1}] \n\t\tAtmosTotVol: {3} AtmosQuant: {2} AtmosRefil: {4} AtmosCons: {5}\n", 
			                                  facility.GetComponent<CFacilityInterface>().FacilityId, 
			                                  facility.GetComponent<CFacilityInterface>().FacilityType,
			                                  Math.Round(facility.GetComponent<CFacilityAtmosphere>().AtmosphereQuantity, 2),
			                                  Math.Round(facility.GetComponent<CFacilityAtmosphere>().AtmosphereVolume, 2),
			                                  Math.Round(facility.GetComponent<CFacilityAtmosphere>().AtmosphereRefillRate, 2),
			                                  Math.Round(facility.GetComponent<CFacilityAtmosphere>().AtmosphereConsumeRate, 2));

		}
		
		float boxWidth = 600;
		float boxHeight = 250;
		GUI.Label(new Rect(Screen.width / 2, 0.0f, boxWidth, boxHeight),
			          "Atmosphere Status'\n" + shipLifeSupportOutput + lifeSupportOutput + facilitiesOutput);
	}
}
