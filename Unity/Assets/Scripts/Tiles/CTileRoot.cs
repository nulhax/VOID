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
	public CGridPoint m_GridPosition;

	public int m_TileTypeMask = 0;
	public List<CTile.EType> m_TileTypes = new List<CTile.EType>();

	public List<CNeighbour> m_NeighbourHood = new List<CNeighbour>();


	// Member Properties
	public static List<CNeighbour> s_PossibleNeighbours = new List<CNeighbour>(
		new CNeighbour[] 
		{
		new CNeighbour(new CGridPoint(-1, 0, 1), EDirection.NorthWest),
		new CNeighbour(new CGridPoint(0, 0, 1), EDirection.North),
		new CNeighbour(new CGridPoint(1, 0, 1), EDirection.NorthEast),
		new CNeighbour(new CGridPoint(1, 0, 0), EDirection.East),
		new CNeighbour(new CGridPoint(1, 0, -1), EDirection.SouthEast),
		new CNeighbour(new CGridPoint(0, 0, -1), EDirection.South),
		new CNeighbour(new CGridPoint(-1, 0, -1), EDirection.SouthWest),
		new CNeighbour(new CGridPoint(-1, 0, 0), EDirection.West),
	});

	
	// Member Methods
	private void Start()
	{
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
			CGridPoint possibleNeightbour = new CGridPoint(m_GridPosition.x + pn.m_GridPointOffset.x, 
			                                               m_GridPosition.y + pn.m_GridPointOffset.y, 
			                                               m_GridPosition.z + pn.m_GridPointOffset.z);
			
			CTileRoot tile = m_Grid.GetTile(possibleNeightbour);
			if(tile != null)
			{
				CNeighbour newNeighbour = new CNeighbour(pn.m_GridPointOffset, pn.m_Direction);
				newNeighbour.m_TileRoot = tile;
				
				m_NeighbourHood.Add(newNeighbour);
			}
		}
	}

	public void UpdateNeighbourhood()
	{
		// Invoke neighbours to find all of their neighbours
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			neighbour.m_TileRoot.FindNeighbours();
		}
	}

	public void UpdateAllCurrentTileMetaData()
	{
		foreach(CTile tile in GetComponents<CTile>())
		{
			tile.UpdateCurrentTileMetaData();
		}
	}

	public CTile GetTile(CTile.EType _TileType)
	{
		Type tileClassType = CTile.GetTileClassType(_TileType);
		return((CTile)gameObject.GetComponent(tileClassType));
	}

	public void SetTileTypeState(CTile.EType _TileType, bool _State)
	{
		if(m_TileTypes.Contains(_TileType) && _State)
			return;

		if(!m_TileTypes.Contains(_TileType) && !_State)
			return;

		CTile tile = null;

		if(_State)
		{
			m_TileTypes.Add(_TileType);
			Type tileClassType = CTile.GetTileClassType(_TileType);
			tile = (CTile)gameObject.AddComponent(tileClassType);
			tile.m_TileRoot = this;
		}
		else if(!_State)
		{
			m_TileTypes.Remove(_TileType);
			tile = GetTile(_TileType);
			tile.ReleaseTileObject();
			Destroy(tile);
		}

		// Set the tile type mask
		CUtility.SetMaskState((int)_TileType, _State, ref m_TileTypeMask);
	}
	
	public bool GetTileTypeState(CTile.EType _TileType)
	{
		return(m_TileTypes.Contains(_TileType));
	}

	public void Clone(CTileRoot _From)
	{
		// Copy all tile states
		for(int i = (int)CTile.EType.INVALID + 1; i < (int)CTile.EType.MAX; ++i)
		{
			CTile.EType type = (CTile.EType)i;
			bool fromState = _From.GetTileTypeState(type);
			SetTileTypeState(type, fromState);

			if(fromState)
				GetTile(type).m_CurrentTileMeta = _From.GetTile(type).m_ActiveTileMeta;
		}
	}

	public void Release()
	{
		if(EventTilePreRelease != null)
            EventTilePreRelease(this);

		// Release tile objects
		foreach(CTile tile in GetComponents<CTile>())
		{
			Destroy(tile);
		}

		// Update the neighbourhood
		UpdateNeighbourhood();
	}
}
