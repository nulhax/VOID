//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFacilityInfo.cs
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


public class CTurretNodeInterface : MonoBehaviour
{
	
	// Member Types

	
	// Member Delegates & Events
	
	
	// Member Fields
	private uint m_TurretNodeId = uint.MaxValue;
	private GameObject m_AttachedTurret = null;


	// Member Properties
	public uint TurretNodeId 
	{
		get{ return(m_TurretNodeId); }			
		set
		{
			if(m_TurretNodeId == uint.MaxValue)
			{
				m_TurretNodeId = value;
			}
			else
			{
				Debug.LogError("Cannot set turret node ID value twice!");
			}
		}			
	}

	public bool IsTurretInstalled
	{
		get{ return(m_AttachedTurret != null); }
	}

	public GameObject AttachedTurret
	{
		get{ return(m_AttachedTurret); }
	}

	
	// Member Methods
	public void SetAttachedTurret(GameObject _Turret)
	{
		m_AttachedTurret = _Turret;
	}
};
