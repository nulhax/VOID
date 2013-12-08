//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CGravityGeneration.cs
//  Description :   Class script for generating gravity
//
//  Author  	:  Nathan Boon
//  Mail    	:  Nathan.Boon@gmail.com
//

// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Implementation
public class CGravityGeneration : CNetworkMonoBehaviour
{
	// Member Data
	const float m_fBaseGravity = -9.81f;
	List<GameObject>   NearbyFacilities;
	CNetworkVar<float> m_fTriggerRadius;
	CNetworkVar<float> m_fCurrentGravityOutput;
	
	// Member Properties
	float TriggerRadius        { get { return (m_fTriggerRadius.Get()); }        set { TriggerRadius = value; } }
	float CurrentGravityOutput { get { return (m_fCurrentGravityOutput.Get()); } set { CurrentGravityOutput = value; } }
	
	// Member Functions
	public override void InstanceNetworkVars()
	{
		m_fTriggerRadius        = new CNetworkVar<float>(OnNetworkVarSync);
		m_fCurrentGravityOutput = new CNetworkVar<float>(OnNetworkVarSync);
	}
	
	void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
		// Do nothing
	}
	
	void Start()
	{	
		if (CNetwork.IsServer)
		{
			// Signup for events
			CGame.Ship.GetComponent<CShipFacilities>().EventOnFaciltiyCreate  += new CShipFacilities.OnFacilityCreate(OnFacilityCreate);
			CGame.Ship.GetComponent<CShipFacilities>().EventOnFaciltiyDestroy += new CShipFacilities.OnFacilityDestroy(OnFacilityDestroy);		
		}
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
				
				CGame.Ship.GetComponent<CFacilityGravity>().AddGravitySource(gameObject);
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
				
				CGame.Ship.GetComponent<CFacilityGravity>().RemoveGravitySource(gameObject);
			}
			
			// Default
			else
			{
				// Do nothing
			}
		}
	}
			
	[AServerMethod]
	void UpdateTriggerRadius(float _fNewTriggerRadius)
	{
		m_fTriggerRadius.Set(_fNewTriggerRadius);
	}
	
	[AServerMethod]
	void UpdateCurrentGravityOutput(float _fNewCurrentGravityOutput)
	{
		m_fCurrentGravityOutput.Set(_fNewCurrentGravityOutput);
	}
	
	public float GetGravityOutput()
	{
		return (m_fCurrentGravityOutput.Get());
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
	void Update(){}
}