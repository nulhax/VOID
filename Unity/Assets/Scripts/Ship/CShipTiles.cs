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


[RequireComponent(typeof(CShipFacilities))]
public class CShipTiles : MonoBehaviour
{
	
	// Member Types
	
	
	// Member Delegates & Events
	

	// Member Fields
	public CGrid m_ShipGrid = null;


	// Member Properties
	

	
	// Member Methods
	public void OnFacilityCreated(CFacilityInterface _Facility)
	{
		// Export the tiles to the grid
		//m_ShipGrid.ImportTileInformation(_Facility.FacilityTiles.ToArray());
	}

};
