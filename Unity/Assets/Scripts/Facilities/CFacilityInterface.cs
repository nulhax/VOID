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


[RequireComponent(typeof(CFacilityAtmosphere))]
[RequireComponent(typeof(CFacilityModules))]
[RequireComponent(typeof(CFacilityExpansion))]
[RequireComponent(typeof(CFacilityGeneral))]
[RequireComponent(typeof(CFacilityGravity))]
[RequireComponent(typeof(CFacilityHull))]
[RequireComponent(typeof(CFacilityOnboardActors))]
[RequireComponent(typeof(CFacilityPower))]
[RequireComponent(typeof(CFacilityTurrets))]
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

		[AServerOnly]
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

		[AServerOnly]
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
		CGalaxyShipCollider galaxyShipCollider = CGameShips.GalaxyShip.GetComponent<CGalaxyShipCollider>();
		galaxyShipCollider.AttachNewCollider("Prefabs/" + CNetwork.Factory.GetRegisteredPrefabFile(CFacilityInterface.GetFacilityPrefab(FacilityType)) + "Ext", transform.localPosition, transform.localRotation);
	
		// Add self to the shipfacilities
		if(!CNetwork.IsServer)
			CGameShips.Ship.GetComponent<CShipFacilities>().AddNewlyCreatedFacility(gameObject, FacilityId, FacilityType);
	}
	
	public static CGameRegistrator.ENetworkPrefab GetFacilityPrefab(EFacilityType _eFacilityType)
	{
		CGameRegistrator.ENetworkPrefab eRegisteredPrefab = CGameRegistrator.ENetworkPrefab.INVALID;
		
		switch (_eFacilityType)
		{
			case EFacilityType.Bridge: eRegisteredPrefab = CGameRegistrator.ENetworkPrefab.FacilityBridge; break;
			case EFacilityType.Factory: eRegisteredPrefab = CGameRegistrator.ENetworkPrefab.FacilityFactory; break;
			case EFacilityType.GravityGenerator: eRegisteredPrefab = CGameRegistrator.ENetworkPrefab.FacilityGravityGenerator; break;
			case EFacilityType.LifeSupportDome: eRegisteredPrefab = CGameRegistrator.ENetworkPrefab.FacilityLifeSupport; break;
			case EFacilityType.Engine: eRegisteredPrefab = CGameRegistrator.ENetworkPrefab.FacilityEngine; break;
			case EFacilityType.Replicator: eRegisteredPrefab = CGameRegistrator.ENetworkPrefab.FacilityReplicator; break;
			case EFacilityType.Scanner: eRegisteredPrefab = CGameRegistrator.ENetworkPrefab.FacilityScanner; break;
			//case EFacilityType.ShieldGenerator: eRegisteredPrefab = CGameResourceLoader.ENetworkRegisteredPrefab.FacilityShieldGenerator; break;
			case EFacilityType.HallwayStraight: eRegisteredPrefab = CGameRegistrator.ENetworkPrefab.HallwayStraight; break;
			case EFacilityType.HallwayCorner: eRegisteredPrefab = CGameRegistrator.ENetworkPrefab.HallwayCorner; break;
			case EFacilityType.HallwayTSection: eRegisteredPrefab = CGameRegistrator.ENetworkPrefab.HallwayTSection; break;
			case EFacilityType.HallwayXSection: eRegisteredPrefab = CGameRegistrator.ENetworkPrefab.HallwayXSection; break;			
		}
		
		return (eRegisteredPrefab);
	}
};
