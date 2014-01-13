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


	public delegate void OnFacilityCreate(GameObject _cFacilty);
	public delegate void OnFacilityDestroy(GameObject _cFacility);
	
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


	public bool ValidateCreateFacility(CFacilityInterface.EFacilityType _eType, uint _uiFacilityId, uint _uiExpansionPortId)
	{
		return (true);
	}


    [AServerMethod]
	public GameObject CreateFacility(CFacilityInterface.EFacilityType _eType, uint _uiFacilityId = uint.MaxValue, uint _uiExpansionPortId = uint.MaxValue, uint _uiAttachToId = uint.MaxValue)
	{
		CExpansionPortInterface cExpansionPort = null;
		
		if (_uiExpansionPortId != uint.MaxValue &&
			_uiAttachToId != uint.MaxValue)
		{
			cExpansionPort = m_mFacilities[_uiFacilityId].GetComponent<CFacilityExpansion>().GetExpansionPort(_uiExpansionPortId).GetComponent<CExpansionPortInterface>();
			
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
		cFacilityInterface.FacilityId = uiFacilityId;
		cFacilityInterface.FacilityType = _eType;
		
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


    [AServerMethod]
	public void DestroyFacility(GameObject _Facility)
	{
		if (EventOnFaciltiyDestroy != null)
		{
			EventOnFaciltiyDestroy(_Facility);
		}
		
		Debug.Log("DestroyFacility(" + _Facility.ToString() + "); was called, but the function is empty so nothing happened. Durp.");
	}


    [AServerMethod]
	public List<GameObject> GetAllFacilities()
	{
		List<GameObject> ReturnList = new List<GameObject>(m_mFacilities.Values);
		
		return (ReturnList);
	}


    [AServerMethod]
	public GameObject GetFacility(uint _uiFacilityId)
	{
        if (_uiFacilityId >= m_mFacilities.Count)
        {
            return (null);
        }
        else
        {
            return (m_mFacilities[_uiFacilityId]);
        }
	}


    [AServerMethod]
	public List<GameObject> FindFacilities(CFacilityInterface.EFacilityType _eType)
	{
        if (m_mFacilityObjects.ContainsKey(_eType))
        {
            return (m_mFacilityObjects[_eType]);
        }
        else
        {
            return (null);
        }
	}


	// Member Fields


	uint m_uiFacilityIdCount = 0;


	Dictionary<uint, GameObject> m_mFacilities = new Dictionary<uint, GameObject>();
	Dictionary<CFacilityInterface.EFacilityType, List<GameObject>> m_mFacilityObjects = new Dictionary<CFacilityInterface.EFacilityType, List<GameObject>>();


};
