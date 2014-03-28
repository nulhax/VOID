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
}