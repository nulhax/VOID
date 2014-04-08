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
	public struct TCreateTileInfo
	{
		public TCreateTileInfo(TGridPoint _GridPoint, ETileType[] _TileTypes)
		{
			m_GridPoint = _GridPoint;
			m_TileTypes = _TileTypes;
		}

		public TGridPoint m_GridPoint;
		public ETileType[] m_TileTypes;
	}

	
	// Member Delegates & Events
	public delegate void HandleTileEvent(CTile _Tile);
	
	public event HandleTileEvent EventTileAdded;
	public event HandleTileEvent EventTileRemoved;

	
	// Member Fields
	public float m_TileSize = 4.0f;

	private Transform m_TileContainer = null;
	private CTileFactory m_TileFactory = null;

	private List<TCreateTileInfo> m_CreateQueue = new List<TCreateTileInfo>();
	private List<TGridPoint> m_DestroyQueue = new List<TGridPoint>();

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
		foreach(TCreateTileInfo createInfo in m_CreateQueue)
		{
			CreateTile(createInfo);
		}
		m_CreateQueue.Clear();

		foreach(TGridPoint point in m_DestroyQueue)
		{
			RemoveTile(point);
		}
		m_DestroyQueue.Clear();
	}

	private void CreateGridObjects()
	{
		m_TileContainer = new GameObject("Tile Container").transform;
		m_TileContainer.parent = transform;
		m_TileContainer.localScale = Vector3.one;
		m_TileContainer.localPosition = Vector3.zero;
		m_TileContainer.localRotation = Quaternion.identity;

		Transform tileFactory = ((GameObject)GameObject.Instantiate(Resources.Load("Prefabs/User Interface/Construction/Tile Factory"))).transform;
		tileFactory.parent = transform;
		tileFactory.localScale = Vector3.one;
		tileFactory.localPosition = Vector3.zero;
		tileFactory.localRotation = Quaternion.identity;

		m_TileFactory = tileFactory.GetComponent<CTileFactory>();
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
			tile = m_GridBoard[_GridPoint.ToString()];
		}
		return(tile);
	}

	public void AddNewTile(TGridPoint _GridPoint, ETileType[] _TileTypes)
	{
		m_CreateQueue.Add(new TCreateTileInfo(_GridPoint, _TileTypes));
	}

	public void ReleaseTile(TGridPoint _GridPoint)
	{
		m_DestroyQueue.Add(_GridPoint);
	}

	public void ImportPreExistingTiles(CTile[] _Tiles)
	{
		foreach(CTile tile in _Tiles)
		{
			if(!m_GridBoard.ContainsKey(tile.m_GridPosition.ToString()))
			{
				m_GridBoard.Add(tile.m_GridPosition.ToString(), tile);
				tile.m_Grid = this;
			}
			else
			{
				Debug.LogWarning("Tile already exists at position " + tile.m_GridPosition.ToString() + ". Tile was not imported.");
			}
		}
	}

	public void ImportTileInformation(CTile[] _Tiles)
	{
		// Keep a list of tiles which werent modified
		List<CTile> m_UnmodifiedTiles = Tiles;	
	
		// Iterate all the new tiles to use
		foreach(CTile tile in _Tiles)
		{
			// Get the meta information of the tile
			int typeIdentifier = tile.m_TileTypeIdentifier;

			// Change the existing tile to match this tile
			if(m_GridBoard.ContainsKey(tile.m_GridPosition.ToString()))
			{
				CTile existingTile = m_GridBoard[tile.m_GridPosition.ToString()];
				existingTile.m_TileTypeIdentifier = typeIdentifier;

				// Replace the meta data
				for(int i = (int)ETileType.INVALID + 1; i < (int)ETileType.MAX; ++i)
				{
					existingTile.SetMetaData((ETileType)i, tile.GetMetaData((ETileType)i));
				}

				// Remove this tile from ones unmodified
				m_UnmodifiedTiles.Remove(existingTile);
			}
			else
			{
				// Get the active tile types
				List<ETileType> tileTypes = new List<ETileType>();
				for(int i = (int)ETileType.INVALID + 1; i < (int)ETileType.MAX; ++i)
				{
					if(tile.GetTileTypeState((ETileType)i))
						tileTypes.Add((ETileType)i);
				}

				AddNewTile(tile.m_GridPosition, tileTypes.ToArray());
			}
		}

		// Remove all tiles that dont exist anymore
		foreach(CTile tile in m_UnmodifiedTiles)
		{
			ReleaseTile(tile.m_GridPosition);
		}
	}
	
	private void CreateTile(TCreateTileInfo _TileInfo)
	{
		if(!m_GridBoard.ContainsKey(_TileInfo.m_GridPoint.ToString()))
		{
			GameObject newtile = new GameObject("Tile");
			newtile.transform.parent = m_TileContainer;
			newtile.transform.localScale = Vector3.one;
			newtile.transform.localRotation = Quaternion.identity;
			newtile.transform.localPosition = GetLocalPosition(_TileInfo.m_GridPoint);

			CTile tile = newtile.AddComponent<CTile>();
			tile.m_Grid = this;
			tile.m_GridPosition = _TileInfo.m_GridPoint;

			// Set the active tile types
			foreach(ETileType type in _TileInfo.m_TileTypes)
			{
				tile.SetTileTypeState(type, true);
			}
			m_GridBoard.Add(_TileInfo.m_GridPoint.ToString(), tile);

			// Find neighbours
			tile.FindNeighbours();

			// Fire event for tile creation
			if(EventTileAdded != null)
				EventTileAdded(tile);
		}
	}

	private void RemoveTile(TGridPoint _GridPoint)
	{
		if (m_GridBoard.ContainsKey(_GridPoint.ToString()))
		{
			CTile tile = m_GridBoard[_GridPoint.ToString()];

			// Fire event for tile removal
			if(EventTileRemoved != null)
				EventTileRemoved(tile);

			// Release
			tile.Release();

			// Destroy
			m_GridBoard.Remove(_GridPoint.ToString());
			Destroy(tile.gameObject);
		}
	}
}


