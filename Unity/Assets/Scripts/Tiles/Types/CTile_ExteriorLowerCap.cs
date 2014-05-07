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


public class CTile_ExteriorLowerCap : CTile 
{
	// Member Types
	public enum EType
	{
		None,
		Cap_1,
		Cap_2_1,
		Cap_2_2,
		Cap_3,
		Cap_4,
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
	static CTile_ExteriorLowerCap()
	{
		// Fill relevant neighbours
		s_RelevantDirections.AddRange(new EDirection[]{ EDirection.NorthEast, EDirection.NorthWest, EDirection.SouthEast, EDirection.SouthWest });
		
		// Fill meta dictionary
		List<EDirection> possibleDirections;
		IEnumerable<IEnumerable<EDirection>> combinations;
		
		AddMetaEntry(EType.Cap_4,
		             new EDirection[]{ EDirection.NorthEast, EDirection.NorthWest, EDirection.SouthEast, EDirection.SouthWest });
		
		AddMetaEntry(EType.Cap_3,
		             new EDirection[]{ EDirection.NorthEast, EDirection.SouthEast, EDirection.SouthWest });
		
		AddMetaEntry(EType.Cap_2_2,
		             new EDirection[]{ EDirection.NorthEast, EDirection.SouthWest });
		
		AddMetaEntry(EType.Cap_2_1, 
		             new EDirection[]{ EDirection.NorthEast, EDirection.SouthEast });
		
		AddMetaEntry(EType.Cap_1, 
		             new EDirection[]{ EDirection.NorthEast });
		
		AddMetaEntry(EType.None, 
		             new EDirection[]{ });
	}
	
	private static void AddMetaEntry(EType _Type, EDirection[] _MaskNeighbours)
	{
		CMeta meta = CreateMetaEntry((int)_Type, _MaskNeighbours);
		s_MetaDictionary.Add(meta.m_TileMask, meta);
	}
	
	private void Awake()
	{
		m_TileType = CTile.EType.Exterior_Lower_Inverse_Corner;
	}
	
	protected override int DetirmineTileMask()
	{
		int tileMask = 0;

		// Get lower tile interface
		CGridPoint upperTilePos = new CGridPoint(m_TileInterface.m_GridPosition.ToVector + Vector3.up);
		CTileInterface upperTileInterface = m_TileInterface.m_Grid.GetTileInterface(upperTilePos);
		
		if(upperTileInterface == null)
			return(tileMask);
		
		// Define the tile mask given its relevant directions, relevant type and neighbour mask state.
		foreach(CNeighbour neighbour in upperTileInterface.m_NeighbourHood)
		{
			if(!s_RelevantDirections.Contains(neighbour.m_Direction))
				continue;
			
			if(GetNeighbourExemptionState(neighbour.m_Direction))
				continue;
			
			bool neighbourCheck = NeighbourCheck(neighbour);

			if(!neighbourCheck)
				continue;
			
			tileMask |= 1 << (int)neighbour.m_Direction;
		}
		
		return(tileMask);
	}
	
	private bool NeighbourCheck(CNeighbour _Neighbour)
	{
		bool diagonalExisits = _Neighbour.m_TileInterface.GetTileTypeState(CTile.EType.Exterior_Wall);
		
		bool leftExisits = _Neighbour.m_TileInterface.m_NeighbourHood.Exists(
			n => n.m_TileInterface.GetTileTypeState(CTile.EType.Exterior_Wall) &&
			n.m_Direction == CNeighbour.GetLeftDirectionNeighbour(CNeighbour.GetOppositeDirection(_Neighbour.m_Direction)));
		
		bool rightExisits = _Neighbour.m_TileInterface.m_NeighbourHood.Exists(
			n => n.m_TileInterface.GetTileTypeState(CTile.EType.Exterior_Wall) &&
			n.m_Direction == CNeighbour.GetRightDirectionNeighbour(CNeighbour.GetOppositeDirection(_Neighbour.m_Direction)));
		
		return(leftExisits & rightExisits && !diagonalExisits);
	}
}