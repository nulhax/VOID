//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CGamePlayers.cs
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


public class CGamePlayers : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


	public static GameObject SelfActor
	{
		get 
		{ 
			GameObject playerActor = null;
			
			if(SelfActorViewId != null)
			{
				playerActor = CNetwork.Factory.FindObject(SelfActorViewId);
			}
			
			return(playerActor); 
		}
	}
	
	public static CNetworkViewId SelfActorViewId
	{
		get 
		{ 
			if (!s_cInstance.m_mPlayersActor.ContainsKey(CNetwork.PlayerId))
			{
				return (null);
			}
			
			return (s_cInstance.m_mPlayersActor[CNetwork.PlayerId]);
		}
	}
	
	public static List<GameObject> PlayerActors
	{
		get 
		{ 
			List<GameObject> actors = new List<GameObject>();
			
			foreach(CNetworkViewId playerID in s_cInstance.m_mPlayersActor.Values)
			{
				actors.Add(CNetwork.Factory.FindObject(playerID));
			}
			
			return (actors); 
		}
	}


// Member Methods


	public override void InstanceNetworkVars()
	{
		// Empty
	}


	public static GameObject FindPlayerActor(ulong _ulPlayerId)
	{
		if (!s_cInstance.m_mPlayersActor.ContainsKey(_ulPlayerId))
		{
			return (null);
		}
		
		return (CNetwork.Factory.FindObject(s_cInstance.m_mPlayersActor[_ulPlayerId]));
	}
	
	
	public static CNetworkViewId FindPlayerActorViewId(ulong _ulPlayerId)
	{
		if (!s_cInstance.m_mPlayersActor.ContainsKey(_ulPlayerId))
		{
			return (null);
		}
		
		return (s_cInstance.m_mPlayersActor[_ulPlayerId]);
	}


	public void Awake()
	{
		Application.runInBackground = true;
		s_cInstance = this;
	}


	void Start()
	{
		CNetwork.Server.EventPlayerConnect += new CNetworkServer.NotifyPlayerConnect(OnPlayerJoin);
		CNetwork.Server.EventPlayerDisconnect += new CNetworkServer.NotifyPlayerDisconnect(OnPlayerDisconnect);
		CNetwork.Server.EventShutdown += new CNetworkServer.NotifyShutdown(OnServerShutdown);
		CNetwork.Connection.EventDisconnect += new CNetworkConnection.OnDisconnect(OnDisconnect);
	}


	void OnDestroy()
	{
	}


	void Update()
	{
		if (CNetwork.IsServer &&
		    m_aUnspawnedPlayers.Count > 0)
		{
			foreach (ulong ulUnspawnedPlayerId in m_aUnspawnedPlayers.ToArray())
			{
				List<GameObject> aPlayerSpawners = CModuleInterface.FindComponentsByType(CModuleInterface.EType.PlayerSpawner);
				
				foreach (GameObject cPlayerSpawner in aPlayerSpawners)
				{
					if (!cPlayerSpawner.GetComponent<CPlayerSpawnerBehaviour>().IsBlocked)
					{
						// Create new player's actor
						GameObject cPlayerActor = CNetwork.Factory.CreateObject((ushort)CGameRegistrator.ENetworkPrefab.PlayerActor);
						
						// Set the parent as the ship
						cPlayerActor.GetComponent<CNetworkView>().SetParent(CGameShips.Ship.GetComponent<CNetworkView>().ViewId);
						
						// Get actor network view id
						CNetworkViewId cActorNetworkViewId = cPlayerActor.GetComponent<CNetworkView>().ViewId;
						
						cPlayerActor.GetComponent<CNetworkView>().SetPosition(cPlayerSpawner.GetComponent<CPlayerSpawnerBehaviour>().m_cSpawnPosition.transform.position);
						cPlayerActor.GetComponent<CNetworkView>().SetRotation(cPlayerSpawner.GetComponent<CPlayerSpawnerBehaviour>().m_cSpawnPosition.transform.rotation.eulerAngles);
						
						// Sync player actor view id with everyone
						InvokeRpcAll("RegisterPlayerActor", ulUnspawnedPlayerId, cActorNetworkViewId);
						
						m_aUnspawnedPlayers.Remove(ulUnspawnedPlayerId);
						break;
					}
				}
			}
		}
	}


	void OnPlayerJoin(CNetworkPlayer _cPlayer)
	{
		// Send created objects to new player
		CNetwork.Factory.SyncPlayer(_cPlayer);
		
		// Server doesn't need this
		if (!_cPlayer.IsHost)
		{
			// Sync current players actor view ids with new player
			foreach (KeyValuePair<ulong, CNetworkViewId> tEntry in m_mPlayersActor)
			{
				InvokeRpc(_cPlayer.PlayerId, "RegisterPlayerActor", tEntry.Key, tEntry.Value);
			}
		}
		
		// Placeholder Test stuff
		CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.ToolTorch);
		CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.ToolRachet);
		CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.ToolAk47);
		CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.ToolExtinguisher);
		CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.Fire);
		CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.ToolMedical);
		//CNetwork.Factory.CreateObject(CGameResourceLoader.ENetworkRegisteredPrefab.BlackMatterCell);
		CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.FuelCell);
		//CNetwork.Factory.CreateObject(CGameResourceLoader.ENetworkRegisteredPrefab.PlasmaCell);
		CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.PowerCell);
		//CNetwork.Factory.CreateObject(CGameResourceLoader.ENetworkRegisteredPrefab.BioCell);
		//CNetwork.Factory.CreateObject(CGameResourceLoader.ENetworkRegisteredPrefab.ReplicatorCell);
		
		m_aUnspawnedPlayers.Add(_cPlayer.PlayerId);
		
		Logger.Write("Created new player actor for player id ({0})", _cPlayer.PlayerId);
	}
	
	
	void OnPlayerDisconnect(CNetworkPlayer _cPlayer)
	{
		CNetworkViewId cPlayerActorNetworkViewId = FindPlayerActorViewId(_cPlayer.PlayerId);

		if (cPlayerActorNetworkViewId != null)
		{
			CNetwork.Factory.DestoryObject(cPlayerActorNetworkViewId);
			
			// Sync unregister player actor view id with everyone
			InvokeRpcAll("UnregisterPlayerActor", _cPlayer.PlayerId);

			Logger.Write("Removed Player Actor for Player Id ({0})", _cPlayer.PlayerId);
		}
	}


	void OnServerShutdown()
	{
		m_mPlayersActor.Clear();
	}


	void OnDisconnect()
	{
		if (!CNetwork.IsServer)
		{
			m_mPlayersActor.Clear();
		}
	}


	[ANetworkRpc]
	void RegisterPlayerActor(ulong _ulPlayerId, CNetworkViewId _cPlayerActorId)
	{
		m_mPlayersActor.Add(_ulPlayerId, _cPlayerActorId);
	}
	
	
	[ANetworkRpc]
	void UnregisterPlayerActor(ulong _ulPlayerId)
	{
		m_mPlayersActor.Remove(_ulPlayerId);
		m_aUnspawnedPlayers.Remove(_ulPlayerId);
	}


// Member Fields


	Dictionary<ulong, CNetworkViewId> m_mPlayersActor = new Dictionary<ulong, CNetworkViewId>();
	List<ulong> m_aUnspawnedPlayers = new List<ulong>();


	static CGamePlayers s_cInstance = null;


};
