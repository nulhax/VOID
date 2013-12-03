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


/* Implementation */


public class CFacilityInterface : MonoBehaviour
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
	
	
	public EFacilityType FacilityType 
	{
		get{return(m_eType);}			
		set
		{
			if(m_eType == EFacilityType.INVALID)
			{
				m_eType = value;
			}
			else
			{
				Debug.LogError("Cannot set room type value twice!");
			}			
		}			
	}


// Member Functions
	public void Awake()
	{	
		// Initialise the expansion ports
		SearchExpansionPorts();
		AddDebugPortNames();
		
		// Generic components to be added for all room types
		gameObject.AddComponent<CRoomAtmosphere>();
		gameObject.AddComponent<CFacilityPower>();
		gameObject.AddComponent<CFacilityGeneral>();
		gameObject.AddComponent<CFacilityHull>();
		
		// Add the network view
		gameObject.AddComponent<CNetworkView>();
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
		//Update
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

	// Member Fields
	
	EFacilityType m_eType = EFacilityType.INVALID;
	
	uint m_uiFacilityID = uint.MaxValue;
	bool m_bIntersecting = false;
	
	List<GameObject> m_aExpansionPorts = new List<GameObject>();

};
