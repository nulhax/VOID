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
	public delegate void HandleGridTileEvent(CTileRoot _Tile);

	public event HandleGridTileEvent EventTilePlaced;
	public event HandleGridTileEvent EventTileRemoved;
	
	// Member Fields
	public float m_TileSize = 4.0f;
	public GameObject m_TileFactoryPrefab = null;

	private Transform m_TileContainer = null;
	private CTileFactory m_TileFactory = null;

	private Dictionary<string, CTileRoot> m_GridBoard = new Dictionary<string, CTileRoot>();

	
	// Member Properties
	public Transform TileContainer
	{
		get { return(m_TileContainer); }
	}

	public CTileFactory TileFactory
	{
		get { return(m_TileFactory); } 
	}

	public List<CTileRoot> Tiles
	{
		get { return(new List<CTileRoot>(m_GridBoard.Values)); }
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

	public CGridPoint GetGridPoint(Vector3 worldPosition)
	{
		Vector3 gridPos = GetGridPosition(worldPosition);

		gridPos.x = Mathf.Round(gridPos.x);
		gridPos.y = Mathf.Round(gridPos.y);
		gridPos.z = Mathf.Round(gridPos.z);

		return(new CGridPoint(gridPos));
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

	public CTileRoot GetTile(CGridPoint _GridPoint)
	{
		CTileRoot tile = null;
		if(m_GridBoard.ContainsKey(_GridPoint.ToString()))
		{
			tile = m_GridBoard[_GridPoint.ToString()];
		}
		return(tile);
	}

	public List<CTileRoot> ImportTileInformation(List<CTileRoot> _Tiles)
	{
		// Keep a list of tiles which werent modified
		List<CTileRoot> unmodifiedTiles = Tiles;
		List<CTileRoot> newTiles = new List<CTileRoot>();
	
		// Iterate all the new tiles to use
		foreach(CTileRoot tile in _Tiles)
		{
			// Get the meta information of the tile
			int typeIdentifier = tile.m_TileTypeMask;

			// If the tile exists, remove from the list of unmodified tiles
			CTileRoot existingTile = GetTile(tile.m_GridPosition);
			if(existingTile != null)
				unmodifiedTiles.Remove(existingTile);

			// Place the tile and clone the info from the original
			CTileRoot newTile = PlaceTile(tile.m_GridPosition, new List<CTile.EType>());
			newTile.Clone(tile);
			newTiles.Add(newTile);
		}

		// Remove all tiles that dont exist anymore
		foreach(CTileRoot tile in unmodifiedTiles)
		{
			RemoveTile(tile.m_GridPosition);
		}

		return(newTiles);
	}
	
	public CTileRoot PlaceTile(CGridPoint _Position, List<CTile.EType> _TileTypes)
	{
		CTileRoot tile = null;

		// If it exists, remove all tile types that shouln't exist
		if(GetTile(_Position) != null)
		{
			tile = GetTile(_Position);

			for(int i = (int)CTile.EType.INVALID + 1; i < (int)CTile.EType.MAX; ++i)
				tile.SetTileTypeState((CTile.EType)i, _TileTypes.Contains((CTile.EType)i));
		}
		else
		{
			// Create the new tile
			GameObject newtile = new GameObject("Tile");
			newtile.transform.parent = m_TileContainer;
			newtile.transform.localScale = Vector3.one;
			newtile.transform.localRotation = Quaternion.identity;
			newtile.transform.localPosition = GetLocalPosition(_Position.ToVector);
			tile = newtile.AddComponent<CTileRoot>();
			tile.m_Grid = this;
			tile.m_GridPosition = _Position;

			m_GridBoard.Add(_Position.ToString(), tile);

			// Update neighbours
			tile.FindNeighbours();
			tile.UpdateNeighbourhood();
		}

		// Set the active tile types
		foreach(CTile.EType type in _TileTypes)
		{
			tile.SetTileTypeState(type, true);
		}
	
		return(tile);
	}

	public void RemoveTile(CGridPoint _GridPoint)
	{
		if(!m_GridBoard.ContainsKey(_GridPoint.ToString()))
			return;

		CTileRoot tile = m_GridBoard[_GridPoint.ToString()];
		m_GridBoard.Remove(_GridPoint.ToString());

		// Release
		tile.UpdateNeighbourhood();
		tile.Release();

		// Destroy
		Destroy(tile.gameObject);
	}
}


