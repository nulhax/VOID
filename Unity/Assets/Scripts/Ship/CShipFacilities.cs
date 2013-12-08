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


// Member Methods


	public void Start()
	{
		
	}


	public void OnDestroy()
	{
		
	}


	public bool ValidateCreateFacility(CFacilityInterface.EType _eType, uint _uiFacilityId, uint _uiExpansionPortId)
	{
		return (true);
	}


	public GameObject CreateFacility(CFacilityInterface.EType _eType, uint _uiFacilityId = uint.MaxValue, uint _uiExpansionPortId = uint.MaxValue, uint _uiAttachToId = uint.MaxValue)
	{
		CExpansionPortInterface cExpansionPort = null;


		if (_uiExpansionPortId != uint.MaxValue &&
			_uiAttachToId != uint.MaxValue)
		{
			cExpansionPort = m_mFacilities[_uiFacilityId].GetComponent<CFacilityInterface>().GetExpansionPort(_uiExpansionPortId).GetComponent<CExpansionPortInterface>();
			
			if(cExpansionPort.HasAttachedFacility == true)
			{
				Debug.LogWarning("Failed to create new room. Port is already in use.");
				return(null);
			}
		}
		
		// Generate facility identifier
		uint uiFacilityId = ++m_uiFacilityIdCount;
		
		// Retrieve the facility prefab
		CGame.ENetworkRegisteredPrefab eRegisteredPrefab = CFacilityInterface.GetFacilityPrefab(_eType);

		// Create facility
		GameObject cNewFacilityObject = CNetwork.Factory.CreateObject(eRegisteredPrefab);
		
		// Set facility properties
		CFacilityInterface cFacilityInterface = cNewFacilityObject.GetComponent<CFacilityInterface>();
		cFacilityInterface.Id = uiFacilityId;
		cFacilityInterface.Type = _eType;
		
		// Set facility parent
		cNewFacilityObject.GetComponent<CNetworkView>().SetParent(GetComponent<CNetworkView>().ViewId);

		// Attach facility expansion port to parent expansion port
		if (cExpansionPort != null)
		{
			cExpansionPort.Attach(_uiAttachToId, cNewFacilityObject);
		}
		
		// Sync position & rotation
		cNewFacilityObject.GetComponent<CNetworkView>().SyncTransformPosition();
		cNewFacilityObject.GetComponent<CNetworkView>().SyncTransformRotation();
		
		// Index facility against its Facility Id
		m_mFacilities.Add(uiFacilityId, cNewFacilityObject);

		// Index facility against its Facility Type
		if (!m_mFacilityObjects.ContainsKey(_eType))
		{
			m_mFacilityObjects.Add(_eType, new List<GameObject>());
		}

		m_mFacilityObjects[_eType].Add(cNewFacilityObject);
		
		// Notify facility creation observers
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
		
		foreach (KeyValuePair<uint,GameObject> Entry in m_mFacilities)
		{
			ReturnList.Add(Entry.Value);
		}
		
		return (ReturnList);
	}


	public GameObject GetFacility(uint _uiFacilityId)
	{
		return (m_mFacilities[_uiFacilityId]);
	}


	public List<GameObject> FindFacilities(CFacilityInterface.EType _eType)
	{
		return (m_mFacilityObjects[_eType]);
	}


	// Member Fields


	uint m_uiFacilityIdCount = 0;


	Dictionary<uint, GameObject> m_mFacilities = new Dictionary<uint, GameObject>();
	Dictionary<CFacilityInterface.EType, List<GameObject>> m_mFacilityObjects = new Dictionary<CFacilityInterface.EType, List<GameObject>>();


};
