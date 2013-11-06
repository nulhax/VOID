﻿//  Auckland
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
		Factory,
		LifeSupportDome,
		GravityGenerator,
		Engine,
		HallwayTSection,
		
		MAX
	}


// Member Delegates & Events


// Member Properties
	
	
	public uint RoomId 
	{
		get{return(m_uiRoomID);}			
		set
		{
			if(m_uiRoomID == 0)
			{
				m_uiRoomID = value;
			}
			else
			{
				Debug.LogError("Cannot set ID value twice");
			}			
		}			
	}


// Member Functions


	public void Awake()
	{
		SearchExpansionPorts();
		AddDebugPortNames();
		

		gameObject.AddComponent<CRoomAtmosphere>();
		gameObject.AddComponent<CRoomPower>();
		gameObject.AddComponent<CRoomGeneral>();
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


	public ERoomType RoomType
	{
		set
		{
			if (m_eType != ERoomType.INVALID)
			{
				Debug.LogError("You cannot change the room type once it has been set. Moron.");
			}
			else
			{
				m_eType = value;
			}
		}

		get
		{
			return (m_eType);
		}
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
			case ERoomType.HallwayTSection: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.HallwayTSection; break;				
			case ERoomType.LifeSupportDome: eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.RoomLifeSupport; break;
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
			DUIField debugName = m_aExpansionPorts[i].gameObject.AddComponent<DUIField>();
			int PortId = i + 1;
			debugName.Initialise("Port " + PortId, Color.green, 72, 0.10f);
		}
	}

	// Member Fields


	ERoomType m_eType = ERoomType.INVALID;
	uint m_uiRoomID = 0;
	
	List<GameObject> m_aExpansionPorts = new List<GameObject>();

};
