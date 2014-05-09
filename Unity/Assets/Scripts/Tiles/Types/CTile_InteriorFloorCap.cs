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


public class CTile_InteriorFloorCap : CTile 
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

	public override List<EDirection> RelevantDirections
	{
		get { return(s_RelevantDirections); }
	}
	
	// Member Methods
	static CTile_InteriorFloorCap()
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
		m_TileType = CTile.EType.Interior_Floor_Inverse_Corner;
	}
	
	protected override int DetirmineTileMask()
	{
		int tileMask = 0;
		
		// Define the tile mask given its relevant directions, relevant type and neighbour mask state.
		foreach(CNeighbour neighbour in m_TileInterface.m_NeighbourHood)
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
		CTile interiorFloor = m_TileInterface.GetTile(CTile.EType.Interior_Floor);
		
		if(interiorFloor == null)
			return(false);

		EDirection leftDir = CNeighbour.GetLeftDirectionNeighbour(_Neighbour.m_Direction);
		EDirection rightDir = CNeighbour.GetRightDirectionNeighbour(_Neighbour.m_Direction);

		bool leftExemption = interiorFloor.GetNeighbourExemptionState(leftDir);
		bool rightExemption = interiorFloor.GetNeighbourExemptionState(rightDir);

		CNeighbour leftNeighbour = m_TileInterface.m_NeighbourHood.Find(n => n.m_Direction == leftDir);
		CNeighbour rightNeighbour = m_TileInterface.m_NeighbourHood.Find(n => n.m_Direction == rightDir);

		bool returnState = leftExemption && rightExemption;
		if(returnState && leftNeighbour != null && rightNeighbour != null)
		{
			CTile neighbourInteriorFloor = _Neighbour.m_TileInterface.GetTile(CTile.EType.Interior_Floor);

			if(neighbourInteriorFloor != null)
			{
				leftExemption = neighbourInteriorFloor.GetNeighbourExemptionState(CNeighbour.GetOppositeDirection(leftDir));
				rightExemption = neighbourInteriorFloor.GetNeighbourExemptionState(CNeighbour.GetOppositeDirection(rightDir));

				returnState = !(leftExemption && rightExemption);
			}
		}

		return(returnState);
	}
}