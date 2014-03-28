//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CGridManager.cs
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


public class CGrid : MonoBehaviour 
{
	
	// Member Types

	
	// Member Delegates & Events
	
	
	// Member Fields
	private GameObject m_TileContainer = null;

	public float m_TileSize = 4.0f;
	public CTileFactory m_TileFactory = null;

	public Dictionary<string, CTileBehaviour> m_GridBoard = new Dictionary<string, CTileBehaviour>();

	
	// Member Properties

	
	// Member Methods
	void Start() 
	{
		// Create the grid objects
		CreateGridObjects();
	}

	void CreateGridObjects()
	{
		m_TileContainer = new GameObject("Tile Container");
		m_TileContainer.transform.parent = transform;
		m_TileContainer.transform.localScale = Vector3.one;
		m_TileContainer.transform.localPosition = Vector3.zero;
		m_TileContainer.transform.localRotation = Quaternion.identity;
	}

	public TGridPoint GetGridPoint(Vector3 worldPosition)
	{
		return(new TGridPoint(GetGridPosition(worldPosition)));
	}

	public Vector3 GetGridPosition(Vector3 worldPosition)
	{
		// Convert the world space to grid space
		Vector3 gridpos = Quaternion.Inverse(transform.rotation) * (worldPosition - transform.position);

		// Scale the position to tilesize and scale
		gridpos = gridpos / m_TileSize / transform.localScale.x;

		// Round each position to be an integer number
		gridpos.x = Mathf.Round(gridpos.x);
		gridpos.y = Mathf.Round(gridpos.y);
		gridpos.z = Mathf.Round(gridpos.z);

		return gridpos;
	}

	public Vector3 GetLocalPosition(TGridPoint _GridPoint)
	{
		// Convert from grid space to local space
		return(_GridPoint.ToVector * m_TileSize);
	}

	public CTile GetTile(TGridPoint _GridPoint)
	{
		CTile tile = null;
		if(m_GridBoard.ContainsKey(_GridPoint.ToString()))
		{
			tile = m_GridBoard[_GridPoint.ToString()].m_Tile;
		}
		return(tile);
	}
	
	public void CreateTile(TGridPoint _GridPoint)
	{
		if(!m_GridBoard.ContainsKey(_GridPoint.ToString()))
		{
			CTile newtile = new CTile(_GridPoint, this);

			GameObject tile = new GameObject("Tile");
			tile.transform.parent = m_TileContainer.transform;
			tile.transform.localScale = Vector3.one;
			tile.transform.localRotation = Quaternion.identity;
			tile.transform.localPosition = GetLocalPosition(_GridPoint);

			CTileBehaviour tb = tile.AddComponent<CTileBehaviour>();
			tb.m_Tile = newtile;
			newtile.m_TileObject = tb;
			m_GridBoard.Add(_GridPoint.ToString(), tb);
			newtile.FindNeighbours();
			newtile.UpdateNeighbours();
			newtile.UpdateAllTileObjects();
		}
	}

	public void RemoveTile(TGridPoint _GridPoint)
	{
		if (m_GridBoard.ContainsKey(_GridPoint.ToString()))
		{
			CTileBehaviour tb = m_GridBoard[_GridPoint.ToString()];
			tb.m_Tile.ReleaseExistingTileObjects();
			m_GridBoard.Remove(_GridPoint.ToString());
			tb.m_Tile.UpdateNeighbours();
			Destroy(tb.gameObject);
		}
	}
}


