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
	public event HandleTileEvent EventTileTypeStateChange;
	public event HandleTileEvent EventTileMetaChanged;


	// Member Fields
	public bool m_Prebuilt = false;
	public int m_TileTypeIdentifier = 0;
	private int m_PreviousTileTypeIdentifier = 0;

	private Dictionary<ETileType, TTileMeta> m_TileMetaData = new Dictionary<ETileType, TTileMeta>();
	private Dictionary<ETileType, TTileMeta> m_CurrentTileMetaData = new Dictionary<ETileType, TTileMeta>();

	public Dictionary<ETileType, List<EDirection>> m_RelevantNeighbours = new Dictionary<ETileType, List<EDirection>>();
	public Dictionary<ETileType, int> m_CurrentTileNeighbourExemptions = new Dictionary<ETileType, int>();

	private bool m_IsDirty = false;
	
	private Dictionary<ETileType, GameObject> m_TileObject = new Dictionary<ETileType, GameObject>();
	private Dictionary<EDirection, GameObject> m_WallInverseObjects = new Dictionary<EDirection, GameObject>();

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
			m_RelevantNeighbours[type] = new List<EDirection>();
			m_CurrentTileNeighbourExemptions[type] = 0;
		}
	}

	private void Start()
	{
		// Need to re-create objects to maintain links
		if(m_Prebuilt)
		{
			List<Transform> children = GetComponentsInChildren<Transform>().ToList();
			children.Remove(transform);

			foreach(Transform child in children)
				Destroy(child.gameObject);
		}

		// Update the neighbourhood
		UpdateNeighbourhood();

		// Update current meta data
		UpdateTileMetaData();
	}

	private void OnDestroy()
	{
		// Update the neighbourhood
		UpdateNeighbourhood();
	}

	private void Update()
	{
		// If any changes have happened recently we need to update neighbours
		if(m_IsDirty)
		{
			UpdateAllTileObjects();
			m_IsDirty = false;
		}
	}

	public void FindNeighbours()
	{
		m_NeighbourHood.Clear();
		
		foreach(CNeighbour pn in s_AllNeighbours) 
		{
			TGridPoint possibleNeightbour = new TGridPoint(x + pn.m_GridPointOffset.x, 
			                                               y + pn.m_GridPointOffset.y, 
			                                               z + pn.m_GridPointOffset.z);
			
			CTile tile = m_Grid.GetTile(possibleNeightbour);
			if(tile != null)
			{
				CNeighbour newNeighbour = new CNeighbour(pn.m_GridPointOffset, pn.m_WorldDirection);
				newNeighbour.m_Tile = tile;
				
				m_NeighbourHood.Add(newNeighbour);
			}
		}
	}

	private void UpdateNeighbourhood()
	{
		// Find all neighbours and invoke them to find others
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			neighbour.m_Tile.FindNeighbours();
		}

		// Invoke an update of their meta data
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			neighbour.m_Tile.UpdateTileMetaData();
		}
	}

	public void UpdateTileMetaData()
	{
		int[] metaIdentifiers = new int[(int)ETileType.MAX];

		// Find the meta identifier for all tile types based on neighbourhood
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			// Main Tiles only care about N, E, S, W neighbours
			if(neighbour.m_WorldDirection == EDirection.North ||
			   neighbour.m_WorldDirection == EDirection.East ||
			   neighbour.m_WorldDirection == EDirection.South ||
			   neighbour.m_WorldDirection == EDirection.West)
			{
				EDirection oppositeDirection = CNeighbour.GetOppositeDirection(neighbour.m_WorldDirection);
				if(neighbour.m_Tile.GetTileTypeState(ETileType.Floor))
				{
					bool state =(neighbour.m_Tile.m_CurrentTileNeighbourExemptions[ETileType.Floor] & (1 << (int)oppositeDirection)) != 0;

					if(!state)
						metaIdentifiers[(int)ETileType.Floor] |= 1 << (int)neighbour.m_WorldDirection;
				}

				if(neighbour.m_Tile.GetTileTypeState(ETileType.Wall_Ext))
				{
					bool state =(neighbour.m_Tile.m_CurrentTileNeighbourExemptions[ETileType.Wall_Ext] & (1 << (int)oppositeDirection)) != 0;

					if(!state)
						metaIdentifiers[(int)ETileType.Wall_Ext] |= 1 << (int)neighbour.m_WorldDirection;
				}

				if(neighbour.m_Tile.GetTileTypeState(ETileType.Wall_Int))
				{
					bool state =(neighbour.m_Tile.m_CurrentTileNeighbourExemptions[ETileType.Wall_Int] & (1 << (int)oppositeDirection)) != 0;

					if(!state)
						metaIdentifiers[(int)ETileType.Wall_Int] |= 1 << (int)neighbour.m_WorldDirection;
				}

				if(neighbour.m_Tile.GetTileTypeState(ETileType.Ceiling))
				{
					bool state =(neighbour.m_Tile.m_CurrentTileNeighbourExemptions[ETileType.Ceiling] & (1 << (int)oppositeDirection)) != 0;

					if(!state)
						metaIdentifiers[(int)ETileType.Ceiling] |= 1 << (int)neighbour.m_WorldDirection;
				}
			}

			// Wall caps care about all directions
			if(neighbour.m_Tile.GetTileTypeState(ETileType.Wall_Ext))
			{
				metaIdentifiers[(int)ETileType.Wall_Ext_Cap] |= 1 << (int)neighbour.m_WorldDirection;
			}

			// External walls get extra meta identitfication when they aren't first story
			if(neighbour.m_WorldDirection == EDirection.Lower)
			{
				metaIdentifiers[(int)ETileType.Wall_Ext] |= 1 << 4;
			}
		}

		// Use the meta indentifiers to find the relevant neighbour types
		for(int i = (int)ETileType.INVALID + 1; i < (int)ETileType.MAX; ++i)
		{
			ETileType type = (ETileType)i;
			m_RelevantNeighbours[type].Clear();

			// Check if the neighbours where included in the meta identifier
			for(int j = (int)EDirection.INVALID + 1; j < (int)EDirection.MAX; ++j)
				if((metaIdentifiers[i] & (1 << j)) != 0)
					m_RelevantNeighbours[type].Add((EDirection)j);
		}

		// Apply the neighbourhood tile exemptions
		metaIdentifiers[(int)ETileType.Floor] &= ~(m_CurrentTileNeighbourExemptions[ETileType.Floor]); 
		metaIdentifiers[(int)ETileType.Wall_Ext] &= ~(m_CurrentTileNeighbourExemptions[ETileType.Wall_Ext]); 
		metaIdentifiers[(int)ETileType.Wall_Int] &= ~(m_CurrentTileNeighbourExemptions[ETileType.Wall_Int]); 
		metaIdentifiers[(int)ETileType.Ceiling] &= ~(m_CurrentTileNeighbourExemptions[ETileType.Ceiling]); 
		metaIdentifiers[(int)ETileType.Wall_Ext_Cap] &= ~(m_CurrentTileNeighbourExemptions[ETileType.Wall_Ext_Cap]); 

		// Check if floor/external wall/ceiling meta data changed
		bool metaChanged = false;
		if(m_TileMetaData[ETileType.Floor].m_Identifier != metaIdentifiers[(int)ETileType.Floor])
		{
			m_TileMetaData[ETileType.Floor] = FindTileMetaInfo(ETileType.Floor, metaIdentifiers[(int)ETileType.Floor]);
			metaChanged = true;
		}
		if(m_TileMetaData[ETileType.Ceiling].m_Identifier != metaIdentifiers[(int)ETileType.Ceiling])
		{
			m_TileMetaData[ETileType.Ceiling] = FindTileMetaInfo(ETileType.Ceiling, metaIdentifiers[(int)ETileType.Ceiling]);
			metaChanged = true;
		}
		if(m_TileMetaData[ETileType.Wall_Ext].m_Identifier != metaIdentifiers[(int)ETileType.Wall_Ext])
		{
			m_TileMetaData[ETileType.Wall_Ext] = FindTileMetaInfo(ETileType.Wall_Ext, metaIdentifiers[(int)ETileType.Wall_Ext]);
			metaChanged = true;
		}

		// Internal walls need to check to see if the external wall piece is a edge
		if(m_TileMetaData[ETileType.Wall_Ext].m_Type == ETileMetaType.Wall_Ext_Edge ||
		   m_TileMetaData[ETileType.Wall_Ext].m_Type == ETileMetaType.Wall_Ext_MS_Edge)
		{
			// We use the external wall pieces and bitshift 4 bits across
			metaIdentifiers[(int)ETileType.Wall_Int] = m_TileMetaData[ETileType.Wall_Ext].m_Identifier;
			metaIdentifiers[(int)ETileType.Wall_Int] |= 1 << 4;
		}
		if(m_TileMetaData[ETileType.Wall_Int].m_Identifier != metaIdentifiers[(int)ETileType.Wall_Int])
		{
			m_TileMetaData[ETileType.Wall_Int] = FindTileMetaInfo(ETileType.Wall_Int, metaIdentifiers[(int)ETileType.Wall_Int]);
			metaChanged = true;
		}

		// Wall caps are special cases which detirmine meta on the fly
		if(m_TileMetaData[ETileType.Wall_Ext_Cap].m_Identifier != metaIdentifiers[(int)ETileType.Wall_Ext_Cap])
		{
			ETileMetaType metaType = ETileMetaType.Wall_Ext_Cap_CornerCap;

			// Check for lower
			if((metaIdentifiers[(int)ETileType.Wall_Ext_Cap] & (1 << (int)EDirection.Lower)) != 0)
				metaType = ETileMetaType.Wall_Ext_Cap_CornerCap_2;

			m_TileMetaData[ETileType.Wall_Ext_Cap] = new TTileMeta(metaType, metaIdentifiers[(int)ETileType.Wall_Ext_Cap], EDirection.North);
			metaChanged = true;
		}

		// Last check to see if the tile types where changed
		if(m_PreviousTileTypeIdentifier != m_TileTypeIdentifier)
		{
			m_PreviousTileTypeIdentifier = m_TileTypeIdentifier;
			metaChanged = true;
		}

		// If any meta data changed
		if(metaChanged)
		{
			// Invoke neighbours to update their meta data
			foreach(CNeighbour neighbour in m_NeighbourHood)
			{
				neighbour.m_Tile.UpdateTileMetaData();
			}

			// Set dirty to update tile types next update
			m_IsDirty = true;

			// Invoke event for meta change
			if(EventTileMetaChanged != null)
				EventTileMetaChanged(this);
		}
	}

	private void UpdateAllTileObjects()
	{
		// Update tile objects
		for(int i = 0; i < (int)ETileType.MAX; ++i)
		{
			ETileType tileType = (ETileType)i;

			// If wall cap, it is a special case
			if(tileType == ETileType.Wall_Ext_Cap)
			{
				// Update Wall Caps
				UpdateWallTileCaps();
				continue;
			}

			// Check if the tile should be active
			if(GetTileTypeState(tileType))
			{
				TTileMeta tileMeta = m_TileMetaData[tileType];

				// If the identifier/variant has changed we need to update the object
				if(m_CurrentTileMetaData[tileType].m_Identifier != tileMeta.m_Identifier ||
				   m_CurrentTileMetaData[tileType].m_Variant != tileMeta.m_Variant)
				{
					// Release the current tile type object
					ReleaseTile(tileType);

					// Create the new tile type object as long as it is not marked as none and the variant is not nothing
					if(tileMeta.m_Type != ETileMetaType.None && 
					   tileMeta.m_Variant != ETileVariant.Nothing)
					{
						m_TileObject[tileType] = m_Grid.TileFactory.InstanceNewTile(tileType, tileMeta.m_Type, tileMeta.m_Variant);
						m_TileObject[tileType].transform.parent = transform;
						m_TileObject[tileType].transform.localPosition = Vector3.zero;
						m_TileObject[tileType].transform.localScale = Vector3.one;
						m_TileObject[tileType].transform.localRotation = GetDirectionRotation(tileMeta.m_LocalNorth);
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

	private void UpdateWallTileCaps()
	{
		int currentWallCapIdentifier = m_CurrentTileMetaData[ETileType.Wall_Ext_Cap].m_Identifier;
		int wallCapIdentifier = m_TileMetaData[ETileType.Wall_Ext_Cap].m_Identifier;

		if(currentWallCapIdentifier != wallCapIdentifier)
		{
			// Extract the missing neighbours from the identifier
			for(int i = (int)EDirection.NorthEast; i <= (int)EDirection.NorthWest; ++i)
			{
				EDirection direction =(EDirection)i;

				// Checking bit for the neighbour, if it doesnt exist continue
				if(!((wallCapIdentifier & (1 << i)) != 0))
				{
					// Get the two neighbouring neighbours
					EDirection dirLeft = CNeighbour.GetLeftDirectionNeighbour(direction);
					EDirection dirRight = CNeighbour.GetRightDirectionNeighbour(direction);

					if(((wallCapIdentifier & (1 << (int)dirLeft)) != 0) && ((wallCapIdentifier & (1 << (int)dirRight)) != 0))
	   				{
						// Only need to make it if it doesnt exist yet
						if(!m_WallInverseObjects.ContainsKey(direction))
						{
							GameObject wallInverseObject = m_Grid.TileFactory.InstanceNewTile(ETileType.Wall_Ext_Cap, 
							                                                            		m_TileMetaData[ETileType.Wall_Ext_Cap].m_Type, 
							                                                            		m_TileMetaData[ETileType.Wall_Ext_Cap].m_Variant);
		   					wallInverseObject.transform.parent = transform;
		   					wallInverseObject.transform.localPosition = Vector3.zero;
		   					wallInverseObject.transform.localScale = Vector3.one;
		   					m_WallInverseObjects.Add(direction, wallInverseObject);
		   
		   					// Rotation part needs to be detirmined based on which neighbour was missing
		   					switch(direction)
		   					{
		   					case EDirection.NorthEast: wallInverseObject.transform.localRotation = Quaternion.identity; break;
		   					case EDirection.SouthEast: wallInverseObject.transform.localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f); break;
		   					case EDirection.SouthWest: wallInverseObject.transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f); break;
		   					case EDirection.NorthWest: wallInverseObject.transform.localRotation = Quaternion.Euler(0.0f, 270.0f, 0.0f); break;
		   					}
						}
	   				}
	   				else if(m_WallInverseObjects.ContainsKey(direction))
	   				{
	   					ReleaseWallInverseTile(m_WallInverseObjects[direction]);
	   					m_WallInverseObjects.Remove(direction);
   					}
				}
				// Remove wall if the neighbour was found
				else if(m_WallInverseObjects.ContainsKey(direction))
				{
					ReleaseWallInverseTile(m_WallInverseObjects[direction]);
					m_WallInverseObjects.Remove(direction);
				}
			}

			m_CurrentTileMetaData[ETileType.Wall_Ext_Cap] = m_TileMetaData[ETileType.Wall_Ext_Cap];
		}
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
		TTileMeta tileMeta = m_TileMetaData[_TileType];
		tileMeta.m_Variant = _TileVariant;

		// Set the new meta data
		SetMetaData(_TileType, tileMeta);
	}

	public ETileVariant GetTileTypeVariant(ETileType _TileType)
	{
		return(m_TileMetaData[_TileType].m_Variant);
	}

	public void SetTileTypeState(ETileType _TileType, bool _State)
	{
		if(_State)
			m_TileTypeIdentifier |= (1 << (int)_TileType);
		else
			m_TileTypeIdentifier &= ~(1 << (int)_TileType);

		if(EventTileTypeStateChange != null)
			EventTileTypeStateChange(this);
	}
	
	public bool GetTileTypeState(ETileType _TileType)
	{
		bool state = ((m_TileTypeIdentifier & (1 << (int)_TileType)) != 0);
		return(state);
	}

	public void SetTileNeighbourExemptionState(ETileType _TileType, EDirection _Direction, bool _State)
	{
		if(_State)
			m_CurrentTileNeighbourExemptions[_TileType] |= (1 << (int)_Direction);
		else
			m_CurrentTileNeighbourExemptions[_TileType] &= ~(1 << (int)_Direction);
	}

	public bool GetTileNeighbourExemptionState(ETileType _TileType, EDirection _Direction)
	{
		bool state = ((m_CurrentTileNeighbourExemptions[_TileType] & (1 << (int)_Direction)) != 0);
		return(state);
	}

	public EDirection GetTileTypeLocalNorth(ETileType _TileType)
	{
		return(m_CurrentTileMetaData[_TileType].m_LocalNorth);
	}

	public void Release()
	{
		// Release tile objects
		for(int i = 0; i < (int)ETileType.MAX; ++i)
		{
			ReleaseTile((ETileType)i);
			m_TileMetaData[(ETileType)i] = new TTileMeta();
		}
	}

	private void ReleaseTile(ETileType _TileType)
	{
		if(_TileType == ETileType.Wall_Ext_Cap)
		{
			ReleaseWallExtCaps();
			return;
		}

		if(!m_TileObject.ContainsKey(_TileType))
			return;

		if(m_TileObject[_TileType] != null)
		{
			m_Grid.TileFactory.ReleaseTileObject(m_TileObject[_TileType]);

			m_TileObject[_TileType] = null;
			return;
		}
	}

	private void ReleaseWallExtCaps()
	{
		foreach(GameObject iwt in m_WallInverseObjects.Values)
		{
			ReleaseWallInverseTile(iwt);
		}
		m_WallInverseObjects.Clear();
	}

	private void ReleaseWallInverseTile(GameObject _InverseWallTile)
	{
		if(_InverseWallTile != null)
		{
			m_Grid.TileFactory.ReleaseTileObject(_InverseWallTile);
			_InverseWallTile = null;
		}
	}

	private static void FillTileMetaData()
	{
		// Floors
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_Cell, 0, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_Middle, 15, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_Hall, 10, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_Hall, 5, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_Edge, 13, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_Edge, 11, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_Edge, 7, EDirection.South);
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_Edge, 14, EDirection.West);
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_Corner, 3, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_Corner, 6, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_Corner, 12, EDirection.South);
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_Corner, 9, EDirection.West);
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_End, 8, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_End, 1, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_End, 2, EDirection.South);
		AddTileMetaInfoEntry(ETileType.Floor, ETileMetaType.Floor_End, 4, EDirection.West);
		
		// Walls Exterior
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.None, 15, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.None, 0, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_Hall, 5, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_Hall, 10, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_Edge, 13, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_Edge, 11, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_Edge, 7, EDirection.South);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_Edge, 14, EDirection.West);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_Corner, 9, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_Corner, 3, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_Corner, 6, EDirection.South);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_Corner, 12, EDirection.West);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_End, 8, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_End, 1, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_End, 2, EDirection.South);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_End, 4, EDirection.West);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.None, 31, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.None, 16, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_MS_Hall, 21, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_MS_Hall, 26, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_MS_Edge, 29, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_MS_Edge, 27, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_MS_Edge, 23, EDirection.South);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_MS_Edge, 30, EDirection.West);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_MS_Corner, 25, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_MS_Corner, 19, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_MS_Corner, 22, EDirection.South);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_MS_Corner, 28, EDirection.West);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_MS_End, 24, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_MS_End, 17, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_MS_End, 18, EDirection.South);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, ETileMetaType.Wall_Ext_MS_End, 20, EDirection.West);


		// Walls Interior
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_Single, 0, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_End, 8, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_End, 1, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_End, 2, EDirection.South);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_End, 4, EDirection.West);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_Middle, 5, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_Middle, 10, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_Corner, 6, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_Corner, 12, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_Corner, 9, EDirection.South);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_Corner, 3, EDirection.West);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_T, 11, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_T, 7, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_T, 14, EDirection.South);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_T, 13, EDirection.West);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_X, 15, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_EdgeT, 30, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_EdgeT, 23, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_EdgeT, 27, EDirection.South);
		AddTileMetaInfoEntry(ETileType.Wall_Int, ETileMetaType.Wall_Int_EdgeT, 29, EDirection.West);

		// Ceiling
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_Cell, 0, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_Middle, 15, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_Hall, 10, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_Hall, 5, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_Edge, 7, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_Edge, 14, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_Edge, 13, EDirection.South);
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_Edge, 11, EDirection.West);
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_Corner, 6, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_Corner, 12, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_Corner, 9, EDirection.South);
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_Corner, 3, EDirection.West);
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_End, 8, EDirection.North);
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_End, 1, EDirection.East);
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_End, 2, EDirection.South);
		AddTileMetaInfoEntry(ETileType.Ceiling, ETileMetaType.Ceiling_End, 4, EDirection.West);
	}
	
	private static void AddTileMetaInfoEntry(ETileType _TileType, ETileMetaType _MetaType, int _Identifier, EDirection _LocalNorth)
	{
		if(!s_TileMetaInfo[_TileType].ContainsKey(_Identifier))
			s_TileMetaInfo[_TileType].Add(_Identifier, new TTileMeta(_MetaType, _Identifier, _LocalNorth));
		else
			Debug.LogError("Tile meta info was already added for: " + _TileType + " : " + _MetaType + " : " + _Identifier);
	}
	
	private TTileMeta FindTileMetaInfo(ETileType _TileType, int _Identifier)
	{
		// Find the meta data for this tile
		if(s_TileMetaInfo[_TileType].ContainsKey(_Identifier))
		{
			return(s_TileMetaInfo[_TileType][_Identifier]);
		}
		else
		{
			// If it wasn't found there was no meta data found for it
			Debug.LogWarning("Tile meta info wasn't found for:  " + _TileType + " : " + _Identifier);

			// Return meta data that is of the same as found to avoid recursive loop
			return(new TTileMeta(ETileMetaType.None, _Identifier, EDirection.North));
		}
	}

	public static Quaternion GetDirectionRotation(EDirection _Direction)
	{
		Quaternion quat = Quaternion.identity;

		switch(_Direction)
		{
		case EDirection.North: quat = Quaternion.identity; break;
		case EDirection.NorthWest: quat = Quaternion.Euler(0.0f, -45.0f, 0.0f); break;
		case EDirection.West: quat = Quaternion.Euler(0.0f, -90.0f, 0.0f); break;
		case EDirection.SouthWest: quat = Quaternion.Euler(0.0f, -135.0f, 0.0f); break;
		case EDirection.South: quat = Quaternion.Euler(0.0f, 180.0f, 0.0f); break;
		case EDirection.SouthEast: quat = Quaternion.Euler(0.0f, 135.0f, 0.0f); break;
		case EDirection.East: quat = Quaternion.Euler(0.0f, 90.0f, 0.0f); break;
		case EDirection.NorthEast: quat = Quaternion.Euler(0.0f, 45.0f, 0.0f); break;
		}

		return(quat);
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
	Wall_Ext_MS_Corner,
	Wall_Ext_MS_Edge,
	Wall_Ext_MS_End,
	Wall_Ext_MS_Hall,
	
	Wall_Int_EdgeT = 200,
	Wall_Int_Middle,
	Wall_Int_Corner,
	Wall_Int_T,
	Wall_Int_X,
	Wall_Int_Single,
	Wall_Int_End,
	
	Ceiling_Middle = 300, 
	Ceiling_Corner, 
	Ceiling_Edge,
	Ceiling_End,
	Ceiling_Hall,
	Ceiling_Cell,
	
	Wall_Ext_Cap_CornerCap = 400,
	Wall_Ext_Cap_CornerCap_2,
}

public enum ETileVariant
{
	INVALID = -1,

	Nothing,
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
		get { return(new TTileMeta(ETileMetaType.None, -1, EDirection.North)); }
	}

	public TTileMeta(ETileMetaType _Type, int _Identifier, EDirection _LocalNorth)
	{
		m_Identifier = _Identifier;
		m_Type = _Type;
		m_Variant = ETileVariant.Default;
		m_LocalNorth = _LocalNorth;
	}
	
	public int m_Identifier;
	public ETileMetaType m_Type;
	public ETileVariant m_Variant;
	public EDirection m_LocalNorth;
}
