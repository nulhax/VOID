//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CGridObject.cs
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


/* Implementation */

public enum EDirection 
{ 
	INVALID = -1,
	
	North, 
	East, 
	South,
	West, 
	NorthEast,
	SouthEast, 
	SouthWest,
	NorthWest,
	Upper,
	Lower,

	MAX
}


[System.Serializable]
public class CNeighbour
{
	public CNeighbour(TGridPoint _GridPointOffset, EDirection _Newdirection)
	{
		m_WorldDirection = _Newdirection;
		m_GridPointOffset = _GridPointOffset;
	}
	
	public EDirection m_WorldDirection;
	public TGridPoint m_GridPointOffset;
	public CTile m_Tile;

	public static EDirection GetOppositeDirection(EDirection _Direction)
	{
		EDirection dir = EDirection.INVALID;

		switch(_Direction)
		{
		case EDirection.North: dir = EDirection.South; break;
		case EDirection.NorthWest: dir = EDirection.SouthEast; break;
		case EDirection.West: dir = EDirection.East; break;
		case EDirection.SouthWest: dir = EDirection.NorthEast; break;
		case EDirection.South: dir = EDirection.North; break;
		case EDirection.SouthEast: dir = EDirection.NorthWest; break;
		case EDirection.East: dir = EDirection.West; break;
		case EDirection.NorthEast: dir = EDirection.SouthWest; break;
		}

		return(dir);
	}
	
	public static EDirection GetLocalDirection(EDirection _LocalNorth, EDirection _Direction)
	{
		EDirection dir = EDirection.INVALID;
		
		switch(_LocalNorth)
		{
		case EDirection.North: 
			switch(_Direction)
			{
			case EDirection.North: dir = EDirection.North; break;
			case EDirection.NorthWest: dir = EDirection.NorthWest; break;
			case EDirection.West: dir = EDirection.West; break;
			case EDirection.SouthWest: dir = EDirection.SouthWest; break;
			case EDirection.South: dir = EDirection.South; break;
			case EDirection.SouthEast: dir = EDirection.SouthEast; break;
			case EDirection.East: dir = EDirection.East; break;
			case EDirection.NorthEast: dir = EDirection.NorthEast; break;
			}
			break;

		case EDirection.NorthEast: 
			switch(_Direction)
			{
			case EDirection.North: dir = EDirection.NorthEast; break;
			case EDirection.NorthWest: dir = EDirection.North; break;
			case EDirection.West: dir = EDirection.NorthWest; break;
			case EDirection.SouthWest: dir = EDirection.South; break;
			case EDirection.South: dir = EDirection.SouthWest; break;
			case EDirection.SouthEast: dir = EDirection.South; break;
			case EDirection.East: dir = EDirection.SouthEast; break;
			case EDirection.NorthEast: dir = EDirection.East; break;
			}
			break;

		case EDirection.East: 
			switch(_Direction)
			{
			case EDirection.North: dir = EDirection.East; break;
			case EDirection.NorthWest: dir = EDirection.NorthEast; break;
			case EDirection.West: dir = EDirection.North; break;
			case EDirection.SouthWest: dir = EDirection.NorthWest; break;
			case EDirection.South: dir = EDirection.West; break;
			case EDirection.SouthEast: dir = EDirection.SouthWest; break;
			case EDirection.East: dir = EDirection.South; break;
			case EDirection.NorthEast: dir = EDirection.SouthEast; break;
			}
			break;

		case EDirection.SouthEast: 
			switch(_Direction)
			{
			case EDirection.North: dir = EDirection.SouthEast; break;
			case EDirection.NorthWest: dir = EDirection.East; break;
			case EDirection.West: dir = EDirection.NorthEast; break;
			case EDirection.SouthWest: dir = EDirection.North; break;
			case EDirection.South: dir = EDirection.NorthWest; break;
			case EDirection.SouthEast: dir = EDirection.West; break;
			case EDirection.East: dir = EDirection.SouthWest; break;
			case EDirection.NorthEast: dir = EDirection.South; break;
			}
			break;

		case EDirection.South: 
			switch(_Direction)
			{
			case EDirection.North: dir = EDirection.South; break;
			case EDirection.NorthWest: dir = EDirection.SouthEast; break;
			case EDirection.West: dir = EDirection.East; break;
			case EDirection.SouthWest: dir = EDirection.NorthEast; break;
			case EDirection.South: dir = EDirection.North; break;
			case EDirection.SouthEast: dir = EDirection.NorthWest; break;
			case EDirection.East: dir = EDirection.West; break;
			case EDirection.NorthEast: dir = EDirection.SouthWest; break;
			}
			break;

		case EDirection.SouthWest:
			switch(_Direction)
			{
			case EDirection.North: dir = EDirection.SouthWest; break;
			case EDirection.NorthWest: dir = EDirection.South; break;
			case EDirection.West: dir = EDirection.SouthEast; break;
			case EDirection.SouthWest: dir = EDirection.East; break;
			case EDirection.South: dir = EDirection.NorthEast; break;
			case EDirection.SouthEast: dir = EDirection.North; break;
			case EDirection.East: dir = EDirection.NorthWest; break;
			case EDirection.NorthEast: dir = EDirection.West; break;
			}
			break;

		case EDirection.West:
			switch(_Direction)
			{
			case EDirection.North: dir = EDirection.West; break;
			case EDirection.NorthWest: dir = EDirection.SouthWest; break;
			case EDirection.West: dir = EDirection.South; break;
			case EDirection.SouthWest: dir = EDirection.SouthEast; break;
			case EDirection.South: dir = EDirection.East; break;
			case EDirection.SouthEast: dir = EDirection.NorthEast; break;
			case EDirection.East: dir = EDirection.North; break;
			case EDirection.NorthEast: dir = EDirection.NorthWest; break;
			}
			break;

		case EDirection.NorthWest:
			switch(_Direction)
			{
			case EDirection.North: dir = EDirection.NorthWest; break;
			case EDirection.NorthWest: dir = EDirection.West; break;
			case EDirection.West: dir = EDirection.SouthWest; break;
			case EDirection.SouthWest: dir = EDirection.South; break;
			case EDirection.South: dir = EDirection.SouthEast; break;
			case EDirection.SouthEast: dir = EDirection.East; break;
			case EDirection.East: dir = EDirection.NorthEast; break;
			case EDirection.NorthEast: dir = EDirection.North; break;
			}
			break;	
		}
		return(dir);
	}

	public static EDirection GetLeftDirectionNeighbour(EDirection _Direction)
	{
		EDirection dir = EDirection.INVALID;

		switch(_Direction)
		{
		case EDirection.North: dir = EDirection.NorthWest; break;
		case EDirection.NorthWest: dir = EDirection.West; break;
		case EDirection.West: dir = EDirection.SouthWest; break;
		case EDirection.SouthWest: dir = EDirection.South; break;
		case EDirection.South: dir = EDirection.SouthEast; break;
		case EDirection.SouthEast: dir = EDirection.East; break;
		case EDirection.East: dir = EDirection.NorthEast; break;
		case EDirection.NorthEast: dir = EDirection.North; break;
		}
		return(dir);
	}

	public static EDirection GetRightDirectionNeighbour(EDirection _Direction)
	{
		EDirection dir = EDirection.INVALID;
		
		switch(_Direction)
		{
		case EDirection.North: dir = EDirection.NorthEast; break;
		case EDirection.NorthEast: dir = EDirection.East; break;
		case EDirection.East: dir = EDirection.SouthEast; break;
		case EDirection.SouthEast: dir = EDirection.South; break;
		case EDirection.South: dir = EDirection.SouthWest; break;
		case EDirection.SouthWest: dir = EDirection.West; break;
		case EDirection.West: dir = EDirection.NorthWest; break;
		case EDirection.NorthWest: dir = EDirection.North; break;
		}
		return(dir);
	}
}