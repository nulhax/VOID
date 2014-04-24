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


public class CTileRoot : MonoBehaviour 
{
	// Member Types


	// Member Delegates & Events
	public delegate void HandleTileEvent(CTileRoot _Self);
	
	public event HandleTileEvent EventTileInitialised;
	public event HandleTileEvent EventTilePreRelease;
	public event HandleTileEvent EventTilePostRelease;
	public event HandleTileEvent EventTileGeometryChanged;
	public event HandleTileEvent EventTileMetaChanged;


	// Member Fields
	public CGrid m_Grid = null;
	public TGridPoint m_GridPosition;

	public int m_TileTypeIdentifier = 0;

	public List<CNeighbour> m_NeighbourHood = new List<CNeighbour>();
	private Dictionary<CTile.EType, CTile> m_Tiles = new Dictionary<CTile.EType, CTile>();


	// Member Properties
	private static List<CNeighbour> s_PossibleNeighbours = new List<CNeighbour>(
		new CNeighbour[] 
		{
		new CNeighbour(new TGridPoint(-1, 0, 1), EDirection.NorthWest),
		new CNeighbour(new TGridPoint(0, 0, 1), EDirection.North),
		new CNeighbour(new TGridPoint(1, 0, 1), EDirection.NorthEast),
		new CNeighbour(new TGridPoint(1, 0, 0), EDirection.East),
		new CNeighbour(new TGridPoint(1, 0, -1), EDirection.SouthEast),
		new CNeighbour(new TGridPoint(0, 0, -1), EDirection.South),
		new CNeighbour(new TGridPoint(-1, 0, -1), EDirection.SouthWest),
		new CNeighbour(new TGridPoint(-1, 0, 0), EDirection.West),
	});

	
	// Member Methods
	private void Start()
	{
		foreach(CTile tile in m_Tiles)
			tile.UpdateCurrentMeta();

		if(EventTileInitialised != null)
			EventTileInitialised(this);
	}

	private void OnDestroy()
	{
		if(EventTilePostRelease != null)
			EventTilePostRelease(this);
	}

	public void FindNeighbours()
	{
		m_NeighbourHood.Clear();
		
		foreach(CNeighbour pn in s_PossibleNeighbours) 
		{
			TGridPoint possibleNeightbour = new TGridPoint(m_GridPosition.x + pn.m_GridPointOffset.x, 
			                                               m_GridPosition.y + pn.m_GridPointOffset.y, 
			                                               m_GridPosition.z + pn.m_GridPointOffset.z);
			
			CTileRoot tile = m_Grid.GetTile(possibleNeightbour);
			if(tile != null)
			{
				CNeighbour newNeighbour = new CNeighbour(pn.m_GridPointOffset, pn.m_Direction);
				newNeighbour.m_Tile = tile;
				
				m_NeighbourHood.Add(newNeighbour);
			}
		}
	}

	public void UpdateNeighbourhood()
	{
		// Find all neighbours and invoke them to find others
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			neighbour.m_Tile.FindNeighbours();
		}
	}

	public void SetTileTypeState(CTile.EType _TileType, bool _State)
	{
		if(_State)
			m_TileTypeIdentifier |= (1 << (int)_TileType);
		else
			m_TileTypeIdentifier &= ~(1 << (int)_TileType);
	}
	
	public bool GetTileTypeState(CTile.EType _TileType)
	{
		bool state = ((m_TileTypeIdentifier & (1 << (int)_TileType)) != 0);
		return(state);
	}

	public void UpdateTileMetaData()
	{
//		int[] metaIdentifiers = new int[(int)CTile.EType.MAX];
//		bool metaChanged = false;
//
//		// Find the meta identifier for all tile types based on neighbourhood
//		foreach(CNeighbour neighbour in m_NeighbourHood)
//		{
//			// Main Tiles only care about N, E, S, W neighbours
//			if(neighbour.m_Direction == EDirection.North ||
//			   neighbour.m_Direction == EDirection.East ||
//			   neighbour.m_Direction == EDirection.South ||
//			   neighbour.m_Direction == EDirection.West)
//			{
//				if(neighbour.m_Tile.GetTileTypeState(CTile.EType.Floor))
//				{
//					if(!m_CurrentTileMetaData[CTile.EType.Floor].m_NeighbourExemptions.Contains(neighbour.m_Direction))
//						metaIdentifiers[(int)CTile.EType.Floor] |= 1 << (int)neighbour.m_Direction;
//				}
//
//				if(neighbour.m_Tile.GetTileTypeState(CTile.EType.Wall_Int))
//				{
//					if(!m_CurrentTileMetaData[CTile.EType.Wall_Ext].m_NeighbourExemptions.Contains(neighbour.m_Direction))
//						metaIdentifiers[(int)CTile.EType.Wall_Ext] |= 1 << (int)neighbour.m_Direction;
//				}
//
//				if(neighbour.m_Tile.GetTileTypeState(CTile.EType.Wall_Int))
//				{
//					if(!m_CurrentTileMetaData[CTile.EType.Wall_Int].m_NeighbourExemptions.Contains(neighbour.m_Direction))
//						metaIdentifiers[(int)CTile.EType.Wall_Int] |= 1 << (int)neighbour.m_Direction;
//				}
//
//				if(neighbour.m_Tile.GetTileTypeState(CTile.EType.Ceiling))
//				{
//					if(!m_CurrentTileMetaData[CTile.EType.Ceiling].m_NeighbourExemptions.Contains(neighbour.m_Direction))
//						metaIdentifiers[(int)CTile.EType.Ceiling] |= 1 << (int)neighbour.m_Direction;
//				}
//			}
//
//			if(neighbour.m_Tile.GetTileTypeState(CTile.EType.Wall_Int))
//			{
//				if(!m_CurrentTileMetaData[CTile.EType.Wall_Int].m_NeighbourExemptions.Contains(neighbour.m_Direction) &&
//				   !m_CurrentTileMetaData[CTile.EType.Wall_Int_Cap].m_NeighbourExemptions.Contains(neighbour.m_Direction))
//					metaIdentifiers[(int)CTile.EType.Wall_Int_Cap] |= 1 << (int)neighbour.m_Direction;
//			}
//
//			if(neighbour.m_Tile.GetTileTypeState(CTile.EType.Wall_Int))
//			{
//				if(!m_CurrentTileMetaData[CTile.EType.Wall_Ext_Cap].m_NeighbourExemptions.Contains(neighbour.m_Direction))
//					metaIdentifiers[(int)CTile.EType.Wall_Ext_Cap] |= 1 << (int)neighbour.m_Direction;
//			}
//		}
//
//		// Check meta data for changes
//		for(int i = (int)CTile.EType.INVALID + 1; i < (int)CTile.EType.MAX; ++i)
//		{
//			CTile.EType type = (CTile.EType)i;
//			if(GetTileTypeState(type))
//				if(UpdateTileMetaInfo(type, metaIdentifiers[i])) 
//					metaChanged = true;
//		}
//
//		// Last check to see if the tile types where changed
//		if(m_PreviousTileTypeIdentifier != m_TileTypeIdentifier)
//		{
//			m_PreviousTileTypeIdentifier = m_TileTypeIdentifier;
//			metaChanged = true;
//		}
//
//		// If any meta data changed
//		if(metaChanged)
//		{
//			// Invoke event for meta change
//			if(EventTileMetaChanged != null)
//				EventTileMetaChanged(this);
//
//			// Invoke neighbours to update their meta data
//			foreach(CNeighbour neighbour in m_NeighbourHood)
//			{
//				neighbour.m_Tile.UpdateTileMetaData();
//			}
//
//			// Set dirty to update tile types next update
//			m_IsDirty = true;
//		}
	}

	private void UpdateAllTileObjects()
	{
		// Update tile objects
		foreach(CTile tile in m_Tiles.Values)
		{
			CTile.EType tileType = tile.m_TileType;

			// Check if the tile should be active
			if(GetTileTypeState(tileType))
			{
				TTileMeta tileMeta = m_CurrentTileMetaData[tileType];

				// If the meta data changed
				if(!m_ActiveTileMetaData[tileType].Equals(tileMeta))
				{
					// Release the current tile type object
					ReleaseTile(tileType);

					// Create the new tile type object as long as it is not marked as none
					if(tileMeta.m_Type != ETileMetaType.None)
					{
						m_Tiles[tileType] = m_Grid.TileFactory.InstanceNewTile(tileType, tileMeta.m_Type, tileMeta.m_Variant);
						m_Tiles[tileType].transform.parent = transform;
						m_Tiles[tileType].transform.localPosition = Vector3.zero;
						m_Tiles[tileType].transform.localScale = Vector3.one;
						m_Tiles[tileType].transform.localRotation = Quaternion.Euler(0.0f, tileMeta.m_Rotations * 90.0f, 0.0f);
					}

					// Update the tiles current meta data for this type
					m_ActiveTileMetaData[tileType] = tileMeta;
				}
			}
			else
			{
				// Release the tile as it is not needed
				ReleaseTile(tileType);
				m_ActiveTileMetaData[tileType] = TTileMeta.Default;
			}
		}

		// Fire event on appearance change
		if(EventTileGeometryChanged != null)
			EventTileGeometryChanged(this);
	}

	public void SetTileTypeVariant(CTile.EType _TileType, ETileVariant _TileVariant)
	{
		TTileMeta tileMeta = m_CurrentTileMetaData[_TileType];
		tileMeta.m_Variant = _TileVariant;
		m_CurrentTileMetaData[_TileType] = tileMeta;

		// Set is dirty
		m_IsDirty = true;
	}

	public ETileVariant GetTileTypeVariant(CTile.EType _TileType)
	{
		return(m_CurrentTileMetaData[_TileType].m_Variant);
	}

	public void SetTileNeighbourExemptionState(CTile.EType _TileType, EDirection _Direction, bool _State)
	{
		if(_State)
		{
			m_CurrentTileMetaData[_TileType].m_NeighbourExemptions.Add(_Direction);
		}
		else
		{
			m_CurrentTileMetaData[_TileType].m_NeighbourExemptions.Remove(_Direction);
		}
	}

	public void ResetTileNeighboutExemptions(CTile.EType _TileType)
	{
		m_CurrentTileMetaData[_TileType].m_NeighbourExemptions.Clear();
	}

	public bool GetTileNeighbourExemptionState(CTile.EType _TileType, EDirection _Direction)
	{
		return(m_CurrentTileMetaData[_TileType].m_NeighbourExemptions.Contains(_Direction));
	}

	public void CloneTileMetaData(CTileRoot _From)
	{
		// Copy the variables from one to the other
		m_TileTypeIdentifier = _From.m_TileTypeIdentifier;
		for(int i = (int)CTile.EType.INVALID + 1; i < (int)CTile.EType.MAX; ++i)
		{
			m_CurrentTileMetaData[(CTile.EType)i] = _From.m_CurrentTileMetaData[(CTile.EType)i];
		}

		// Set is dirty
		m_IsDirty = true;
	}

	public void Release()
	{
		if(EventTilePreRelease != null)
            EventTilePreRelease(this);

		// Release tile objects
		for(int i = (int)CTile.EType.INVALID + 1; i < (int)CTile.EType.MAX; ++i)
		{
			ReleaseTile((CTile.EType)i);
			m_TileTypeIdentifier = 0;
		}

		// Update the neighbourhood
		UpdateNeighbourhood();
	}

	private void ReleaseTile(CTile.EType _TileType)
	{
		if(!m_Tiles.ContainsKey(_TileType))
			return;

		if(m_Tiles[_TileType] != null)
		{
			m_Grid.TileFactory.ReleaseTileObject(m_Tiles[_TileType]);

			m_Tiles[_TileType] = null;
			return;
		}
	}

	private static void FillTileMetaData()
	{
		// Floors
		AddTileMetaInfoEntry(CTile.EType.Floor, ETileMetaType.Floor_Cell, 
		                     new EDirection[]{});
		
		AddTileMetaInfoEntry(CTile.EType.Floor, ETileMetaType.Floor_Middle, 
		                     new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });
		
		AddTileMetaInfoEntry(CTile.EType.Floor, ETileMetaType.Floor_Hall, 			
		                     new EDirection[]{ EDirection.East, EDirection.West });
		
		AddTileMetaInfoEntry(CTile.EType.Floor, ETileMetaType.Floor_Edge,
		                     new EDirection[]{ EDirection.North, EDirection.South, EDirection.West });
		
		AddTileMetaInfoEntry(CTile.EType.Floor, ETileMetaType.Floor_Corner,
		                     new EDirection[]{ EDirection.North, EDirection.East });
		
		AddTileMetaInfoEntry(CTile.EType.Floor, ETileMetaType.Floor_End,
		                     new EDirection[]{ EDirection.West });
		
		// Walls Exterior
		AddTileMetaInfoEntry(CTile.EType.Wall_Ext, ETileMetaType.None,
		                     new EDirection[]{ });

		AddTileMetaInfoEntry(CTile.EType.Wall_Ext, ETileMetaType.Wall_Ext_Cell,
		                     new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });

		AddTileMetaInfoEntry(CTile.EType.Wall_Ext, ETileMetaType.Wall_Ext_Hall,
		                     new EDirection[]{ EDirection.West, EDirection.East });
		
		AddTileMetaInfoEntry(CTile.EType.Wall_Ext, ETileMetaType.Wall_Ext_Edge,
		                     new EDirection[]{ EDirection.East });
		
		AddTileMetaInfoEntry(CTile.EType.Wall_Ext, ETileMetaType.Wall_Ext_Corner,
		                     new EDirection[]{ EDirection.South, EDirection.East });
		
		AddTileMetaInfoEntry(CTile.EType.Wall_Ext, ETileMetaType.Wall_Ext_End,
		                     new EDirection[]{ EDirection.North, EDirection.East, EDirection.South });
		
		// Walls Interior
		AddTileMetaInfoEntry(CTile.EType.Wall_Int, ETileMetaType.Wall_Int_Cell,
		                     new EDirection[]{ });

		AddTileMetaInfoEntry(CTile.EType.Wall_Int, ETileMetaType.None,
		                     new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });
		
		AddTileMetaInfoEntry(CTile.EType.Wall_Int, ETileMetaType.Wall_Int_Hall,
		                     new EDirection[]{ EDirection.North, EDirection.South });
		
		AddTileMetaInfoEntry(CTile.EType.Wall_Int, ETileMetaType.Wall_Int_Edge,
		                     new EDirection[]{ EDirection.North, EDirection.West, EDirection.South });
		
		AddTileMetaInfoEntry(CTile.EType.Wall_Int, ETileMetaType.Wall_Int_Corner,
		                     new EDirection[]{ EDirection.North, EDirection.West });
		
		AddTileMetaInfoEntry(CTile.EType.Wall_Int, ETileMetaType.Wall_Int_End,
		                     new EDirection[]{ EDirection.West });
		
		// Ceiling
		AddTileMetaInfoEntry(CTile.EType.Ceiling, ETileMetaType.Ceiling_Cell, 
		                     new EDirection[]{});
		
		AddTileMetaInfoEntry(CTile.EType.Ceiling, ETileMetaType.Ceiling_Middle, 
		                     new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });
		
		AddTileMetaInfoEntry(CTile.EType.Ceiling, ETileMetaType.Ceiling_Hall, 			
		                     new EDirection[]{ EDirection.East, EDirection.West });
		
		AddTileMetaInfoEntry(CTile.EType.Ceiling, ETileMetaType.Ceiling_Edge,
		                     new EDirection[]{ EDirection.North, EDirection.South, EDirection.East });
		
		AddTileMetaInfoEntry(CTile.EType.Ceiling, ETileMetaType.Ceiling_Corner,
		                     new EDirection[]{ EDirection.South, EDirection.East });
		
		AddTileMetaInfoEntry(CTile.EType.Ceiling, ETileMetaType.Ceiling_End,
		                     new EDirection[]{ EDirection.West });

		// Inverse Caps
		List<EDirection> possibleDirections;
		IEnumerable<IEnumerable<EDirection>> combinations;

		// Walls Exterior Caps
		AddTileMetaInfoEntry(CTile.EType.Wall_Ext_Cap, ETileMetaType.Wall_Ext_Cap_4,
		                     new EDirection[]{ EDirection.NorthWest, EDirection.NorthEast, EDirection.SouthEast, EDirection.SouthWest });
		
		AddTileMetaInfoEntry(CTile.EType.Wall_Ext_Cap, ETileMetaType.Wall_Ext_Cap_3,
		                     new EDirection[]{ EDirection.NorthEast, EDirection.SouthEast, EDirection.SouthWest });

		AddTileMetaInfoEntry(CTile.EType.Wall_Ext_Cap, ETileMetaType.Wall_Ext_Cap_2_2,
		                     new EDirection[]{ EDirection.NorthEast, EDirection.SouthWest });

		possibleDirections = new List<EDirection>(new EDirection[]{ EDirection.West, EDirection.SouthWest, EDirection.NorthWest });
		combinations = CUtility.GetPowerSet(possibleDirections);
		foreach(var collection in combinations)  
		{  
			List<EDirection> directions = collection.ToList();
			directions.Add(EDirection.NorthEast);
			directions.Add(EDirection.SouthEast);

			if(directions.Contains(EDirection.NorthWest))
				if(!directions.Contains(EDirection.West))
					continue;
			
			if(directions.Contains(EDirection.SouthWest))
				if(!directions.Contains(EDirection.West))
					continue;
			
			AddTileMetaInfoEntry(CTile.EType.Wall_Ext_Cap, ETileMetaType.Wall_Ext_Cap_2_1, directions.ToArray());
		}  

		possibleDirections = new List<EDirection>(new EDirection[]{ EDirection.West, EDirection.South, EDirection.SouthEast, EDirection.SouthWest, EDirection.NorthWest });
		combinations = CUtility.GetPowerSet(possibleDirections);
		foreach(var collection in combinations)  
		{  
			List<EDirection> directions = collection.ToList();
			directions.Add(EDirection.NorthEast);
			
			if(directions.Contains(EDirection.NorthWest))
				if(!directions.Contains(EDirection.West))
					continue;

			if(directions.Contains(EDirection.SouthEast))
				if(!directions.Contains(EDirection.South))
					continue;

			if(directions.Contains(EDirection.SouthWest))
				if(!directions.Contains(EDirection.West) && !directions.Contains(EDirection.South))
					continue;
			
			AddTileMetaInfoEntry(CTile.EType.Wall_Ext_Cap, ETileMetaType.Wall_Ext_Cap_1, directions.ToArray());
		} 

		possibleDirections = new List<EDirection>(new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West, EDirection.SouthEast, EDirection.SouthWest, EDirection.NorthEast, EDirection.NorthWest });
		combinations = CUtility.GetPowerSet(possibleDirections);
		foreach(var collection in combinations)  
		{  
			List<EDirection> directions = collection.ToList(); 
			bool matchFound = false;
			
			if(directions.Contains(EDirection.NorthWest) && !matchFound)
				if(!directions.Contains(EDirection.North) && !directions.Contains(EDirection.West))
					matchFound = true;
			
			if(directions.Contains(EDirection.NorthEast) && !matchFound)
				if(!directions.Contains(EDirection.North) && !directions.Contains(EDirection.East))
					matchFound = true;
			
			if(directions.Contains(EDirection.SouthEast) && !matchFound)
				if(!directions.Contains(EDirection.South) && !directions.Contains(EDirection.East))
					matchFound = true;
			
			if(directions.Contains(EDirection.SouthWest) && !matchFound)
				if(!directions.Contains(EDirection.South) && !directions.Contains(EDirection.West))
					matchFound = true;
			
			if(!matchFound)
				AddTileMetaInfoEntry(CTile.EType.Wall_Ext_Cap, ETileMetaType.None, directions.ToArray());
		}

		// Walls Interior Caps
		AddTileMetaInfoEntry(CTile.EType.Wall_Int_Cap, ETileMetaType.Wall_Int_Cap_4,
		                     new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });

		AddTileMetaInfoEntry(CTile.EType.Wall_Int_Cap, ETileMetaType.Wall_Int_Cap_3,
				             new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West, EDirection.NorthWest });

		AddTileMetaInfoEntry(CTile.EType.Wall_Int_Cap, ETileMetaType.Wall_Int_Cap_2_2,
		                     new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West, EDirection.NorthWest, EDirection.SouthEast });

		possibleDirections = new List<EDirection>(new EDirection[]{ EDirection.West, EDirection.SouthWest, EDirection.NorthWest });
		combinations = CUtility.GetPowerSet(possibleDirections);
		foreach(var collection in combinations)  
		{  
			List<EDirection> directions = collection.ToList();
			directions.Add(EDirection.North);
			directions.Add(EDirection.East); 
			directions.Add(EDirection.South); 

			if(!directions.Contains(EDirection.SouthWest))
				if(directions.Contains(EDirection.South) && directions.Contains(EDirection.West))
					continue;
			
			if(!directions.Contains(EDirection.NorthWest))
				if(directions.Contains(EDirection.North) && directions.Contains(EDirection.West))
					continue;
			
			AddTileMetaInfoEntry(CTile.EType.Wall_Int_Cap, ETileMetaType.Wall_Int_Cap_2_1, directions.ToArray());
		}

		possibleDirections = new List<EDirection>(new EDirection[]{ EDirection.SouthEast, EDirection.SouthWest, EDirection.NorthWest, EDirection.South, EDirection.West });
		combinations = CUtility.GetPowerSet(possibleDirections);
		foreach(var collection in combinations)  
		{  
			List<EDirection> directions = collection.ToList();
			directions.Add(EDirection.North);
			directions.Add(EDirection.East);

			if(!directions.Contains(EDirection.SouthEast))
				if(directions.Contains(EDirection.South) && directions.Contains(EDirection.East))
					continue;

			if(!directions.Contains(EDirection.SouthWest))
				if(directions.Contains(EDirection.South) && directions.Contains(EDirection.West))
					continue;

			if(!directions.Contains(EDirection.NorthWest))
				if(directions.Contains(EDirection.North) && directions.Contains(EDirection.West))
					continue;

			AddTileMetaInfoEntry(CTile.EType.Wall_Int_Cap, ETileMetaType.Wall_Int_Cap_1, directions.ToArray());
		}  

		possibleDirections = new List<EDirection>(new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West, EDirection.SouthEast, EDirection.SouthWest, EDirection.NorthEast, EDirection.NorthWest });
		combinations = CUtility.GetPowerSet(possibleDirections);
		foreach(var collection in combinations)  
		{  
			List<EDirection> directions = collection.ToList(); 
			bool matchFound = false;
			
			if(!directions.Contains(EDirection.NorthWest) && !matchFound)
				if(directions.Contains(EDirection.North) && directions.Contains(EDirection.West))
					matchFound = true;

			if(!directions.Contains(EDirection.NorthEast) && !matchFound)
				if(directions.Contains(EDirection.North) && directions.Contains(EDirection.East))
					matchFound = true;

			if(!directions.Contains(EDirection.SouthEast) && !matchFound)
				if(directions.Contains(EDirection.South) && directions.Contains(EDirection.East))
					matchFound = true;

			if(!directions.Contains(EDirection.SouthWest) && !matchFound)
				if(directions.Contains(EDirection.South) && directions.Contains(EDirection.West))
					matchFound = true;

			if(!matchFound)
				AddTileMetaInfoEntry(CTile.EType.Wall_Int_Cap, ETileMetaType.None, directions.ToArray());
		}
	}
}

public enum ETileMetaType 
{ 
	None = -1,
	
	Floor_Middle = 0, 
	Floor_Corner, 
	Floor_Edge,
	Floor_End,
	Floor_Hall,
	Floor_Cell,
	
	Wall_Ext_Corner = 100, 
	Wall_Ext_Edge,
	Wall_Ext_End,
	Wall_Ext_Hall,
	Wall_Ext_Cell,
	
	Wall_Int_Corner = 200, 
	Wall_Int_Edge,
	Wall_Int_End,
	Wall_Int_Hall,
	Wall_Int_Cell,
	
	Ceiling_Middle = 300, 
	Ceiling_Corner, 
	Ceiling_Edge,
	Ceiling_End,
	Ceiling_Hall,
	Ceiling_Cell,
	
	Wall_Ext_Cap_1 = 400,
	Wall_Ext_Cap_2_1,
	Wall_Ext_Cap_2_2,
	Wall_Ext_Cap_3,
	Wall_Ext_Cap_4,

	Wall_Int_Cap_1 = 500,
	Wall_Int_Cap_2_1,
	Wall_Int_Cap_2_2,
	Wall_Int_Cap_3,
	Wall_Int_Cap_4,
}
