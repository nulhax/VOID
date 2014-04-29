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


public class CTile_InteriorFloor : CTile 
{
	// Member Types
	public enum EType
	{
		INVALID = -1,
		
		None,
		Middle, 
		Corner, 
		Edge,
		End,
		Hall,
		Cell,
		
		MAX,
	}
	
	public enum EVariant
	{
		INVALID = -1,
		
		Floor,
		
		MAX
	}
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	static protected List<EDirection> s_RelevantDirections = new List<EDirection>();
	static protected Dictionary<int, CTile.CMeta> s_MetaDictionary = new Dictionary<int, CTile.CMeta>();
	
	
	// Member Properties
	public override Dictionary<int, CTile.CMeta> TileMetaDictionary
	{
		get { return(s_MetaDictionary); }
	}

	
	// Member Methods
	static CTile_InteriorFloor()
	{
		// Fill relevant neighbours
		s_RelevantDirections.AddRange(new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });
		
		// Fill meta dictionary
		AddMetaEntry(EType.Cell,
		             new EDirection[]{ });
		
		AddMetaEntry(EType.Middle,
		             new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });
		
		AddMetaEntry(EType.Hall,
		             new EDirection[]{ EDirection.North, EDirection.South });
		
		AddMetaEntry(EType.Edge,
		             new EDirection[]{ EDirection.North, EDirection.South, EDirection.West });
		
		AddMetaEntry(EType.Corner,
		             new EDirection[]{ EDirection.North, EDirection.West });
		
		AddMetaEntry(EType.End,
		             new EDirection[]{ EDirection.West });
	}
	
	private static void AddMetaEntry(EType _Type, EDirection[] _MaskNeighbours)
	{
		CMeta meta = CreateMetaEntry((int)_Type, _MaskNeighbours);
		s_MetaDictionary.Add(meta.m_TileMask, meta);
	}
	
	private void Awake()
	{
		m_TileType = CTile.EType.InteriorFloor;
	}

	protected override int DetirmineTileMask()
	{
		int tileMask = 0;
		
		// Define the tile mask given its relevant directions, relevant type and neighbour mask state.
		foreach(CNeighbour neighbour in m_TileInterface.m_NeighbourHood)
		{
			if(!s_RelevantDirections.Contains(neighbour.m_Direction))
				continue;
			
			if(!neighbour.m_TileInterface.GetTileTypeState(CTile.EType.InteriorFloor) &&
			   !neighbour.m_TileInterface.GetTileTypeState(CTile.EType.ExteriorWall))
				continue;
			
			if(GetNeighbourExemptionState(neighbour.m_Direction))
				continue;
			
			tileMask |= 1 << (int)neighbour.m_Direction;
		}
		
		return(tileMask);
	}
}