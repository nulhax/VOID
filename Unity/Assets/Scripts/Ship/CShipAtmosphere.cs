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


public class CShipAtmosphere : CNetworkMonoBehaviour 
{

// Member Types


// Member Delegates & Events


    public delegate void HandleAtmospherePreUpdate();
    public event HandleAtmospherePreUpdate EventAtmospherePreUpdate;


    public delegate void HandleAtmospherePostUpdate();
    public event HandleAtmospherePostUpdate EventAtmospherePostUpdate;


// Member Properties
	
	public float ShipAtmosphereGeneration
	{
		get { return(m_fGlobalAtmosphereGenerationRate.Value); }
		set { m_fGlobalAtmosphereGenerationRate.Value = value; }
	}
	
	public float ShipAtmosphericQuality
	{
		get { return(m_fGlobalAtmosphericQuality.Get()); }
	}

	
// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_fGlobalAtmosphericQuality = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_fGlobalAtmosphereGenerationRate = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}


    public void Update()
    {
        if (CNetwork.IsServer)
        {
            // Notify observers
            if (EventAtmospherePreUpdate != null) EventAtmospherePreUpdate();

            // Create list of facilities that require refilling
            List<GameObject> aRefillingFacilities = new List<GameObject>();

            foreach (KeyValuePair<CFacilityInterface.EType, List<GameObject>> tEntry in CFacilityInterface.GetAllFacilities())
            {
                tEntry.Value.ForEach((GameObject cFacilityObject) =>
                {
                    CFacilityAtmosphere cFacilityAtmosphere = cFacilityObject.GetComponent<CFacilityAtmosphere>();

                    if (cFacilityAtmosphere.IsRefillingRequired)
                    {
                        aRefillingFacilities.Add(cFacilityObject);
                    }
                });
            }

            // Calculate the total generation available 
			float fTotalGeneration = ShipAtmosphereGeneration;
			m_AtmosphereGenerators.ForEach((GameObject cGeneratorObject) =>
            {
                fTotalGeneration += cGeneratorObject.GetComponent<CAtmosphereGeneratorBehaviour>().AtmosphereGenerationRate;
            });

			// Set the network var
			ShipAtmosphereGeneration = fTotalGeneration;

            // Calculate delta atmosphere
            float fDeltaGeneration = fTotalGeneration * Time.deltaTime;

            if (aRefillingFacilities.Count > 0)
            {
                // TODO: Make facilities use the unused atmosphere
                float fDeltaFacilityGeneration = fDeltaGeneration / aRefillingFacilities.Count;
                float fUnusedAtmosphere = 0.0f;

                aRefillingFacilities.ForEach((GameObject _cRefillingFacility) =>
                {
                    // Cap max generation to 5%
                    if (fDeltaFacilityGeneration > _cRefillingFacility.GetComponent<CFacilityAtmosphere>().Volume * 0.05f * Time.deltaTime)
                    {
                        fDeltaFacilityGeneration = _cRefillingFacility.GetComponent<CFacilityAtmosphere>().Volume * 0.05f * Time.deltaTime;
                    }

                    fUnusedAtmosphere += _cRefillingFacility.GetComponent<CFacilityAtmosphere>().ChangeQuantityByAmount(fDeltaFacilityGeneration);
                });
            }

            if (EventAtmospherePostUpdate != null) EventAtmospherePostUpdate();
        }
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        // Empty
    }

	[AServerOnly]
	public void RegisterAtmosphereGenerator(GameObject _Generator)
	{
		if(!m_AtmosphereGenerators.Contains(_Generator))
		{
			m_AtmosphereGenerators.Add(_Generator);
		}
	}
	
	[AServerOnly]
	public void UnregisterAtmosphereGenerator(GameObject _Generator)
	{
		if(m_AtmosphereGenerators.Contains(_Generator))
		{
			m_AtmosphereGenerators.Remove(_Generator);
		}
	}


    /*
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
    */


    /*
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
			where facility.GetComponent<CFacilityAtmosphere>().IsAtmosphereRefillingRequired
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
    */


    /*
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
     * */


// Member Fields

	private List<GameObject> m_AtmosphereGenerators = new List<GameObject>();

    CNetworkVar<float> m_fGlobalAtmosphericQuality = null;
	CNetworkVar<float> m_fGlobalAtmosphereGenerationRate = null;

}
