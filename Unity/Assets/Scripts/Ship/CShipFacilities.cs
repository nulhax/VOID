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
	public delegate void HandleFacilityEvent(GameObject _Facilty);
	
	public event HandleFacilityEvent EventFaciltiyCreated;
	public event HandleFacilityEvent EventFaciltiyDestroyed;


	// Member Fields
	public CGrid m_ShipGrid = null;

	private uint m_FacilityIdCount = 0;

	private List<uint> m_UnusedFacilityIds = new List<uint>();
	private Dictionary<uint, GameObject> m_FacilityObjects = new Dictionary<uint, GameObject>();


	// Member Properties
	[AServerOnly]
	public List<GameObject> Facilities
	{
		get { return (new List<GameObject>(m_FacilityObjects.Values)); }
	}


	// Member Methods
	[AServerOnly]
	public void ImportNewGridTiles(List<CTileRoot> _AllTiles, List<List<CTileRoot>> _FacilityTiles)
	{
		// Import all of the tiles to the ship
		List<CTileRoot> newTiles = m_ShipGrid.ImportTileInformation(_AllTiles);

		// Destoy all facilities
		foreach(GameObject facility in Facilities)
			DestoryFacility(facility);

		// Create all new facilities
		foreach(List<CTileRoot> facilityTiles in _FacilityTiles)
			CreateFacility(facilityTiles);
	}

	[AServerOnly]
	public void CreateFacility(List<CTileRoot> _FacilityTiles)
	{
		// Get the converted facility interior tiles
		List<CTileRoot> interiorTiles = new List<CTileRoot>();
		foreach(CTileRoot tile in _FacilityTiles)
		{
			if(tile.GetTileTypeState(CTile.EType.Wall_Int))
				interiorTiles.Add(m_ShipGrid.GetTile(tile.m_GridPosition));
		}

		// Instantiate an empty facility
		GameObject newFacility = CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.Facility);
		newFacility.transform.parent = transform;
		newFacility.transform.localPosition = Vector3.zero;
		newFacility.transform.localRotation = Quaternion.identity;

		// Give the facility its tiles
		newFacility.GetComponent<CFacilityTiles>().InteriorTiles = interiorTiles;

		// Give facility an id
		uint facilityId = ++m_FacilityIdCount;
		newFacility.GetComponent<CFacilityInterface>().FacilityId = facilityId;
		
		// Add facility to dictionaries
		m_FacilityObjects.Add(facilityId, newFacility);
		
		// Notify observers
		if (EventFaciltiyCreated != null) 
			EventFaciltiyCreated(newFacility);
	}

	private void DestoryFacility(GameObject _Facility)
    {
        // Remove facility from dictionaries
		m_FacilityObjects.Remove(_Facility.GetComponent<CFacilityInterface>().FacilityId);

        // Notify observers
        if (EventFaciltiyDestroyed != null) 
			EventFaciltiyDestroyed(_Facility);

		// Destroy the facility
		Destroy(_Facility);
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
