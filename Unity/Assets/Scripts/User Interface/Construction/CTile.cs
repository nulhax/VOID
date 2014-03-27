//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CTile.cs
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

[System.Serializable]
public class CTile : CGridObject 
{
	// Member Types
	public enum ETileType
	{
		INVALID = -1,

		Floor,
		Wall,

		MAX
	}

	public struct TFloorTileMeta
	{
		public enum EType 
		{ 
			INVALID = -1,
			
			Middle, 
			Corner, 
			Edge,
			End,
			Hall,
			Cell,
			
			MAX
		}

		public TFloorTileMeta(EType _Type, int _Identifier, Quaternion _Rotation)
		{
			m_FloorTileIdentifier = _Identifier;
			m_FloorType = _Type;
			m_FloorTileRotation = _Rotation;
		}
		
		public int m_FloorTileIdentifier;
		public EType m_FloorType;
		public Quaternion m_FloorTileRotation;
	}

	public struct TWallTileMeta
	{
		public enum EType 
		{ 
			INVALID = -1,
			 
			None,
			Corner, 
			Edge,
			End,
			Hall,
			Cell,
			InverseCorner,
			
			MAX
		}
		
		public TWallTileMeta(EType _Type, int _Identifier, Quaternion _Rotation)
		{
			m_WallTileIdentifier = _Identifier;
			m_WallType = _Type;
			m_WallTileRotation = _Rotation;
		}
		
		public int m_WallTileIdentifier;
		public EType m_WallType;
		public Quaternion m_WallTileRotation;
	}

	
	// Member Delegates & Events
	
	
	// Member Fields
	public ETileType m_TileType;
	public GameObject m_TileObject;
	public List<CNeighbour> m_NeighbourHood = new List<CNeighbour>();

	public int m_FloorTileMetaIdentifier = 0;
	private TFloorTileMeta.EType m_FloorObjectType = TFloorTileMeta.EType.INVALID;
	private GameObject m_FloorObject = null;

	public int m_WallTileMetaIdentifier = 0;
	private TWallTileMeta.EType m_WallObjectType = TWallTileMeta.EType.INVALID;
	private GameObject m_WallObject = null;
	
	private Dictionary<CNeighbour.EDirection, GameObject> m_WallInverseObjects = new Dictionary<CNeighbour.EDirection, GameObject>();

	static private bool s_DictionariesInitialised = false;
	static private Dictionary<int, TFloorTileMeta> s_FloorTileMetaInfo = new Dictionary<int, TFloorTileMeta>();
	static private Dictionary<int, TWallTileMeta> s_WallTileMetaInfo = new Dictionary<int, TWallTileMeta>();

	public List<CNeighbour> s_AllNeighbours = new List<CNeighbour>(
				new CNeighbour[] 
				{
					new CNeighbour(new Point(0, 0, 1), CNeighbour.EDirection.North),
					new CNeighbour(new Point(1, 0, 0), CNeighbour.EDirection.East),
					new CNeighbour(new Point(0, 0, -1), CNeighbour.EDirection.South),
					new CNeighbour(new Point(-1, 0, 0), CNeighbour.EDirection.West),
					new CNeighbour(new Point(1, 0, 1), CNeighbour.EDirection.NorthEast),
					new CNeighbour(new Point(1, 0, -1), CNeighbour.EDirection.SouthEast),
					new CNeighbour(new Point(-1, 0, -1), CNeighbour.EDirection.SouthWest),
					new CNeighbour(new Point(-1, 0, 1), CNeighbour.EDirection.NorthWest),
				});

	// Member Properties

	
	// Member Methods
	public CTile(Point _Point)
	: this(_Point.x, _Point.y, _Point.z)
	{
	}

	public CTile(int x, int y, int z)
	: base(x, y, z)
	{
		// If the dictionary is empty we need to fill it
		if(!s_DictionariesInitialised)
		{
			FillFloorTileMetaData();
			FillWallTileMetaData();

			s_DictionariesInitialised = true;
		}
	}

	private static void FillFloorTileMetaData()
	{
		AddFloorMetaEntry(TFloorTileMeta.EType.INVALID, -1, Quaternion.identity);

		AddFloorMetaEntry(TFloorTileMeta.EType.Middle, 15, Quaternion.identity);

		AddFloorMetaEntry(TFloorTileMeta.EType.Cell, 0, Quaternion.identity);

		AddFloorMetaEntry(TFloorTileMeta.EType.Hall, 10, Quaternion.identity);
		AddFloorMetaEntry(TFloorTileMeta.EType.Hall, 5, Quaternion.Euler(0.0f, 90.0f, 0.0f));

		AddFloorMetaEntry(TFloorTileMeta.EType.Edge, 7, Quaternion.identity);
		AddFloorMetaEntry(TFloorTileMeta.EType.Edge, 14, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddFloorMetaEntry(TFloorTileMeta.EType.Edge, 13, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddFloorMetaEntry(TFloorTileMeta.EType.Edge, 11, Quaternion.Euler(0.0f, 270.0f, 0.0f));

		AddFloorMetaEntry(TFloorTileMeta.EType.Corner, 6, Quaternion.identity);
		AddFloorMetaEntry(TFloorTileMeta.EType.Corner, 12, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddFloorMetaEntry(TFloorTileMeta.EType.Corner, 9, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddFloorMetaEntry(TFloorTileMeta.EType.Corner, 3, Quaternion.Euler(0.0f, 270.0f, 0.0f));

		AddFloorMetaEntry(TFloorTileMeta.EType.End, 2, Quaternion.identity);
		AddFloorMetaEntry(TFloorTileMeta.EType.End, 4, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddFloorMetaEntry(TFloorTileMeta.EType.End, 8, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddFloorMetaEntry(TFloorTileMeta.EType.End, 1, Quaternion.Euler(0.0f, 270.0f, 0.0f));
	}

	private static void FillWallTileMetaData()
	{
		AddWallMetaEntry(TWallTileMeta.EType.INVALID, -1, Quaternion.identity);

		AddWallMetaEntry(TWallTileMeta.EType.None, 15, Quaternion.identity);

		AddWallMetaEntry(TWallTileMeta.EType.Cell, 0, Quaternion.identity);
		
		AddWallMetaEntry(TWallTileMeta.EType.Hall, 10, Quaternion.identity);
		AddWallMetaEntry(TWallTileMeta.EType.Hall, 5, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		
		AddWallMetaEntry(TWallTileMeta.EType.Edge, 7, Quaternion.identity);
		AddWallMetaEntry(TWallTileMeta.EType.Edge, 14, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddWallMetaEntry(TWallTileMeta.EType.Edge, 13, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddWallMetaEntry(TWallTileMeta.EType.Edge, 11, Quaternion.Euler(0.0f, 270.0f, 0.0f));
		
		AddWallMetaEntry(TWallTileMeta.EType.Corner, 6, Quaternion.identity);
		AddWallMetaEntry(TWallTileMeta.EType.Corner, 12, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddWallMetaEntry(TWallTileMeta.EType.Corner, 9, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddWallMetaEntry(TWallTileMeta.EType.Corner, 3, Quaternion.Euler(0.0f, 270.0f, 0.0f));
		
		AddWallMetaEntry(TWallTileMeta.EType.End, 2, Quaternion.identity);
		AddWallMetaEntry(TWallTileMeta.EType.End, 4, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddWallMetaEntry(TWallTileMeta.EType.End, 8, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddWallMetaEntry(TWallTileMeta.EType.End, 1, Quaternion.Euler(0.0f, 270.0f, 0.0f));
	}
	
	private static void AddFloorMetaEntry(TFloorTileMeta.EType _Type, int _Identifier, Quaternion _Rotation)
	{
		s_FloorTileMetaInfo.Add(_Identifier, new TFloorTileMeta(_Type, _Identifier, _Rotation));
	}

	private static void AddWallMetaEntry(TWallTileMeta.EType _Type, int _Identifier, Quaternion _Rotation)
	{
		s_WallTileMetaInfo.Add(_Identifier, new TWallTileMeta(_Type, _Identifier, _Rotation));
	}

	public void FindNeighbours()
	{
		List<CNeighbour> neighbours = new List<CNeighbour>();
		foreach(CNeighbour pn in s_AllNeighbours) 
		{
			CTile possibleNeightbour = new CTile(X + pn.gridPositionOffset.x, 
			                                     Y + pn.gridPositionOffset.y, 
			                                     Z + pn.gridPositionOffset.z);
			
			if(CGrid.I.m_GridBoard.ContainsKey(possibleNeightbour.ToString()))
			{
				CNeighbour newNeighbour = new CNeighbour(pn.gridPositionOffset, pn.direction);
				newNeighbour.tile = CGrid.I.m_GridBoard[possibleNeightbour.ToString()].tile;

				neighbours.Add(newNeighbour);
			}
		}
		
		if (neighbours.Count > 0)
			m_NeighbourHood = neighbours;

		// Update the tile object immediately
		UpdateTileObjects();
	}

	public void UpdateNeighbours()
	{
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			neighbour.tile.FindNeighbours();
		}
	}

	public void UpdateTileObjects()
	{
		if(m_TileObject == null)
			return;

		UpdateFloorTileObject();
		UpdateWallTileObject();
		UpdateWallInverseObjects();
	}

	public void UpdateFloorTileObject()
	{
		// Find the meta identifier
		m_FloorTileMetaIdentifier = 0;
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			// Only need to base off the n, e, s, w neightbours for floor
			if(neighbour.direction == CNeighbour.EDirection.North ||
			   neighbour.direction == CNeighbour.EDirection.East ||
			   neighbour.direction == CNeighbour.EDirection.South ||
			   neighbour.direction == CNeighbour.EDirection.West)
			{
				m_FloorTileMetaIdentifier |= 1 << (int)neighbour.direction;
			}
		}

		Quaternion rotation = Quaternion.identity;
		TFloorTileMeta.EType type = TFloorTileMeta.EType.Middle;
		
		// Find the meta data for this tile
		if(s_FloorTileMetaInfo.ContainsKey(m_FloorTileMetaIdentifier))
		{
			type = s_FloorTileMetaInfo[m_FloorTileMetaIdentifier].m_FloorType;
			rotation = s_FloorTileMetaInfo[m_FloorTileMetaIdentifier].m_FloorTileRotation;
		}
		else
		{
			// If it wasn't found there was no meta data found for it
			Debug.LogWarning("TileFloor Meta wasn't found for a tile within its neighbourhood.");
		}
		
		// If the tile type has changed
		if(m_FloorObjectType != type)
		{
			ReleaseFloorTile();
			
			m_FloorObjectType = type;
			m_FloorObject = CGrid.I.m_TileFactory.NewFloorTile(type);
			m_FloorObject.transform.parent = m_TileObject.transform;
			m_FloorObject.transform.localPosition = Vector3.zero;
			m_FloorObject.transform.localScale = Vector3.one;
			m_FloorObject.transform.localRotation = rotation;
		}
	}

	public void UpdateWallTileObject()
	{
		// Find the meta identifier
		m_WallTileMetaIdentifier = 0;
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			// Only need to base off the n, e, s, w neightbours for wall
			if(neighbour.direction == CNeighbour.EDirection.North ||
			   neighbour.direction == CNeighbour.EDirection.East ||
			   neighbour.direction == CNeighbour.EDirection.South ||
			   neighbour.direction == CNeighbour.EDirection.West)
			{
				m_WallTileMetaIdentifier |= 1 << (int)neighbour.direction;
			}
		}
		
		Quaternion rotation = Quaternion.identity;
		TWallTileMeta.EType type = TWallTileMeta.EType.None;
		
		// Find the meta data for this tile
		if(s_WallTileMetaInfo.ContainsKey(m_WallTileMetaIdentifier))
		{
			type = s_WallTileMetaInfo[m_WallTileMetaIdentifier].m_WallType;
			rotation = s_WallTileMetaInfo[m_WallTileMetaIdentifier].m_WallTileRotation;
		}
		else
		{
			// If it wasn't found there was no meta data found for it
			Debug.LogWarning("TileWall Meta wasn't found for a tile within its neighbourhood.");
		}
		
		// If the tile type has changed
		if(m_WallObjectType != type)
		{
			ReleaseWallTile();
			
			m_WallObjectType = type;

			// None wall type will have no wall object to instantiate
			if(type != TWallTileMeta.EType.None)
			{
				m_WallObject = CGrid.I.m_TileFactory.NewWallTile(type);
				m_WallObject.transform.parent = m_TileObject.transform;
				m_WallObject.transform.localPosition = Vector3.zero;
				m_WallObject.transform.localScale = Vector3.one;
				m_WallObject.transform.localRotation = rotation;
			}
			else
			{
				m_WallObject = null;
			}
		}
	}

	public void UpdateWallInverseObjects()
	{
		// Find the missing neighbours
		List<CNeighbour.EDirection> missingNeighbours = new List<CNeighbour.EDirection>((CNeighbour.EDirection[])Enum.GetValues(typeof(CNeighbour.EDirection)));
		List<CNeighbour.EDirection> currentNeighbours = new List<CNeighbour.EDirection>();
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			currentNeighbours.Add(neighbour.direction);
			missingNeighbours.Remove(neighbour.direction);

			// If we find a neighbour that is ne, nw, se, sw, remove the current tile piece
			if(m_WallInverseObjects.ContainsKey(neighbour.direction))
			{
				ReleaseWallInverseTile(m_WallInverseObjects[neighbour.direction]);
				m_WallInverseObjects.Remove(neighbour.direction);
			}
		}

		// Inverse walls look for empty spaces
		foreach(CNeighbour.EDirection direction in missingNeighbours)
		{
			// Only need to base off the ne, ne, sw, se neightbours for inverse wall
			if(direction == CNeighbour.EDirection.NorthEast ||
			   direction == CNeighbour.EDirection.SouthEast ||
			   direction == CNeighbour.EDirection.SouthWest ||
			   direction == CNeighbour.EDirection.NorthWest)
			{
				// Get the tangent directions of this direction from the enum name
				string dir1 = direction.ToString().Contains("North") ? "North" : "South";
				string dir2 = direction.ToString().Contains("East") ? "East" : "West";
				CNeighbour.EDirection tang1 = (CNeighbour.EDirection)Enum.Parse(typeof(CNeighbour.EDirection), dir1);
				CNeighbour.EDirection tang2 = (CNeighbour.EDirection)Enum.Parse(typeof(CNeighbour.EDirection), dir2);
				
				// If these neighbours both exist then add inverse wall cap
				if(currentNeighbours.Contains(tang1) && currentNeighbours.Contains(tang2) &&
				   !m_WallInverseObjects.ContainsKey(direction))
				{
					GameObject wallInverseObject = CGrid.I.m_TileFactory.NewWallTile(TWallTileMeta.EType.InverseCorner);
					wallInverseObject.transform.parent = m_TileObject.transform;
					wallInverseObject.transform.localPosition = Vector3.zero;
					wallInverseObject.transform.localScale = Vector3.one;
					m_WallInverseObjects.Add(direction, wallInverseObject);

					// Rotation part needs to be detirmined based on which neighbour was missing
					switch(direction)
					{
					case CNeighbour.EDirection.NorthEast: wallInverseObject.transform.localRotation = Quaternion.identity; break;
					case CNeighbour.EDirection.SouthEast: wallInverseObject.transform.localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f); break;
					case CNeighbour.EDirection.SouthWest: wallInverseObject.transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f); break;
					case CNeighbour.EDirection.NorthWest: wallInverseObject.transform.localRotation = Quaternion.Euler(0.0f, 270.0f, 0.0f); break;
					}
				}
				else if(m_WallInverseObjects.ContainsKey(direction))
				{
					ReleaseWallInverseTile(m_WallInverseObjects[direction]);
					m_WallInverseObjects.Remove(direction);
				}
			}
		}
	}
	
	public void ReleaseExistingTileObjects()
	{
		// Release floor tile object
		ReleaseFloorTile();

		// Release wall tile object
		ReleaseWallTile();

		// Release all inverse wall objects
		foreach(GameObject iwt in m_WallInverseObjects.Values)
		{
			ReleaseWallInverseTile(iwt);
		}
		m_WallInverseObjects.Clear();
	}

	private void ReleaseFloorTile()
	{
		if(m_FloorObject != null)
		{
			CGrid.I.m_TileFactory.ReleaseTileObject(m_FloorObject);
			m_FloorObjectType = TFloorTileMeta.EType.INVALID;
			m_FloorObject = null;
		}
	}

	private void ReleaseWallTile()
	{
		if(m_WallObject != null)
		{
			CGrid.I.m_TileFactory.ReleaseTileObject(m_WallObject);
			m_WallObjectType = TWallTileMeta.EType.INVALID;
			m_WallObject = null;
		}
	}

	private void ReleaseWallInverseTile(GameObject _InverseWallTile)
	{
		if(_InverseWallTile != null)
		{
			CGrid.I.m_TileFactory.ReleaseTileObject(_InverseWallTile);
			_InverseWallTile = null;
		}
	}
}
