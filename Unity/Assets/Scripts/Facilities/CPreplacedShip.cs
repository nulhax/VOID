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


public class CPreplacedShip : MonoBehaviour
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public Transform m_TilesCollection = null;
	
	// Member Properties


	// Member Methods
	private void Start()
	{
		if(CNetwork.IsServer)
			InitialisePreplacedTilesAndModules();

		// Destroy self
		Destroy(gameObject);
	}

	[AServerOnly]
	private void InitialisePreplacedTilesAndModules()
	{
		CShipModules shipModules = CGameShips.Ship.GetComponent<CShipModules>();
		CShipFacilities shipFacilities = CGameShips.Ship.GetComponent<CShipFacilities>();

		// Create all of the preplaced modules to the ship
		foreach(CPreplacedModule preplacedModule in gameObject.GetComponentsInChildren<CPreplacedModule>())
		{
			CModuleInterface module = shipModules.CreateModule(
				preplacedModule.m_PreplacedModuleType, 
				preplacedModule.transform.position, 
				preplacedModule.transform.rotation);
			
			module.Build(1.0f);
		}

		// Get the tiles which reside within the tiles collection resource
		List<CTileInterface> tiles = new List<CTileInterface>();
		foreach(Transform child in m_TilesCollection)
			tiles.Add(child.GetComponent<CTileInterface>());

		// Import the facility to the prefabricator
		shipFacilities.ImportNewGridTiles(tiles);
	}
};
