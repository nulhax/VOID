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
	public void ImportNewGridTiles(List<CTileInterface> _AllTiles, List<List<CTileInterface>> _FacilityInteriorTiles)
	{
		// Import all of the tiles to the ship
		List<CTileInterface> newTiles = m_ShipGrid.ImportTileInformation(_AllTiles);

		// Destoy all facilities
		foreach(GameObject facility in Facilities)
			DestoryFacility(facility);

		// Create all new facilities
		foreach(List<CTileInterface> facilityTiles in _FacilityInteriorTiles)
			CreateFacility(facilityTiles);

		// Reconfigure the entry triggers
		CGameShips.GalaxyShip.GetComponent<CGalaxyShipFacilities>().ReconfigureEntryTriggers(this);
	}

	[AServerOnly]
	public void CreateFacility(List<CTileInterface> _FacilityTiles)
	{
		// Get the converted facility interior tiles
		List<CTileInterface> interiorTiles = new List<CTileInterface>();
		foreach(CTileInterface tile in _FacilityTiles)
		{
			if(tile.GetTileTypeState(CTile.EType.Interior_Wall))
				interiorTiles.Add(m_ShipGrid.GetTileInterface(tile.m_GridPosition));
		}

		// Instantiate an empty facility
		GameObject newFacility = CNetwork.Factory.CreateGameObject(CGameRegistrator.ENetworkPrefab.Facility);
		newFacility.name = "Facility " + m_FacilityIdCount;
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

	[AServerOnly]
	private void DestoryFacility(GameObject _Facility)
    {
        // Remove facility from dictionaries
		m_FacilityObjects.Remove(_Facility.GetComponent<CFacilityInterface>().FacilityId);

        // Notify observers
        if (EventFaciltiyDestroyed != null) 
			EventFaciltiyDestroyed(_Facility);

		// Destroy the facility
		CNetwork.Factory.DestoryGameObject(_Facility);
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
