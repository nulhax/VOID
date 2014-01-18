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


    [AServerOnly]
	public GameObject CreateFacility(CFacilityInterface.EType _eType, uint _uiFacilityId = uint.MaxValue, uint _uiExpansionPortId = uint.MaxValue, uint _uiAttachToId = uint.MaxValue)
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
		uint uiFacilityId = m_uiFacilityIdCount;
		
		// Retrieve the facility prefab
		CGameRegistrator.ENetworkPrefab eRegisteredPrefab = CFacilityInterface.GetPrefabType(_eType);

		// Create facility
		GameObject cNewFacilityObject = CNetwork.Factory.CreateObject(eRegisteredPrefab);
		
		// Set facility properties
		CFacilityInterface cFacilityInterface = cNewFacilityObject.GetComponent<CFacilityInterface>();
		cFacilityInterface.FacilityId = uiFacilityId;
		cFacilityInterface.FacilityType = _eType;
		m_uiFacilityIdCount++;
		
		// Set facility parent
		cNewFacilityObject.GetComponent<CNetworkView>().SetParent(GetComponent<CNetworkView>().ViewId);

		// Attach facility expansion port to parent expansion port
		if (cExpansionPort != null)
		{
			cExpansionPort.Attach(_uiAttachToId, cNewFacilityObject);
		}

		// Initialise the facility expansion ports
		cNewFacilityObject.GetComponent<CFacilityExpansion>().InitialiseExpansionPorts();
		
		// Sync position & rotation
		cNewFacilityObject.GetComponent<CNetworkView>().SyncTransformPosition();
		cNewFacilityObject.GetComponent<CNetworkView>().SyncTransformRotation();

		// Server adds the facility instantaniously
		AddNewlyCreatedFacility(cNewFacilityObject, uiFacilityId, _eType);

		// Notify facility creation observers
		if (EventOnFaciltiyCreate != null)
		{
			EventOnFaciltiyCreate(cNewFacilityObject);
		}

		return (cNewFacilityObject);
	}

	public void AddNewlyCreatedFacility(GameObject _Facility, uint _FacilityId, CFacilityInterface.EType _FacilityType)
	{
		// Index facility against its Facility Id
		m_mFacilities.Add(_FacilityId, _Facility);
		
		// Index facility against its Facility Type
		if (!m_mFacilityObjects.ContainsKey(_FacilityType))
		{
			m_mFacilityObjects.Add(_FacilityType, new List<GameObject>());
		}
		
		m_mFacilityObjects[_FacilityType].Add(_Facility);
	}


    [AServerOnly]
	public void DestroyFacility(GameObject _Facility)
	{
		if (EventOnFaciltiyDestroy != null)
		{
			EventOnFaciltiyDestroy(_Facility);
		}
		
		Debug.Log("DestroyFacility(" + _Facility.ToString() + "); was called, but the function is empty so nothing happened. Durp.");
	}


    [AServerOnly]
	public List<GameObject> GetAllFacilities()
	{
		List<GameObject> ReturnList = new List<GameObject>(m_mFacilities.Values);
		
		return (ReturnList);
	}


    [AServerOnly]
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


    [AServerOnly]
	public List<GameObject> FindFacilities(CFacilityInterface.EType _eType)
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
	Dictionary<CFacilityInterface.EType, List<GameObject>> m_mFacilityObjects = new Dictionary<CFacilityInterface.EType, List<GameObject>>();


};
