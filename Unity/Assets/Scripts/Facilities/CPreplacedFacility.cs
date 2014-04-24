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
	
	
	// Member Properties


	// Member Methods
	private void Start()
	{
		// Get the tiles which reside under this facility
		List<CTile> tiles = new List<CTile>(gameObject.GetComponentsInChildren<CTile>());
		
		// Find the tiles which are internal only
		List<CTile> interiorTiles = new List<CTile>(
			from ineriorTile in tiles
			where ineriorTile.GetTileTypeState(ETileType.Wall_Int)
			select ineriorTile);

		List<List<CTile>> facilities = new List<List<CTile>>();
		facilities.Add(interiorTiles);

		// Import the facility to the ship
		CShipFacilities shipFacilities = CUtility.FindInParents<CShipFacilities>(gameObject);
		shipFacilities.ImportNewGridTiles(tiles, facilities);

		// Create all of the preplaced modules and add to the facility
		GameObject facility = shipFacilities.Facilities.First();
		foreach(CPreplacedModule preplacedModule in gameObject.GetComponentsInChildren<CPreplacedModule>())
		{
			preplacedModule.CreateModule(facility);
		}

		// Destroy self
		Destroy(gameObject);
	}
};
