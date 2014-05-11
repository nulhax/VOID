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

		// Debug: add a cube to the facilit for each tile
//		foreach(CTileInterface tileInterface in m_InteriorTiles)
//		{
//			GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
//
//			cube.transform.transform.position = tileInterface.transform.position + Vector3.up * 2.0f;
//			cube.transform.localScale = Vector3.one * 4;
//
//			Destroy(cube.collider);
//
//			Color col = Color.clear;
//			switch(GetComponent<CFacilityInterface>().FacilityId)
//			{
//			case 0: col = Color.red; break;
//			case 1: col = Color.green; break;
//			case 2: col = Color.blue; break;
//			case 3: col = Color.cyan; break;
//			case 4: col = Color.magenta; break;
//			case 5: col = Color.yellow; break;
//			case 6: col = Color.white; break;
//			case 7: col = Color.black; break;
//			case 8: col = Color.grey; break;
//			}
//		
//			cube.renderer.material.color = col;
//		}
	}
};
