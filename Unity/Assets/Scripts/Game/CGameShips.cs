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


	public static TNetworkViewId ShipViewId
	{
		get { return (s_cInstance.m_cShipViewId); }
	}


	public static CShipGalaxySimulatior ShipGalaxySimulator
	{
		get 
        {
            if (Ship == null)
                return (null);

            return (Ship.GetComponent<CShipGalaxySimulatior>()); 
        }
	}


	public static GameObject GalaxyShip
	{
		get 
        {
            if (Ship == null)
                return (null);

            return (Ship.GetComponent<CShipGalaxySimulatior>().GalaxyShip); 
        }
	}


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
        _cRegistrar.RegisterRpc(this, "SetShipNetworkViewId");
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
	void SetShipNetworkViewId(TNetworkViewId _cShipViewId)
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

        //GameObject cBirdgeObject = CNetwork.Factory.CreateObject(CFacilityInterface.GetPrefabType(CFacilityInterface.EType.Bridge));
        //cBirdgeObject.GetComponent<CFacilityExpansion>().GetExpansionPort(0).GetComponent<CExpansionPortBehaviour>().CreateFacility(CFacilityInterface.EType.Airlock, 0);
        //GameObject cBirdgeObject = CNetwork.Factory.CreateObject(CFacilityInterface.GetPrefabType(CFacilityInterface.EType.Airlock));

        //cBirdgeObject.GetComponent<CFacilityExpansion>().GetExpansionPort(0).GetComponent<CExpansionPortBehaviour>().CreateFacility(CFacilityInterface.EType.Test, 0);


		//cShipObject.GetComponent<CShipFacilities>().CreateFacility(CFacilityInterface.EType.Bridge);
        //cShipObject.GetComponent<CShipFacilities>().CreateFacility(CFacilityInterface.EType.Airlock, cBirdgeObject.GetComponent<CFacilityInterface>().FacilityId, 0, 0);
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
		InvokeRpc(_cPlayer.PlayerId, "SetShipNetworkViewId", m_cShipViewId);
	}


// Member Fields


	TNetworkViewId m_cShipViewId = null;
	
	
	static CGameShips s_cInstance = null;


};
