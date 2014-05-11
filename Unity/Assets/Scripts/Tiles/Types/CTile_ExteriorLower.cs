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


public class CTile_ExteriorLower : CTile 
{
	// Member Types
	public enum EType
	{
		None,
		Middle, 
		Corner, 
		Edge,
		End,
		Hall,
		Cell,
	}

	
	// Member Delegates & Events
	
	
	// Member Fields
	static public List<EDirection> s_RelevantDirections = new List<EDirection>();
	static protected Dictionary<int, CTile.CMeta> s_MetaDictionary = new Dictionary<int, CTile.CMeta>();
	
	
	// Member Properties
	public override Dictionary<int, CTile.CMeta> TileMetaDictionary
	{
		get { return(s_MetaDictionary); }
	}

	public override List<EDirection> RelevantDirections
	{
		get { return(s_RelevantDirections); }
	}
	
	// Member Methods
	static CTile_ExteriorLower()
	{
		// Fill relevant neighbours
		s_RelevantDirections.AddRange(new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });

		// Fill meta dictionary
		AddMetaEntry(EType.None,
		             new EDirection[]{ });
		
		AddMetaEntry(EType.Middle,
		             new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });
		
		AddMetaEntry(EType.Hall,
		             new EDirection[]{ EDirection.West, EDirection.East });
		
		AddMetaEntry(EType.Edge,
		             new EDirection[]{ EDirection.East });
		
		AddMetaEntry(EType.Corner,
		             new EDirection[]{ EDirection.South, EDirection.East });
		
		AddMetaEntry(EType.End,
		             new EDirection[]{ EDirection.North, EDirection.East, EDirection.South });
	}
	
	private static void AddMetaEntry(EType _Type, EDirection[] _MaskNeighbours)
	{
		CMeta meta = CreateMetaEntry((int)_Type, _MaskNeighbours);
		s_MetaDictionary.Add(meta.m_TileMask, meta);
	}
	
	private void Awake()
	{
		m_TileType = CTile.EType.Exterior_Lower;
	}

	protected override int DetirmineTileMask()
	{
		int tileMask = 0;

		// Define the tile mask given its relevant directions, relevant type and neighbour mask state.
		foreach(CNeighbour neighbour in m_TileInterface.m_NeighbourHood)
		{
			if(!s_RelevantDirections.Contains(neighbour.m_Direction))
				continue;
			
			if(!neighbour.m_TileInterface.GetTileTypeState(CTile.EType.Interior_Wall))
				continue;
			
			if(GetNeighbourExemptionState(neighbour.m_Direction))
				continue;
			
			tileMask |= 1 << (int)neighbour.m_Direction;
		}

		// Get upper tile interface
		CGridPoint upperTilePos = new CGridPoint(m_TileInterface.m_GridPosition.ToVector + Vector3.up);
		CTileInterface upperTileInterface = m_TileInterface.m_Grid.GetTileInterface(upperTilePos);

		if(upperTileInterface == null)
			return(tileMask);

		if(upperTileInterface.GetTileTypeState(CTile.EType.Interior_Wall))
		{
			foreach(EDirection dir in s_RelevantDirections)
				tileMask |= 1 << (int)dir;
			return(tileMask);
		}

		// Define the tile mask given its relevant directions, relevant type and neighbour mask state.
		foreach(CNeighbour neighbour in upperTileInterface.m_NeighbourHood)
		{
			if(!s_RelevantDirections.Contains(neighbour.m_Direction))
				continue;
			
			if(!neighbour.m_TileInterface.GetTileTypeState(CTile.EType.Interior_Wall))
				continue;
			
			if(GetNeighbourExemptionState(neighbour.m_Direction))
				continue;
			
			tileMask |= 1 << (int)neighbour.m_Direction;
		}
		
		return(tileMask);
	}
}