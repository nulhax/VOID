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
	public Vector3 FirstPersonPosition
	{
		get{ return(new Vector3(m_fFirstPersonEquipOffsetX,m_fFirstPersonEquipOffsetY,m_fFirstPersonEquipOffsetZ));}
	}

	public Vector3 ThirdPersonPosition
	{
		get{ return(new Vector3(m_fThirdPersonEquipOffsetX,m_fThirdPersonEquipOffsetY,m_fThirdPersonEquipOffsetZ));}
	}

    public Vector3 ModifiedPosition
    {
		set { m_vThirdPersonModifiedOffset = value;}
		get { return(m_vThirdPersonModifiedOffset);}
    }

	public Vector3 RightHandRotation
	{
		set { m_vRightHandRotation = value;}
		get { return(m_vRightHandRotation);}
	}

	public Vector3 LeftHandRotation
	{
		set { m_vLeftHandRotation = value;}
		get { return(m_vLeftHandRotation);}
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

	//These fields dictate where the tool will be held relative to the player (first person model)
	public float m_fFirstPersonEquipOffsetX;
	public float m_fFirstPersonEquipOffsetY;
	public float m_fFirstPersonEquipOffsetZ;

    //These fields dictate where the tool will be held relative to the player (third person model)
	public float m_fThirdPersonEquipOffsetX;
	public float m_fThirdPersonEquipOffsetY;
	public float m_fThirdPersonEquipOffsetZ;

	//Rotation
	public Vector3 m_vRightHandRotation;
	public Vector3 m_vLeftHandRotation;

    //This vector will store the location where external scripts have placed the tool
    Vector3 m_vFirstPersonModifiedOffset;
	Vector3 m_vThirdPersonModifiedOffset;

    //These fields dictate how far the tool can roam, depending on where the player is aiming
	public float m_fVerticalDeviation;
	public float m_fLateralDeviation;
}
