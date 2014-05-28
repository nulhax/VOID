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

	public List<GameObject> m_InteriorDoors = new List<GameObject>();
	public List<GameObject> m_ExteriorDoors = new List<GameObject>();

	private uint m_FacilityIdCount = 0;

	private List<uint> m_UnusedFacilityIds = new List<uint>();
	private Dictionary<uint, GameObject> m_FacilityObjects = new Dictionary<uint, GameObject>();


	// Member Properties
	public List<GameObject> Facilities
	{
		get { return(new List<GameObject>(m_FacilityObjects.Values)); }
	}


	// Member Methods
	[AServerOnly]
	public void ImportNewGridTiles(List<CTileInterface> _AllTiles)
	{
		// Disable all players disembarking
		foreach(GameObject playerActor in CGamePlayers.PlayerActors)
		{
			CActorBoardable actorBoardable = playerActor.GetComponent<CActorBoardable>();
			actorBoardable.m_CanDisembark = false;
		}

		// Import all of the tiles to the ship
		m_ShipGrid.ImportTileInformation(_AllTiles);

		// Detirmine facilities
		List<List<CTileInterface>> facilitTiles = DetirmineFacilityTiles(m_ShipGrid);

		// Destoy all previous facilities
		foreach(GameObject facility in Facilities)
			DestoryFacility(facility);

		// Create all new facilities
		foreach(List<CTileInterface> facility in facilitTiles)
			CreateFacility(facility);

		foreach(CTileInterface tileInterface in m_ShipGrid.TileInterfaces)
			tileInterface.UpdateAllTileObjects();

		// Configure doors
		ConfigureDoors();

		// Reconfigure the entry triggers
		CGameShips.GalaxyShip.GetComponent<CGalaxyShipFacilities>().ReconfigureCollidersAndTriggers();

		// Disable all players disembarking
		foreach(GameObject playerActor in CGamePlayers.PlayerActors)
		{
			CActorBoardable actorBoardable = playerActor.GetComponent<CActorBoardable>();
			actorBoardable.m_CanDisembark = true;
		}

		// Sync each tile to the player
		//m_ShipGrid.SyncAllTilesToAllPlayers();

		// Static batch all tiles
		//StaticBatchingUtility.Combine(m_ShipGrid.m_TileContainer.gameObject);
	}

	[AServerOnly]
	private List<List<CTileInterface>> DetirmineFacilityTiles(CGrid _Grid)
	{
		// Make a list of facilities which lists each tile
		List<List<CTileInterface>> facilityTilesList = new List<List<CTileInterface>>();
		
		// Select only the tiles which are interior tiles
		List<CTileInterface> interiorTiles = _Grid.TileInterfaces.FindAll(tile => tile.GetTileTypeState(CTile.EType.Interior_Wall));

		// Itterate each of the interior tiles
		foreach(CTileInterface tileInterface in interiorTiles)
		{
			// Create a list of faclity tile lists for this tile interface
			List<List<CTileInterface>> tileInterfacesFacilitiesList = new List<List<CTileInterface>>();

			// Check each of its floor neighbours within the neighbourhood
			CTile interiorWallTile = tileInterface.GetTile(CTile.EType.Interior_Wall);
			foreach(CNeighbour neighbour in tileInterface.m_NeighbourHood)
			{
				if(!interiorWallTile.RelevantDirections.Contains(neighbour.m_Direction))
					continue;

				// First check for neighbour exemption
				bool occlusion = interiorWallTile.m_NeighbourExemptions.Contains(neighbour.m_Direction);

				// Then check for exterior tile
				if(!occlusion)
					occlusion = neighbour.m_TileInterface.GetTileTypeState(CTile.EType.Exterior_Wall);

				if(!occlusion)
					FindFacilityTilesListItem(tileInterface, neighbour.m_TileInterface, facilityTilesList, tileInterfacesFacilitiesList);
	
				// Debug, draw occlusion state ray
				DebugDrawTileInterfaceConnectionLine(tileInterface, neighbour.m_TileInterface, _Grid.m_TileSize, occlusion);
			}

			// Check upper tile ceiling occlusion
			CTileInterface upperNeighbourTileInterface = _Grid.GetTileInterface(new CGridPoint(tileInterface.m_GridPosition.ToVector + Vector3.up));
			CTile interiorCeilingTile = tileInterface.GetTile(CTile.EType.Interior_Ceiling);
			if(interiorCeilingTile != null && upperNeighbourTileInterface != null)
			{
				bool occlusion = interiorCeilingTile.m_CurrentTileMeta.m_MetaType == (int)CTile_InteriorCeiling.EType.Middle;

				if(!occlusion)
					FindFacilityTilesListItem(tileInterface, upperNeighbourTileInterface, facilityTilesList, tileInterfacesFacilitiesList);

				// Debug, draw occlusion state ray
				DebugDrawTileInterfaceConnectionLine(tileInterface, upperNeighbourTileInterface, _Grid.m_TileSize, occlusion);
			}

			// Check lower tile ceiling occlusion
			CTileInterface lowerNeighbourTileInterface = _Grid.GetTileInterface(new CGridPoint(tileInterface.m_GridPosition.ToVector - Vector3.up));
			CTile interiorFloorTile = tileInterface.GetTile(CTile.EType.Interior_Floor);
			if(interiorFloorTile != null && lowerNeighbourTileInterface != null)
			{
				bool occlusion = interiorFloorTile.m_CurrentTileMeta.m_MetaType == (int)CTile_InteriorFloor.EType.Middle;
				
				if(!occlusion)
					FindFacilityTilesListItem(tileInterface, lowerNeighbourTileInterface, facilityTilesList, tileInterfacesFacilitiesList);
				
				// Debug, draw occlusion state ray
				DebugDrawTileInterfaceConnectionLine(tileInterface, lowerNeighbourTileInterface, _Grid.m_TileSize, occlusion);
			}

			// If there was no facilities added add this to a new list
			if(tileInterfacesFacilitiesList.Count == 0)
			{
				List<CTileInterface> list = new List<CTileInterface>();
				list.Add(tileInterface);

				facilityTilesList.Add(list);
				continue;
			}
			
			// Check if this tile belongs to more than one list
			if(tileInterfacesFacilitiesList.Count > 1)
			{
				List<CTileInterface> newList = new List<CTileInterface>();
				
				// Remove these lists from the main list and add to new list
				foreach(List<CTileInterface> list in tileInterfacesFacilitiesList)
				{
					newList.AddRange(list);
					facilityTilesList.Remove(list);
				}
				
				// Add combined list to the main list
				facilityTilesList.Add(newList);
			}
		}
		
		return(facilityTilesList);
	}
	
	private void FindFacilityTilesListItem(CTileInterface _TileInterface, CTileInterface _NeighbourTile, 
	                                       List<List<CTileInterface>> _FacilityTilesList, List<List<CTileInterface>> _TileInterfacesFacilitiesList)
	{
		// Find the facility tile list item in which this neighbour belongs to
		List<CTileInterface> facilityTilesItem = _FacilityTilesList.Find(list => list.Contains(_NeighbourTile));
		if(facilityTilesItem != null)
		{
			// Add the first facility tile list if there are none
			if(_TileInterfacesFacilitiesList.Count == 0)
				facilityTilesItem.Add(_TileInterface);
			
			// Save the facilities this tile belongs to
			if(!_TileInterfacesFacilitiesList.Contains(facilityTilesItem))
				_TileInterfacesFacilitiesList.Add(facilityTilesItem);
		}
	}

	private void DebugDrawTileInterfaceConnectionLine(CTileInterface _First, CTileInterface _Second, float _TileSize, bool _OcclusionState)
	{
		Vector3 origin = _First.transform.position + _First.transform.rotation * Vector3.up * _TileSize * 0.5f;
		Vector3 dir = (_Second.transform.position - _First.transform.position).normalized;
		Debug.DrawLine(origin, origin + (dir * _TileSize * 0.45f), _OcclusionState ? Color.red : Color.green, 5.0f);
	}

	private void ConfigureDoors()
	{
		// Destroy all current doors
		foreach(GameObject door in m_InteriorDoors)
			CNetwork.Factory.DestoryGameObject(door);

		foreach(GameObject door in m_ExteriorDoors)
			CNetwork.Factory.DestoryGameObject(door);

		List<KeyValuePair<CTile, CTile>> interiorDoorwayPairs = new List<KeyValuePair<CTile, CTile>>(); 
		List<KeyValuePair<CTile, CTile>> exteriorDoorwayPairs = new List<KeyValuePair<CTile, CTile>>(); 

		foreach(CTileInterface tileInterface in m_ShipGrid.TileInterfaces)
		{
			CTile interiorWallTile = tileInterface.GetTile(CTile.EType.Interior_Wall);

			if(interiorWallTile == null)
				continue;

			foreach(CTile.CModification modification in interiorWallTile.m_Modifications)
			{
				if(modification.m_Modification != (int)CTile_InteriorWall.EModification.Door)
					continue;

				CNeighbour neighbour = tileInterface.m_NeighbourHood.Find(n => n.m_Direction == modification.m_WorldSide);
				if(neighbour == null)
					continue;

				CTile neighbourInteriorWall = neighbour.m_TileInterface.GetTile(CTile.EType.Interior_Wall);
				if(neighbourInteriorWall != null)
				{
					if(interiorDoorwayPairs.Exists(pair => pair.Value == interiorWallTile && pair.Key == neighbourInteriorWall))
						continue;

					interiorDoorwayPairs.Add(new KeyValuePair<CTile, CTile>(interiorWallTile, neighbourInteriorWall));
				}

				CTile neighbourExteriorWall = neighbour.m_TileInterface.GetTile(CTile.EType.Exterior_Wall);
				if(neighbourExteriorWall != null)
				{
					if(exteriorDoorwayPairs.Exists(pair => pair.Value == interiorWallTile && pair.Key == neighbourExteriorWall))
						continue;
					
					exteriorDoorwayPairs.Add(new KeyValuePair<CTile, CTile>(interiorWallTile, neighbourExteriorWall));
				}
			}
		}

		foreach(KeyValuePair<CTile, CTile> pair in interiorDoorwayPairs)
		{
			GameObject door = CNetwork.Factory.CreateGameObject(CGameRegistrator.ENetworkPrefab.InteriorDoor);

			CNetworkView doorNetworkView = door.GetComponent<CNetworkView>();
			doorNetworkView.SetParent(CGameShips.ShipViewId);
			doorNetworkView.SetPosition((pair.Key.transform.position + pair.Value.transform.position) * 0.5f);
			doorNetworkView.SetRotation(Quaternion.LookRotation((pair.Key.transform.position - pair.Value.transform.position).normalized));

			m_InteriorDoors.Add(door);

			CDoorInterface doorInterface = door.GetComponent<CDoorInterface>();
			GameObject firstFacility = Facilities.Find(f => f.GetComponent<CFacilityTiles>().InteriorTiles.Contains(pair.Key.m_TileInterface));
			GameObject secondFacility = Facilities.Find(f => f.GetComponent<CFacilityTiles>().InteriorTiles.Contains(pair.Value.m_TileInterface));

			if(firstFacility != null)
			{
				firstFacility.GetComponent<CFacilityInterface>().RegisterInteriorDoor(doorInterface);
				doorInterface.m_FirstConnectedFacility = firstFacility;
			}

			if(secondFacility != null)
			{
				secondFacility.GetComponent<CFacilityInterface>().RegisterInteriorDoor(doorInterface);
				doorInterface.m_SecondConnectedFacility = secondFacility;
			}
		}

		foreach(KeyValuePair<CTile, CTile> pair in exteriorDoorwayPairs)
		{
			GameObject door = CNetwork.Factory.CreateGameObject(CGameRegistrator.ENetworkPrefab.ExteriorDoor);
			CNetworkView doorNetworkView = door.GetComponent<CNetworkView>();
			doorNetworkView.SetParent(CGameShips.ShipViewId);
			doorNetworkView.SetPosition((pair.Key.transform.position + pair.Value.transform.position) * 0.5f);
			doorNetworkView.SetRotation(Quaternion.LookRotation((pair.Value.transform.position - pair.Key.transform.position).normalized));
			
			m_ExteriorDoors.Add(door);

			CDoorInterface doorInterface = door.GetComponent<CDoorInterface>();
			CTileInterface interiorTile = pair.Key.m_TileType == CTile.EType.Exterior_Wall ? pair.Value.m_TileInterface : pair.Key.m_TileInterface;

			GameObject facility = Facilities.Find(f => f.GetComponent<CFacilityTiles>().InteriorTiles.Contains(interiorTile));

			if(facility != null)
			{
				facility.GetComponent<CFacilityInterface>().RegisterExteriorDoor(doorInterface);
				doorInterface.m_FirstConnectedFacility = facility;
			}
		}
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

		// Give facility an id
		uint facilityId = ++m_FacilityIdCount;
		newFacility.GetComponent<CFacilityInterface>().FacilityId = facilityId;

		// Give the facility its tiles
		newFacility.GetComponent<CFacilityTiles>().InteriorTiles = interiorTiles;
		newFacility.GetComponent<CFacilityTiles>().ConfigureFacilityTiles();

		// Add facility to dictionaries
		m_FacilityObjects.Add(facilityId, newFacility);
		
		// Notify observers
		if(EventFaciltiyCreated != null) 
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
