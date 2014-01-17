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

	private CNetworkVar<float> m_ShipAtmosphericQuality = null;

	// Member Properties
	public float ShipAtmosphericQuality
	{
		get { return(m_ShipAtmosphericQuality.Get()); }
		
		[AServerOnly]
		set { m_ShipAtmosphericQuality.Set(value); }
	}

	
	// Member Methods
	public override void InstanceNetworkVars()
	{
		m_ShipAtmosphericQuality = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
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

	public void RegisterLifeSupportSystem(GameObject _LifeSupportSystem)
	{
		if(!m_LifeSupportSystems.Contains(_LifeSupportSystem))
		{
			m_LifeSupportSystems.Add(_LifeSupportSystem);
		}
	}

	public void UnregisterLifeSupportSystem(GameObject _LifeSupportSystem)
	{
		if(m_LifeSupportSystems.Contains(_LifeSupportSystem))
		{
			m_LifeSupportSystems.Remove(_LifeSupportSystem);
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
			foreach(GameObject ls in m_LifeSupportSystems)
			{
				CLifeSupportSystem lss = ls.GetComponent<CLifeSupportSystem>();

				if(lss.IsAtmosphereGenerationActive)
				{
					combinedOutput += lss.AtmosphereGenerationRate;
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
		shipLifeSupportOutput += string.Format("\tQuality: [{0}%]\n", 
		                                       Math.Round(ShipAtmosphericQuality * 100.0f, 1)); 

		string lifeSupportOutput = "LifeSupportInfo\n";
		foreach(GameObject ls in m_LifeSupportSystems)
		{
			CFacilityInterface fi = null;
			if(ls.GetComponent<CBridgeLifeSupportSystem>() == null)
				fi = ls.GetComponent<CFacilityInterface>();
			else
				fi = ls.transform.parent.GetComponent<CFacilityInterface>();

			CLifeSupportSystem lss = ls.GetComponent<CLifeSupportSystem>();

			lifeSupportOutput += string.Format("\tFacility [{0}] Type [{1}] \n\t\tIsGenActive: [{2}] GenRate: [{3}] SupportCap: [{4}]\n", 
			                                   fi.FacilityId, 
			                                   fi.FacilityType,
			                                   lss.IsAtmosphereGenerationActive,
			                                   Math.Round(lss.AtmosphereGenerationRate, 2),
			                                   Math.Round(lss.AtmosphereCapacitySupport, 2));	                                  
		}

		string facilitiesOutput = "FacilityAtmosphereInfo\n";
		foreach(GameObject facility in gameObject.GetComponent<CShipFacilities>().GetAllFacilities())
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
			          "Atmosphere Status'\n" + shipLifeSupportOutput + lifeSupportOutput + facilitiesOutput);
	}
}
