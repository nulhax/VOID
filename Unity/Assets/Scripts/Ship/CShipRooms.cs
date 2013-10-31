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
	}


	public bool ValidateCreateRoom(CRoomInterface.ERoomType _eType, uint _uiRoomId, uint _uiExpansionPortId)
	{


		return (true);
	}


	public GameObject CreateRoom(CRoomInterface.ERoomType _eType, uint _uiRoomId, uint _uiExpansionPortId)
	{
		GameObject cRoomGameObject = CNetwork.Factory.CreateObject(CGame.EPrefab.HullPeiceSmall1);
		cRoomGameObject.GetComponent<CNetworkView>().InvokeRpcAll("SetTransformPosition", 10.0f, 0.0f, 0.0f);


		uint uiPieceId = ++m_uiRoomIdCount;
		m_mRooms.Add(m_uiRoomIdCount, cRoomGameObject);


		return (cRoomGameObject);
	}


	public GameObject FindHullPeice(uint _uiId)
	{
		return (m_mRooms[_uiId]);
	}


// Member Fields


	uint m_uiRoomIdCount;


	Dictionary<uint, GameObject> m_mRooms = new Dictionary<uint, GameObject>();


};
