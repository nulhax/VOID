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


public class CTileWallInt : CTile 
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
	static protected Dictionary<int, CTile.TMeta> s_MetaDictionary = new Dictionary<int, CTile.TMeta>();

	
	// Member Properties

	
	// Member Methods
	static CTileWallInt()
	{
		AddMetaEntry(CTileWallInt.EType.Cell,
		             new EDirection[]{ });
		
		AddMetaEntry(CTileWallInt.EType.None,
		             new EDirection[]{ EDirection.North, EDirection.East, EDirection.South, EDirection.West });
		
		AddMetaEntry(CTileWallInt.EType.Hall,
		             new EDirection[]{ EDirection.North, EDirection.South });
		
		AddMetaEntry(CTileWallInt.EType.Edge,
		             new EDirection[]{ EDirection.North, EDirection.West, EDirection.South });
		
		AddMetaEntry(CTileWallInt.EType.Corner,
		             new EDirection[]{ EDirection.North, EDirection.West });
		
		AddMetaEntry(CTileWallInt.EType.End,
		             new EDirection[]{ EDirection.West });
	}

	private void Awake()
	{
		m_TileType = CTile.EType.Wall_Int;
	}

	private static void AddMetaEntry(CTile.EType _Type, EDirection[] _MaskNeighbours)
	{
		CTile.TMeta meta = CreateMetaEntry((int)_Type, _MaskNeighbours);
		s_MetaDictionary.Add(meta.m_IdentifierMask, meta);
	}

	public override void UpdateCurrentMeta()
	{
		int tileMask = 0;
		
		// Find the meta identifier for all tile types based on neighbourhood
		foreach(CNeighbour neighbour in m_TileRoot.m_NeighbourHood)
		{
			// Main Tiles only care about N, E, S, W neighbours
			if(neighbour.m_Direction == EDirection.North ||
			   neighbour.m_Direction == EDirection.East ||
			   neighbour.m_Direction == EDirection.South ||
			   neighbour.m_Direction == EDirection.West)
			{
				if(neighbour.m_Tile.GetTileTypeState(CTile.EType.Wall_Int))
				{
					if(!m_NeighbourExemptions.Contains(neighbour.m_Direction))
						tileMask |= 1 << (int)neighbour.m_Direction;
				}

			}
		}
		
		// Update the tile meta info
		bool metaChanged = UpdateTileMetaInfo(tileMask);
		
		// If any meta data changed
		if(metaChanged)
		{
			// Invoke neighbours to update their meta data
			foreach(CNeighbour neighbour in m_TileRoot.m_NeighbourHood)
			{
				neighbour.m_Tile.UpdateTileMetaData();
			}
			
			// Set dirty to update tile types next update
			m_IsDirty = true;
		}
	}

	private bool UpdateTileMetaInfo(int _TileMask)
	{	
		// Find the tile mask out of possible 4 rotations
		for(int i = 0; i < 4; ++i)
		{
			int tileMask = 0;
			for(int j = 0; j < (int)EDirection.MAX; ++j)
			{
				// Define a new mask based on neighbours that exist in the mask
				if((_TileMask & (1 << j)) != 0)
				{
					// "Rotate" the mask by 90*, 2 bits ac for each rotation
					int dirMask = j - (2 * i);
					if(dirMask < 0)
						dirMask += 8;
					
					tileMask |= 1 << dirMask;
				}
			}
			
			// Return the result if it is found with the correct rotation
			if(s_MetaDictionary.ContainsKey(tileMask))
			{
				CTile.TMeta newTileMeta = s_MetaDictionary[tileMask];
				newTileMeta.m_Rotations = i;
				newTileMeta.m_Variant = m_CurrentTileMeta.m_Variant;
				newTileMeta.m_NeighbourExemptionMask = m_CurrentTileMeta.m_NeighbourExemptionMask;

				// Update the tile meta
				m_CurrentTileMeta = newTileMeta;
				
				// Return true if there is a change in meta data
				return(!m_ActiveTileMeta.Equals(m_CurrentTileMeta));
			}
		}
		
		Debug.LogWarning("Tile Meta data wasn't found for: " + m_TileType + " Mask: " + _TileMask);
		
		return(false);
	}
}