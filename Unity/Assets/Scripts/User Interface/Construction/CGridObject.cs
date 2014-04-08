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


public abstract class CGridObject : MonoBehaviour
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public CGrid m_Grid = null;
	public TGridPoint m_GridPosition;

	public List<CNeighbour> m_NeighbourHood = new List<CNeighbour>();

	protected List<CNeighbour> s_AllNeighbours = new List<CNeighbour>(
		new CNeighbour[] 
		{
		new CNeighbour(new TGridPoint(0, 0, 1), EDirection.North),
		new CNeighbour(new TGridPoint(1, 0, 0), EDirection.East),
		new CNeighbour(new TGridPoint(0, 0, -1), EDirection.South),
		new CNeighbour(new TGridPoint(-1, 0, 0), EDirection.West),
		new CNeighbour(new TGridPoint(1, 0, 1), EDirection.NorthEast),
		new CNeighbour(new TGridPoint(1, 0, -1), EDirection.SouthEast),
		new CNeighbour(new TGridPoint(-1, 0, -1), EDirection.SouthWest),
		new CNeighbour(new TGridPoint(-1, 0, 1), EDirection.NorthWest),
		new CNeighbour(new TGridPoint(0, 1, 0), EDirection.Upper),
		new CNeighbour(new TGridPoint(0, -1, 0), EDirection.Lower),
	});
	
	// Member Properties
	public int x 
	{ 
		get { return m_GridPosition.x; }
		set { m_GridPosition.x = value; } 
	}

	public int y 
	{ 
		get { return m_GridPosition.y; }
		set { m_GridPosition.y = value; } 
	}

	public int z 
	{ 
		get { return m_GridPosition.z ; }
		set { m_GridPosition.z = value; } 
	}



	
	// Member Methods
	public override string ToString()
	{
		return(m_GridPosition.ToString());
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