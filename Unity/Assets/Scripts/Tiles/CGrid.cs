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
	public delegate void HandleGridTileEvent(CTile _Tile);

	public event HandleGridTileEvent EventTilePlaced;
	public event HandleGridTileEvent EventTileRemoved;
	
	// Member Fields
	public float m_TileSize = 4.0f;
	public GameObject m_TileFactoryPrefab = null;

	private Transform m_TileContainer = null;
	private CTileFactory m_TileFactory = null;

	private Dictionary<string, CTile> m_GridBoard = new Dictionary<string, CTile>();

	
	// Member Properties
	public Transform TileContainer
	{
		get { return(m_TileContainer); }
	}

	public CTileFactory TileFactory
	{
		get { return(m_TileFactory); } 
	}

	public List<CTile> Tiles
	{
		get { return(new List<CTile>(m_GridBoard.Values)); }
	}


	// Member Methods
	private void Awake() 
	{
		// Create the grid objects
		CreateGridObjects();
	}

	private void Update()
	{

	}

	private void CreateGridObjects()
	{
		m_TileContainer = new GameObject("Tile Container").transform;
		m_TileContainer.parent = transform;
		m_TileContainer.localScale = Vector3.one;
		m_TileContainer.localPosition = Vector3.zero;
		m_TileContainer.localRotation = Quaternion.identity;

		Transform tileFactory = ((GameObject)GameObject.Instantiate(m_TileFactoryPrefab)).transform;
		tileFactory.parent = transform;
		tileFactory.localScale = Vector3.one;
		tileFactory.localPosition = Vector3.zero;
		tileFactory.localRotation = Quaternion.identity;

		m_TileFactory = tileFactory.GetComponent<CTileFactory>();
	}

	public TGridPoint GetGridPoint(Vector3 worldPosition)
	{
		Vector3 gridPos = GetGridPosition(worldPosition);

		gridPos.x = Mathf.Round(gridPos.x);
		gridPos.y = Mathf.Round(gridPos.y);
		gridPos.z = Mathf.Round(gridPos.z);

		return(new TGridPoint(gridPos));
	}

	public Vector3 GetGridPosition(Vector3 _WorldPosition)
	{
		// Convert the world space to grid space
		Vector3 gridpos = Quaternion.Inverse(transform.rotation) * (_WorldPosition - transform.position);

		// Scale the position to tilesize and scale
		gridpos = gridpos / m_TileSize / transform.localScale.x;

		return gridpos;
	}

	public Vector3 GetLocalPosition(Vector3 _WorldPosition)
	{
		// Convert from grid space to local space
		return(_WorldPosition * m_TileSize);
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

	public List<CTile> ImportTileInformation(List<CTile> _Tiles)
	{
		// Keep a list of tiles which werent modified
		List<CTile> unmodifiedTiles = Tiles;
		List<CTile> newTiles = new List<CTile>();
	
		// Iterate all the new tiles to use
		foreach(CTile tile in _Tiles)
		{
			// Get the meta information of the tile
			int typeIdentifier = tile.m_TileTypeIdentifier;

			// If the tile exists, remove from the list of unmodified tiles
			CTile existingTile = GetTile(tile.m_GridPosition);
			if(existingTile != null)
				unmodifiedTiles.Remove(existingTile);

			// Place the tile and clone the info from the original
			CTile newTile = PlaceTile(tile.m_GridPosition, new List<ETileType>());
			newTile.CloneTileMetaData(tile);
			newTiles.Add(newTile);
		}

		// Remove all tiles that dont exist anymore
		foreach(CTile tile in unmodifiedTiles)
		{
			RemoveTile(tile.m_GridPosition);
		}

		return(newTiles);
	}
	
	public CTile PlaceTile(TGridPoint _Position, List<ETileType> _TileTypes)
	{
		CTile tile = null;

		// If it exists, destory it
		if(m_GridBoard.ContainsKey(_Position.ToString()))
		{
			RemoveTile(_Position);
		}

		// Create the new tile
		GameObject newtile = new GameObject("Tile");
		newtile.transform.parent = m_TileContainer;
		newtile.transform.localScale = Vector3.one;
		newtile.transform.localRotation = Quaternion.identity;
		newtile.transform.localPosition = GetLocalPosition(_Position.ToVector);
		tile = newtile.AddComponent<CTile>();
		tile.m_Grid = this;
		tile.m_GridPosition = _Position;

		// Set the active tile types
		foreach(ETileType type in _TileTypes)
		{
			tile.SetTileTypeState(type, true);
		}
		m_GridBoard.Add(_Position.ToString(), tile);

		// Update neighbours
		tile.FindNeighbours();
		tile.UpdateNeighbourhood();
	
		return(tile);
	}

	public void RemoveTile(TGridPoint _GridPoint)
	{
		if(!m_GridBoard.ContainsKey(_GridPoint.ToString()))
			return;

		CTile tile = m_GridBoard[_GridPoint.ToString()];
		m_GridBoard.Remove(_GridPoint.ToString());

		// Release
		tile.UpdateNeighbourhood();
		tile.Release();

		// Destroy
		Destroy(tile.gameObject);
	}
}


