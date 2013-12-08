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


    public enum EType
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
	
	
	public EType Type 
	{
		get{ return(m_eType.Get()); }		
	
		set
		{
			if(CNetwork.IsServer)
			{
				if(m_eType.Get() == EType.INVALID)
				{
					m_eType.Set(value);
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
        m_eType = new CNetworkVar<EType>(OnNetworkVarSync, EType.INVALID);
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
		interiorTrigger.ActorEnteredTrigger += new CInteriorTrigger.FacilityActorInteriorTriggerHandler(CGame.Ship.GetComponent<CShipOnboardActors>().ActorEnteredFacility);
		interiorTrigger.ActorExitedTrigger += new CInteriorTrigger.FacilityActorInteriorTriggerHandler(CGame.Ship.GetComponent<CShipOnboardActors>().ActorExitedFacility);	
	}
	
	public void Start()
	{
		// Attach the collider for the facility to the galaxy ship
		CGalaxyShipCollider galaxyShipCollider = CGame.Ship.GetComponent<CShipGalaxySimulatior>().GalaxyShip.GetComponent<CGalaxyShipCollider>();
		galaxyShipCollider.AttachNewCollider("Prefabs/" + CNetwork.Factory.GetRegisteredPrefabFile(CFacilityInterface.GetFacilityPrefab(Type)) + "Ext", transform.localPosition, transform.localRotation);
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
	
	
	public static CGame.ENetworkRegisteredPrefab GetFacilityPrefab(EType _eFacilityType)
	{
		CGame.ENetworkRegisteredPrefab eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.INVALID;
		
		switch (_eFacilityType)
		{
			case EType.Bridge: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.FacilityBridge; break;
			case EType.Factory: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.FacilityFactory; break;
			case EType.GravityGenerator: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.FacilityGravityGenerator; break;
			case EType.LifeSupportDome: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.FacilityLifeSupport; break;
			case EType.Engine: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.FacilityEngine; break;
			case EType.Replicator: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.FacilityReplicator; break;
			case EType.Scanner: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.FacilityScanner; break;
			//case EFacilityType.ShieldGenerator: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.FacilityShieldGenerator; break;
			case EType.HallwayStraight: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.HallwayStraight; break;
			case EType.HallwayCorner: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.HallwayCorner; break;
			case EType.HallwayTSection: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.HallwayTSection; break;
			case EType.HallwayXSection: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.HallwayXSection; break;			
		}
		
		return (eRegisteredPrefab);
	}


	void SearchExpansionPorts()
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
	
	void FindInteriorTrigger()
	{
		m_InteriorTrigger = transform.FindChild("InteriorTrigger").gameObject;
		
		if(m_InteriorTrigger == null)
			Debug.LogError("Interior Trigger not founf for this facility! Gravity and atmosphere will not function!");
	}
	

	void AddDebugPortNames()
	{
		for(int i = 0; i < m_aExpansionPorts.Count; i++) 
		{
			CDUIField portName = m_aExpansionPorts[i].gameObject.AddComponent<CDUIField>();
			int PortId = i + 1;
			portName.Initialise("Port " + PortId, Color.green, 72, 0.10f);
		}
	}
	

	void OnTriggerEnter(Collider _Entity)
	{
		//If this room is intersecting another room, panic.
		if(_Entity.gameObject.tag == "Facility")
		{
			m_bIntersecting = true;
		}
	}   
	

	void OnTriggerExit(Collider _Entity)
	{
		if(_Entity.gameObject.tag == "Facility")
		{
			m_bIntersecting = false;
		}
	}


	// Member Fields


	CNetworkVar<EType> m_eType = null;
	

	uint m_uiFacilityID = uint.MaxValue;
	bool m_bIntersecting = false;
	

	List<GameObject> m_aExpansionPorts = new List<GameObject>();
	GameObject m_InteriorTrigger = null;


};
