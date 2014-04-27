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
	static CTile_InteriorCeiling()
	{
		// Set relevant type
		s_RelevantType = CTile.EType.InteriorCeiling;

		// Fill relevant neighbours
		s_RelevantDirections.AddRange(new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });

		// Fill meta dictionary
		AddMetaEntry(EType.Cell,
		             new EDirection[]{ });
		
		AddMetaEntry(EType.Middle,
		             new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });
		
		AddMetaEntry(EType.Hall,
		             new EDirection[]{ EDirection.East, EDirection.West });
		
		AddMetaEntry(EType.Edge,
		             new EDirection[]{ EDirection.North, EDirection.South, EDirection.East });
		
		AddMetaEntry(EType.Corner,
		             new EDirection[]{ EDirection.South, EDirection.East });
		
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
		m_TileType = CTile.EType.InteriorCeiling;
	}
}