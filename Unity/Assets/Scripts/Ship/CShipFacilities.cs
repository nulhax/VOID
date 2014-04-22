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
	public delegate void HandleFacilityEvent(CFacilityInterface _Facilty);
	
	public event HandleFacilityEvent EventFaciltiyCreated;
	public event HandleFacilityEvent EventFaciltiyDestroyed;


	// Member Fields
	public CGrid m_ShipGrid = null;

	private uint m_FacilityIdCount = 0;
	
	private Dictionary<uint, GameObject> m_FacilityObjects = new Dictionary<uint, GameObject>();


	// Member Properties
	[AServerOnly]
	public List<GameObject> Facilities
	{
		get { return (new List<GameObject>(m_FacilityObjects.Values)); }
	}


	// Member Methods
	public void CreateFacility(List<CTile> _FacilityTiles)
	{
		// Import the tile information into the grid
		List<CTile> facilityTiles = m_ShipGrid.ImportTileInformation(_FacilityTiles);

		// Instantiate an empty facility

	}

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

        // Notify observers
        if (EventFaciltiyCreated != null) 
			EventFaciltiyCreated(_Facility);
    }

	
	public void UnregisterFacility(CFacilityInterface _Facility)
    {
        // Remove facility from dictionaries
		m_FacilityObjects.Remove(_Facility.FacilityId);

        // Notify observers
        if (EventFaciltiyDestroyed != null) 
			EventFaciltiyDestroyed(_Facility);
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
};
