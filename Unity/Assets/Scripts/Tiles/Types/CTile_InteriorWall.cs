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


public class CTile_InteriorWall : CTile 
{
	// Member Types
	public enum EType
	{
		None,
		Corner, 
		Edge,
		End,
		Hall,
		Cell,
	}

	public enum EModifications
	{
		Door,
		Window,
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
	static CTile_InteriorWall()
	{
		// Fill relevant neighbours
		s_RelevantDirections.AddRange(new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });

		// Fill meta dictionary
		AddMetaEntry(EType.Cell,
		             new EDirection[]{ });
		
		AddMetaEntry(EType.None,
		             new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });
		
		AddMetaEntry(EType.Hall,
		             new EDirection[]{ EDirection.North, EDirection.South });
		
		AddMetaEntry(EType.Edge,
		             new EDirection[]{ EDirection.North, EDirection.West, EDirection.South });
		
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
		m_TileType = CTile.EType.Interior_Wall;

		EventTileObjectChanged += UpdateWallModifications;
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
		
		return(tileMask);
	}

	protected void UpdateWallModifications(CTile _Self)
	{
		List<CModification> modifications = GetModificationsFromMask(m_CurrentTileMeta.m_ModificationMask);


	}

	public List<CModification> GetModificationsFromMask(int _ModificationMask)
	{
		List<CModification> modifications = new List<CModification>();

		foreach(var mod in Enum.GetValues(typeof(EModifications)))
		{
			for(int i = (int)EDirection.INVALID + 1; i < (int)EDirection.MAX; ++i)
			{
				int value = ((int)mod * (int)EDirection.MAX) + i;
				if(CUtility.GetMaskState(value, _ModificationMask))
				{
					modifications.Add(new CModification((int)mod, (EDirection)i));
				}
			}
		}

		return(modifications);
	}
}