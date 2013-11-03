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
		if (CNetwork.IsServer)
		{
			RaycastHit hit;
		 	Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit, 1000))
			{		
				if(hit.collider.gameObject.name == "ExpansionPort")
				{
					if(Input.GetMouseButtonDown(0))
					{
						uint RoomId = hit.collider.transform.parent.GetComponent<CRoomInterface>().RoomId;
						uint PortId = hit.collider.gameObject.GetComponent<CExpansionPortInterface>().ExpansionPortId;
						CreateRoom(CRoomInterface.ERoomType.Factory, RoomId, PortId);  				
					}
				}			
			}   
		}
	}


	public bool ValidateCreateRoom(CRoomInterface.ERoomType _eType, uint _uiRoomId, uint _uiExpansionPortId)
	{
		
		
		return (true);
	}


	public GameObject CreateRoom(CRoomInterface.ERoomType _eType, uint _uiRoomId, uint _uiExpansionPortId)
	{
		CGame.ENetworkRegisteredPrefab eRegisteredPrefab = CRoomInterface.GetRoomPrefab(_eType);
		GameObject cNewRoomObject = CNetwork.Factory.CreateObject(eRegisteredPrefab);
		
		
		if (_uiRoomId == 0)
		{
			cNewRoomObject.GetComponent<CNetworkView>().InvokeRpcAll("SetParent", GetComponent<CNetworkView>().ViewId);			
		}
		else
		{
			//Attach the new room to the expansion port selected			
			GameObject cExpansionPort =  m_mRooms[_uiRoomId].GetComponent<CRoomInterface>().GetExpansionPort(_uiExpansionPortId);
			cExpansionPort.GetComponent<CExpansionPortInterface>().Attach(0, cNewRoomObject);
			//cNewRoomObject.GetComponent<CNetworkView>().InvokeRpcAll("SetParent", GetRoom(_uiRoomId).GetComponent<CNetworkView>().ViewId);
			cNewRoomObject.GetComponent<CNetworkView>().InvokeRpcAll("SetParent", GetComponent<CNetworkView>().ViewId);			
		}
		
		
		// Sync object's position
		cNewRoomObject.GetComponent<CNetworkView>().InvokeRpcAll("SetTransformPosition", cNewRoomObject.transform.position.x, 
													 									 cNewRoomObject.transform.position.y, 
													 									 cNewRoomObject.transform.position.z);

		// Sync object's rotation
		cNewRoomObject.GetComponent<CNetworkView>().InvokeRpcAll("SetTransformRotation", cNewRoomObject.transform.eulerAngles.x,
													 									 cNewRoomObject.transform.eulerAngles.y,
													 									 cNewRoomObject.transform.eulerAngles.z);
			
		
		// Create the room's doors and initialise the control console
		CRoomGeneral room = cNewRoomObject.GetComponent<CRoomGeneral>();
		room.ServerCreateDoors();
		room.CreateRoomControlConsole();
		
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
