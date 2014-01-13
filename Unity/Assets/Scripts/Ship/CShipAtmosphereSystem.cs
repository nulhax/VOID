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


public class CShipAtmosphereSystem : MonoBehaviour 
{
	// Member Types


	// Member Delegates & Events

	
	// Member Fields
	private List<GameObject> m_AtmosphereDistributors = new List<GameObject>();

	// Member Properties

	
	// Member Methods
	public void Update()
	{
		// Get the facilities that are leaking and requiring refill
		var facilitiesRequiringRefilling = 
			from facility in gameObject.GetComponent<CShipFacilities>().GetAllFacilities()
			where facility.GetComponent<CFacilityAtmosphere>().IsAtmosphereRefillingEnabled && facility.GetComponent<CFacilityAtmosphere>().IsAtmosphereLeaking
			select facility;

		// If there are facilities requiring this, calculate the refilling
		if(facilitiesRequiringRefilling.ToArray().Length != 0)
		{
			// Get the combined output of all atmosphere distributors
			float combinedOutput = 0.0f;
			foreach(GameObject dist in m_AtmosphereDistributors)
			{
				combinedOutput += dist.GetComponent<CLifeSupportAtmosphereDistribution>().AtmosphereDistributionRate;
			}

			// Calculate the output for each facility evenly
			float evenDistribution = combinedOutput / facilitiesRequiringRefilling.ToArray().Length;

			// Apply this regeneration to the facilities
			foreach(GameObject facility in facilitiesRequiringRefilling)
			{
				//facility.GetComponent<CFacilityAtmosphere>().
			}
		}
	}

	public void RegisterAtmosphereDistributor(GameObject _LifeSupportDistributor)
	{
		if(!m_AtmosphereDistributors.Contains(_LifeSupportDistributor))
		{
			m_AtmosphereDistributors.Add(_LifeSupportDistributor);
		}
	}

	public void UnregisterAtmosphereDistributor(GameObject _LifeSupportDistributor)
	{
		if(m_AtmosphereDistributors.Contains(_LifeSupportDistributor))
		{
			m_AtmosphereDistributors.Remove(_LifeSupportDistributor);
		}
	}
}
