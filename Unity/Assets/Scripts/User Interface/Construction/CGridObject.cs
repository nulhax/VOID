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
	public Point m_Location;
	
	
	// Member Properties
	public int X { get { return m_Location.x; } }
	public int Y { get { return m_Location.y; } }
	public int Z { get { return m_Location.z; } }
	
	
	// Member Methods
	public CGridObject(int x, int y, int z)
		: this(new Point(x, y, z))
	{
	}
	
	public CGridObject(Point location)
	{
		m_Location = location;
	}
	
	public override string ToString()
	{
		return(GridHash(m_Location));
	}

	static public string GridHash(Point _Location)
	{
		return string.Format("[{0}, {1}, {2}]", _Location.x, _Location.y, _Location.z);
	}
}