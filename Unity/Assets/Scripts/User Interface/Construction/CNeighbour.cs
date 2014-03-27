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


public abstract class CGridObject
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public Point Location;


	// Member Properties
	public int X { get { return Location.X; } }
	public int Y { get { return Location.Y; } }
	public int Z { get { return Location.Z; } }


	// Member Methods
	public CGridObject(int x, int y, int z)
		: this(new Point(x, y, z))
	{
	}
	
	public CGridObject(Point location)
	{
		Location = location;
	}
	
	public override string ToString()
	{
		return string.Format("[{0}, {1}, {2}]", X, Y, Z);
	}
}

[System.Serializable]
public class Point
{
	public int X, Y, Z;
	public Point(int x, int y, int z)
	{
		X = x;
		Y = y;
		Z = z;
	}
}

[System.Serializable]
public class CNeighbour
{
	public CNeighbour(Point gridoffset, Direction newdirection)
	{
		direction = newdirection;
		gridPositionOffset = gridoffset;
	}
	
	public enum Direction 
	{ 
		INVALID = -1,
		
		North, 
		Northeast, 
		East, 
		Southeast, 
		South, 
		Southwest, 
		West, 
		Northwest 
	}
	public Direction direction;
	
	public Point gridPositionOffset;
	
	public CTile tile;
}