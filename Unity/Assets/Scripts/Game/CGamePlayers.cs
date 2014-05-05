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


	public delegate void NotifyPlayerActivity(ulong _PlayerId);

	public event NotifyPlayerActivity EventPlayerJoin;
	public event NotifyPlayerActivity EventPlayerLeave;

    public delegate void NotifyPlayerActorRegister(ulong _ulPlayerId, GameObject _cPlayerActor);
    public delegate void NotifyPlayerActorUnregister(ulong _ulPlayerId);

    public event NotifyPlayerActorRegister EventActorRegister;
    public event NotifyPlayerActorUnregister EventActorUnregister;


// Member Properties


	public string LocalPlayerName
	{
		get
		{
			return(m_mPlayersNames[CNetwork.PlayerId]);
		}
	}

	public static CGamePlayers Instance
	{
		get
		{
			return(s_cInstance);
		}
	}

	public static GameObject SelfActor
	{
		get 
		{ 
			GameObject playerActor = null;
			
			if(SelfActorViewId != null)
			{
				playerActor = CNetwork.Factory.FindGameObject(SelfActorViewId);
			}
			
			return(playerActor); 
		}
	}

	public static GameObject SelfActorHead
	{
		get 
		{ 
			GameObject playerActorHead = null;
			
			if(SelfActorViewId != null)
			{
				playerActorHead = CNetwork.Factory.FindGameObject(SelfActorViewId).GetComponent<CPlayerHead>().Head;
			}
			
			return(playerActorHead); 
		}
	}

	public static TNetworkViewId SelfActorViewId
	{
		get 
		{ 
			if (!s_cInstance.m_mPlayersActors.ContainsKey(CNetwork.PlayerId))
			{
				return (null);
			}
			
			return (s_cInstance.m_mPlayersActors[CNetwork.PlayerId]);
		}
	}

	public static List<ulong> Players
	{
		get { return (new List<ulong>(s_cInstance.m_mPlayersActors.Keys)); }
	}
	
	public static List<GameObject> PlayerActors
	{
		get 
		{ 
			List<GameObject> actors = new List<GameObject>();
			
			foreach(TNetworkViewId playerID in s_cInstance.m_mPlayersActors.Values)
			{
				actors.Add(CNetwork.Factory.FindGameObject(playerID));
			}
			
			return (actors); 
		}
	}


// Member Methods


	public override void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar)
	{
		m_sNetworkedPlayerName = _cRegistrar.CreateReliableNetworkVar<string>(OnNetworkVarSync, "");

        _cRegistrar.RegisterRpc(this, "RegisterPlayerName");
        _cRegistrar.RegisterRpc(this, "UnregisterPlayerName");
        _cRegistrar.RegisterRpc(this, "RemoteRegisterPlayerActor");
        _cRegistrar.RegisterRpc(this, "RemoteUnregisterPlayerActor");
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
            _cStream.Write(CGamePlayers.s_cInstance.m_sPlayerName);
								
			CGamePlayers.m_bSerializeName = false;
		}
	}

	
	public static void UnserializeData(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		ENetworkAction eNetworkAction = (ENetworkAction)_cStream.Read<byte>();
		
		switch (eNetworkAction)
		{
			case ENetworkAction.ActionSendPlayerName:
			{
				string sPlayerName = _cStream.Read<string>();

				//Send all dictionary entries to new player
				foreach (KeyValuePair<ulong, string> entry in CGamePlayers.s_cInstance.m_mPlayersNames) 
				{
					//Make sure you don't send RPC to yourself. Foreach loops will not let you modify the collections object (dictionary) you are operating on.
					if(_cNetworkPlayer.PlayerId != CNetwork.PlayerId)
					{
						CGamePlayers.s_cInstance.InvokeRpc(_cNetworkPlayer.PlayerId, "RegisterPlayerName", entry.Key, entry.Value);
					}
				}
						
				// Send new player name to all other players
				CGamePlayers.s_cInstance.InvokeRpcAll("RegisterPlayerName", _cNetworkPlayer.PlayerId, sPlayerName);
				
				break;
			}
		}
	}


	public static GameObject GetPlayerActor(ulong _ulPlayerId)
	{
		if (!s_cInstance.m_mPlayersActors.ContainsKey(_ulPlayerId))
		{
			return (null);
		}
		
		return (CNetwork.Factory.FindGameObject(s_cInstance.m_mPlayersActors[_ulPlayerId]));
	}


	public static TNetworkViewId GetPlayerActorViewId(ulong _ulPlayerId)
	{
		if (!s_cInstance.m_mPlayersActors.ContainsKey(_ulPlayerId))
		{
			return (null);
		}
		
		return (s_cInstance.m_mPlayersActors[_ulPlayerId]);
	}


	public static ulong GetPlayerActorsPlayerId(TNetworkViewId _PlayerActorViewId)
	{
		if (!s_cInstance.m_mPlayerActorsPlayers.ContainsKey(_PlayerActorViewId))
		{
			return(ulong.MinValue);
		}
		
		return (s_cInstance.m_mPlayerActorsPlayers[_PlayerActorViewId]);
	}


	public static ulong GetPlayerActorsPlayerId(GameObject _PlayerActor)
	{
		if(_PlayerActor.GetComponent<CNetworkView>() == null)
			return(ulong.MinValue);
		else
			return(GetPlayerActorsPlayerId(_PlayerActor.GetComponent<CNetworkView>().ViewId));
	}


	public static string GetPlayerName(ulong _ulPlayerId)
	{
		if (!s_cInstance.m_mPlayersNames.ContainsKey(_ulPlayerId))
		{
			return(string.Empty);
		}
		
		return (s_cInstance.m_mPlayersNames[_ulPlayerId]);
	}


	public void Awake()
	{
		Application.runInBackground = true;
		s_cInstance = this;
	}


	void Start()
	{
		CNetwork.Server.EventPlayerConnect += new CNetworkServer.NotifyPlayerConnect(OnEventServerPlayerJoin);
		CNetwork.Server.EventPlayerDisconnect += new CNetworkServer.NotifyPlayerDisconnect(OnEventServerPlayerDisconnect);
		CNetwork.Server.EventShutdown += new CNetworkServer.NotifyShutdown(OnEventServerShutdown);
		CNetwork.Connection.EventDisconnect += new CNetworkConnection.OnDisconnect(OnEventConnectionDisconnect);
		CGame.Instance.EventNameChange += new CGame.NotifyNameChange(OnPlayerNameChange);
	}


	void OnDestroy() { }


	void Update()
	{
		if (CNetwork.IsServer &&
		    m_aUnspawnedPlayers.Count > 0)
		{
			foreach (ulong ulUnspawnedPlayerId in m_aUnspawnedPlayers.ToArray())
			{
			    List<GameObject> aPlayerSpawners = CGameShips.Ship.GetComponent<CShipModules>().FindModulesByType(CModuleInterface.EType.PlayerSpawner);

                if (aPlayerSpawners == null ||
                    aPlayerSpawners.Count == 0)
                {
                    break;
                }
				
				foreach (GameObject cPlayerSpawner in aPlayerSpawners)
				{
					if (!cPlayerSpawner.GetComponent<CPlayerSpawnerBehaviour>().IsBlocked)
					{
                        m_aUnspawnedPlayers.Remove(ulUnspawnedPlayerId);

						// Create new player's actor
						GameObject cPlayerActor = CNetwork.Factory.CreateGameObject((ushort)CGameRegistrator.ENetworkPrefab.PlayerActor);
						
						// Set the parent as the ship
						//cPlayerActor.GetComponent<CNetworkView>().SetParent(CGameShips.Ship.GetComponent<CNetworkView>().ViewId);
						
						// Get actor network view id
						TNetworkViewId cActorNetworkViewId = cPlayerActor.GetComponent<CNetworkView>().ViewId;
						
						cPlayerActor.GetComponent<CNetworkView>().SetPosition(cPlayerSpawner.GetComponent<CPlayerSpawnerBehaviour>().m_cSpawnPosition.transform.position);
						cPlayerActor.GetComponent<CNetworkView>().SetRotation(cPlayerSpawner.GetComponent<CPlayerSpawnerBehaviour>().m_cSpawnPosition.transform.rotation);

						// Sync player actor view id with everyone
						InvokeRpcAll("RemoteRegisterPlayerActor", ulUnspawnedPlayerId, cActorNetworkViewId);

                        cPlayerActor.GetComponent<CPlayerHealth>().m_EventHealthStateChanged += RespawnPlayer;
						
						break;
					}
				}
			}
		}
	}


    void RespawnPlayer(GameObject _SourcePlayer, CPlayerHealth.HealthState _eHealthCurrentState, CPlayerHealth.HealthState _eHealthPreviousState)
    {
        // If the previous health state was DEAD
        // And current health state is ALIVE
        if (_eHealthCurrentState == CPlayerHealth.HealthState.DEAD)
        {
            // Save a list of currently constructed spawners
            List<GameObject> aPlayerSpawners = CGameShips.Ship.GetComponent<CShipModules>().FindModulesByType(CModuleInterface.EType.PlayerSpawner);

            // Iterate through every spawner
            foreach (GameObject cPlayerSpawner in aPlayerSpawners)
            {
                // If the spawner is not blocked
                if (!cPlayerSpawner.GetComponent<CPlayerSpawnerBehaviour>().IsBlocked)
                {
                    //Ensure the collider is enabled!
                    _SourcePlayer.rigidbody.collider.enabled = true;

                    // "Board" the ship
                    // Note: Does nothing unless the player 'dies' outside the ship
                    _SourcePlayer.GetComponent<CActorBoardable>().BoardActor();

                    // Set the player's position and rotation based upon the spawner's position and rotation
                    _SourcePlayer.GetComponent<CNetworkView>().SetPosition(cPlayerSpawner.GetComponent<CPlayerSpawnerBehaviour>().m_cSpawnPosition.transform.position);
                    _SourcePlayer.GetComponent<CNetworkView>().SetRotation(cPlayerSpawner.GetComponent<CPlayerSpawnerBehaviour>().m_cSpawnPosition.transform.rotation);

                    // Heal the player to max health
                    _SourcePlayer.GetComponent<CPlayerHealth>().ApplyHeal(_SourcePlayer.GetComponent<CPlayerHealth>().MaxHealth);

                    // TODO: Reset other variables such as suit atmosphere and equipped tools

                    // Break loop
                    break;
                }
            }
        }
    }


	void OnEventServerPlayerJoin(CNetworkPlayer _cPlayer)
	{
		// Send created objects to new player
		CNetwork.Factory.SyncPlayer(_cPlayer);
		
		// Server doesn't need this
		if (!_cPlayer.IsHost)
		{
			// Sync current players actor view ids with new player
			foreach (KeyValuePair<ulong, TNetworkViewId> tEntry in m_mPlayersActors)
			{
                InvokeRpc(_cPlayer.PlayerId, "RemoteRegisterPlayerActor", tEntry.Key, tEntry.Value);
			}
		}
		
		// Placeholder Test stuff
		//CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.ToolRatchet);
		//CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.ToolMiningDrill);
		//CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.ToolExtinguisher);
		//CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.Fire);
		//CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.ToolMedical);
		//CNetwork.Factory.CreateObject(CGameResourceLoader.ENetworkRegisteredPrefab.BlackMatterCell);
		//CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.FuelCell);
		//CNetwork.Factory.CreateObject(CGameResourceLoader.ENetworkRegisteredPrefab.PlasmaCell);
		//CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.PowerCell);
		//CNetwork.Factory.CreateObject(CGameResourceLoader.ENetworkRegisteredPrefab.BioCell);
		//CNetwork.Factory.CreateObject(CGameResourceLoader.ENetworkRegisteredPrefab.ReplicatorCell);


		//Place module gun
		//GameObject tool = CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.ToolModuleGun);
		//tool.GetComponent<CNetworkView>().SetPosition(new Vector3(-10.0f, -8.0f, -13.0f));
        GameObject tool = CNetwork.Factory.CreateGameObject(CGameRegistrator.ENetworkPrefab.ToolAk47);
		tool.GetComponent<CNetworkView>().SetPosition(new Vector3(-10.0f, 0.0f, -13.0f));
		//Place Ratchet
        GameObject ratchet = CNetwork.Factory.CreateGameObject(CGameRegistrator.ENetworkPrefab.ToolRatchet);
		ratchet.GetComponent<CNetworkView>().SetPosition(new Vector3(-10.0f, 0.0f, -13.0f));


		m_aUnspawnedPlayers.Add(_cPlayer.PlayerId);

		//Send all dictionary entries to all players.

		Logger.Write("Created new player actor for player id ({0})", _cPlayer.PlayerId);
	}
	
	
	void OnEventServerPlayerDisconnect(CNetworkPlayer _cPlayer)
	{
		TNetworkViewId cPlayerActorNetworkViewId = GetPlayerActorViewId(_cPlayer.PlayerId);

		if (cPlayerActorNetworkViewId != null)
		{
			CNetwork.Factory.DestoryGameObject(cPlayerActorNetworkViewId);
			
			// Sync unregister player actor view id with everyone
			InvokeRpcAll("RemoteUnregisterPlayerActor", _cPlayer.PlayerId);

			//Remove player from dictionary of player names
			InvokeRpcAll("UnregisterPlayerName", _cPlayer.PlayerId);

			Logger.Write("Removed Player Actor for Player Id ({0})", _cPlayer.PlayerId);
		}
	}


	void OnEventServerShutdown()
	{
		m_mPlayersActors.Clear();
		m_mPlayerActorsPlayers.Clear();
		m_mPlayersNames.Clear();
	}


	void OnEventConnectionDisconnect()
	{
		if (!CNetwork.IsServer)
		{
			m_mPlayersActors.Clear();
			m_mPlayerActorsPlayers.Clear();
			m_mPlayersActors.Remove(CNetwork.PlayerId);
		}

		m_bSerializeName = true;
	}


	// Create RPC Call to take in playerID (ulong) and string (player username)
	[ANetworkRpc]
	void RegisterPlayerName(ulong _ulPlayerID, string _sPlayerUserName)
	{
		if(m_mPlayersNames.ContainsKey(_ulPlayerID))
		{
			m_mPlayersNames[_ulPlayerID] = _sPlayerUserName;
			//Debug.LogError("Changed Player Name: " + _sPlayerUserName);
		}
		else
		{
			m_mPlayersNames.Add(_ulPlayerID, _sPlayerUserName);
			//Debug.LogError("Added player: " + _sPlayerUserName);
		}

		// Call this after the name is registered
		if(EventPlayerJoin != null)
			EventPlayerJoin(_ulPlayerID);
	}


	[ANetworkRpc]
	void UnregisterPlayerName(ulong _ulPlayerID)
	{
		if(m_mPlayersNames.ContainsKey(_ulPlayerID))
		{
			m_mPlayersNames.Remove(_ulPlayerID);
		}

		// Call this after the name is unregistered
		if(EventPlayerLeave != null)
			EventPlayerLeave(_ulPlayerID);
	}


	[ANetworkRpc]
	void RemoteRegisterPlayerActor(ulong _ulPlayerId, TNetworkViewId _cPlayerActorId)
	{
		m_mPlayersActors.Add(_ulPlayerId, _cPlayerActorId);
		m_mPlayerActorsPlayers.Add(_cPlayerActorId, _ulPlayerId);

        if (EventActorRegister != null) EventActorRegister(_ulPlayerId, _cPlayerActorId.GameObject);
	}
	
	
	[ANetworkRpc]
	void RemoteUnregisterPlayerActor(ulong _ulPlayerId)
	{
		m_mPlayerActorsPlayers.Remove(m_mPlayersActors[_ulPlayerId]);
		m_mPlayersActors.Remove(_ulPlayerId);

		m_aUnspawnedPlayers.Remove(_ulPlayerId);

        if (EventActorUnregister != null) EventActorUnregister(_ulPlayerId);
	}


    void OnGUI()
    {
        if (CNetwork.IsConnectedToServer)
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
                if (Input.GetKey(KeyCode.Tab))
                {
                    GUI.Box(new Rect(100, 100, 400, 400), "Player List ");

                    int iStartY = 115;

                    foreach (KeyValuePair<ulong, string> entry in m_mPlayersNames)
                    {
                        GUI.Label(new Rect(110, iStartY, 400, 400), "Player: " + entry.Value);
                        iStartY += 10;
                    }
                }
            }
        }
    }


	void OnPlayerNameChange(string _sPlayerName)
	{
		ulong ulPlayerID = CNetwork.PlayerId;

		m_mPlayersNames[ulPlayerID] = _sPlayerName;
		m_sPlayerName = _sPlayerName;

		CGamePlayers.m_bSerializeName = true;
	}


// Member Fields


	Dictionary<ulong, TNetworkViewId> m_mPlayersActors = new Dictionary<ulong, TNetworkViewId>();
	Dictionary<TNetworkViewId, ulong> m_mPlayerActorsPlayers = new Dictionary<TNetworkViewId, ulong>();
	Dictionary<ulong, string> m_mPlayersNames = new Dictionary<ulong, string>();

	List<ulong> m_aUnspawnedPlayers = new List<ulong>();

	CNetworkVar<string> m_sNetworkedPlayerName = null;
	List<string> m_PlayerNamesList = new List<string>();

	string m_sPlayerName = System.Environment.UserDomainName + ": " + System.Environment.UserName;

	static CGamePlayers s_cInstance = null;

	static bool m_bSerializeName = true;


};
