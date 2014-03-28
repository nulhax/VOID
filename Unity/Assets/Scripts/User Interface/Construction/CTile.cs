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
using System.Linq;


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
			None,
			
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
			EdgeT,
			End,
			Hall,
			InverseCorner,

			InternalMiddle,
			InternalCorner,
			InternalT,
			InternalX,
			
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
	public CTileBehaviour m_TileObject = null;
	public List<CNeighbour> m_NeighbourHood = new List<CNeighbour>();

	public int m_FloorMetaIdentifier = 0;
	private TFloorTileMeta.EType m_FloorObjectType = TFloorTileMeta.EType.INVALID;
	private GameObject m_FloorObject = null;

	public int m_WallExteriorMetaIdentifier = 0;
	public int m_WallInteriorMetaIdentifier = 0;
	private TWallTileMeta.EType m_WallObjectType = TWallTileMeta.EType.INVALID;
	private GameObject m_WallObject = null;
	
	private Dictionary<CNeighbour.EDirection, GameObject> m_WallInverseObjects = new Dictionary<CNeighbour.EDirection, GameObject>();

	static private bool s_DictionariesInitialised = false;
	static private Dictionary<int, TFloorTileMeta> s_FloorTileMetaInfo = new Dictionary<int, TFloorTileMeta>();
	static private Dictionary<int, TWallTileMeta> s_WallExteriorTileMetaInfo = new Dictionary<int, TWallTileMeta>();
	static private Dictionary<int, TWallTileMeta> s_WallInteriorTileMetaInfo = new Dictionary<int, TWallTileMeta>();

	public List<CNeighbour> s_AllNeighbours = new List<CNeighbour>(
				new CNeighbour[] 
				{
					new CNeighbour(new TGridPoint(0, 0, 1), CNeighbour.EDirection.North),
					new CNeighbour(new TGridPoint(1, 0, 0), CNeighbour.EDirection.East),
					new CNeighbour(new TGridPoint(0, 0, -1), CNeighbour.EDirection.South),
					new CNeighbour(new TGridPoint(-1, 0, 0), CNeighbour.EDirection.West),
					new CNeighbour(new TGridPoint(1, 0, 1), CNeighbour.EDirection.NorthEast),
					new CNeighbour(new TGridPoint(1, 0, -1), CNeighbour.EDirection.SouthEast),
					new CNeighbour(new TGridPoint(-1, 0, -1), CNeighbour.EDirection.SouthWest),
					new CNeighbour(new TGridPoint(-1, 0, 1), CNeighbour.EDirection.NorthWest),
				});

	// Member Properties

	
	// Member Methods
	public CTile(TGridPoint _Point, CGrid _Grid)
	: this(_Point.x, _Point.y, _Point.z, _Grid)
	{

	}

	public CTile(int x, int y, int z, CGrid _Grid)
	: base(x, y, z, _Grid)
	{
		// If the dictionary is empty we need to fill it
		if(!s_DictionariesInitialised)
		{
			FillFloorTileMetaData();
			FillWallExteriorMetaData();
			FillWallInteriorMetaData();

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

	private static void FillWallExteriorMetaData()
	{
		AddWallExternalMetaEntry(TWallTileMeta.EType.INVALID, -1, Quaternion.identity);

		AddWallExternalMetaEntry(TWallTileMeta.EType.None, 15, Quaternion.identity);
		AddWallExternalMetaEntry(TWallTileMeta.EType.None, 0, Quaternion.identity);
		
		AddWallExternalMetaEntry(TWallTileMeta.EType.Hall, 10, Quaternion.identity);
		AddWallExternalMetaEntry(TWallTileMeta.EType.Hall, 5, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		
		AddWallExternalMetaEntry(TWallTileMeta.EType.Edge, 7, Quaternion.identity);
		AddWallExternalMetaEntry(TWallTileMeta.EType.Edge, 14, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddWallExternalMetaEntry(TWallTileMeta.EType.Edge, 13, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddWallExternalMetaEntry(TWallTileMeta.EType.Edge, 11, Quaternion.Euler(0.0f, 270.0f, 0.0f));
		
		AddWallExternalMetaEntry(TWallTileMeta.EType.Corner, 6, Quaternion.identity);
		AddWallExternalMetaEntry(TWallTileMeta.EType.Corner, 12, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddWallExternalMetaEntry(TWallTileMeta.EType.Corner, 9, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddWallExternalMetaEntry(TWallTileMeta.EType.Corner, 3, Quaternion.Euler(0.0f, 270.0f, 0.0f));
		
		AddWallExternalMetaEntry(TWallTileMeta.EType.End, 2, Quaternion.identity);
		AddWallExternalMetaEntry(TWallTileMeta.EType.End, 4, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddWallExternalMetaEntry(TWallTileMeta.EType.End, 8, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddWallExternalMetaEntry(TWallTileMeta.EType.End, 1, Quaternion.Euler(0.0f, 270.0f, 0.0f));
	}

	private static void FillWallInteriorMetaData()
	{
		AddWallInternalMetaEntry(TWallTileMeta.EType.INVALID, -1, Quaternion.identity);
		
		AddWallInternalMetaEntry(TWallTileMeta.EType.InternalX, 15, Quaternion.identity);
		
		AddWallInternalMetaEntry(TWallTileMeta.EType.None, 0, Quaternion.identity);
		
		AddWallInternalMetaEntry(TWallTileMeta.EType.InternalMiddle, 10, Quaternion.identity);
		AddWallInternalMetaEntry(TWallTileMeta.EType.InternalMiddle, 5, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		
		AddWallInternalMetaEntry(TWallTileMeta.EType.InternalT, 7, Quaternion.identity);
		AddWallInternalMetaEntry(TWallTileMeta.EType.InternalT, 14, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddWallInternalMetaEntry(TWallTileMeta.EType.InternalT, 13, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddWallInternalMetaEntry(TWallTileMeta.EType.InternalT, 11, Quaternion.Euler(0.0f, 270.0f, 0.0f));
		
		AddWallInternalMetaEntry(TWallTileMeta.EType.InternalCorner, 6, Quaternion.identity);
		AddWallInternalMetaEntry(TWallTileMeta.EType.InternalCorner, 12, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddWallInternalMetaEntry(TWallTileMeta.EType.InternalCorner, 9, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddWallInternalMetaEntry(TWallTileMeta.EType.InternalCorner, 3, Quaternion.Euler(0.0f, 270.0f, 0.0f));
		
		AddWallInternalMetaEntry(TWallTileMeta.EType.InternalMiddle, 2, Quaternion.identity);
		AddWallInternalMetaEntry(TWallTileMeta.EType.InternalMiddle, 4, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddWallInternalMetaEntry(TWallTileMeta.EType.InternalMiddle, 8, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddWallInternalMetaEntry(TWallTileMeta.EType.InternalMiddle, 1, Quaternion.Euler(0.0f, 270.0f, 0.0f));
	}
	
	private static void AddFloorMetaEntry(TFloorTileMeta.EType _Type, int _Identifier, Quaternion _Rotation)
	{
		s_FloorTileMetaInfo.Add(_Identifier, new TFloorTileMeta(_Type, _Identifier, _Rotation));
	}

	private static void AddWallExternalMetaEntry(TWallTileMeta.EType _Type, int _Identifier, Quaternion _Rotation)
	{
		s_WallExteriorTileMetaInfo.Add(_Identifier, new TWallTileMeta(_Type, _Identifier, _Rotation));
	}

	private static void AddWallInternalMetaEntry(TWallTileMeta.EType _Type, int _Identifier, Quaternion _Rotation)
	{
		s_WallInteriorTileMetaInfo.Add(_Identifier, new TWallTileMeta(_Type, _Identifier, _Rotation));
	}

	public void FindNeighbours()
	{
		m_NeighbourHood.Clear();

		foreach(CNeighbour pn in s_AllNeighbours) 
		{
			TGridPoint possibleNeightbour = new TGridPoint(	x + pn.gridPositionOffset.x, 
						                                    y + pn.gridPositionOffset.y, 
						                                    z + pn.gridPositionOffset.z);
			
			if(m_Grid.m_GridBoard.ContainsKey(possibleNeightbour.ToString()))
			{
				CNeighbour newNeighbour = new CNeighbour(pn.gridPositionOffset, pn.direction);
				newNeighbour.tile = m_Grid.m_GridBoard[possibleNeightbour.ToString()].m_Tile;

				m_NeighbourHood.Add(newNeighbour);
			}
		}
	}

	public void UpdateNeighbours()
	{
		// Find all neighbours
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			neighbour.tile.FindNeighbours();
		}

		// Update floor/external wall tile objects
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			if(neighbour.tile.m_TileObject == null)
				continue;

			neighbour.tile.UpdateFloorTile();
			neighbour.tile.UpdateExternalWallTile();
		}

		// Update inverse wall/internal wall tile objects
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			if(neighbour.tile.m_TileObject == null)
				continue;
			
			neighbour.tile.UpdateWallInverseCap();
			neighbour.tile.UpdateInternalWallTile(false);
		}

	}

	public void UpdateAllTileObjects()
	{
		if(m_TileObject == null)
			return;

		UpdateFloorTile();
		UpdateExternalWallTile();
		UpdateInternalWallTile(false);
		UpdateWallInverseCap();
	}

	public void UpdateFloorTile()
	{
		// Find the meta identifier
		m_FloorMetaIdentifier = 0;
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			// Only need to base off the n, e, s, w neightbours for floor
			if(neighbour.direction == CNeighbour.EDirection.North ||
			   neighbour.direction == CNeighbour.EDirection.East ||
			   neighbour.direction == CNeighbour.EDirection.South ||
			   neighbour.direction == CNeighbour.EDirection.West)
			{
				m_FloorMetaIdentifier |= 1 << (int)neighbour.direction;
			}
		}

		Quaternion rotation = Quaternion.identity;
		TFloorTileMeta.EType type = TFloorTileMeta.EType.Middle;
		
		// Find the meta data for this tile
		if(s_FloorTileMetaInfo.ContainsKey(m_FloorMetaIdentifier))
		{
			type = s_FloorTileMetaInfo[m_FloorMetaIdentifier].m_FloorType;
			rotation = s_FloorTileMetaInfo[m_FloorMetaIdentifier].m_FloorTileRotation;
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
			m_FloorObject = m_Grid.m_TileFactory.NewFloorTile(type);
			m_FloorObject.transform.parent = m_TileObject.transform;
			m_FloorObject.transform.localPosition = Vector3.zero;
			m_FloorObject.transform.localScale = Vector3.one;
			m_FloorObject.transform.localRotation = rotation;
		}
	}

	public void UpdateExternalWallTile()
	{
		// Find the meta identifier
		int wallExteriorMetaIdentifier = 0;
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			// Only need to base off the n, e, s, w neightbours for wall
			if(neighbour.direction == CNeighbour.EDirection.North ||
			   neighbour.direction == CNeighbour.EDirection.East ||
			   neighbour.direction == CNeighbour.EDirection.South ||
			   neighbour.direction == CNeighbour.EDirection.West)
			{
				wallExteriorMetaIdentifier |= 1 << (int)neighbour.direction;
			}
		}

		// If the neighbourhood hasn't changed, return out
		if(wallExteriorMetaIdentifier != m_WallExteriorMetaIdentifier)
			m_WallExteriorMetaIdentifier = wallExteriorMetaIdentifier;
		else
			return;

		Quaternion rotation = Quaternion.identity;
		TWallTileMeta.EType type = TWallTileMeta.EType.None;
		
		// Find the meta data for this tile
		if(s_WallExteriorTileMetaInfo.ContainsKey(m_WallExteriorMetaIdentifier))
		{
			type = s_WallExteriorTileMetaInfo[m_WallExteriorMetaIdentifier].m_WallType;
			rotation = s_WallExteriorTileMetaInfo[m_WallExteriorMetaIdentifier].m_WallTileRotation;
		}
		else
		{
			// If it wasn't found there was no meta data found for it
			Debug.LogWarning("TileWallExternal Meta wasn't found for a tile within its neighbourhood.");
		}

		ReleaseWallTile();
		m_WallObjectType = type;

		// None wall type will have no wall object to instantiate
		if(type != TWallTileMeta.EType.None)
		{
			// Check Edge for special case
			if(type == TWallTileMeta.EType.Edge)
			{
				// Change type to EdgeT if interior wall found
				foreach(CNeighbour neighbour in m_NeighbourHood)
					if((int)neighbour.tile.m_WallObjectType >= (int)TWallTileMeta.EType.InternalMiddle)
						type = TWallTileMeta.EType.EdgeT;
			}

			m_WallObject = m_Grid.m_TileFactory.NewWallTile(type);
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

	public void UpdateWallInverseCap()
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
					GameObject wallInverseObject = m_Grid.m_TileFactory.NewWallTile(TWallTileMeta.EType.InverseCorner);
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

	public void UpdateInternalWallTile(bool _PlaceNew)
	{
		// Only tiles with no wall can have an interior wall
		if(m_WallObjectType != TWallTileMeta.EType.None)
			return;

		// Find the meta identifier
		m_WallInteriorMetaIdentifier = 0;
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			// Only need to base off the n, e, s, w neightbours for wall
			if(neighbour.direction == CNeighbour.EDirection.North ||
			   neighbour.direction == CNeighbour.EDirection.East ||
			   neighbour.direction == CNeighbour.EDirection.South ||
			   neighbour.direction == CNeighbour.EDirection.West)
			{
				// Ignore all other walls except for internal walls unless placing
				if((int)neighbour.tile.m_WallObjectType > (int)TWallTileMeta.EType.InternalMiddle || 
				   (_PlaceNew && neighbour.tile.m_WallObjectType != TWallTileMeta.EType.None))
				{
					m_WallInteriorMetaIdentifier |= 1 << (int)neighbour.direction;
				}
			}
		}

		Quaternion rotation = Quaternion.identity;
		TWallTileMeta.EType type = TWallTileMeta.EType.InternalMiddle;
		
		// Find the meta data for this tile
		if(s_WallInteriorTileMetaInfo.ContainsKey(m_WallInteriorMetaIdentifier))
		{
			type = s_WallInteriorTileMetaInfo[m_WallInteriorMetaIdentifier].m_WallType;
			rotation = s_WallInteriorTileMetaInfo[m_WallInteriorMetaIdentifier].m_WallTileRotation;
		}
		else
		{
			// If it wasn't found there was no meta data found for it
			Debug.LogWarning("TileWallInternal Meta wasn't found for a tile within its neighbourhood.");
		}

		// If the tile type has changed
		if(m_WallObjectType != type)
		{
			ReleaseWallTile();
			
			m_WallObjectType = type;

			m_WallObject = m_Grid.m_TileFactory.NewWallTile(type);
			m_WallObject.transform.parent = m_TileObject.transform;
			m_WallObject.transform.localPosition = Vector3.zero;
			m_WallObject.transform.localScale = Vector3.one;
			m_WallObject.transform.localRotation = rotation;
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
			m_Grid.m_TileFactory.ReleaseTileObject(m_FloorObject);
			m_FloorObjectType = TFloorTileMeta.EType.None;
			m_FloorObject = null;
		}
	}

	private void ReleaseWallTile()
	{
		if(m_WallObject != null)
		{
			m_Grid.m_TileFactory.ReleaseTileObject(m_WallObject);
			m_WallObjectType = TWallTileMeta.EType.None;
			m_WallObject = null;
		}
	}

	private void ReleaseWallInverseTile(GameObject _InverseWallTile)
	{
		if(_InverseWallTile != null)
		{
			m_Grid.m_TileFactory.ReleaseTileObject(_InverseWallTile);
			_InverseWallTile = null;
		}
	}
}
