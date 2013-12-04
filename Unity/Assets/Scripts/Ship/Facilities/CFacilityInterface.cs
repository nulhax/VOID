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


public class CFacilityInterface : CNetworkMonoBehaviour
{

// Member Types


	public enum EFacilityType : int
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
		
		MAX
	}


// Member Delegates & Events
	
	
// Member Fields
	
	private CNetworkVar<int> m_eFacilityType = null;
	
	private uint m_uiFacilityID = uint.MaxValue;
	private bool m_bIntersecting = false;
	
	private List<GameObject> m_aExpansionPorts = new List<GameObject>();
	private GameObject m_InteriorTrigger = null;

// Member Properties
	
	
	public uint FacilityId 
	{
		get{return(m_uiFacilityID);}			
		set
		{
			if(m_uiFacilityID == uint.MaxValue)
			{
				m_uiFacilityID = value;
			}
			else
			{
				Debug.LogError("Cannot set room ID value twice!");
			}
		}			
	}
	
	
	public EFacilityType FacilityType 
	{
		get{return((EFacilityType)m_eFacilityType.Get());}			
		set
		{
			if(CNetwork.IsServer)
			{
				if((EFacilityType)m_eFacilityType.Get() == EFacilityType.INVALID)
				{
					m_eFacilityType.Set((int)value);
				}
				else
				{
					Debug.LogError("Cannot set room type value twice!");
				}	
			}
			else
			{
				Debug.LogError("Only the server can set the room type!");
			}
		}			
	}


// Member Functions
	public override void InstanceNetworkVars()
    {
		m_eFacilityType = new CNetworkVar<int>(OnNetworkVarSync, (int)EFacilityType.INVALID);
    }
	
	public void OnNetworkVarSync(INetworkVar _cVarInstance)
	{
		
	}
	
	public void Awake()
	{	
		// Initialise the expansion ports
		SearchExpansionPorts();
		AddDebugPortNames();
		
		// Generic components to be added for all room types
		//gameObject.AddComponent<CFacilityHull>();
		gameObject.AddComponent<CFacilityGravity>();
		//gameObject.AddComponent<CFacilityAtmosphere>();
		gameObject.AddComponent<CFacilityPower>();
		gameObject.AddComponent<CFacilityGeneral>();
		
		// Add the network view
		gameObject.AddComponent<CNetworkView>();
		
		// Register this rooms internal triggers to the ship actors
		CInteriorTrigger interiorTrigger = gameObject.GetComponentInChildren<CInteriorTrigger>();
		interiorTrigger.ActorEnteredTrigger += new CInteriorTrigger.FacilityActorInteriorTriggerHandler(CGame.Ship.GetComponent<CShipActors>().ActorEnteredFacility);
		interiorTrigger.ActorExitedTrigger += new CInteriorTrigger.FacilityActorInteriorTriggerHandler(CGame.Ship.GetComponent<CShipActors>().ActorExitedFacility);	
	}
	
	public void Start()
	{
		// Attach the collider for the facility to the galaxy ship
		CGalaxyShipCollider galaxyShipCollider = CGame.Ship.GetComponent<CShipGalaxySimulatior>().GalaxyShip.GetComponent<CGalaxyShipCollider>();
		galaxyShipCollider.AttachNewCollider("Prefabs/" + CNetwork.Factory.GetRegisteredPrefabFile(CFacilityInterface.GetFacilityPrefab(FacilityType)) + "Ext", transform.localPosition, transform.localRotation);
	}

	public GameObject GetExpansionPort(uint _uiExpansionPortId)
	{
		return (m_aExpansionPorts[(int)_uiExpansionPortId]);
	}


	public List<GameObject> ExpansionPorts
	{
		get { return (m_aExpansionPorts); } 
	}


	public uint ExpansionPortsCount
	{
		get { return ((uint)m_aExpansionPorts.Count); }
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


	private void SearchExpansionPorts()
	{
		int iCount = 0;
		for (int i = 0; i < transform.childCount; ++i)
		{
			if (transform.GetChild(i).name == CExpansionPortInterface.s_GameObjectName)
			{
				transform.GetChild(i).gameObject.GetComponent<CExpansionPortInterface>().ExpansionPortId = (uint)iCount++;
				m_aExpansionPorts.Add(transform.GetChild(i).gameObject);
			}
		}
	}
	
	private void FindInteriorTrigger()
	{
		m_InteriorTrigger = transform.FindChild("InteriorTrigger").gameObject;
		
		if(m_InteriorTrigger == null)
			Debug.LogError("Interior Trigger not founf for this facility! Gravity and atmosphere will not function!");
	}
	
	private void AddDebugPortNames()
	{
		for(int i = 0; i < m_aExpansionPorts.Count; i++) 
		{
			CDUIField portName = m_aExpansionPorts[i].gameObject.AddComponent<CDUIField>();
			int PortId = i + 1;
			portName.Initialise("Port " + PortId, Color.green, 72, 0.10f);
		}
	}
	
	private void OnTriggerEnter(Collider _Entity)
	{
		//If this room is intersecting another room, panic.
		if(_Entity.gameObject.tag == "Facility")
		{
			m_bIntersecting = true;
		}
	}   
	
	private void OnTriggerExit(Collider _Entity)
	{
		if(_Entity.gameObject.tag == "Facility")
		{
			m_bIntersecting = false;
		}
	}
};
