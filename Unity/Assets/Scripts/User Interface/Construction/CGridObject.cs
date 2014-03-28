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
	public TGridPoint m_Location;
	public CGrid m_Grid = null;
	
	// Member Properties
	public int x 
	{ 
		get { return m_Location.x; }
		set { m_Location.x = value; } 
	}

	public int y 
	{ 
		get { return m_Location.y; }
		set { m_Location.y = value; } 
	}

	public int z 
	{ 
		get { return m_Location.z ; }
		set { m_Location.z = value; } 
	}
	
	
	// Member Methods
	public CGridObject(int x, int y, int z, CGrid _Grid)
	: this(new TGridPoint(x, y, z), _Grid)
	{
	}
	
	public CGridObject(TGridPoint location, CGrid _Grid)
	{
		m_Grid = _Grid;
		m_Location = location;
	}
	
	public override string ToString()
	{
		return(m_Location.ToString());
	}
}

[System.Serializable]
public class TGridPoint
{
	public int x, y, z;
	public TGridPoint(int _x, int _y, int _z)
	{
		x = _x;
		y = _y;
		z = _z;
	}

	public TGridPoint(Vector3 _Pos)
	{
		x = Mathf.RoundToInt(_Pos.x);
		y = Mathf.RoundToInt(_Pos.y);
		z = Mathf.RoundToInt(_Pos.z);
	}

	public Vector3 ToVector
	{
		get { return(new Vector3((float)x, (float)y, (float)z)); }
	}
	
	public override string ToString()
	{
		return string.Format("[{0}, {1}, {2}]", x, y, z);
	}
}