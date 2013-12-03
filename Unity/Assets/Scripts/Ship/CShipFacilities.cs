//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShipHull.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CShipFacilities : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties
	
	
// Member Fields
	uint m_uiFacilityIdCount;	
	Dictionary<uint, GameObject> m_Facilities = new Dictionary<uint, GameObject>();
	

// Member Methods


	public bool ValidateCreateFacility(CFacilityInterface.EFacilityType _eType, uint _uiFacilityId, uint _uiExpansionPortId)
	{
		return (true);
	}


	public GameObject CreateFacility(CFacilityInterface.EFacilityType _eType, uint _uiFacilityId = uint.MaxValue, uint _uiExpansionPortId = uint.MaxValue, uint _uiAttachToId = uint.MaxValue)
	{
		CExpansionPortInterface cExpansionPort = null;
		if(_uiExpansionPortId != uint.MaxValue && _uiAttachToId != uint.MaxValue)
		{
			cExpansionPort = m_Facilities[_uiFacilityId].GetComponent<CFacilityInterface>().GetExpansionPort(_uiExpansionPortId).GetComponent<CExpansionPortInterface>();
			
			if(cExpansionPort.HasAttachedFacility == true)
			{
				Debug.LogWarning("Failed to create new room. Port already in use");
				return(null);
			}
		}
		
		uint uiFacilityId = ++m_uiFacilityIdCount;
		
		CGame.ENetworkRegisteredPrefab eRegisteredPrefab = CFacilityInterface.GetFacilityPrefab(_eType);
		GameObject cNewFacilityObject = CNetwork.Factory.CreateObject(eRegisteredPrefab);
		
		CFacilityInterface roomInterface = cNewFacilityObject.GetComponent<CFacilityInterface>();
		roomInterface.FacilityId = uiFacilityId;
		roomInterface.FacilityType = _eType;
		
		cNewFacilityObject.transform.parent = transform;
		
		if(cExpansionPort != null)
			cExpansionPort.Attach(_uiAttachToId, cNewFacilityObject);			
			
		cNewFacilityObject.GetComponent<CNetworkView>().SyncParent();
		cNewFacilityObject.GetComponent<CNetworkView>().SyncTransformPosition();
		cNewFacilityObject.GetComponent<CNetworkView>().SyncTransformRotation();
		
		m_Facilities.Add(uiFacilityId, cNewFacilityObject);
		
		return (cNewFacilityObject);
	}
	
	
	public List<GameObject> GetAllFacilities()
	{
		List<GameObject> ReturnList = new List<GameObject>();
		
		foreach (KeyValuePair<uint,GameObject> Entry in m_Facilities)
		{
			ReturnList.Add(Entry.Value);
		}
		
		return (ReturnList);
	}

	public GameObject GetFacility(uint _uiFacilityId)
	{
		return (m_Facilities[_uiFacilityId]);
	}
};
