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


public class CTile_InteriorCeiling : CTile 
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
	
	public enum EVariant
	{
		INVALID = -1,
		
		Wall,
		Door,
		Window,
		
		MAX
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

	
	// Member Methods
	static CTile_InteriorCeiling()
	{
		// Fill relevant neighbours
		s_RelevantDirections.AddRange(new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });

		// Fill meta dictionary
		AddMetaEntry(EType.None,
		             new EDirection[]{ });
		
		AddMetaEntry(EType.Middle,
		             new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });
		
		AddMetaEntry(EType.Hall,
		             new EDirection[]{ EDirection.East, EDirection.West });
		
		AddMetaEntry(EType.Edge,
		             new EDirection[]{ EDirection.East });
		
		AddMetaEntry(EType.Corner,
		             new EDirection[]{ EDirection.East, EDirection.South });
		
		AddMetaEntry(EType.End,
		             new EDirection[]{ EDirection.East, EDirection.North, EDirection.South });
	}
	
	private static void AddMetaEntry(EType _Type, EDirection[] _MaskNeighbours)
	{
		CMeta meta = CreateMetaEntry((int)_Type, _MaskNeighbours);
		s_MetaDictionary.Add(meta.m_TileMask, meta);
	}
	
	private void Awake()
	{
		m_TileType = CTile.EType.Interior_Ceiling;
	}

	protected override int DetirmineTileMask()
	{
		int tileMask = 0;

		// Define the tile mask given its relevant directions, relevant type and neighbour mask state.
		foreach(CNeighbour neighbour in m_TileInterface.m_NeighbourHood)
		{
			if(!s_RelevantDirections.Contains(neighbour.m_Direction))
				continue;
			
			if(!neighbour.m_TileInterface.GetTileTypeState(CTile.EType.Interior_Ceiling) &&
			   !neighbour.m_TileInterface.GetTileTypeState(CTile.EType.Exterior_Wall))
				continue;
			
			if(GetNeighbourExemptionState(neighbour.m_Direction))
				continue;

			tileMask |= 1 << (int)neighbour.m_Direction;
		}
		
		return(tileMask);
	}
}