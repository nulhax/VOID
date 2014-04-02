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
	public TGridPoint m_Location;
	public EDirection m_LocalNorth = EDirection.North;

	public List<CNeighbour> m_NeighbourHood = new List<CNeighbour>();
	public List<CNeighbour> m_ExemptNeighbours = new List<CNeighbour>();

	private List<CNeighbour> s_AllNeighbours = new List<CNeighbour>(
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
	public override string ToString()
	{
		return(m_Location.ToString());
	}

	public void FindNeighbours()
	{
		m_NeighbourHood.Clear();
		
		foreach(CNeighbour pn in s_AllNeighbours) 
		{
			TGridPoint possibleNeightbour = new TGridPoint(x + pn.m_GridPointOffset.x, 
			                                               y + pn.m_GridPointOffset.y, 
			                                               z + pn.m_GridPointOffset.z);
			
			CTile tile = m_Grid.GetTile(possibleNeightbour);
			if(tile != null)
			{
				CNeighbour newNeighbour = new CNeighbour(pn.m_GridPointOffset, pn.m_WorldDirection);
				newNeighbour.m_Tile = tile;
				
				m_NeighbourHood.Add(newNeighbour);
			}
		}
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