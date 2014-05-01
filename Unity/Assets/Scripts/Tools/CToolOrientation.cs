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
		get{ return(new Vector3(m_fEquipOffsetX,m_fEquipOffsetY,m_fEquipOffsetZ));}
	}

    public Vector3 ModifiedPosition
    {
        set { m_vModifiedOffset = value;}
        get { return(m_vModifiedOffset);}
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

    //These fields dictate where the tool will be held relative to the player
	public float m_fEquipOffsetX;
	public float m_fEquipOffsetY;
	public float m_fEquipOffsetZ;

    //This vector will store the location where external scripts have placed the tool
    Vector3 m_vModifiedOffset;

    //These fields dictate how far the tool can roam, depending on where the player is aiming
	public float m_fVerticalDeviation;
	public float m_fLateralDeviation;
}
