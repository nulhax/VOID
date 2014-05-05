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
using System.Linq;


/* Implementation */


public class CPreplacedFacility : MonoBehaviour
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public GameObject m_TilesCollection = null;
	
	// Member Properties


	// Member Methods
	private void Start()
	{
		if(CNetwork.IsServer)
			InitialisePreplacedTiles();

		// Destroy self
		Destroy(gameObject);
	}

	[AServerOnly]
	private void InitialisePreplacedTiles()
	{
		m_TilesCollection = (GameObject)GameObject.Instantiate(m_TilesCollection);

		// Get the tiles which reside under this facility
		List<CTileInterface> tiles = new List<CTileInterface>(m_TilesCollection.GetComponentsInChildren<CTileInterface>());
		
		// Find the tiles which are internal only
		List<CTileInterface> interiorTiles = new List<CTileInterface>(
			from ineriorTile in tiles
			where ineriorTile.GetTileTypeState(CTile.EType.Interior_Wall)
			select ineriorTile);
		
		List<List<CTileInterface>> facilities = new List<List<CTileInterface>>();
		facilities.Add(interiorTiles);
		
		// Import the facility to the ship
		CShipFacilities shipFacilities = CUtility.FindInParents<CShipFacilities>(gameObject);
		shipFacilities.ImportNewGridTiles(tiles, facilities);
		
		// Create all of the preplaced modules and add to the facility
		GameObject facility = shipFacilities.Facilities.First();
		foreach(CPreplacedModule preplacedModule in gameObject.GetComponentsInChildren<CPreplacedModule>())
		{
			CGameShips.Ship.GetComponent<CShipModules>().CreateModule(preplacedModule.m_PreplacedModuleType, preplacedModule.transform.position, 0.0f);
		}

		Destroy(m_TilesCollection);
	}
};
