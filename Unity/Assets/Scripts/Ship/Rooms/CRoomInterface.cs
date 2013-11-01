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


	public enum ERoomType : short
	{
		INVALID,
		Bridge,
		Factory,
		LifeSupportDome,
		GravityGenerator,
		Engine
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
		}
		
		return (eRegisteredPrefab);
	}


	private void SearchExpansionPorts()
	{
		for (int i = 0; i < transform.childCount; ++i)
		{
			if (transform.GetChild(i).name == CExpansionPortInterface.ksGameObjectName)
			{
				transform.GetChild(i).gameObject.GetComponent<CExpansionPortInterface>().ExpansionPortId = (uint)i;
				m_aExpansionPorts.Add(transform.GetChild(i).gameObject);
			}
		}
	}


// Member Fields


	ERoomType m_eType = ERoomType.INVALID;
	uint m_uiRoomID = 0;
	List<GameObject> m_aExpansionPorts = new List<GameObject>();


};
