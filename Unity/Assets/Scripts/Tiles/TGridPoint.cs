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