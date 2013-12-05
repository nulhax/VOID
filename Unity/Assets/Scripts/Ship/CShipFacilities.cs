//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShipFacilities.cs
//  Description :   --------------------------
//
//  Author  	:  Multiple
//  Mail    	:  N/A
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
	public delegate void OnFacilityCreate(GameObject _Facilty);
	public delegate void OnFacilityDestroy(GameObject _Facility);
	
	public event OnFacilityCreate EventOnFaciltiyCreate;
	public event OnFacilityDestroy EventOnFaciltiyDestroy;

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
				Debug.LogWarning("Failed to create new room. Port is already in use.");
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
		
		// Facility creation event
		if (EventOnFaciltiyCreate != null)
		{
			EventOnFaciltiyCreate(cNewFacilityObject);
		}
		
		return (cNewFacilityObject);
	}
	
	public void DestroyFacility(GameObject _Facility)
	{
		if (EventOnFaciltiyDestroy != null)
		{
			EventOnFaciltiyDestroy(_Facility);
		}
		
		Debug.Log("DestroyFacility(" + _Facility.ToString() + "); was called, but the function is empty so nothing happened. Durp.");
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
