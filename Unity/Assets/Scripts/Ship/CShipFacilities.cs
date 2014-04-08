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


	public delegate void HandleFacilityEvent(CFacilityInterface _cFacilty);
	
	public event HandleFacilityEvent EventOnFaciltiyCreate;
	public event HandleFacilityEvent EventOnFaciltiyDestroy;

// Member Fields
	
	public CGrid m_ShipGrid = null;
	private uint m_FacilityIdCount = 0;
	
	private Dictionary<uint, GameObject> m_FacilityObjects = new Dictionary<uint, GameObject>();
	private Dictionary<CFacilityInterface.EType, List<GameObject>> m_FacilityTypes = new Dictionary<CFacilityInterface.EType, List<GameObject>>();


// Member Properties


	[AServerOnly]
	public List<GameObject> Facilities
	{
		get { return (new List<GameObject>(m_FacilityObjects.Values)); }
	}


// Member Methods

	
    public void RegisterFacility(CFacilityInterface _Facility)
    {
		if(CNetwork.IsServer)
		{
	        // Generate id for facility
	        uint facilityId = ++ m_FacilityIdCount;

	        // Give facility an id
			_Facility.FacilityId = facilityId;

			// Parent facility to ship
			_Facility.NetworkView.SetParent(gameObject.GetComponent<CNetworkView>().ViewId);
		}

        // Add facility to dictionaries
		m_FacilityObjects.Add(_Facility.FacilityId, _Facility.gameObject);

		if (!m_FacilityTypes.ContainsKey(_Facility.FacilityType))
        {
			m_FacilityTypes.Add(_Facility.FacilityType, new List<GameObject>());
        }

		m_FacilityTypes[_Facility.FacilityType].Add(_Facility.gameObject);

        // Notify observers
        if (EventOnFaciltiyCreate != null) 
			EventOnFaciltiyCreate(_Facility);

		// Export the tiles to the grid
		m_ShipGrid.ImportPreExistingTiles(_Facility.FacilityTiles.ToArray());
    }

	
	public void UnregisterFacility(CFacilityInterface _Facility)
    {
        // Remove facility from dictionaries
		m_FacilityObjects.Remove(_Facility.FacilityId);
		m_FacilityTypes[_Facility.FacilityType].Remove(_Facility.gameObject);

        // Notify observers
        if (EventOnFaciltiyDestroy != null) 
			EventOnFaciltiyDestroy(_Facility);
	}
	
	public GameObject GetFacility(uint _uiFacilityId)
	{
        if (_uiFacilityId >= m_FacilityObjects.Count)
        {
            return (null);
        }
        else
        {
            return (m_FacilityObjects[_uiFacilityId]);
        }
	}
	
	public List<GameObject> FindFacilities(CFacilityInterface.EType _eType)
	{
        if (m_FacilityTypes.ContainsKey(_eType))
        {
            return (m_FacilityTypes[_eType]);
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

};
