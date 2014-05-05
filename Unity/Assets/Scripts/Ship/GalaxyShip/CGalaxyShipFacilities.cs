//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CGalaxyShipCollider.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CGalaxyShipFacilities : MonoBehaviour 
{
	// Member Types
	
	
	// Member Fields
	public GameObject m_EntryTrigger = null;
	public GameObject m_Collider = null;


	// Member Properies


	// Member Methods
	public void ReconfigureEntryTriggers(CShipFacilities _ShipFacilities)
	{	
		foreach(CInteriorTrigger interiorTrigger in m_EntryTrigger.GetComponentsInChildren<CInteriorTrigger>())
			Destroy(interiorTrigger.gameObject);

		foreach(GameObject facility in _ShipFacilities.Facilities)
		{
			foreach(CTileInterface tileInterface in facility.GetComponent<CFacilityTiles>().InteriorTiles)
			{
				Vector3 pos = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyPos(tileInterface.transform.position);
				Quaternion rot = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyRot(tileInterface.transform.rotation);

				GameObject entryTrigger = new GameObject("Trigger");
				entryTrigger.transform.parent = m_EntryTrigger.transform;
				entryTrigger.transform.position = pos;
				entryTrigger.transform.rotation = rot;
				entryTrigger.layer = gameObject.layer;

				BoxCollider boxCollider = entryTrigger.AddComponent<BoxCollider>();
				boxCollider.center = new Vector3(0.0f, 2.0f, 0.0f);
				boxCollider.size = new Vector3(4.0f, 4.0f, 4.0f);
				boxCollider.isTrigger = true;

				CInteriorTrigger interiorTrigger = entryTrigger.AddComponent<CInteriorTrigger>();
				interiorTrigger.SetParentFacility(facility);
			}
		}

		foreach(Transform child in m_Collider.transform)
			Destroy(child.gameObject);

		foreach(CTileInterface tileInterface in _ShipFacilities.m_ShipGrid.Tiles)
		{
			if(!tileInterface.GetTileTypeState(CTile.EType.Exterior_Wall))
				continue;

			Vector3 pos = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyPos(tileInterface.transform.position);
			Quaternion rot = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyRot(tileInterface.transform.rotation);

			GameObject collider = new GameObject("Collider");
			collider.transform.parent = m_Collider.transform;
			collider.transform.position = pos;
			collider.transform.rotation = rot;
			collider.layer = gameObject.layer;

			tileInterface.UpdateAllTileObjects();

			Collider tileCollider = tileInterface.GetTile(CTile.EType.Exterior_Wall).collider;
			if(tileCollider == null)
				tileCollider = tileInterface.GetTile(CTile.EType.Exterior_Wall).GetComponentInChildren<Collider>();
	
			if(tileCollider != null && tileInterface.GetTile(CTile.EType.Exterior_Wall).m_Modifications.Count == 0)
			{
				BoxCollider boxCollider = collider.AddComponent<BoxCollider>();
				boxCollider.size = tileCollider.bounds.size;
				boxCollider.center = new Vector3(0.0f, 2.0f, 0.0f);
			}
		}
	}
}
