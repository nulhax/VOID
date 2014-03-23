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


	[AServerOnly]
	public List<GameObject> Facilities
	{
		get { return (new List<GameObject>(m_mFacilityObjects.Values)); }
	}


// Member Methods


    [AServerOnly]
    public void RegisterFacility(GameObject _cFacilityObject)
    {
        CFacilityInterface cFacilityInterface = _cFacilityObject.GetComponent<CFacilityInterface>();

        // Generate id for facility
        uint uiFacilityId = ++ m_uiFacilityIdCount;

        // Give facility an id
        cFacilityInterface.FacilityId = uiFacilityId;

        // Add facility to dictionaries
        m_mFacilityObjects.Add(uiFacilityId, _cFacilityObject);

        if (!m_mFacilityTypes.ContainsKey(cFacilityInterface.FacilityType))
        {
            m_mFacilityTypes.Add(cFacilityInterface.FacilityType, new List<GameObject>());
        }

        m_mFacilityTypes[cFacilityInterface.FacilityType].Add(_cFacilityObject);

        // Notify observers
        if (EventOnFaciltiyCreate != null) EventOnFaciltiyCreate(_cFacilityObject);
    }


    [AServerOnly]
    public void UnregisterFacility(GameObject _cFacilityObject)
    {
        CFacilityInterface cFacilityInterface = _cFacilityObject.GetComponent<CFacilityInterface>();

        // Remove facility from dictionaries
        m_mFacilityObjects.Remove(cFacilityInterface.FacilityId);
        m_mFacilityTypes[cFacilityInterface.FacilityType].Remove(_cFacilityObject);

        // Notify observers
        if (EventOnFaciltiyDestroy != null) EventOnFaciltiyDestroy(_cFacilityObject);
    }


    [AClientOnly]
    public void AddNewlyCreatedFacility(GameObject _Facility, uint _FacilityId, CFacilityInterface.EType _FacilityType)
    {
        // Index facility against its Facility Id
        m_mFacilityObjects.Add(_FacilityId, _Facility);

        // Index facility against its Facility Type
        if (!m_mFacilityTypes.ContainsKey(_FacilityType))
        {
            m_mFacilityTypes.Add(_FacilityType, new List<GameObject>());
        }

        m_mFacilityTypes[_FacilityType].Add(_Facility);
    }


    [AServerOnly]
	public GameObject GetFacility(uint _uiFacilityId)
	{
        if (_uiFacilityId >= m_mFacilityObjects.Count)
        {
            return (null);
        }
        else
        {
            return (m_mFacilityObjects[_uiFacilityId]);
        }
	}


    [AServerOnly]
	public List<GameObject> FindFacilities(CFacilityInterface.EType _eType)
	{
        if (m_mFacilityTypes.ContainsKey(_eType))
        {
            return (m_mFacilityTypes[_eType]);
        }
        else
        {
            return (null);
        }
	}


    void Start()
    {
        // Empty
    }


	// Member Fields


	uint m_uiFacilityIdCount = 0;


	Dictionary<uint, GameObject> m_mFacilityObjects = new Dictionary<uint, GameObject>();
	Dictionary<CFacilityInterface.EType, List<GameObject>> m_mFacilityTypes = new Dictionary<CFacilityInterface.EType, List<GameObject>>();


};
