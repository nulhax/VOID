//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRoomInfo.cs
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


public class CRoomInterface : MonoBehaviour
{

// Member Types


	public enum ERoomType
	{
		INVALID = -1,
		
		Bridge,
		Engine,
		Factory,
		GravityGenerator,
		LifeSupportDome,
		Replicator,
		Scanner,
		HallwayStraight,
		HallwayCorner,
		HallwayTSection,
		HallwayXSection,
		
		MAX
	}


// Member Delegates & Events


// Member Properties
	
	
	public uint RoomId 
	{
		get{return(m_uiRoomID);}			
		set
		{
			if(m_uiRoomID == uint.MaxValue)
			{
				m_uiRoomID = value;
			}
			else
			{
				Debug.LogError("Cannot set room ID value twice!");
			}			
		}			
	}
	
	
	public ERoomType RoomType 
	{
		get{return(m_eType);}			
		set
		{
			if(m_eType == ERoomType.INVALID)
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
		// Initialis ethe expansion ports
		SearchExpansionPorts();
		AddDebugPortNames();
		
		// Generic components to be added for all room types
		gameObject.AddComponent<CRoomAtmosphere>();
		gameObject.AddComponent<CRoomPower>();
		gameObject.AddComponent<CRoomGeneral>();
			
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
	
	
	public static CGame.ENetworkRegisteredPrefab GetRoomPrefab(ERoomType _eRoomType)
	{
		CGame.ENetworkRegisteredPrefab eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.INVALID;
		
		switch (_eRoomType)
		{
			case ERoomType.Bridge: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.RoomBridge; break;
			case ERoomType.Factory: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.RoomFactory; break;
			case ERoomType.GravityGenerator: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.RoomGravityGenerator; break;
			case ERoomType.LifeSupportDome: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.RoomLifeSupport; break;
			case ERoomType.Engine: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.RoomEngine; break;
			case ERoomType.Replicator: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.RoomReplicator; break;
			case ERoomType.Scanner: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.RoomScanner; break;
			case ERoomType.HallwayStraight: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.HallwayStraight; break;
			case ERoomType.HallwayCorner: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.HallwayCorner; break;
			case ERoomType.HallwayTSection: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.HallwayTSection; break;
			case ERoomType.HallwayXSection: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.HallwayXSection; break;			
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
			//portName.Initialise("Port " + PortId, Color.green, 72, 0.10f);
		}
	}
	
	private void OnTriggerEnter(Collider _Entity)
	{
		//If this room is intersecting another room, panic.
		if(_Entity.gameObject.tag == "Room")
		{
			m_bIntersecting = true;
		}
	}   
	
	private void OnTriggerExit(Collider _Entity)
	{
		if(_Entity.gameObject.tag == "Room")
		{
			m_bIntersecting = false;
		}
	}

	// Member Fields
	
	ERoomType m_eType = ERoomType.INVALID;
	
	uint m_uiRoomID = uint.MaxValue;
	bool m_bIntersecting = false;
	
	List<GameObject> m_aExpansionPorts = new List<GameObject>();

};
