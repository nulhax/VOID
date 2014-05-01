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
	public delegate void HandleGridTileEvent(CTileInterface _Tile);

	public event HandleGridTileEvent EventTileInterfaceCreated;

	
	// Member Fields
	public float m_TileSize = 4.0f;
	public GameObject m_TileFactoryPrefab = null;
	public GameObject m_TileContainer = null;

	private CTileFactory m_TileFactory = null;

	private Dictionary<string, CTileInterface> m_GridBoard = new Dictionary<string, CTileInterface>();

	
	// Member Properties
	public GameObject TileContainer
	{
		get { return(m_TileContainer); }
	}

	public CTileFactory TileFactory
	{
		get { return(m_TileFactory); } 
	}

	public List<CTileInterface> Tiles
	{
		get { return(new List<CTileInterface>(m_GridBoard.Values)); }
	}


	// Member Methods
	private void Start() 
	{
		// Create the grid objects
		CreateGridObjects();

		// Register for when players join
		CGamePlayers.Instance.EventPlayerJoin += OnPlayerJoin;
	}

	[AServerOnly]
	private void OnPlayerJoin(ulong _PlayerId)
	{
		// Sync each tile to the player
		foreach(CTileInterface tileInterface in Tiles)
		{
			tileInterface.SyncAllTilesToPlayer(_PlayerId);
		}
	}

	private void CreateGridObjects()
	{
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

	public CTileInterface GetTile(CGridPoint _GridPoint)
	{
		CTileInterface tile = null;
		if(m_GridBoard.ContainsKey(_GridPoint.ToString()))
		{
			tile = m_GridBoard[_GridPoint.ToString()];
		}
		return(tile);
	}

	public List<CTileInterface> ImportTileInformation(List<CTileInterface> _Tiles)
	{
		// Keep a list of tiles which werent modified
		List<CTileInterface> unmodifiedTiles = Tiles;
		List<CTileInterface> newTiles = new List<CTileInterface>();
	
		// Iterate all the new tiles to use
		foreach(CTileInterface tile in _Tiles)
		{
			// If the tile exists, remove from the list of unmodified tiles
			CTileInterface existingTile = GetTile(tile.m_GridPosition);
			if(existingTile != null)
				unmodifiedTiles.Remove(existingTile);

			// Place the tile and clone the info from the original
			CTileInterface newTile = PlaceTile(tile.m_GridPosition);
			newTile.Clone(tile);
			newTiles.Add(newTile);
		}

		// Remove all tiles that dont exist anymore
		foreach(CTileInterface tile in unmodifiedTiles)
		{
			RemoveTile(tile.m_GridPosition);
		}

 		return(newTiles);
	}

	[AServerOnly]
	public CTileInterface PlaceTile(CGridPoint _Position)
	{
		CTileInterface tileInterface = GetTile(_Position);

		// Create if it doesnt exist yet
		if(tileInterface == null)
		{
			// Instantiate the new tile
			GameObject newtile = CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.Tile);

			newtile.transform.parent = m_TileContainer.transform;
			newtile.transform.localScale = Vector3.one;
			newtile.transform.localRotation = Quaternion.identity;
			newtile.transform.localPosition = GetLocalPosition(_Position.ToVector);

			CNetworkView tileNetworkView = newtile.GetComponent<CNetworkView>();
			tileNetworkView.SyncParent();
			tileNetworkView.SyncTransformScale();
			tileNetworkView.SyncTransformLocalPosition();
			tileNetworkView.SyncTransformLocalEuler();

			tileInterface = newtile.GetComponent<CTileInterface>();
			tileInterface.m_Grid = this;
			tileInterface.m_GridPosition = _Position;

			m_GridBoard.Add(_Position.ToString(), tileInterface);

			// Update neighbours
			tileInterface.FindNeighbours();
			tileInterface.UpdateNeighbourhood();

			if(EventTileInterfaceCreated != null)
				EventTileInterfaceCreated(tileInterface);
		}
	
		return(tileInterface);
	}

	[AServerOnly]
	public void RemoveTile(CGridPoint _Position)
	{
		CTileInterface tileInterface = GetTile(_Position);
		
		// Create if it doesnt exist yet
		if(tileInterface == null)
			return;

		m_GridBoard.Remove(_Position.ToString());

		// Update the neighbourhood
		tileInterface.UpdateNeighbourhood();

		// Destroy
		CNetwork.Factory.DestoryObject(tileInterface.gameObject);
	}
}

