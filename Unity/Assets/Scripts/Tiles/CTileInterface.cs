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


public class CTileInterface : CNetworkMonoBehaviour 
{
	// Member Types


	// Member Delegates & Events
	public delegate void HandleTileInterfaceEvent(CTileInterface _Self);

	public event HandleTileInterfaceEvent EventTileGeometryChanged;


	// Member Fields
	public CGrid m_Grid = null;
	public CGridPoint m_GridPosition = null;

	public List<CTile.EType> m_TileTypes = new List<CTile.EType>();
	public List<CNeighbour> m_NeighbourHood = new List<CNeighbour>();

	private CNetworkVar<byte> m_RemoteTileTypeMask = null;


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
	public override void InstanceNetworkVars(CNetworkViewRegistrar _Registrar)
	{
		m_RemoteTileTypeMask = _Registrar.CreateReliableNetworkVar<byte>(OnNetworkVarSync, 0);
	}

	private void OnNetworkVarSync(INetworkVar _SynedVar)
	{
		if(m_RemoteTileTypeMask == _SynedVar)
		{
			// Get the difference of tile types since last sync
			for(int i = (int)CTile.EType.INVALID + 1; i < (int)CTile.EType.MAX; ++i)
			{
				bool currentState = CUtility.GetMaskState(i, (int)m_RemoteTileTypeMask.Value);
				bool previousState = CUtility.GetMaskState(i, (int)m_RemoteTileTypeMask.PreviousValue);

				if(currentState && !previousState)
				{
					CreateTileComponent((CTile.EType)i);
				}

				if(!currentState && previousState)
				{
					RemoveTileComponent((CTile.EType)i);
				}
			}
		}
	}
	
	private void CreateTileComponent(CTile.EType _TileType)
	{
		Type tileClassType = CTile.GetTileClassType(_TileType);
		CTile tile = (CTile)gameObject.AddComponent(tileClassType);
		tile.m_TileInterface = this;
		tile.EventTileObjectChanged += OnTileObjectChange;
	}

	private void RemoveTileComponent(CTile.EType _TileType)
	{
		CTile tile = GetTile(_TileType);
		tile.ReleaseTileObject();
		Destroy(tile);
	}

	[AServerOnly]
	public void FindNeighbours()
	{
		m_NeighbourHood.Clear();
		
		foreach(CNeighbour pn in s_PossibleNeighbours) 
		{
			CGridPoint possibleNeightbour = new CGridPoint(m_GridPosition.x + pn.m_GridPointOffset.x, 
			                                               m_GridPosition.y + pn.m_GridPointOffset.y, 
			                                               m_GridPosition.z + pn.m_GridPointOffset.z);
			
			CTileInterface tile = m_Grid.GetTile(possibleNeightbour);
			if(tile != null)
			{
				CNeighbour newNeighbour = new CNeighbour(pn.m_GridPointOffset, pn.m_Direction);
				newNeighbour.m_TileInterface = tile;
				
				m_NeighbourHood.Add(newNeighbour);
			}
		}
	}

	[AServerOnly]
	public void UpdateNeighbourhood()
	{
		// Invoke neighbours to find all of their neighbours
		foreach(CNeighbour neighbour in m_NeighbourHood)
		{
			neighbour.m_TileInterface.FindNeighbours();
		}
	}

	[AServerOnly]
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

	[AServerOnly]
	public void SetTileTypeState(CTile.EType _TileType, bool _State)
	{
		if(m_TileTypes.Contains(_TileType) && _State)
			return;

		if(!m_TileTypes.Contains(_TileType) && !_State)
			return;

		if(_State)
		{
			m_TileTypes.Add(_TileType);
		}
		else
		{
			m_TileTypes.Remove(_TileType);
		}

		// Set the remote mask state
		int newMask = (int)m_RemoteTileTypeMask.Value;
		CUtility.SetMaskState((int)_TileType, _State, ref newMask);
		m_RemoteTileTypeMask.Value = (byte)newMask;
	}
	
	public bool GetTileTypeState(CTile.EType _TileType)
	{
		return(CUtility.GetMaskState((int)_TileType, (int)m_RemoteTileTypeMask.Value));
	}

	[AServerOnly]
	public void Clone(CTileInterface _From)
	{
		// Copy all tile states
		for(int i = (int)CTile.EType.INVALID + 1; i < (int)CTile.EType.MAX; ++i)
		{
			CTile.EType type = (CTile.EType)i;
			bool fromState = _From.GetTileTypeState(type);
			SetTileTypeState(type, fromState);

			// Copy over the current meta data
			if(fromState)
				GetTile(type).m_CurrentTileMeta = _From.GetTile(type).m_CurrentTileMeta;
		}
	}

	public void Release()
	{
		// Release tile objects
		foreach(CTile tile in GetComponents<CTile>())
		{
			Destroy(tile);
		}

		// Update the neighbourhood
		UpdateNeighbourhood();
	}

	private void OnTileObjectChange(CTile _Tile)
	{
		if(EventTileGeometryChanged != null)
			EventTileGeometryChanged(this);
	}
}
