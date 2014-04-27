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


public class CTile_ExternalWallCap : CTile 
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
	static protected CTile.EType s_RelevantType = new CTile.EType();
	static protected List<EDirection> s_RelevantDirections = new List<EDirection>();
	
	static protected Dictionary<int, CTile.CMeta> s_MetaDictionary = new Dictionary<int, CTile.CMeta>();
	
	
	// Member Properties
	public override CTile.EType RelevantType
	{
		get { return(s_RelevantType); }
	}
	
	public override List<EDirection> RelevantDirections
	{
		get { return(s_RelevantDirections); }
	}
	
	public override Dictionary<int, CTile.CMeta> TileMetaDictionary
	{
		get { return(s_MetaDictionary); }
	}
	
	// Member Methods
	static CTile_ExternalWallCap()
	{
		// Set relevant type
		s_RelevantType = CTile.EType.InteriorWall;

		// Fill relevant neighbours
		s_RelevantDirections.AddRange(new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West, 
			EDirection.NorthEast, EDirection.NorthWest, EDirection.SouthEast, EDirection.SouthWest });

		// Fill meta dictionary
		List<EDirection> possibleDirections;
		IEnumerable<IEnumerable<EDirection>> combinations;

		AddMetaEntry(EType.Cap_4,
		             new EDirection[]{ EDirection.NorthWest, EDirection.NorthEast, EDirection.SouthEast, EDirection.SouthWest });
		
		AddMetaEntry(EType.Cap_3,
		             new EDirection[]{ EDirection.NorthEast, EDirection.SouthEast, EDirection.SouthWest });
		
		AddMetaEntry(EType.Cap_2_2,
		             new EDirection[]{ EDirection.NorthEast, EDirection.SouthWest });
		
		possibleDirections = new List<EDirection>(new EDirection[]{ EDirection.West, EDirection.SouthWest, EDirection.NorthWest });
		combinations = CUtility.GetPowerSet(possibleDirections);
		foreach(var collection in combinations)  
		{  
			List<EDirection> directions = collection.ToList();
			directions.Add(EDirection.NorthEast);
			directions.Add(EDirection.SouthEast);
			
			if(directions.Contains(EDirection.NorthWest))
				if(!directions.Contains(EDirection.West))
					continue;
			
			if(directions.Contains(EDirection.SouthWest))
				if(!directions.Contains(EDirection.West))
					continue;
			
			AddMetaEntry(EType.Cap_2_1, directions.ToArray());
		}  
		
		possibleDirections = new List<EDirection>(new EDirection[]{ EDirection.West, EDirection.South, EDirection.SouthEast, EDirection.SouthWest, EDirection.NorthWest });
		combinations = CUtility.GetPowerSet(possibleDirections);
		foreach(var collection in combinations)  
		{  
			List<EDirection> directions = collection.ToList();
			directions.Add(EDirection.NorthEast);
			
			if(directions.Contains(EDirection.NorthWest))
				if(!directions.Contains(EDirection.West))
					continue;
			
			if(directions.Contains(EDirection.SouthEast))
				if(!directions.Contains(EDirection.South))
					continue;
			
			if(directions.Contains(EDirection.SouthWest))
				if(!directions.Contains(EDirection.West) && !directions.Contains(EDirection.South))
					continue;
			
			AddMetaEntry(EType.Cap_1, directions.ToArray());
		} 
		
		possibleDirections = new List<EDirection>(new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West, EDirection.SouthEast, EDirection.SouthWest, EDirection.NorthEast, EDirection.NorthWest });
		combinations = CUtility.GetPowerSet(possibleDirections);
		foreach(var collection in combinations)  
		{  
			List<EDirection> directions = collection.ToList(); 
			bool matchFound = false;
			
			if(directions.Contains(EDirection.NorthWest) && !matchFound)
				if(!directions.Contains(EDirection.North) && !directions.Contains(EDirection.West))
					matchFound = true;
			
			if(directions.Contains(EDirection.NorthEast) && !matchFound)
				if(!directions.Contains(EDirection.North) && !directions.Contains(EDirection.East))
					matchFound = true;
			
			if(directions.Contains(EDirection.SouthEast) && !matchFound)
				if(!directions.Contains(EDirection.South) && !directions.Contains(EDirection.East))
					matchFound = true;
			
			if(directions.Contains(EDirection.SouthWest) && !matchFound)
				if(!directions.Contains(EDirection.South) && !directions.Contains(EDirection.West))
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
		m_TileType = CTile.EType.ExteriorWallCap;
	}
}