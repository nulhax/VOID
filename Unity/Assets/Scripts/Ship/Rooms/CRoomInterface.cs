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
		Factory,
		LifeSupportDome,
		GravityGenerator,
		Engine
	}


// Member Delegates & Events


// Member Properties


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
		// Empty
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
		return (m_aGameObjects[(int)_uiExpansionPortId]);
	}


	public List<GameObject> ExpansionPorts
	{
		get { return (m_aGameObjects); } 
	}


	public uint ExpansionPortsCount
	{
		get { return ((uint)m_aGameObjects.Count); }
	}


	private void SearchExpansionPorts()
	{
		for (int i = 0; i < transform.childCount; ++i)
		{
			if (transform.GetChild(i).name == CExpansionPortInterface.ksGameObjectName)
			{
				m_aGameObjects.Add(transform.GetChild(i).gameObject);
			}
		}
	}


// Member Fields


	ERoomType m_eType = ERoomType.INVALID;


	List<GameObject> m_aGameObjects = new List<GameObject>();


};
