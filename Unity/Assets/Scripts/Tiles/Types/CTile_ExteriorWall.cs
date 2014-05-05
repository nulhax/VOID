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


public class CTile_ExteriorWall : CTile 
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

	public enum EModification
	{
		Default = -1,
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
	static CTile_ExteriorWall()
	{
		// Fill relevant neighbours
		s_RelevantDirections.AddRange(new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });

		// Fill meta dictionary
		AddMetaEntry(EType.None,
		             new EDirection[]{ });
		
		AddMetaEntry(EType.Cell,
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
		m_TileType = CTile.EType.Exterior_Wall;
		
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
		List<CModification> currentModifications = GetModificationsFromMask(m_CurrentTileMeta.m_ModificationMask);
		List<EDirection> defaultSides = new List<EDirection>(s_RelevantDirections);
		foreach(CModification mod in currentModifications)
			defaultSides.Remove(mod.m_Side);
		
		foreach(Transform child in m_TileObject.transform)
		{
			child.gameObject.SetActive(false);
			
			EDirection side = EDirection.INVALID;
			for(int i = (int)EDirection.INVALID + 1; i < (int)EDirection.MAX; ++i)
			{
				if(child.name.Contains(((EDirection)i).ToString()))
				{
					side = (EDirection)i; 
					break;
				}
			}
			
			EModification modType = EModification.Default;
			foreach(var mod in Enum.GetValues(typeof(EModification)))
			{
				if(child.name.Contains(((EModification)mod).ToString()))
				{
					modType = (EModification)mod; 
					break;
				}
			}
			
			if(currentModifications.Exists(m => m.m_Modification == (int)modType && m.m_Side == side))
			{
				child.gameObject.SetActive(true);
				continue;
			}
			
			if(defaultSides.Contains(side) && modType == EModification.Default)
			{
				child.gameObject.SetActive(true); 
				continue;
			}
		}
	}
	
	public List<CModification> GetModificationsFromMask(int _ModificationMask)
	{
		List<CModification> modifications = new List<CModification>();
		
		foreach(var mod in Enum.GetValues(typeof(EModification)))
		{
			for(int i = (int)EDirection.INVALID + 1; i < (int)EDirection.MAX; ++i)
			{
				int value = ((int)mod * (int)EDirection.MAX) + i;
				if(CUtility.GetMaskState(value, _ModificationMask))
				{
					modifications.Add(new CModification((int)mod, GetUnrotatedDirection((EDirection)i)));
				}
			}
		}
		
		return(modifications);
	}
}