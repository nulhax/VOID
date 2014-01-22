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
	enum ENetworkAction
	{
		INVALID,

		ActionSendPlayerName,

		MAX
	}

// Member Delegates & Events


// Member Properties
	public Dictionary<ulong, string> PlayerList
	{
		get
		{
			return(m_mPlayerName);
		}
	}

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
		m_sNetworkedPlayerName = new CNetworkVar<string>(OnNetworkVarSync, "");
	}

	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		if(_cSyncedNetworkVar == m_sNetworkedPlayerName)
		{
			bool bAddToList = true;

			foreach(string Name in m_PlayerNamesList)
			{
				if(Name == m_sNetworkedPlayerName.Get ())
				{
					bAddToList = false;
				}
			}

			if(bAddToList)
			{
				m_PlayerNamesList.Add(m_sNetworkedPlayerName.Get());
				Debug.Log("Added " + m_sNetworkedPlayerName.Get() + " to game");
			}
			else
			{
				Debug.Log("Name " + m_sNetworkedPlayerName.Get() + " Was already taken!");
			}
		}
	}
		
	public static void SerializeData(CNetworkStream _cStream)
	{
		if(m_bSerializeName)
		{
			_cStream.Write((byte)ENetworkAction.ActionSendPlayerName);
			_cStream.WriteString(CGamePlayers.s_cInstance.m_sPlayerName);

			CGamePlayers.m_bSerializeName = false;
		}
	}

	
	public static void UnserializeData(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		ENetworkAction eNetworkAction = (ENetworkAction)_cStream.ReadByte();
		
		switch (eNetworkAction)
		{
			case ENetworkAction.ActionSendPlayerName:
			{
				string sPlayerName = _cStream.ReadString();

				CGamePlayers.s_cInstance.InvokeRpcAll("RegisterPlayerName", _cNetworkPlayer.PlayerId, sPlayerName);
				
				break;
			}
		}
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
		CGame.Instance.EventNameChange += new CGame.NotifyNameChange(OnPlayerNameChange);
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
			List<GameObject> aPlayerSpawners = CModuleInterface.FindModulesByType(CModuleInterface.EType.PlayerSpawner);
				
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
        CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.ToolWiringKit);
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
	// Create RPC Call to take in playerID (ulong) and string (player username)
	void RegisterPlayerName(ulong _ulPlayerID, string _sPlayerUserName)
	{
		if(m_mPlayerName.ContainsKey(_ulPlayerID))
		{
			m_mPlayerName[_ulPlayerID] = _sPlayerUserName;
			Debug.LogError("Changed Player Name: " + _sPlayerUserName);
		}
		else
		{
			m_mPlayerName.Add(_ulPlayerID, _sPlayerUserName);
			Debug.LogError("Added player: " + _sPlayerUserName);
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


    void OnGUI()
    {
		GUIStyle cStyle = new GUIStyle();
        if (CGamePlayers.SelfActor == null)
        {
            // Draw un-spawned message
            cStyle.fontSize = 40;
            cStyle.normal.textColor = Color.white;

            GUI.Label(new Rect(Screen.width / 2 - 290, Screen.height / 2 - 50, 576, 100),
                      "Waiting for spawner to be available...", cStyle);
        }

		if (CGamePlayers.SelfActor != null)
		{

			if(Input.GetKey(KeyCode.Tab))
			{
				GUI.TextArea(new Rect(100, 100, 400, 400), "Player List ");

				int iStartY = 115;

				foreach(KeyValuePair<ulong, string> entry in m_mPlayerName)
				{
					GUI.Label(new Rect(110, iStartY, 400, 400), "Player: " + entry.Value);
					iStartY += 10;
				}
			}
		}
    }

	void OnPlayerNameChange(string _sPlayerName)
	{
		ulong ulPlayerID = CNetwork.PlayerId;

		m_mPlayerName[ulPlayerID] = _sPlayerName;
		m_sPlayerName = _sPlayerName;

		CGamePlayers.m_bSerializeName = true;
	}
// Member Fields


	Dictionary<ulong, CNetworkViewId> m_mPlayersActor = new Dictionary<ulong, CNetworkViewId>();
	Dictionary<ulong, string> m_mPlayerName = new Dictionary<ulong, string>();

	List<ulong> m_aUnspawnedPlayers = new List<ulong>();

	CNetworkVar<string> m_sNetworkedPlayerName = null;
	List<string> m_PlayerNamesList = new List<string>();

	string m_sPlayerName = System.Environment.UserDomainName + ": " + System.Environment.UserName;

	static CGamePlayers s_cInstance = null;

	static bool m_bSerializeName = true;
};
