//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CGame.cs
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


public class CGame : CNetworkMonoBehaviour
{

// Member Types


	public const ushort kusServerPort = 9836;


	public enum ENetworkRegisteredPrefab : ushort
	{
		// DO NOT MAKE THIS LOOK SHIT - put your shit in a LOGICAL place please!
		// DO NOT MAKE THIS LOOK SHIT - put your shit in a LOGICAL place please!
		// DO NOT MAKE THIS LOOK SHIT - put your shit in a LOGICAL place please!
		// DO NOT MAKE THIS LOOK SHIT - put your shit in a LOGICAL place please!
		
		INVALID,
		
		// Ships
		Ship,
		GalaxyShip,
		
		// Ship: Facilities
		FacilityBridge, 
		FacilityFactory,
		FacilityScanner,
		FacilityReplicator,
		FacilityEngine,
		FacilityGravityGenerator, 
		FacilityShieldGenerator, 
		FacilityLifeSupport,
		
		// Ship: Doors
		Door,
		
		// Ship: Hallways
		HallwayStraight,
		HallwayCorner,
		HallwayTSection, 
		HallwayXSection,
		
		// Player
        PlayerActor,
		
		// Register prefabs: Tools
        ToolTorch, 
		ToolRachet, 
		ToolExtinguisher, 
		
		// Galaxy
		GalaxyParent,
		Asteroid_FIRST,
		Asteroid_LAST = Asteroid_FIRST + 3,
		
		// Hazards
        Fire,
		
		// Modules: General
		BlackMatterCell,
		FuelCell,
		PlasmaCell,
		PowerCell,
		PanelFuseBox,
		ControlConsole,
		
		// Modules: Bridge
		Cockpit,
		
		MAX
	}


// Member Delegates & Events


// Member Properties
	
	
	public static CGame Instance
	{
		get { return (s_cInstance); }
	}
	
	public static GameObject PlayerActor
	{
		get 
		{ 
			GameObject playerActor = null;
			
			if(Instance.m_usActorViewId != 0)
			{
				playerActor = CNetwork.Factory.FindObject(Instance.m_usActorViewId);
			}
			
			return(playerActor); 
		}
	}
	
	public static ushort PlayerActorViewId
	{
		get { return (s_cInstance.m_usActorViewId); }
	}
	
	public static List<GameObject> PlayerActors
	{
		get 
		{ 
			List<GameObject> actors = new List<GameObject>();
				
			foreach(ushort playerID in s_cInstance.m_mPlayersActor.Values)
			{
				actors.Add(CNetwork.Factory.FindObject(playerID));
			}
			
			return (actors); 
		}
	}

	public static GameObject Ship
	{
		get { return (CNetwork.Factory.FindObject(s_cInstance.m_usShipViewId)); }
	}

	public static ushort ShipViewId
	{
		get { return (s_cInstance.m_usShipViewId); }
	}
	
	public static GameObject GalaxyShip
	{
		get { return (Ship.GetComponent<CShipGalaxySimulatior>().GalaxyShip); }
	}

// Member Functions


	public override void InstanceNetworkVars()
	{
		// Empty
	}


    public void Awake()
    {
		Application.runInBackground = true;
        s_cInstance = this;
    }


    public void Start()
    {
		// Sign up to events
		CNetwork.Server.EventPlayerConnect += new CNetworkServer.NotifyPlayerConnect(OnPlayerJoin);
		CNetwork.Server.EventPlayerDisconnect += new CNetworkServer.NotifyPlayerDisconnect(OnPlayerDisconnect);
		CNetwork.Server.EventStartup += new CNetworkServer.NotifyStartup(OnServerStartup);
        CNetwork.Server.EventShutdown += new CNetworkServer.NotifyShutdown(OnServerShutdown);
        CNetwork.Connection.EventConnectionAccepted += new CNetworkConnection.OnConnect(OnConnect);
        CNetwork.Connection.EventDisconnect += new CNetworkConnection.OnDisconnect(OnDisconnect);
		
		// Register Prefabs. Register your prefabs inside this function
		RegisterPrefabs();

		// Register serialization targets
        CNetworkConnection.RegisterThrottledSerializationTarget(CPlayerMotor.SerializePlayerState, CPlayerMotor.UnserializePlayerState);
		CNetworkConnection.RegisterThrottledSerializationTarget(CPlayerHead.SerializePlayerState, CPlayerHead.UnserializePlayerState);
		CNetworkConnection.RegisterThrottledSerializationTarget(CBridgeCockpit.SerializeCockpitInteractions, CBridgeCockpit.UnserializeCockpitInteractions);
       	CNetworkConnection.RegisterThrottledSerializationTarget(CDUIInteraction.SerializeDUIInteractions, CDUIInteraction.UnserializeDUIInteraction);
		CNetworkConnection.RegisterSerializationTarget(CPlayerBelt.SerializeBeltState, CPlayerBelt.UnserializeBeltState);
		CNetworkConnection.RegisterSerializationTarget(CPlayerBackPack.SerializeOutbound, CPlayerBackPack.UnserializeInbound);
		
		// Start server (Development Only)
		CNetwork.Server.Startup(kusServerPort, m_sServerTitle, 8);

		// Connect to server (Development Only)
		CNetwork.Connection.ConnectToServer("localhost", kusServerPort, "");
    }
	
	private void RegisterPrefabs()
	{
		// Ships
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.Ship, "Ship/Ship");
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.GalaxyShip, "Ship/WorldShip");
		
		// Ship: Facilities
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.FacilityBridge, "Ship/Facilities/Bridge/Bridge");
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.FacilityFactory, "Ship/Facilities/Factory/Factory");
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.FacilityScanner, "Ship/Facilities/Scanner/Scanner");
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.FacilityReplicator, "Ship/Facilities/Replicator/Replicator");
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.FacilityEngine, "Ship/Facilities/Engine/Engine");
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.FacilityGravityGenerator, "Ship/Facilities/Gravity Generator/GravityGenerator");
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.FacilityShieldGenerator, "Ship/Facilities/Shield Generator/ShieldGenerator");
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.FacilityLifeSupport, "Ship/Facilities/Life Support/LifeSupport");
		
		// Ship: Doors
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.Door, "Ship/Doors/Door");
		
		// Ship: Hallways
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.HallwayStraight, "Ship/Hallways/HallwayStraight");
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.HallwayCorner, "Ship/Hallways/HallwayCorner");
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.HallwayTSection, "Ship/Hallways/HallwayTSection");
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.HallwayXSection, "Ship/Hallways/HallwayXSection");
		
		// Player
        CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.PlayerActor, "Player/Player Actor");
		
		// Register prefabs: Tools
        CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.ToolTorch, "Tools/ToolTorch");
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.ToolRachet, "Tools/ToolRachet");
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.ToolExtinguisher, "Tools/ToolExtinguisher");
		
		// Galaxy
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.GalaxyParent, "GalaxyParent");
        for(ushort us = 0; us <= ENetworkRegisteredPrefab.Asteroid_LAST - ENetworkRegisteredPrefab.Asteroid_FIRST; ++us)    // All asteroids.
            CNetwork.Factory.RegisterPrefab((ushort)((ushort)ENetworkRegisteredPrefab.Asteroid_FIRST + us), "Galaxy/Asteroid" + us.ToString());
		
		// Hazards
        CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.Fire, "Hazards/Fire");
		
		// Modules: General
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.BlackMatterCell, "Modules/General/BlackMatterCell");
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.FuelCell, "Modules/General/FuelCell");
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.PlasmaCell, "Modules/General/PlasmaCell");
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.PowerCell, "Modules/General/PowerCell");
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.PanelFuseBox, "Modules/General/PanelFuseBox");
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.ControlConsole, "Modules/General/DUI/CurvedMonitor_wide");
		
		// Modules: Bridge
		CNetwork.Factory.RegisterPrefab(ENetworkRegisteredPrefab.Cockpit, "Ship/Facilities/Bridge/Cockpit");
		
	}

	public void Update()
	{
		DebugProcessInputs();
	}


    public void OnGUI()
    {
        // Host server
		if (!CNetwork.Connection.IsConnected && 
            !CNetwork.Server.IsActive)
        {
			float fScreenCenterX = Screen.width / 2;
			float fScreenCenterY = Screen.height / 2;

			GUI.Label(new Rect(fScreenCenterX - 226, fScreenCenterY - 180, 100, 30), "Server Title");
			m_sServerTitle = GUI.TextField(new Rect(fScreenCenterX - 230, fScreenCenterY - 150, 200, 30), m_sServerTitle, 32);
			m_fNumSlots = GUI.HorizontalSlider(new Rect(fScreenCenterX - 230, fScreenCenterY - 50, 200, 30), m_fNumSlots, 1.0f, 32.0f);
			GUI.Label(new Rect(fScreenCenterX - 158, fScreenCenterY - 80, 100, 30), "Slots: " + ((uint)m_fNumSlots).ToString());

			if (GUI.Button(new Rect(fScreenCenterX + 60, fScreenCenterY - 80, 160, 50), "Start Server") &&
				m_sServerTitle.Length > 1 &&
				m_fNumSlots > 0)
			{
				CNetwork.Server.Startup(kusServerPort, m_sServerTitle, (uint)m_fNumSlots);
			}
        }

        // Shutdown server
        if (CNetwork.IsServer &&
            GUI.Button(new Rect(140, 20, 130, 50), "Shutdown Server"))
        {
            CNetwork.Server.Shutdown();
        }

        // Disconnection from server
        if (CNetwork.Connection.IsConnected &&
            GUI.Button(new Rect(20, 20, 100, 50), "Disconnect"))
        {
            CNetwork.Connection.Disconnect();
        }

        // Draw lan server list
		if (!CNetwork.Connection.IsConnected)
        {
			DrawLobbyGui();
        }
    }
	

	public static GameObject FindPlayerActor(ulong _ulPlayerId)
	{
		return (CNetwork.Factory.FindObject(s_cInstance.m_mPlayersActor[_ulPlayerId]));
	}


	void DrawLobbyGui()
	{
		float fViewWidth = 450;
		float fViewHeight = 150;
		float fPositionX = Screen.width / 2 - fViewWidth / 2;
		float fPositionY = Screen.height / 2 + 50;

		// Tab
		m_iActiveTab = GUI.Toolbar(new Rect(fPositionX, fPositionY - 30, 250, 30), m_iActiveTab, m_saTabTitles);

		// Set the active server list to draw
		List<CNetworkScanner.TServer> aServerList = null;

		// Set to online servers
		if (m_iActiveTab == 0)
		{
			aServerList = CNetwork.Scanner.GetOnlineServers();

			if (GUI.Button(new Rect(fPositionX + fViewWidth / 4, fPositionY + fViewHeight, 225, 30), "Refresh Online Servers"))
			{
				CNetwork.Scanner.RefreshOnlineServers();
			}
		}

		// Set to lan servers
		else
		{
			aServerList = CNetwork.Scanner.GetLanServers();

			if (GUI.Button(new Rect(fPositionX + fViewWidth / 4, fPositionY + fViewHeight, 225, 30), "Refresh Lan Servers"))
			{
				CNetwork.Scanner.RefreshLanServers(kusServerPort);
			}
		}

		// Background image
		GUI.Box(new Rect(fPositionX, fPositionY, fViewWidth, fViewHeight), "");

		// Start scroll box
		//GUI.BeginScrollView(new Rect(fPositionX, fPositionY, fViewWidth, fViewHeight), Vector2.zero, new Rect(0, 0, fViewWidth - 20, fViewHeight + 1), false, true);
		//GUILayout.BeginScrollView(new Vector2(fPositionX, fPositionY), GUILayout.Width(fViewWidth), GUILayout.Height(fViewHeight));
		GUILayout.BeginArea(new Rect(fPositionX, fPositionY, fViewWidth, fViewHeight));
		GUILayout.BeginVertical();
		GUILayout.Space(14);
		

		foreach (CNetworkScanner.TServer tServer in aServerList)
		{
			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			GUILayout.Space(14);

			// Title
			GUILayout.Label(tServer.tServerInfo.sTitle);
			GUILayout.Space(5);

			// Slots
			GUILayout.Label(tServer.tServerInfo.bNumAvaiableSlots + " / " + tServer.tServerInfo.bNumSlots);
			GUILayout.Space(5);

			// Latency
			GUILayout.Label(tServer.uiLatency.ToString());
			GUILayout.Space(5);

			// Connect button
			if ((CNetwork.IsServer &&
				tServer.cGuid.g == CNetwork.Server.RakPeer.GetMyGUID().g ||
				!CNetwork.IsServer) &&
				GUILayout.Button("Connect"))
			{
				CNetwork.Connection.ConnectToServer(tServer.sIp, tServer.usPort, "");
			}

			GUILayout.Space(14);
			GUILayout.EndHorizontal();
		}



		GUILayout.Space(14);
		GUILayout.EndVertical();
		GUILayout.EndArea();

		// End scroll box
	//GUILayout.EndScrollView(
	}


	void DebugProcessInputs()
	{
		// Lock Cursor toggle
		if(Input.GetKeyDown(KeyCode.F1))
		{
			Screen.lockCursor = !Screen.lockCursor;

			if (CNetwork.Connection.IsConnected &&
				!CNetwork.Connection.IsDownloadingInitialGameData)
			{
				PlayerActor.GetComponent<CPlayerHead>().InputFrozen = !Screen.lockCursor;
			}
		}

		// Quick quit game
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();

			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#endif
		}
	}


	void OnPlayerJoin(CNetworkPlayer _cPlayer)
	{
		// Send created objects to new player
		CNetwork.Factory.SyncPlayer(_cPlayer);
		
		// Create new player's actor
		GameObject cPlayerActor = CNetwork.Factory.CreateObject((ushort)ENetworkRegisteredPrefab.PlayerActor);
		
		// Set the parent as the ship
		cPlayerActor.transform.parent = Ship.transform;
		cPlayerActor.GetComponent<CNetworkView>().SyncParent();
		
		// Get actor network view id
		ushort usActorNetworkViewId = cPlayerActor.GetComponent<CNetworkView>().ViewId;

		// Save which player owns which actor
		m_mPlayersActor.Add(_cPlayer.PlayerId, usActorNetworkViewId);

		// Tell connecting player to update their network player id 
		InvokeRpc(_cPlayer.PlayerId, "SetActorNetworkViewId", usActorNetworkViewId);

		// Tell connecting player which is the ship's network view id
		InvokeRpc(_cPlayer.PlayerId, "SetShipNetworkViewId", m_usShipViewId);
		
		Logger.Write("Created new player actor for player id ({0})", _cPlayer.PlayerId);
		
		// Placeholder Test stuff
      	CNetwork.Factory.CreateObject(ENetworkRegisteredPrefab.ToolTorch);
		CNetwork.Factory.CreateObject(ENetworkRegisteredPrefab.ToolRachet);
		CNetwork.Factory.CreateObject(ENetworkRegisteredPrefab.BlackMatterCell);
		CNetwork.Factory.CreateObject(ENetworkRegisteredPrefab.FuelCell);
		CNetwork.Factory.CreateObject(ENetworkRegisteredPrefab.PlasmaCell);
		CNetwork.Factory.CreateObject(ENetworkRegisteredPrefab.PowerCell);

		CNetwork.Factory.CreateObject(ENetworkRegisteredPrefab.Fire);
		CNetwork.Factory.CreateObject(ENetworkRegisteredPrefab.ToolExtinguisher);


		_cPlayer.SetDownloadingInitialGameStateComplete();
	}


	void OnPlayerDisconnect(CNetworkPlayer _cPlayer)
	{
		ushort usPlayerActorNetworkViewId = m_mPlayersActor[_cPlayer.PlayerId];


		CNetwork.Factory.DestoryObject(usPlayerActorNetworkViewId);


		m_mPlayersActor.Remove(_cPlayer.PlayerId);


		Logger.Write("Removed Player Actor for Player Id ({0})", _cPlayer.PlayerId);
	}


	void OnServerStartup()
	{
        System.Diagnostics.Debug.Assert(CNetwork.IsServer);

        // DO FIRST (i.e. before anything in the game world is created).
        // The server manages the galaxy - the clients just receive notifications when stuff appears and disappears.
        gameObject.AddComponent<CGalaxy>();

		// Create ship object
		GameObject cShipObject = CNetwork.Factory.CreateObject(ENetworkRegisteredPrefab.Ship);

		// Save view id
		m_usShipViewId = cShipObject.GetComponent<CNetworkView>().ViewId;
		
		cShipObject.GetComponent<CShipFacilities>().CreateFacility(CFacilityInterface.EFacilityType.Bridge, 0);
	}


	void OnServerShutdown()
	{
        System.Diagnostics.Debug.Assert(CNetwork.IsServer);

		m_mPlayersActor.Clear();
		m_usActorViewId = 0;
        m_usShipViewId = 0;

        // DO LAST (i.e. after everything in the game world is destroyed).
        Destroy(gameObject.GetComponent<CGalaxy>());
	}


    void OnConnect()
    {
        // DO FIRST (i.e. before anything in the game world is created).
        // The server manages the galaxy - the clients just receive notifications when stuff appears and disappears.
        if(!CNetwork.IsServer)
            gameObject.AddComponent<CGalaxy>();
    }


	void OnDisconnect()
	{
		GameObject.Find("Main Camera").camera.enabled = true;
		
		m_usActorViewId = 0;
		
        // DO LAST (i.e. after everything in the game world is destroyed).
        if (!CNetwork.IsServer)
            Destroy(gameObject.GetComponent<CGalaxy>());
	}


	void OnApplicationFocus(bool _bFocused)
	{
	}


	[ANetworkRpc]
	void SetActorNetworkViewId(ushort _usActorViewId)
	{
		m_usActorViewId = _usActorViewId;

		// Notice
		Logger.Write("My actor network view id is ({0})", m_usActorViewId);
	}


	[ANetworkRpc]
	void SetShipNetworkViewId(ushort _usShipViewId)
	{
		m_usShipViewId = _usShipViewId;

		// Notice
		Logger.Write("The ship's network view id is ({0})", m_usShipViewId);
	}


// Member Variables


	string m_sServerTitle = System.Environment.UserDomainName + ": " + System.Environment.UserName;
	string[] m_saTabTitles = { "Online Servers", "Lan Servers" };


	float m_fNumSlots = 16.0f;
	

	int m_iActiveTab = 1;


	ushort m_usActorViewId = 0;
	ushort m_usShipViewId = 0;


	Dictionary<ulong, ushort> m_mPlayersActor = new Dictionary<ulong, ushort>();

	
	static CGame s_cInstance = null;


};