//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShipHull.cs
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


public class CShipRooms : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Functions


	public void Start()
	{
		
	}


	public void OnDestroy()
	{
		
	}


	public bool ValidateCreateRoom(CRoomInterface.ERoomType _eType, uint _uiRoomId, uint _uiExpansionPortId)
	{
		return (true);
	}


	public GameObject CreateRoom(CRoomInterface.ERoomType _eType, uint _uiRoomId, uint _uiExpansionPortId = uint.MaxValue, uint _uiAttachToId = uint.MaxValue)
	{
		CExpansionPortInterface cExpansionPort = null;
		if(_uiExpansionPortId != uint.MaxValue && _uiAttachToId != uint.MaxValue)
		{
			cExpansionPort = m_mRooms[_uiRoomId].GetComponent<CRoomInterface>().GetExpansionPort(_uiExpansionPortId).GetComponent<CExpansionPortInterface>();
			
			if(cExpansionPort.HasAttachedRoom == true)
			{
				Debug.LogWarning("Failed to create new room. Port already in use");
				return(null);
			}
		}
		
		uint uiRoomId = ++m_uiRoomIdCount;
		
		CGame.ENetworkRegisteredPrefab eRegisteredPrefab = CRoomInterface.GetRoomPrefab(_eType);
		GameObject cNewRoomObject = CNetwork.Factory.CreateObject(eRegisteredPrefab);
		
		CRoomInterface roomInterface = cNewRoomObject.GetComponent<CRoomInterface>();
		roomInterface.RoomId = uiRoomId;
		roomInterface.RoomType = _eType;
		
		cNewRoomObject.transform.parent = transform;
		
		if(cExpansionPort != null)
			cExpansionPort.Attach(_uiAttachToId, cNewRoomObject);			
			
		cNewRoomObject.GetComponent<CNetworkView>().SyncParent();
		cNewRoomObject.GetComponent<CNetworkView>().SyncTransformPosition();
		cNewRoomObject.GetComponent<CNetworkView>().SyncTransformRotation();
		
		m_mRooms.Add(uiRoomId, cNewRoomObject);
		
		return (cNewRoomObject);
	}
	
	
	public List<GameObject> GetAllRooms()
	{
		List<GameObject> ReturnList = new List<GameObject>();
		
		foreach (KeyValuePair<uint,GameObject> Entry in m_mRooms)
		{
			ReturnList.Add(Entry.Value);
		}
		
		return (ReturnList);
	}

	public GameObject GetRoom(uint _uiRoomId)
	{
		return (m_mRooms[_uiRoomId]);
	}


// Member Fields


	uint m_uiRoomIdCount;	
	Dictionary<uint, GameObject> m_mRooms = new Dictionary<uint, GameObject>();
	
};
