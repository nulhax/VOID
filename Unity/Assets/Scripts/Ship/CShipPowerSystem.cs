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
	private List<GameObject> m_PowerGeneratorSystems = new List<GameObject>();
	
	// Member Properties
	
	
	// Member Methods
	public override void InstanceNetworkVars()
	{
		//m_fShipAtmosphericQuality = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}
	
	public void OnNetworkVarSync(INetworkVar _cVarInstance)
	{
		
	}
	
	public void Update()
	{
		if(CNetwork.IsServer)
		{
			UpdateFacilityPowerDistribution();
		}
	}
	
	public void RegisterPowerGeneratorSystem(GameObject _PowerGeneratorSystem)
	{
		if(!m_PowerGeneratorSystems.Contains(_PowerGeneratorSystem))
		{
			m_PowerGeneratorSystems.Add(_PowerGeneratorSystem);
		}
	}
	
	public void UnregisterPowerGeneratorSystem(GameObject _PowerGeneratorSystem)
	{
		if(m_PowerGeneratorSystems.Contains(_PowerGeneratorSystem))
		{
			m_PowerGeneratorSystems.Remove(_PowerGeneratorSystem);
		}
	}
	
	[AServerOnly]
	private void UpdateFacilityPowerDistribution()
	{
	}
	
	public void OnGUI()
	{
//		string shipLifeSupportOutput = "ShipLifeSupportInfo\n";
//		shipLifeSupportOutput += string.Format("\tAtmosQual: {0}%\n", 
//		                                       Math.Round(ShipAtmosphericQuality * 100.0f, 1)); 
//		
//		string lifeSupportOutput = "LifeSupportInfo\n";
//		foreach(GameObject ls in m_LifeSupportSystems)
//		{
//			lifeSupportOutput += string.Format("\tFacility [{0}] Type [{1}] AtmosDist: {2} AtmosSupp: {3}\n", 
//			                                   ls.GetComponent<CFacilityInterface>().FacilityId, 
//			                                   ls.GetComponent<CFacilityInterface>().FacilityType,
//			                                   Math.Round(ls.GetComponent<CLifeSupportSystem>().AtmosphereDistributionRate, 2),
//			                                   Math.Round(ls.GetComponent<CLifeSupportSystem>().AtmosphereCapacitySupport, 2));	                                  
//		}
//		
//		string facilitiesOutput = "FacilityAtmosphereInfo\n";
//		foreach(GameObject facility in gameObject.GetComponent<CShipFacilities>().GetAllFacilities())
//		{
//			facilitiesOutput += string.Format("\tFacility [{0}] Type [{1}] \n\t\tAtmosTotVol: {3} AtmosQuant: {2} AtmosRefil: {4} AtmosCons: {5}\n", 
//			                                  facility.GetComponent<CFacilityInterface>().FacilityId, 
//			                                  facility.GetComponent<CFacilityInterface>().FacilityType,
//			                                  Math.Round(facility.GetComponent<CFacilityAtmosphere>().AtmosphereQuantity, 2),
//			                                  Math.Round(facility.GetComponent<CFacilityAtmosphere>().AtmosphereVolume, 2),
//			                                  Math.Round(facility.GetComponent<CFacilityAtmosphere>().AtmosphereRefillRate, 2),
//			                                  Math.Round(facility.GetComponent<CFacilityAtmosphere>().AtmosphereConsumeRate, 2));
//			
//		}
//		
//		float boxWidth = 600;
//		float boxHeight = 250;
//		GUI.Label(new Rect(Screen.width / 2, 0.0f, boxWidth, boxHeight),
//		          "Atmosphere Status'\n" + shipLifeSupportOutput + lifeSupportOutput + facilitiesOutput);
	}
}
