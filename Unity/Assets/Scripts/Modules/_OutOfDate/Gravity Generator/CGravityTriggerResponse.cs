//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CGravityTriggerResponse.cs
//  Description :   Class script for the gravity trigger
//
//  Author  	:  Nathan Boon
//  Mail    	:  Nathan.Boon@gmail.com
//

// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Implementation
public class CGravityTriggerResponse : CNetworkMonoBehaviour
{
	// Member Data
	List<GameObject>   NearbyFacilities;
	CNetworkVar<float> m_fTriggerRadius;
	CNetworkVar<float> m_fCurrentGravityOutput;
	
	// Member Properties
	float TriggerRadius        { get { return (m_fTriggerRadius.Get()); } set { TriggerRadius = value; } }
	float CurrentGravityOutput { get { return (m_fCurrentGravityOutput.Get()); } set { CurrentGravityOutput = value; } }
	
	// Member Functions
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_fTriggerRadius        = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync);
		m_fCurrentGravityOutput = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync);
	}
	
	void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
		// Do nothing
	}
	
	void Start()
	{	
		// Signup for events
		CGameShips.Ship.GetComponent<CShipFacilities>().EventOnFaciltiyCreate  += new CShipFacilities.OnFacilityCreate(OnFacilityCreate);
		CGameShips.Ship.GetComponent<CShipFacilities>().EventOnFaciltiyDestroy += new CShipFacilities.OnFacilityDestroy(OnFacilityDestroy);
	}


	void OnFacilityCreate(GameObject _Facility)
	{
		// If trigger radius >= (Vec1 - Vec2).magnitude 
		if (m_fTriggerRadius.Get() >= ((_Facility.transform.position - transform.position).magnitude))
		{
			// If facility is NOT a member of the facility list
			if (!ListSearch(_Facility))
			{
				NearbyFacilities.Add(_Facility);
				
				// Apply gravity to facility
			}
			
			// Default
			else
			{
				Debug.LogError("CGravityTriggerResponse.cs attempted" +
				               "to call OnFacilityCreate(" + _Facility.ToString() + ")." +
				               "Event parameter object is already a member of the facility list.");
			}
		}
	}

	void OnFacilityDestroy(GameObject _Facility)
	{
		// If trigger radius <= (Vec1 - Vec2).magnitude 
		if (m_fTriggerRadius.Get() <= ((_Facility.transform.position - transform.position).magnitude))
		{
			// If facility IS a member of the facility list
			if (ListSearch(_Facility))
			{
				NearbyFacilities.Remove(_Facility);
				
				// Remove gravity from facility
			}
			
			// Default
			else
			{
				// Do nothing
			}
		}
	}
			
	[AServerOnly]
	void UpdateTriggerRadius(float _fNewTriggerRadius)
	{
		m_fTriggerRadius.Set(_fNewTriggerRadius);
	}
	
	[AServerOnly]
	void UpdateCurrentGravityOutput(float _fNewCurrentGravityOutput)
	{
		m_fCurrentGravityOutput.Set(_fNewCurrentGravityOutput);
	}
	
	bool ListSearch(GameObject _Object)
	{
		// Temporary return variable
		bool bReturn = false;
		
		// Search the list for a matching entry
		foreach (GameObject Entry in NearbyFacilities)
		{
			// If matching entry is found
			if (Entry == _Object)
			{
				// Set return value
				bReturn = true;
				break;
			}
		}
		
		return (bReturn);
	}
	
	// Unused functions
	void UForEachte(){}
}