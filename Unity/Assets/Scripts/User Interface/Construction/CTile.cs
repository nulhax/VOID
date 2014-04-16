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


public class CTile : CGridObject 
{
	// Member Types


	// Member Delegates & Events
	public delegate void HandleTileEvent(CTile _Self);

	public event HandleTileEvent EventTileAppearanceChanged;
	public event HandleTileEvent EventTileMetaChanged;


	// Member Fields
	public bool m_Prebuilt = false;

	public int m_TileTypeIdentifier = 0;
	private int m_PreviousTileTypeIdentifier = 0;

	private Dictionary<ETileType, TTileMeta> m_TileMetaData = new Dictionary<ETileType, TTileMeta>();
	private Dictionary<ETileType, TTileMeta> m_CurrentTileMetaData = new Dictionary<ETileType, TTileMeta>();
	private Dictionary<ETileType, ETileVariant> m_CurrentTileVariants = new Dictionary<ETileType, ETileVariant>();
	private Dictionary<ETileType, List<EDirection>> m_CurrentTileNeighbourExemptions = new Dictionary<ETileType, List<EDirection>>();

	private bool m_IsDirty = false;

	private Dictionary<ETileType, GameObject> m_TileObject = new Dictionary<ETileType, GameObject>();

	static private bool s_DictionariesInitialised = false;
	static private Dictionary<ETileType, Dictionary<int, TTileMeta>> s_TileMetaInfo = new Dictionary<ETileType, Dictionary<int, TTileMeta>>();


	// Member Properties

	
	// Member Methods
	private void Awake()
	{
		// If the dictionary is empty we need to fill it
		if(!s_DictionariesInitialised)
		{
			for(int i = (int)ETileType.INVALID + 1; i < (int)ETileType.MAX; ++i)
				s_TileMetaInfo[(ETileType)i] = new Dictionary<int, TTileMeta>();

			FillTileMetaData();

			s_DictionariesInitialised = true;
		}

		// Initialise default meta data with invalid identifiers
		for(int i = (int)ETileType.INVALID + 1; i < (int)ETileType.MAX; ++i)
		{
			ETileType type = (ETileType)i;
			m_TileMetaData[type] = TTileMeta.Default;
			m_CurrentTileMetaData[type] = TTileMeta.Default;
			m_CurrentTileVariants[type] = ETileVariant.Default;
			m_CurrentTileNeighbourExemptions[type] = new List<EDirection>();
		}
	}

	private void Start()
	{
		// Fire Tile Creation Event
		m_Grid.TileCreate(this);

		// Need to re-create objects to maintain links
		if(m_Prebuilt)
		{
			List<Transform> children = GetComponentsInChildren<Transform>().ToList();
			children.Remove(transform);

			foreach(Transform child in children)
				Destroy(child.gameObject);
		}

		UpdateTileMetaData();
	}

	private void Update()
	{
		if(m_IsDirty)
		{
			UpdateAllTileObjects();
			m_IsDirty = false;
		}
	}

	public void FindNeighbours()
	{
		m_NeighbourHood.Clear();
		
		foreach(CNeighbour pn in s_AllPossibleNeighbours) 
		{
			TGridPoint possibleNeightbour = new TGridPoint(x + pn.m_GridPointOffset.x, 
			                                               y + pn.m_GridPointOffset.y, 
			                                               z + pn.m_GridPointOffset.z);
			
			CTile tile = m_Grid.GetTile(possibleNeightbour);
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

	public void UpdateTileMetaData()
	{
		int[] metaIdentifiers = new int[(int)ETileType.MAX];
		bool metaChanged = false;

		// Find the meta identifier for all tile types based on neighbourhood
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			// Main Tiles only care about N, E, S, W neighbours
			if(neighbour.m_Direction == EDirection.North ||
			   neighbour.m_Direction == EDirection.East ||
			   neighbour.m_Direction == EDirection.South ||
			   neighbour.m_Direction == EDirection.West)
			{
				if(neighbour.m_Tile.GetTileTypeState(ETileType.Floor))
				{
					if(!m_CurrentTileNeighbourExemptions[ETileType.Floor].Contains(neighbour.m_Direction))
						metaIdentifiers[(int)ETileType.Floor] |= 1 << (int)neighbour.m_Direction;
				}

				if(neighbour.m_Tile.GetTileTypeState(ETileType.Wall_Int))
				{
					if(!m_CurrentTileNeighbourExemptions[ETileType.Wall_Ext].Contains(neighbour.m_Direction))
						metaIdentifiers[(int)ETileType.Wall_Ext] |= 1 << (int)neighbour.m_Direction;
				}

				if(neighbour.m_Tile.GetTileTypeState(ETileType.Wall_Int))
				{
					if(!m_CurrentTileNeighbourExemptions[ETileType.Wall_Int].Contains(neighbour.m_Direction))
						metaIdentifiers[(int)ETileType.Wall_Int] |= 1 << (int)neighbour.m_Direction;
				}

				if(neighbour.m_Tile.GetTileTypeState(ETileType.Ceiling))
				{
					if(!m_CurrentTileNeighbourExemptions[ETileType.Ceiling].Contains(neighbour.m_Direction))
						metaIdentifiers[(int)ETileType.Ceiling] |= 1 << (int)neighbour.m_Direction;
				}
			}

			if(neighbour.m_Tile.GetTileTypeState(ETileType.Wall_Int))
			{
				if(!m_CurrentTileNeighbourExemptions[ETileType.Wall_Int].Contains(neighbour.m_Direction) &&
				   !m_CurrentTileNeighbourExemptions[ETileType.Wall_Int_Cap].Contains(neighbour.m_Direction))
					metaIdentifiers[(int)ETileType.Wall_Int_Cap] |= 1 << (int)neighbour.m_Direction;
			}

			if(neighbour.m_Tile.GetTileTypeState(ETileType.Wall_Int))
			{
				if(!m_CurrentTileNeighbourExemptions[ETileType.Wall_Int].Contains(neighbour.m_Direction) &&
				   !m_CurrentTileNeighbourExemptions[ETileType.Wall_Ext_Cap].Contains(neighbour.m_Direction))
					metaIdentifiers[(int)ETileType.Wall_Ext_Cap] |= 1 << (int)neighbour.m_Direction;
			}
		}

		// Check if floor/external wall/ceiling meta data changed
		if(GetTileTypeState(ETileType.Floor))
			if(UpdateTileMetaInfo(ETileType.Floor, metaIdentifiers[(int)ETileType.Floor])) 
				metaChanged = true;

		if(GetTileTypeState(ETileType.Ceiling))
			if(UpdateTileMetaInfo(ETileType.Ceiling, metaIdentifiers[(int)ETileType.Ceiling])) 
				metaChanged = true;

		if(GetTileTypeState(ETileType.Wall_Ext))
			if(UpdateTileMetaInfo(ETileType.Wall_Ext, metaIdentifiers[(int)ETileType.Wall_Ext])) 
				metaChanged = true;

		if(GetTileTypeState(ETileType.Wall_Int))
			if(UpdateTileMetaInfo(ETileType.Wall_Int, metaIdentifiers[(int)ETileType.Wall_Int])) 
				metaChanged = true;

		if(GetTileTypeState(ETileType.Wall_Int_Cap))
			if(UpdateTileMetaInfo(ETileType.Wall_Int_Cap, metaIdentifiers[(int)ETileType.Wall_Int_Cap])) 
				metaChanged = true;

		if(GetTileTypeState(ETileType.Wall_Ext_Cap))
			if(UpdateTileMetaInfo(ETileType.Wall_Ext_Cap, metaIdentifiers[(int)ETileType.Wall_Ext_Cap])) 
				metaChanged = true;

		// Last check to see if the tile types where changed
		if(m_PreviousTileTypeIdentifier != m_TileTypeIdentifier)
		{
			m_PreviousTileTypeIdentifier = m_TileTypeIdentifier;
			metaChanged = true;
		}

		// If any meta data changed
		if(metaChanged)
		{
			// Invoke event for meta change
			if(EventTileMetaChanged != null)
				EventTileMetaChanged(this);

			// Invoke neighbours to update their meta data
			foreach(CNeighbour neighbour in m_NeighbourHood)
			{
				neighbour.m_Tile.UpdateTileMetaData();
			}

			// Set dirty to update tile types next update
			m_IsDirty = true;
		}
	}

	private void UpdateAllTileObjects()
	{
		// Update tile objects
		for(int i = 0; i < (int)ETileType.MAX; ++i)
		{
			ETileType tileType = (ETileType)i;

			// Check if the tile should be active
			if(GetTileTypeState(tileType))
			{
				TTileMeta tileMeta = m_TileMetaData[tileType];

				// Apply the variant
				tileMeta.m_Variant = m_CurrentTileVariants[tileType];

				// If the meta data changed
				if(!m_CurrentTileMetaData[tileType].Equals(tileMeta))
				{
					// Release the current tile type object
					ReleaseTile(tileType);

					// Create the new tile type object as long as it is not marked as none
					if(tileMeta.m_Type != ETileMetaType.None)
					{
						m_TileObject[tileType] = m_Grid.TileFactory.InstanceNewTile(tileType, tileMeta.m_Type, tileMeta.m_Variant);
						m_TileObject[tileType].transform.parent = transform;
						m_TileObject[tileType].transform.localPosition = Vector3.zero;
						m_TileObject[tileType].transform.localScale = Vector3.one;
						m_TileObject[tileType].transform.localRotation = Quaternion.Euler(0.0f, tileMeta.m_Rotations * 90.0f, 0.0f);
					}

					// Update the tiles current meta data for this type
					m_CurrentTileMetaData[tileType] = tileMeta;
				}
			}
			else
			{
				// Release the tile as it is not needed
				ReleaseTile(tileType);
				m_CurrentTileMetaData[tileType] = TTileMeta.Default;
			}
		}

		// Fire event on appearance change
		if(EventTileAppearanceChanged != null)
			EventTileAppearanceChanged(this);
	}

	public void SetMetaData(ETileType _TileType, TTileMeta _Meta)
	{
		m_TileMetaData[_TileType] = _Meta;
		
		// Meta type changed so this tile needs update
		m_IsDirty = true;
	}
	
	public TTileMeta GetMetaData(ETileType _TileType)
	{
		return(m_TileMetaData[_TileType]);
	}

	public void SetTileTypeVariant(ETileType _TileType, ETileVariant _TileVariant)
	{
		m_CurrentTileVariants[_TileType] = _TileVariant;

		// Variant type changed so this tile needs update
		m_IsDirty = true;
	}

	public ETileVariant GetTileTypeVariant(ETileType _TileType)
	{
		return(m_CurrentTileVariants[_TileType]);
	}

	public void SetTileTypeState(ETileType _TileType, bool _State)
	{
		if(_State)
			m_TileTypeIdentifier |= (1 << (int)_TileType);
		else
			m_TileTypeIdentifier &= ~(1 << (int)_TileType);
	}
	
	public bool GetTileTypeState(ETileType _TileType)
	{
		bool state = ((m_TileTypeIdentifier & (1 << (int)_TileType)) != 0);
		return(state);
	}

	public void SetTileNeighbourExemptionState(ETileType _TileType, EDirection _Direction, bool _State)
	{
		if(_State)
		{
			m_CurrentTileNeighbourExemptions[_TileType].Add(_Direction);
			return;
		}

		if(!_State)
		{
			m_CurrentTileNeighbourExemptions[_TileType].Remove(_Direction);
			return;
		}
	}

	public bool GetTileNeighbourExemptionState(ETileType _TileType, EDirection _Direction)
	{
		return(m_CurrentTileNeighbourExemptions[_TileType].Contains(_Direction));
	}

	public void Release()
	{
		// Fire Tile Release Event
		m_Grid.TileRelease(this);

		// Release tile objects
		for(int i = 0; i < (int)ETileType.MAX; ++i)
		{
			ReleaseTile((ETileType)i);
			m_TileMetaData[(ETileType)i] = new TTileMeta();
			m_TileTypeIdentifier = 0;
		}

		// Update the neighbourhood
		UpdateNeighbourhood();
	}

	private void ReleaseTile(ETileType _TileType)
	{
		if(!m_TileObject.ContainsKey(_TileType))
			return;

		if(m_TileObject[_TileType] != null)
		{
			m_Grid.TileFactory.ReleaseTileObject(m_TileObject[_TileType]);

			m_TileObject[_TileType] = null;
			return;
		}
	}

	private static void AddTileMetaInfoEntry(ETileType _TileType, ETileMetaType _MetaType, EDirection[] _MaskNeighbours)
	{
		// Define the mask neighbours into a int mask
		int mask = 0;
		foreach(EDirection direction in _MaskNeighbours)
		{
			mask |= 1 << (int)direction;
		}

		if(!s_TileMetaInfo[_TileType].ContainsKey(mask))
		{
			s_TileMetaInfo[_TileType].Add(mask, new TTileMeta(_MetaType, mask));
		}
		else if(_TileType != ETileType.Wall_Ext_Cap && _TileType != ETileType.Wall_Int_Cap)
		{
			Debug.LogError("Tile meta info was already added for: " + _TileType + " : " + _MetaType + " : " + mask);
		}
	}
	
	private bool UpdateTileMetaInfo(ETileType _TileType, int _TileMask)
	{
		// Get the tile meta info as of this point
		TTileMeta tileMeta = m_TileMetaData[_TileType];

		// Find the tile mask out of possible 4 rotations
		for(int i = 0; i < 4; ++i)
		{
			int tileMask = 0;
			for(int j = 0; j < (int)EDirection.MAX; ++j)
			{
				// Define a new mask based on neighbours that exist in the mask
				if((_TileMask & (1 << j)) != 0)
				{
					// "Rotate" the mask by 90*, 2 bits ac for each rotation
					int dirMask = j - (2 * i);
					if(dirMask < 0)
						dirMask += 8;
					
					tileMask |= 1 << dirMask;
				}
			}
			
			// Return the result if it is found with the correct rotation
			if(s_TileMetaInfo[_TileType].ContainsKey(tileMask))
			{
				TTileMeta newTileMeta = s_TileMetaInfo[_TileType][tileMask];
				newTileMeta.m_Rotations = i;
				
				// Update the tile meta
				m_TileMetaData[_TileType] = newTileMeta;
				
				// Return true if there is a change in meta data
				if(!tileMeta.Equals(newTileMeta))
					return(true);
				else 
					return(false);
			}
		}

		Debug.LogWarning("Tile Meta data wasn't found for: " + _TileType + " Mask: " + _TileMask);

		return(false);
	}

	private static void FillTileMetaData()
	{
		// Floors
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_Cell, 
		                     new EDirection[]{});
		
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_Middle, 
		                     new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });
		
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_Hall, 			
		                     new EDirection[]{ EDirection.East, EDirection.West });
		
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_Edge,
		                     new EDirection[]{ EDirection.North, EDirection.South, EDirection.West });
		
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_Corner,
		                     new EDirection[]{ EDirection.North, EDirection.East });
		
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_End,
		                     new EDirection[]{ EDirection.West });
		
		// Walls Exterior
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.None,
		                     new EDirection[]{ });

		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_Cell,
		                     new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });

		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_Hall,
		                     new EDirection[]{ EDirection.West, EDirection.East });
		
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_Edge,
		                     new EDirection[]{ EDirection.East });
		
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_Corner,
		                     new EDirection[]{ EDirection.South, EDirection.East });
		
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_End,
		                     new EDirection[]{ EDirection.North, EDirection.East, EDirection.South });
		
		// Walls Interior
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_Cell,
		                     new EDirection[]{ });

		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.None,
		                     new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });
		
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_Hall,
		                     new EDirection[]{ EDirection.North, EDirection.South });
		
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_Edge,
		                     new EDirection[]{ EDirection.North, EDirection.West, EDirection.South });
		
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_Corner,
		                     new EDirection[]{ EDirection.North, EDirection.West });
		
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_End,
		                     new EDirection[]{ EDirection.West });
		
		// Ceiling
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_Cell, 
		                     new EDirection[]{});
		
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_Middle, 
		                     new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });
		
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_Hall, 			
		                     new EDirection[]{ EDirection.East, EDirection.West });
		
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_Edge,
		                     new EDirection[]{ EDirection.North, EDirection.South, EDirection.West });
		
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_Corner,
		                     new EDirection[]{ EDirection.North, EDirection.East });
		
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_End,
		                     new EDirection[]{ EDirection.West });

		// Inverse Caps
		List<EDirection> possibleDirections;
		IEnumerable<IEnumerable<EDirection>> combinations;

		// Walls Exterior Caps
		AddTileMetaInfoEntry(ETileType.Wall_Ext_Cap, ETileMetaType.Wall_Ext_Cap_4,
		                     new EDirection[]{ EDirection.NorthWest, EDirection.NorthEast, EDirection.SouthEast, EDirection.SouthWest });
		
		AddTileMetaInfoEntry(ETileType.Wall_Ext_Cap, ETileMetaType.Wall_Ext_Cap_3,
		                     new EDirection[]{ EDirection.NorthEast, EDirection.SouthEast, EDirection.SouthWest });

		AddTileMetaInfoEntry(ETileType.Wall_Ext_Cap, ETileMetaType.Wall_Ext_Cap_2_2,
		                     new EDirection[]{ EDirection.NorthEast, EDirection.SouthWest });

		possibleDirections = new List<EDirection>( new EDirection[]{ EDirection.West, EDirection.SouthWest, EDirection.NorthWest });
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
			
			AddTileMetaInfoEntry(ETileType.Wall_Ext_Cap, ETileMetaType.Wall_Ext_Cap_2_1, directions.ToArray());
		}  

		possibleDirections = new List<EDirection>( new EDirection[]{ EDirection.West, EDirection.South, EDirection.SouthEast, EDirection.SouthWest, EDirection.NorthWest });
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
			
			AddTileMetaInfoEntry(ETileType.Wall_Ext_Cap, ETileMetaType.Wall_Ext_Cap_1, directions.ToArray());
		} 

		possibleDirections = new List<EDirection>( new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West, EDirection.SouthEast, EDirection.SouthWest, EDirection.NorthEast, EDirection.NorthWest });
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
				AddTileMetaInfoEntry(ETileType.Wall_Ext_Cap, ETileMetaType.None, directions.ToArray());
		}

		// Walls Interior Caps
		AddTileMetaInfoEntry(ETileType.Wall_Int_Cap, ETileMetaType.Wall_Int_Cap_4,
		                     new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });

		AddTileMetaInfoEntry(ETileType.Wall_Int_Cap, ETileMetaType.Wall_Int_Cap_3,
				             new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West, EDirection.NorthWest });

		AddTileMetaInfoEntry(ETileType.Wall_Int_Cap, ETileMetaType.Wall_Int_Cap_2_2,
		                     new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West, EDirection.NorthWest, EDirection.SouthEast });

		possibleDirections = new List<EDirection>( new EDirection[]{ EDirection.West, EDirection.SouthWest, EDirection.NorthWest });
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
			
			AddTileMetaInfoEntry(ETileType.Wall_Int_Cap, ETileMetaType.Wall_Int_Cap_2_1, directions.ToArray());
		}

		possibleDirections = new List<EDirection>( new EDirection[]{ EDirection.SouthEast, EDirection.SouthWest, EDirection.NorthWest, EDirection.South, EDirection.West });
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

			AddTileMetaInfoEntry(ETileType.Wall_Int_Cap, ETileMetaType.Wall_Int_Cap_1, directions.ToArray());
		}  

		possibleDirections = new List<EDirection>( new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West, EDirection.SouthEast, EDirection.SouthWest, EDirection.NorthEast, EDirection.NorthWest });
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
				AddTileMetaInfoEntry(ETileType.Wall_Int_Cap, ETileMetaType.None, directions.ToArray());
		}
	}
}

public enum ETileType
{
	INVALID = -1,
	
	Floor,
	Wall_Ext,
	Wall_Int,
	Ceiling,
	Wall_Ext_Cap,
	Wall_Int_Cap,

	MAX
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

public enum ETileVariant
{
	INVALID = -1,
	
	Default,
	Opening,
	Window,
	
	MAX
}

[System.Serializable]
public struct TTileMeta
{	
	public static TTileMeta Default
	{
		get { return(new TTileMeta(ETileMetaType.None, -1)); }
	}

	public TTileMeta(ETileMetaType _Type, int _Identifier)
	{
		m_TileMask = _Identifier;
		m_Type = _Type;
		m_Variant = ETileVariant.Default;
		m_Rotations = 0;
	}
	
	public int m_TileMask;
	public ETileMetaType m_Type;
	public int m_Rotations;
	public ETileVariant m_Variant;
}
