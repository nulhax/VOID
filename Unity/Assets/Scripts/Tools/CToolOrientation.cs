//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;

/* Implementation */

public class CToolOrientation : MonoBehaviour 
{
	// Member Types
	
	// Member Delegates & Events
	
	// Member Properties
	public Vector3 Position
	{
		get{ return(new Vector3(m_fLocalOffsetX,m_fLocalOffsetY,m_fLocalOffsetZ));}
	}

	public float VerticalDeviation
	{
		get{ return(m_fVerticalDeviation); }
	}

	public float LateralDeviation
	{
		get{ return(m_fLateralDeviation); }
	}

	// Member Fields
	public float m_fLocalOffsetX;
	public float m_fLocalOffsetY;
	public float m_fLocalOffsetZ;
	public float m_fVerticalDeviation;
	public float m_fLateralDeviation;
}
