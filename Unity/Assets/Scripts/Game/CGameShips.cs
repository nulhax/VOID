//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CGameShips.cs
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


public class CGameShips : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


	public static GameObject Ship
	{
		get { return (CNetwork.Factory.FindObject(s_cInstance.m_cShipViewId)); }
	}


	public static CNetworkViewId ShipViewId
	{
		get { return (s_cInstance.m_cShipViewId); }
	}


	public static CShipGalaxySimulatior ShipGalaxySimulator
	{
		get { return (Ship.GetComponent<CShipGalaxySimulatior>()); }
	}


	public static GameObject GalaxyShip
	{
		get { return (Ship.GetComponent<CShipGalaxySimulatior>().GalaxyShip); }
	}


// Member Methods


	public override void InstanceNetworkVars()
	{
		// Empty
	}


	public void Awake()
	{
		Application.runInBackground = true;
		s_cInstance = this;
	}


	void Start()
	{
		CNetwork.Connection.EventDisconnect += new CNetworkConnection.OnDisconnect(OnDisconnect);
		CNetwork.Server.EventStartup += new CNetworkServer.NotifyStartup(OnServerStartup);
		CNetwork.Server.EventShutdown += new CNetworkServer.NotifyShutdown(OnServerShutdown);
		CNetwork.Server.EventPlayerConnect += new CNetworkServer.NotifyPlayerConnect(OnPlayerJoin);
	}


	void OnDestroy()
	{
		// Empty
	}


	void Update()
	{
		// Empty
	}


	[ANetworkRpc]
	void SetShipNetworkViewId(CNetworkViewId _cShipViewId)
	{
		m_cShipViewId = _cShipViewId;
		
		// Notice
		Logger.Write("The ship's network view id is ({0})", m_cShipViewId);
	}


	void OnServerStartup()
	{
		// Create ship object
		GameObject cShipObject = CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.Ship);

		m_cShipViewId = cShipObject.GetComponent<CNetworkView>().ViewId;

		cShipObject.GetComponent<CShipFacilities>().CreateFacility(CFacilityInterface.EType.Bridge);
	}


	void OnServerShutdown()
	{
		m_cShipViewId = null;
	}


	void OnDisconnect()
	{
		if(!CNetwork.IsServer)
		{
			m_cShipViewId = null;
		}
	}


	void OnPlayerJoin(CNetworkPlayer _cPlayer)
	{
		// Tell connecting player which is the ship's network view id
		InvokeRpc(_cPlayer.PlayerId, "SetShipNetworkViewId", new object[]{ m_cShipViewId });
	}


// Member Fields


	CNetworkViewId m_cShipViewId = null;
	
	
	static CGameShips s_cInstance = null;


};
