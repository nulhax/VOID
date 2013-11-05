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


	public void Update()
	{
		// Do it from the control consoles
		
//		if (CNetwork.IsServer)
//		{
//			RaycastHit hit;
//		 	Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
//			if (Physics.Raycast (ray, out hit, 1000))
//			{		
//				if(hit.collider.gameObject.name == "ExpansionPort")
//				{
//					if(Input.GetMouseButtonDown(0))
//					{
//						uint RoomId = hit.collider.transform.parent.GetComponent<CRoomInterface>().RoomId;
//						uint PortId = hit.collider.gameObject.GetComponent<CExpansionPortInterface>().ExpansionPortId;
//						CreateRoom(CRoomInterface.ERoomType.Factory, RoomId, PortId, 0);  				
//					}
//				}			
//			}   
//		}
	}


	public bool ValidateCreateRoom(CRoomInterface.ERoomType _eType, uint _uiRoomId, uint _uiExpansionPortId)
	{
		
		
		return (true);
	}


	public GameObject CreateRoom(CRoomInterface.ERoomType _eType, uint _uiRoomId, uint _uiExpansionPortId, uint _uiAttachToId)
	{
		CGame.ENetworkRegisteredPrefab eRegisteredPrefab = CRoomInterface.GetRoomPrefab(_eType);
		GameObject cNewRoomObject = CNetwork.Factory.CreateObject(eRegisteredPrefab);
		
		
		if (_uiRoomId != 0)
		{
			//Attach the new room to the expansion port selected			
			GameObject cExpansionPort =  m_mRooms[_uiRoomId].GetComponent<CRoomInterface>().GetExpansionPort(_uiExpansionPortId);
			cExpansionPort.GetComponent<CExpansionPortInterface>().Attach(_uiAttachToId, cNewRoomObject);			
		}
	
		cNewRoomObject.transform.parent = transform;
			
		cNewRoomObject.GetComponent<CNetworkView>().SyncParent();
		cNewRoomObject.GetComponent<CNetworkView>().SyncTransformPosition();
		cNewRoomObject.GetComponent<CNetworkView>().SyncTransformRotation();
			
		
		// Create the room's doors and initialise the control console
		CRoomGeneral room = cNewRoomObject.GetComponent<CRoomGeneral>();
		room.ServerCreateDoors();
		room.ServerCreateControlConsole();
		
		uint uiRoomId = ++m_uiRoomIdCount;
		m_mRooms.Add(uiRoomId, cNewRoomObject);
		
		cNewRoomObject.GetComponent<CRoomInterface>().RoomId = uiRoomId;		

		return (cNewRoomObject);
	}


	public GameObject GetRoom(uint _uiRoomId)
	{
		return (m_mRooms[_uiRoomId]);
	}	


// Member Fields


	uint m_uiRoomIdCount;	
	Dictionary<uint, GameObject> m_mRooms = new Dictionary<uint, GameObject>();
	
};
