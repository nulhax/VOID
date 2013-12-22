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

[RequireComponent(typeof(CFacilityGravity))]
[RequireComponent(typeof(CFacilityExpansion))]
[RequireComponent(typeof(CFacilityTurrets))]
[RequireComponent(typeof(CFacilityOnboardActors))]
[RequireComponent(typeof(CFacilityGeneral))]
[RequireComponent(typeof(CNetworkView))]
public class CFacilityInterface : CNetworkMonoBehaviour
{

// Member Types


    public enum EFacilityType
	{
		INVALID = -1,
		
		Bridge,
		Engine,
		Factory,
		GravityGenerator,
		LifeSupportDome,
		Replicator,
		Scanner,
		//ShieldGenerator,
		HallwayStraight,
		HallwayCorner,
		HallwayTSection,
		HallwayXSection,

		MAX,
	}


// Member Delegates & Events


	
// Member Fields
	private CNetworkVar<uint> m_FacilityID = null;
	private CNetworkVar<EFacilityType> m_FacilityType = null;


// Member Properties
	public uint FacilityId 
	{
		get{return(m_FacilityID.Get());}			
		set
		{
			if(m_FacilityID.Get() == uint.MaxValue)
			{
				m_FacilityID.Set(value);
			}
			else
			{
				Debug.LogError("Cannot set facility ID value twice!");
			}
		}			
	}
	
	
	public EFacilityType FacilityType 
	{
		get { return(m_FacilityType.Get()); }
		set
		{
			if(m_FacilityType.Get() == EFacilityType.INVALID)
			{
				m_FacilityType.Set(value);
			}
			else
			{
				Debug.LogError("Cannot set facility type value twice!");
			}
		}
	}


// Member Methods
	public override void InstanceNetworkVars()
	{
		m_FacilityID = new CNetworkVar<uint>(OnNetworkVarSync, uint.MaxValue);
		m_FacilityType = new CNetworkVar<EFacilityType>(OnNetworkVarSync, EFacilityType.INVALID);
	}
	
	public void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{

	}

	public void Start()
	{
		// Attach the collider for the facility to the galaxy ship
		CGalaxyShipCollider galaxyShipCollider = CGame.GalaxyShip.GetComponent<CGalaxyShipCollider>();
		galaxyShipCollider.AttachNewCollider("Prefabs/" + CNetwork.Factory.GetRegisteredPrefabFile(CFacilityInterface.GetFacilityPrefab(FacilityType)) + "Ext", transform.localPosition, transform.localRotation);
	}
	
	public static CGame.ENetworkRegisteredPrefab GetFacilityPrefab(EFacilityType _eFacilityType)
	{
		CGame.ENetworkRegisteredPrefab eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.INVALID;
		
		switch (_eFacilityType)
		{
			case EFacilityType.Bridge: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.FacilityBridge; break;
			case EFacilityType.Factory: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.FacilityFactory; break;
			case EFacilityType.GravityGenerator: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.FacilityGravityGenerator; break;
			case EFacilityType.LifeSupportDome: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.FacilityLifeSupport; break;
			case EFacilityType.Engine: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.FacilityEngine; break;
			case EFacilityType.Replicator: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.FacilityReplicator; break;
			case EFacilityType.Scanner: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.FacilityScanner; break;
			//case EFacilityType.ShieldGenerator: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.FacilityShieldGenerator; break;
			case EFacilityType.HallwayStraight: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.HallwayStraight; break;
			case EFacilityType.HallwayCorner: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.HallwayCorner; break;
			case EFacilityType.HallwayTSection: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.HallwayTSection; break;
			case EFacilityType.HallwayXSection: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.HallwayXSection; break;			
		}
		
		return (eRegisteredPrefab);
	}
};
