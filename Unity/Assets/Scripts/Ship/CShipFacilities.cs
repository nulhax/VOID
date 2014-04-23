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
	[AServerOnly]
	public void ImportNewGridTiles(List<CTile> _AllTiles, List<List<CTile>> _FacilityTiles)
	{
		// Import all of the tiles to the ship
		List<CTile> newTiles = m_ShipGrid.ImportTileInformation(_AllTiles);

		// Destoy all facilities
		foreach(GameObject facility in Facilities)
			Network.Destroy(facility);

		// Create all new facilities
		foreach(List<CTile> facilityTiles in _FacilityTiles)
			CreateFacility(facilityTiles);
	}

	[AServerOnly]
	public void CreateFacility(List<CTile> _FacilityTiles)
	{
		// Get the converted facility interior tiles
		List<CTile> interiorTiles = new List<CTile>();
		foreach(CTile tile in _FacilityTiles)
		{
			if(tile.GetTileTypeState(ETileType.Wall_Int))
				interiorTiles.Add(m_ShipGrid.GetTile(tile.m_GridPosition));
		}

		// Instantiate an empty facility
		GameObject newFacility = CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.Facility);
		newFacility.transform.parent = transform;
		newFacility.transform.localPosition = Vector3.zero;
		newFacility.transform.localRotation = Quaternion.identity;

		CNetworkView netowrkView = newFacility.GetComponent<CNetworkView>();

		// Give the facility its tiles
		newFacility.GetComponent<CFacilityTiles>().InteriorTiles = interiorTiles;
	}

    public void RegisterFacility(CFacilityInterface _Facility)
    {
		if(CNetwork.IsServer)
		{
	        // Generate id for facility
	        uint facilityId = ++m_FacilityIdCount;

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
