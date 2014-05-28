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
	public void ReconfigureCollidersAndTriggers()
	{	
		CShipFacilities shipFacilities = CGameShips.Ship.GetComponent<CShipFacilities>();

		foreach(CEntryTrigger entryTrigger in m_EntryTrigger.GetComponentsInChildren<CEntryTrigger>())
			Destroy(entryTrigger.gameObject);

		foreach(GameObject facility in shipFacilities.Facilities)
		{
			foreach(CTileInterface tileInterface in facility.GetComponent<CFacilityTiles>().InteriorTiles)
			{
				Vector3 pos = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyPos(tileInterface.transform.position);
				Quaternion rot = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyRot(tileInterface.transform.rotation);

				GameObject trigger = new GameObject("Trigger");
				trigger.transform.parent = m_EntryTrigger.transform;
				trigger.transform.position = pos;
				trigger.transform.rotation = rot;
				trigger.layer = gameObject.layer;

				BoxCollider boxCollider = trigger.AddComponent<BoxCollider>();
				boxCollider.center = new Vector3(0.0f, 2.0f, 0.0f);
				boxCollider.size = new Vector3(3.9f, 3.9f, 3.9f);
				boxCollider.isTrigger = true;

				CEntryTrigger entryTrigger = trigger.AddComponent<CEntryTrigger>();
				CInteriorTrigger interiorTrigger = tileInterface.GetComponent<CInteriorTrigger>();
				entryTrigger.m_ReferencedInteriorTrigger = interiorTrigger;
			}
		}

		foreach(Transform child in m_Collider.transform)
			Destroy(child.gameObject);

		foreach(CTileInterface tileInterface in shipFacilities.m_ShipGrid.TileInterfaces)
		{
			if(!tileInterface.GetTileTypeState(CTile.EType.Exterior_Wall) &&
			   !tileInterface.GetTileTypeState(CTile.EType.Exterior_Upper) &&
			   !tileInterface.GetTileTypeState(CTile.EType.Exterior_Lower))
				continue;

			Vector3 pos = Vector3.zero;
			Quaternion rot = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyRot(tileInterface.transform.rotation);
			
			GameObject collider = new GameObject("Collider");
			collider.transform.parent = m_Collider.transform;
			collider.transform.position = pos;
			collider.transform.rotation = rot;
			collider.layer = gameObject.layer;

			foreach(Collider tileCollider in tileInterface.GetComponentsInChildren<Collider>())
			{
				if(tileCollider.gameObject.activeSelf && !tileCollider.isTrigger)
				{
					BoxCollider boxCollider = collider.AddComponent<BoxCollider>();
					boxCollider.size = tileCollider.bounds.size;
					boxCollider.center = tileCollider.bounds.center - CGameShips.Ship.transform.position;
				}
			}
		}
	}
}
