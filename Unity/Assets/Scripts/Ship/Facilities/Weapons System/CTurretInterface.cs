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

[RequireComponent(typeof(CTurretController))]
[RequireComponent(typeof(CNetworkView))]
public class CTurretInterface : CNetworkMonoBehaviour
{
	
	// Member Types
	public enum ETurretType
	{
		INVALID,
		
		Laser,
	}

	
	// Member Delegates & Events
	
	
	// Member Fields
	private CNetworkVar<uint> m_TurretId = null;
	private CNetworkVar<ETurretType> m_TurretType = null;

	
	// Member Properties
	public uint TurretId 
	{
		get{ return(m_TurretId.Get()); }			
		set
		{
			if(m_TurretId.Get() == uint.MaxValue)
			{
				m_TurretId.Set(value);
			}
			else
			{
				Debug.LogError("Cannot set turret ID value twice!");
			}
		}			
	}

	public ETurretType TurretType
	{
		get { return(m_TurretType.Get()); }
		set 
		{
			if(m_TurretType.Get() == ETurretType.INVALID)
			{
				m_TurretType.Set(value);
			}
			else
			{
				Debug.LogError("Cannot set turret type twice");
			}
		}
	}

	
	// Member Methods
	public override void InstanceNetworkVars()
	{
		m_TurretId = new CNetworkVar<uint>(OnNetworkVarSync, uint.MaxValue);
		m_TurretType = new CNetworkVar<ETurretType>(OnNetworkVarSync, ETurretType.INVALID);
	}
	
	public void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if(_cSyncedVar == m_TurretId)
		{
			// Find the turret node this turret was created on
			GameObject turretNode = transform.parent.GetComponent<CFacilityTurrets>().GetTurretNode(m_TurretId.Get());
			
			// Attach this turret to it
			turretNode.GetComponent<CTurretNodeInterface>().SetAttachedTurret(gameObject);
		}
	}

	public static CGame.ENetworkRegisteredPrefab GetTurretPrefab(ETurretType _TurretType)
	{
		CGame.ENetworkRegisteredPrefab eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.INVALID;
		
		switch (_TurretType)
		{
			case ETurretType.Laser: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.LaserTurret; break;		
		}
		
		return (eRegisteredPrefab);
	}
};
