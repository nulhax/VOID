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
	public enum ETileType
	{
		INVALID = -1,

		Floor,
		Wall_Ext,
		Wall_Int,

		MAX
	}

	// Member Delegates & Events
	
	
	// Member Fields
	public List<CNeighbour> m_NeighbourHood = new List<CNeighbour>();

	public int m_FloorMetaID = 0;
	public int m_WallExtMetaID = 0;
	public int m_WallIntMetaID = 0;
	public int m_WallCapMetaID = 0;

	private Dictionary<ETileType, TTileMeta> m_TileMetaData = new Dictionary<ETileType, TTileMeta>();
	private Dictionary<ETileType, TTileMeta> m_CurrentTileMetaData = new Dictionary<ETileType, TTileMeta>();

	private int m_WallCapIdentifier = 0;
	private int m_CurrentWallCapIdentifier = 0;

	private bool m_ContainsInternalWall = false;

	private bool m_IsDirty = false;

	private GameObject m_FloorObject = null;
	private GameObject m_WallObject = null;
	private Dictionary<CNeighbour.EDirection, GameObject> m_WallInverseObjects = new Dictionary<CNeighbour.EDirection, GameObject>();

	static private bool s_DictionariesInitialised = false;

	static private Dictionary<ETileType, Dictionary<int, TTileMeta>> s_TileMetaInfo = new Dictionary<ETileType, Dictionary<int, TTileMeta>>();

	private List<CNeighbour> s_AllNeighbours = new List<CNeighbour>(
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
	private void Awake()
	{
		// If the dictionary is empty we need to fill it
		if(!s_DictionariesInitialised)
		{
			s_TileMetaInfo[ETileType.Floor] = new Dictionary<int, TTileMeta>();
			s_TileMetaInfo[ETileType.Wall_Ext] = new Dictionary<int, TTileMeta>();
			s_TileMetaInfo[ETileType.Wall_Int] = new Dictionary<int, TTileMeta>();

			FillTileMetaData();

			s_DictionariesInitialised = true;
		}

		// Initialise default meta data with invalid identifiers
		m_TileMetaData[ETileType.Floor] = new TTileMeta(TTileMeta.EType.None, -1, Quaternion.identity);
		m_TileMetaData[ETileType.Wall_Ext] = new TTileMeta(TTileMeta.EType.None, -1, Quaternion.identity);
		m_TileMetaData[ETileType.Wall_Int] = new TTileMeta(TTileMeta.EType.None, -1, Quaternion.identity);
		m_CurrentTileMetaData[ETileType.Floor] = new TTileMeta(TTileMeta.EType.None, -1, Quaternion.identity);
		m_CurrentTileMetaData[ETileType.Wall_Ext] = new TTileMeta(TTileMeta.EType.None, -1, Quaternion.identity);
		m_CurrentTileMetaData[ETileType.Wall_Int] = new TTileMeta(TTileMeta.EType.None, -1, Quaternion.identity);
	}

	private void Start()
	{
		// Find all of the neighbours
		FindNeighbours();
		
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

	private void UpdateNeighbourhood()
	{
		// Find all neighbours and invoke them to find others
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			neighbour.tile.FindNeighbours();
		}

		// Invoke an update of their meta data
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			neighbour.tile.UpdateTileMetaData();
		}
	}

	private void FindNeighbours()
	{
		m_NeighbourHood.Clear();
		
		foreach(CNeighbour pn in s_AllNeighbours) 
		{
			TGridPoint possibleNeightbour = new TGridPoint(x + pn.gridPositionOffset.x, 
			                                               y + pn.gridPositionOffset.y, 
			                                               z + pn.gridPositionOffset.z);

			CTile tile = m_Grid.GetTile(possibleNeightbour);
			if(tile != null)
			{
				CNeighbour newNeighbour = new CNeighbour(pn.gridPositionOffset, pn.direction);
				newNeighbour.tile = tile;
				
				m_NeighbourHood.Add(newNeighbour);
			}
		}
	}

	public void UpdateAllTileObjects()
	{
		// Update all tile objects
		UpdateFloorTile();
		UpdateWallTile();
	}

	private void UpdateTileMetaData()
	{
		int[] metaIdentifiers = new int[(int)ETileType.MAX];
		int wallCapIdentifier = 0;

		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			// Floors/External Walls only care about N, E, S, W neighbours
			if(neighbour.direction == CNeighbour.EDirection.North ||
			   neighbour.direction == CNeighbour.EDirection.East ||
			   neighbour.direction == CNeighbour.EDirection.South ||
			   neighbour.direction == CNeighbour.EDirection.West)
			{
				metaIdentifiers[(int)ETileType.Floor] |= 1 << (int)neighbour.direction;
				metaIdentifiers[(int)ETileType.Wall_Ext] |= 1 << (int)neighbour.direction;
			}

			// Wall caps care about all directions
			wallCapIdentifier |= 1 << (int)neighbour.direction;
		}

		// Check if floor/external wall meta data changed
		bool metaChanged = false;
		if(m_TileMetaData[ETileType.Floor].m_Identifier != metaIdentifiers[(int)ETileType.Floor])
		{
			m_TileMetaData[ETileType.Floor] = FindTileMetaInfo(ETileType.Floor, metaIdentifiers[(int)ETileType.Floor]);
			metaChanged = true;
		}
		if(m_TileMetaData[ETileType.Wall_Ext].m_Identifier != metaIdentifiers[(int)ETileType.Wall_Ext])
		{
			m_TileMetaData[ETileType.Wall_Ext] = FindTileMetaInfo(ETileType.Wall_Ext, metaIdentifiers[(int)ETileType.Wall_Ext]);
			metaChanged = true;
		}

		// Internal walls make another pass as they rely on wall meta information
		if(m_ContainsInternalWall)
		{
			foreach(CNeighbour neighbour in m_NeighbourHood)
			{
				// Internal walls only care about N, E, S, W neighbours
				if(neighbour.direction == CNeighbour.EDirection.North ||
				   neighbour.direction == CNeighbour.EDirection.East ||
				   neighbour.direction == CNeighbour.EDirection.South ||
				   neighbour.direction == CNeighbour.EDirection.West)
				{
					if(neighbour.tile.m_ContainsInternalWall)
					{
						metaIdentifiers[(int)ETileType.Wall_Int] |= 1 << (int)neighbour.direction;
					}
				}
			}

			// If i was an edge piece, add on some value to differentiate this piece
			if(m_TileMetaData[ETileType.Wall_Ext].m_Type == TTileMeta.EType.Wall_Ext_Edge)
			{
				metaIdentifiers[(int)ETileType.Wall_Int] = m_TileMetaData[ETileType.Wall_Ext].m_Identifier;
				metaIdentifiers[(int)ETileType.Wall_Int] |= 1 << 4;
			}

			// If the final value was 0, add on some value to differentiate this piece]
			if(metaIdentifiers[(int)ETileType.Wall_Int] == 0)
			{
				metaIdentifiers[(int)ETileType.Wall_Int] |= 1 << 5;
			}
		}

		// Check if internal wall meta data changed
		if(m_TileMetaData[ETileType.Wall_Int].m_Identifier != metaIdentifiers[(int)ETileType.Wall_Int])
		{
			m_TileMetaData[ETileType.Wall_Int] = FindTileMetaInfo(ETileType.Wall_Int, metaIdentifiers[(int)ETileType.Wall_Int]);
			metaChanged = true;
		}

		// Wall caps are special cases
		if(wallCapIdentifier != m_WallCapIdentifier)
		{
			m_WallCapIdentifier = wallCapIdentifier;
			metaChanged = true;
		}

		// If any meta data changed
		if(metaChanged)
		{
			// Invoke neighbours to update their meta data
			foreach(CNeighbour neighbour in m_NeighbourHood)
			{
				neighbour.tile.UpdateTileMetaData();
			}

			// Set dirty to update tile types next update
			m_IsDirty = true;
		}

		// Save for inspector debugging purposes
		m_FloorMetaID = m_TileMetaData[ETileType.Floor].m_Identifier;
		m_WallExtMetaID = m_TileMetaData[ETileType.Wall_Ext].m_Identifier;
		m_WallIntMetaID = m_TileMetaData[ETileType.Wall_Int].m_Identifier;
		m_WallCapMetaID = m_WallCapIdentifier;
	}

	public TTileMeta GetMetaData(ETileType _MetaType)
	{
		return(m_TileMetaData[_MetaType]);
	}

	private void UpdateFloorTile()
	{
		ETileType tileType = ETileType.Floor;

		if(m_CurrentTileMetaData[tileType].m_Identifier != m_TileMetaData[tileType].m_Identifier)
		{
			ReleaseFloorTile();

			if(m_TileMetaData[tileType].m_Type != TTileMeta.EType.None)
			{
				m_FloorObject = m_Grid.m_TileFactory.NewTile(tileType, m_TileMetaData[tileType].m_Type);
				m_FloorObject.transform.parent = transform;
				m_FloorObject.transform.localPosition = Vector3.zero;
				m_FloorObject.transform.localScale = Vector3.one;
				m_FloorObject.transform.localRotation = m_TileMetaData[tileType].m_Rotation;
			}

			m_CurrentTileMetaData[tileType] = m_TileMetaData[tileType];
		}
	}

	private void UpdateWallTile()
	{
		if(m_CurrentTileMetaData[ETileType.Wall_Ext].m_Identifier != m_TileMetaData[ETileType.Wall_Ext].m_Identifier ||
		   m_CurrentTileMetaData[ETileType.Wall_Int].m_Identifier != m_TileMetaData[ETileType.Wall_Int].m_Identifier)
		{
			ReleaseWallTile();

			UpdateExternalWallTile();
			UpdateInternalWallTile();
		}

		UpdateWallTileCaps();
	}

	private void UpdateExternalWallTile()
	{
		TTileMeta tileMeta = m_TileMetaData[ETileType.Wall_Ext];
		if(!m_ContainsInternalWall)
		{
			if(tileMeta.m_Type != TTileMeta.EType.None)
			{
				m_WallObject = m_Grid.m_TileFactory.NewTile(ETileType.Wall_Ext, tileMeta.m_Type);
				m_WallObject.transform.parent = transform;
				m_WallObject.transform.localPosition = Vector3.zero;
				m_WallObject.transform.localScale = Vector3.one;
				m_WallObject.transform.localRotation = tileMeta.m_Rotation;
			}
		}

		m_CurrentTileMetaData[ETileType.Wall_Ext] = tileMeta;
	}

	private void UpdateInternalWallTile()
	{
		TTileMeta tileMeta = m_TileMetaData[ETileType.Wall_Int];
		if(m_ContainsInternalWall)
		{
			if(tileMeta.m_Type != TTileMeta.EType.None)
			{
				m_WallObject = m_Grid.m_TileFactory.NewTile(ETileType.Wall_Int, tileMeta.m_Type);
				m_WallObject.transform.parent = transform;
				m_WallObject.transform.localPosition = Vector3.zero;
				m_WallObject.transform.localScale = Vector3.one;
				m_WallObject.transform.localRotation = tileMeta.m_Rotation;
			}
		}

		m_CurrentTileMetaData[ETileType.Wall_Int] = tileMeta;
	}

	private void UpdateWallTileCaps()
	{
		if(m_CurrentWallCapIdentifier != m_WallCapIdentifier)
		{
			// Extract the missing neighbours from the identifier
			for(int i = (int)CNeighbour.EDirection.NorthEast; i <= (int)CNeighbour.EDirection.NorthWest; ++i)
			{
				CNeighbour.EDirection direction =(CNeighbour.EDirection)i;

				// Checking bit for the neighbour, if it doesnt exist continue
				if(!((m_WallCapIdentifier & (1 << i)) != 0))
				{
					// Get the two neighbouring neighbours
					CNeighbour.EDirection dirLeft = CNeighbour.GetLeftDirectionNeighbour(direction);
					CNeighbour.EDirection dirRight = CNeighbour.GetRightDirectionNeighbour(direction);

					if(((m_WallCapIdentifier & (1 << (int)dirLeft)) != 0) && ((m_WallCapIdentifier & (1 << (int)dirRight)) != 0))
	   				{
						// Only need to make it if it doesnt exist yet
						if(!m_WallInverseObjects.ContainsKey(direction))
						{
		   					GameObject wallInverseObject = m_Grid.m_TileFactory.NewTile(ETileType.Wall_Ext, TTileMeta.EType.Wall_Ext_CornerCap);
		   					wallInverseObject.transform.parent = transform;
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

			m_CurrentWallCapIdentifier = m_WallCapIdentifier;
		}
	}

	public void PlaceInternalWall()
	{
		// Cannot place internal wall on exterior walls except for edge
		if(m_TileMetaData[ETileType.Wall_Ext].m_Type == TTileMeta.EType.None || 
		   m_TileMetaData[ETileType.Wall_Ext].m_Type == TTileMeta.EType.Wall_Ext_Edge)
		{
			m_ContainsInternalWall = true;
		}

		// Update the meta data
		UpdateTileMetaData();
	}

	public void RemoveInternalWall()
	{
		if(m_ContainsInternalWall == true)
		{
			m_ContainsInternalWall = false;

			// Update the meta data
			UpdateTileMetaData();
		}
	}

	public void Release()
	{
		// Release floor tile object
		ReleaseFloorTile();

		// Release wall tile object
		ReleaseWallTile();

		// Release all inverse wall objects
		ReleaseWallInverseTiles();

		// Clear meta data
		m_TileMetaData[ETileType.Floor] = new TTileMeta();
		m_TileMetaData[ETileType.Wall_Ext] = new TTileMeta();
		m_TileMetaData[ETileType.Wall_Int] = new TTileMeta();
	}

	private void ReleaseFloorTile()
	{
		if(m_FloorObject != null)
		{
			m_Grid.m_TileFactory.ReleaseTileObject(m_FloorObject);

			m_FloorObject = null;
		}
	}

	private void ReleaseWallTile()
	{
		if(m_WallObject != null)
		{
			m_Grid.m_TileFactory.ReleaseTileObject(m_WallObject);

			m_WallObject = null;
		}
	}

	private void ReleaseWallInverseTiles()
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
			m_Grid.m_TileFactory.ReleaseTileObject(_InverseWallTile);
			_InverseWallTile = null;
		}
	}

	private static void FillTileMetaData()
	{
		// Floors
		AddTileMetaInfoEntry(ETileType.Floor, TTileMeta.EType.Floor_Middle, 15, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Floor, TTileMeta.EType.Floor_Cell, 0, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Floor, TTileMeta.EType.Floor_Hall, 10, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Floor, TTileMeta.EType.Floor_Hall, 5, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Floor, TTileMeta.EType.Floor_Edge, 7, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Floor, TTileMeta.EType.Floor_Edge, 14, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Floor, TTileMeta.EType.Floor_Edge, 13, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Floor, TTileMeta.EType.Floor_Edge, 11, Quaternion.Euler(0.0f, 270.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Floor, TTileMeta.EType.Floor_Corner, 6, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Floor, TTileMeta.EType.Floor_Corner, 12, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Floor, TTileMeta.EType.Floor_Corner, 9, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Floor, TTileMeta.EType.Floor_Corner, 3, Quaternion.Euler(0.0f, 270.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Floor, TTileMeta.EType.Floor_End, 2, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Floor, TTileMeta.EType.Floor_End, 4, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Floor, TTileMeta.EType.Floor_End, 8, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Floor, TTileMeta.EType.Floor_End, 1, Quaternion.Euler(0.0f, 270.0f, 0.0f));
		
		// Walls Exterior
		AddTileMetaInfoEntry(ETileType.Wall_Ext, TTileMeta.EType.None, 15, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, TTileMeta.EType.None, 0, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, TTileMeta.EType.Wall_Ext_Hall, 10, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, TTileMeta.EType.Wall_Ext_Hall, 5, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Ext, TTileMeta.EType.Wall_Ext_Edge, 7, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, TTileMeta.EType.Wall_Ext_Edge, 14, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Ext, TTileMeta.EType.Wall_Ext_Edge, 13, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Ext, TTileMeta.EType.Wall_Ext_Edge, 11, Quaternion.Euler(0.0f, 270.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Ext, TTileMeta.EType.Wall_Ext_Corner, 6, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, TTileMeta.EType.Wall_Ext_Corner, 12, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Ext, TTileMeta.EType.Wall_Ext_Corner, 9, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Ext, TTileMeta.EType.Wall_Ext_Corner, 3, Quaternion.Euler(0.0f, 270.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Ext, TTileMeta.EType.Wall_Ext_End, 2, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Wall_Ext, TTileMeta.EType.Wall_Ext_End, 4, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Ext, TTileMeta.EType.Wall_Ext_End, 8, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Ext, TTileMeta.EType.Wall_Ext_End, 1, Quaternion.Euler(0.0f, 270.0f, 0.0f));
		
		// Walls Interior
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.None, 0, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_Single, 32, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_End, 1, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_End, 2, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_End, 4, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_End, 8, Quaternion.Euler(0.0f, 270.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_Middle, 10, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_Middle, 5, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_Corner, 6, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_Corner, 12, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_Corner, 9, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_Corner, 3, Quaternion.Euler(0.0f, 270.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_T, 14, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_T, 13, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_T, 11, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_T, 7, Quaternion.Euler(0.0f, 270.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_X, 15, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_EdgeT, 23, Quaternion.identity);
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_EdgeT, 30, Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_EdgeT, 29, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddTileMetaInfoEntry(ETileType.Wall_Int, TTileMeta.EType.Wall_Int_EdgeT, 27, Quaternion.Euler(0.0f, 270.0f, 0.0f));
	}
	
	private static void AddTileMetaInfoEntry(ETileType _TileType, TTileMeta.EType _MetaType, int _Identifier, Quaternion _Rotation)
	{
		if(!s_TileMetaInfo[_TileType].ContainsKey(_Identifier))
			s_TileMetaInfo[_TileType].Add(_Identifier, new TTileMeta(_MetaType, _Identifier, _Rotation));
		else
			Debug.LogError("Tile meta info wasn already added for: " + _TileType + ": " + _Identifier);
	}
	
	private static TTileMeta FindTileMetaInfo(ETileType _Type, int _Identifier)
	{
		// Find the meta data for this tile
		if(s_TileMetaInfo[_Type].ContainsKey(_Identifier))
		{
			return(s_TileMetaInfo[_Type][_Identifier]);
		}
		else
		{
			// If it wasn't found there was no meta data found for it
			Debug.LogWarning("Tile meta info wasn't found for: " + _Type + ": " + _Identifier);

			// Return meta data that is of the same as found to avoid recursive loop
			return(new TTileMeta(TTileMeta.EType.None, _Identifier, Quaternion.identity));
		}
	}
}

[System.Serializable]
public struct TTileMeta
{
	public enum EType 
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
		Wall_Ext_CornerCap,

		Wall_Int_EdgeT = 200,
		Wall_Int_Middle,
		Wall_Int_Corner,
		Wall_Int_T,
		Wall_Int_X,
		Wall_Int_Single,
		Wall_Int_End,
	}
	
	public TTileMeta(EType _Type, int _Identifier, Quaternion _Rotation)
	{
		m_Identifier = _Identifier;
		m_Type = _Type;
		m_Rotation = _Rotation;
	}
	
	public int m_Identifier;
	public EType m_Type;
	public Quaternion m_Rotation;
}
