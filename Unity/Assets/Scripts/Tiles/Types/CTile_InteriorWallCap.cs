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


public class CTile_InteriorWallCap : CTile 
{
	// Member Types
	public enum EType
	{
		INVALID = -1,
		
		None,
		Cap_1,
		Cap_2_1,
		Cap_2_2,
		Cap_3,
		Cap_4,
		
		MAX,
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
	static protected List<EDirection> s_RelevantDirections = new List<EDirection>();
	static protected Dictionary<int, CTile.CMeta> s_MetaDictionary = new Dictionary<int, CTile.CMeta>();
	
	
	// Member Properties
	public override Dictionary<int, CTile.CMeta> TileMetaDictionary
	{
		get { return(s_MetaDictionary); }
	}
	
	// Member Methods
	static CTile_InteriorWallCap()
	{
		// Fill relevant neighbours
		s_RelevantDirections.AddRange(new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West, 
			EDirection.NorthEast, EDirection.NorthWest, EDirection.SouthEast, EDirection.SouthWest });
		
		// Fill meta dictionary
		List<EDirection> possibleDirections;
		IEnumerable<IEnumerable<EDirection>> combinations;

		AddMetaEntry(EType.Cap_4,
		             new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });
		
		AddMetaEntry(EType.Cap_3,
		             new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West, EDirection.NorthWest });
		
		AddMetaEntry(EType.Cap_2_2,
		             new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West, EDirection.NorthWest, EDirection.SouthEast });
		
		possibleDirections = new List<EDirection>(new EDirection[]{ EDirection.West, EDirection.SouthWest, EDirection.NorthWest });
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
			
			AddMetaEntry(EType.Cap_2_1, directions.ToArray());
		}
		
		possibleDirections = new List<EDirection>(new EDirection[]{ EDirection.SouthEast, EDirection.SouthWest, EDirection.NorthWest, EDirection.South, EDirection.West });
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
			
			AddMetaEntry(EType.Cap_1, directions.ToArray());
		}  
		
		possibleDirections = new List<EDirection>(new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West, EDirection.SouthEast, EDirection.SouthWest, EDirection.NorthEast, EDirection.NorthWest });
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
				AddMetaEntry(EType.None, directions.ToArray());
		}
	}
	
	private static void AddMetaEntry(EType _Type, EDirection[] _MaskNeighbours)
	{
		CMeta meta = CreateMetaEntry((int)_Type, _MaskNeighbours);
		s_MetaDictionary.Add(meta.m_TileMask, meta);
	}
	
	private void Awake()
	{
		m_TileType = CTile.EType.InteriorWallCap;
	}

	protected override bool IsNeighbourRelevant(CNeighbour _Neighbour)
	{
		if(!s_RelevantDirections.Contains(_Neighbour.m_Direction))
			return(false);
		
		if(!_Neighbour.m_TileInterface.GetTileTypeState(CTile.EType.InteriorWall))
			return(false);
		
		if(CUtility.GetMaskState((int)_Neighbour.m_Direction, m_CurrentTileMeta.m_NeighbourMask))
			return(false);

		CTile internalWallTile = m_TileInterface.GetTile(CTile.EType.InteriorWall);
		if(internalWallTile != null && 
		   CUtility.GetMaskState((int)_Neighbour.m_Direction, internalWallTile.m_CurrentTileMeta.m_NeighbourMask))
			return(false);

		return(true);
	}
}