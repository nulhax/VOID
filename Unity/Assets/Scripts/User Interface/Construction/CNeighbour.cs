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

	NorthWest,
	North, 
	NorthEast,
	East,
	SouthEast,
	South,
	SouthWest,
	West,

	MAX
}


[System.Serializable]
public class CNeighbour
{
	public CNeighbour(TGridPoint _GridPointOffset, EDirection _Newdirection)
	{
		m_Direction = _Newdirection;
		m_GridPointOffset = _GridPointOffset;
	}
	
	public EDirection m_Direction;
	public TGridPoint m_GridPointOffset;
	public CTile m_Tile;

	public static EDirection GetOppositeDirection(EDirection _Direction)
	{
		int direction = (int)_Direction - 4;

		if(direction < 0)
			direction += 8;

		return((EDirection)direction);
	}

	public static EDirection GetLeftDirectionNeighbour(EDirection _Direction)
	{
		int direction = (int)_Direction - 1;
		
		if(direction < 0)
			direction += 8;

		return((EDirection)direction);
	}

	public static EDirection GetRightDirectionNeighbour(EDirection _Direction)
	{
		int direction = (int)_Direction + 1;
		
		if(direction >= 8)
			direction -= 8;
		
		return((EDirection)direction);
	}
}