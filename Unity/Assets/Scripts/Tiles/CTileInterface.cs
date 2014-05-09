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

	private CNetworkVar<ushort> m_RemoteTileTypeMask = null;


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
	public override void RegisterNetworkComponents(CNetworkViewRegistrar _Registrar)
	{
		m_RemoteTileTypeMask = _Registrar.CreateReliableNetworkVar<ushort>(OnNetworkVarSync, 0);

		_Registrar.RegisterRpc(this, "RemoteSetCurrentMeta");
	}


    public CTile.EType GetTileType(GameObject _cUnknownObject)
    {
        foreach (CTile tile in GetComponents<CTile>())
        {
            if (tile.m_TileObject == _cUnknownObject)
            {
                return (tile.m_TileType);
            }
        }

        return (CTile.EType.INVALID);
    }


	private void OnNetworkVarSync(INetworkVar _SynedVar)
	{
		if(m_RemoteTileTypeMask == _SynedVar)
		{
			// Configure the tile components based on the tile mask set
			for(int i = (int)CTile.EType.INVALID + 1; i < (int)CTile.EType.MAX; ++i)
			{
				bool currentState = CUtility.GetMaskState(i, (int)m_RemoteTileTypeMask.Value);

				if(currentState)
					CreateTileComponent((CTile.EType)i);
				else
					RemoveTileComponent((CTile.EType)i);
			}
		}
	}
	
	private void CreateTileComponent(CTile.EType _TileType)
	{
		Type tileClassType = CTile.GetTileClassType(_TileType);

		if(GetTile(_TileType) != null)
			return;

		CTile tile = (CTile)gameObject.AddComponent(tileClassType);
		tile.m_TileInterface = this;
		tile.EventTileObjectChanged += OnTileObjectChange;
	}

	private void RemoveTileComponent(CTile.EType _TileType)
	{
		CTile tile = GetTile(_TileType);

		if(tile == null)
			return;

		tile.ReleaseTileObject();
		Destroy(tile);
	}

	private void Start()
	{
		m_Grid = CUtility.FindInParents<CGrid>(gameObject);
	}

	private void OnDestroy()
	{
		// Release tile objects
		foreach(CTile tile in GetComponents<CTile>())
			Destroy(tile);
	}

	public CTile GetTile(CTile.EType _TileType)
	{
		Type tileClassType = CTile.GetTileClassType(_TileType);
		return((CTile)gameObject.GetComponent(tileClassType));
	}

	[AServerOnly]
	public void InvokeTileCurrentMetaUpdate(CTile _Tile)
	{
		// Invoke all clients to set the current meta for this tile
		InvokeRpcAll("RemoteSetCurrentMeta", _Tile.m_TileType, _Tile.m_CurrentTileMeta.m_TileMask, 
		             _Tile.m_CurrentTileMeta.m_MetaType, _Tile.m_CurrentTileMeta.m_Rotations, _Tile.m_CurrentTileMeta.m_ModificationMask);
	}

	[AServerOnly]
	public void SyncAllTilesToPlayer(ulong _PlayerId)
	{
		foreach(CTile tile in GetComponents<CTile>())
		{
			// Invoke all clients to set the current meta for this tile
			InvokeRpc(_PlayerId, "RemoteSetCurrentMeta", tile.m_TileType, tile.m_CurrentTileMeta.m_TileMask, 
			          tile.m_CurrentTileMeta.m_MetaType, tile.m_CurrentTileMeta.m_Rotations, tile.m_CurrentTileMeta.m_ModificationMask);
		}
	}

	[ANetworkRpc]
	private void RemoteSetCurrentMeta(CTile.EType _TileType, int _TileMask, int _MetaType, int _Rotations, int _ModificationMask)
	{
		CTile tile = GetTile(_TileType);
		tile.m_CurrentTileMeta.m_TileMask = _TileMask;
		tile.m_CurrentTileMeta.m_MetaType = _MetaType;
		tile.m_CurrentTileMeta.m_Rotations = _Rotations;
		tile.m_CurrentTileMeta.m_ModificationMask = _ModificationMask;
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
			
			CTileInterface tile = m_Grid.GetTileInterface(possibleNeightbour);
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

	[ContextMenu("Update All Current Tile Meta Data")]
	[AServerOnly]
	public void UpdateAllCurrentTileMetaData()
	{
		// Update the tiles type mask
		UpdateTileTypeMask();

		// Update all tiles meta data
		foreach(CTile tile in GetComponents<CTile>())
		{
			tile.UpdateCurrentTileMetaData();
		}
	}

	[ContextMenu("Update All Tile Objects")]
	[AServerOnly]
	public void UpdateAllTileObjects()
	{
		// Update all tiles meta data
		foreach(CTile tile in GetComponents<CTile>())
		{
			tile.UpdateTileObject();
		}
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
	}

	[AServerOnly]
	public bool GetTileTypeState(CTile.EType _TileType)
	{
		return(m_TileTypes.Contains(_TileType));
	}

	[AServerOnly]
	public void UpdateTileTypeMask()
	{
		// Set the remote mask state
		int newMask = 0;
		foreach(CTile.EType tileType in m_TileTypes)
			CUtility.SetMaskState((int)tileType, true, ref newMask);
		bool changed = m_RemoteTileTypeMask.Value != (ushort)newMask;
		m_RemoteTileTypeMask.Value = (ushort)newMask;

		if(!changed)
			return;

		// Update all neighbour meta data if the mask has changed
		foreach(CNeighbour neighbour in m_NeighbourHood)
			neighbour.m_TileInterface.UpdateAllCurrentTileMetaData();
	}

	[AServerOnly]
	public void Clone(CTileInterface _From)
	{
		// Copy the tile types
		m_TileTypes.Clear();
		m_TileTypes.AddRange(_From.m_TileTypes);

		// Define the new tile type mask
		UpdateTileTypeMask();

		// Copy all tile meta data and exemption states
		foreach(CTile.EType otherTileType in _From.m_TileTypes)
		{
			CTile otherTile = _From.GetTile(otherTileType);
			CTile tile = GetTile(otherTileType);

			if(tile == null || otherTile == null)
			{
				Debug.LogError("Tile clone cannot happen as something went wrong with this tile type mask! Missing Type: " + otherTileType);
				continue;
			}

			// Copy over the current meta data
			tile.m_CurrentTileMeta = new CTile.CMeta(otherTile.m_CurrentTileMeta);

			// Copy the neighbour exemptions
			tile.m_NeighbourExemptions.Clear();
			tile.m_NeighbourExemptions.AddRange(otherTile.m_NeighbourExemptions);

			// Copy the tile modifications
			tile.m_Modifications.Clear();
			tile.m_Modifications.AddRange(otherTile.m_Modifications);
		}
	}

	private void OnTileObjectChange(CTile _Tile)
	{
		if(EventTileGeometryChanged != null)
			EventTileGeometryChanged(this);
	}
}
