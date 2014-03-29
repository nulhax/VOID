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

	private Dictionary<string, CTile> m_GridBoard = new Dictionary<string, CTile>();

	
	// Member Properties
	public GameObject TileContainer
	{
		get { return(m_TileContainer); }
	}
	
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
		gridpos = (gridpos + m_TileContainer.transform.localPosition) / m_TileSize / transform.localScale.x;

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
			tile = m_GridBoard[_GridPoint.ToString()];
		}
		return(tile);
	}
	
	public void CreateTile(TGridPoint _GridPoint)
	{
		if(!m_GridBoard.ContainsKey(_GridPoint.ToString()))
		{
			GameObject newtile = new GameObject("Tile");
			newtile.transform.parent = m_TileContainer.transform;
			newtile.transform.localScale = Vector3.one;
			newtile.transform.localRotation = Quaternion.identity;
			newtile.transform.localPosition = GetLocalPosition(_GridPoint);

			CTile tile = newtile.AddComponent<CTile>();
			tile.m_Grid = this;
			tile.m_Location = _GridPoint;

			m_GridBoard.Add(_GridPoint.ToString(), tile);
		}
	}

	public void RemoveTile(TGridPoint _GridPoint)
	{
		if (m_GridBoard.ContainsKey(_GridPoint.ToString()))
		{
			CTile tile = m_GridBoard[_GridPoint.ToString()];
			tile.Release();

			m_GridBoard.Remove(_GridPoint.ToString());
			Destroy(tile.gameObject);
		}
	}
}


