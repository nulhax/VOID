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


[System.Serializable]
public class CNeighbour
{
	public CNeighbour(TGridPoint gridoffset, EDirection newdirection)
	{
		direction = newdirection;
		gridPositionOffset = gridoffset;
	}
	
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
	}
	
	public EDirection direction;
	public TGridPoint gridPositionOffset;
	public CTile tile;

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