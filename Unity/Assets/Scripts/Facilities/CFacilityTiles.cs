//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFacilityInfo.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/* Implementation */


public class CFacilityTiles : MonoBehaviour
{
	// Member Types


	// Member Delegates & Events


	// Member Fields
	public List<CTileInterface> m_InteriorTiles = new List<CTileInterface>();

	
	// Member Properties
	public List<CTileInterface> InteriorTiles
	{
		get { return(m_InteriorTiles); }
		set { m_InteriorTiles = value; }
	}

	// Member Methods
	private void Start()
	{
		// Configure the interior tiles to have volume and triggers
		ConfigureFacilityTiles();
	}

	private void ConfigureFacilityTiles()
	{
		// Add an interior trigger to all interior tiles
		foreach(CTileInterface tile in m_InteriorTiles)
		{
			// Add a trigger box collider
			BoxCollider boxCollider = tile.gameObject.AddMissingComponent<BoxCollider>();
			boxCollider.center = new Vector3(0.0f, 2.0f, 0.0f);
			boxCollider.size = new Vector3(4.0f, 4.0f, 4.0f);
			boxCollider.isTrigger = true;

			// Add the interior trigger to this collider
			CInteriorTrigger interiorTrigger = tile.gameObject.AddMissingComponent<CInteriorTrigger>();
			interiorTrigger.SetParentFacility(gameObject);
		}

//		if(m_CombinedMesh == null)
//			Debug.LogError("Facility " + gameObject.name + " is missing its CombinedMesh instance. Ensure this is connected or the facility will be broken.");
//		
//		MeshCollider mc = null;
//		
//		// Create the triggers/colliders
//		GameObject internalTrigger = new GameObject("_InteriorTrigger");
//		GameObject collider = new GameObject("_Collider");
//		GameObject exitTrigger = new GameObject("_ExitTrigger");
//		GameObject entryTrigger = new GameObject("_ExitTrigger");
//		internalTrigger.tag = "GalaxyShip";
//		collider.tag      = "GalaxyShip";
//		exitTrigger.tag = "GalaxyShip";
//		entryTrigger.tag = "GalaxyShip";
//		
//		// Create the exterior version of the facility
//		GameObject extFacility = new GameObject("_" + gameObject.name + "Ext");
//		
//		// Child the exit trigger and interior trigger to the facility
//		exitTrigger.transform.parent = transform;
//		exitTrigger.transform.localPosition = Vector3.zero;
//		exitTrigger.transform.localRotation = Quaternion.identity;
//		internalTrigger.transform.parent = transform;
//		internalTrigger.transform.localPosition = Vector3.zero;
//		internalTrigger.transform.localRotation = Quaternion.identity;
//		
//		// Child the entry trigger and collider to the exterior facility
//		entryTrigger.transform.parent = extFacility.transform;
//		entryTrigger.transform.localPosition = Vector3.zero;
//		entryTrigger.transform.localRotation = Quaternion.identity;
//		collider.transform.parent = extFacility.transform;
//		collider.transform.localPosition = Vector3.zero;
//		collider.transform.localRotation = Quaternion.identity;
//		
//		// Set the exterior facility on the galaxy layer
//		CUtility.SetLayerRecursively(extFacility, LayerMask.NameToLayer("Galaxy"));
//		
//		// Configure the internal trigger
//		internalTrigger.AddComponent<CInteriorTrigger>();
//		mc = internalTrigger.AddComponent<MeshCollider>();
//		mc.sharedMesh = m_CombinedMesh;
//		mc.convex = true;
//		mc.isTrigger = true;
//		
//		// Configure the exit trigger
//		exitTrigger.transform.localScale = Vector3.one * 1.02f;
//		exitTrigger.AddComponent<CExitTrigger>();
//		mc = exitTrigger.AddComponent<MeshCollider>();
//		mc.sharedMesh = m_CombinedMesh;
//		mc.convex = true;
//		mc.isTrigger = true;
//		
//		// Configure the entry trigger
//		entryTrigger.transform.localScale = Vector3.one * 1.02f;
//		entryTrigger.AddComponent<CEntryTrigger>();
//		mc = entryTrigger.AddComponent<MeshCollider>();
//		mc.sharedMesh = m_CombinedMesh;
//		mc.convex = true;
//		mc.isTrigger = true;
//		
//		// Configure the collider trigger
//		mc = collider.AddComponent<MeshCollider>();
//		mc.sharedMesh = m_CombinedMesh;
//		mc.convex = true;
//		
//		// Attach the exterior to the facility to the galaxy ship
//		CGalaxyShipFacilities galaxyShipCollider = CGameShips.GalaxyShip.GetComponent<CGalaxyShipFacilities>();
//		galaxyShipCollider.AttachNewFacility(extFacility, transform.localPosition, transform.localRotation);
	}
};
