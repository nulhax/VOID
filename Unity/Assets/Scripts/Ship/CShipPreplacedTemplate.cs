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


public class CShipPreplacedTemplate : MonoBehaviour
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public Transform m_Template = null;
	
	// Member Properties


	// Member Methods
	private void Start()
	{
		if(CNetwork.IsServer)
			InitialisePreplacedTilesAndModules();
	}

	[AServerOnly]
	private void InitialisePreplacedTilesAndModules()
	{
		CShipModules shipModules = CGameShips.Ship.GetComponent<CShipModules>();
		CShipFacilities shipFacilities = CGameShips.Ship.GetComponent<CShipFacilities>();

		if(m_Template == null)
			return;

		// Get the tiles and modules which reside within the template
		List<CTileInterface> tiles = new List<CTileInterface>();
		List<CModuleInterface> modules = new List<CModuleInterface>();

		foreach(Transform child in m_Template)
		{
			CTileInterface tileInterface = child.GetComponent<CTileInterface>();
			if(tileInterface != null)
				tiles.Add(tileInterface);

			CModuleInterface module = child.GetComponent<CModuleInterface>();
			if(module != null)
				modules.Add(module);
		}

		// Create all of the preplaced modules to the ship
		foreach(CModuleInterface preplacedModule in modules)
		{
			CModuleInterface module = shipModules.CreateModule(
				preplacedModule.ModuleType, 
				preplacedModule.transform.position, 
				preplacedModule.transform.rotation);
			
			module.Build(1.0f);
		}

		// Import the facility to the prefabricator
		shipFacilities.ImportNewGridTiles(tiles);
	}
};
