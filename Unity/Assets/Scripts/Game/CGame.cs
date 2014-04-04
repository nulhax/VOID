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
using System.Xml.Linq;


[RequireComponent(typeof(CGamePlayers))]
[RequireComponent(typeof(CGameRegistrator))]
[RequireComponent(typeof(CGameShips))]
public class CGame : CNetworkMonoBehaviour
{

// Member Types


    public const ushort kusServerPort = 1337;


// Member Delegates & Events
	public delegate void NotifyNameChange(string _sNewPlayerName);
	
	public event NotifyNameChange EventNameChange; 

// Member Properties
	public string PlayerName
	{
		get { return(m_sPlayerName); }
		set { m_sPlayerName = value; }
	}
	
	public static CGame Instance
	{
		get { return (s_cInstance); }
	}

    public DungeonMaster DungeonMasterInstance { get { return m_DungeonMaster; } }

// Member Functions


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
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
		CNetwork.Server.EventStartup += new CNetworkServer.NotifyStartup(OnServerStartup);
        CNetwork.Connection.EventConnectionAccepted += new CNetworkConnection.OnConnect(OnConnect);
        CNetwork.Connection.EventDisconnect += new CNetworkConnection.OnDisconnect(OnDisconnect);
	
		// Start server (Development Only)
		CNetwork.Server.Startup(kusServerPort, m_sServerTitle, "DefaultName", 8);

		// Connect to server (Development Only)
		CNetwork.Connection.ConnectToServer("localhost", kusServerPort, "");

        // Initialise the dungeon master
        // Note: This may need to be moved should the lobby system change
        if (InitialiseDungeonMaster)
        {
            m_DungeonMaster = gameObject.AddComponent<DungeonMaster>();
        }
    }
	

	public void Update()
	{
		DebugProcessInputs();
	}


    public void OnGUI()
    {
		float fViewWidth = 450;
		float fPositionX = Screen.width / 2 - fViewWidth / 2;
		float fPositionY = Screen.height / 2 + 50;
		float fScreenCenterX = Screen.width / 2;
		float fScreenCenterY = Screen.height / 2;

		// Host server
		if (!CNetwork.Connection.IsConnected && 
            !CNetwork.Server.IsActive)
        {
			GUI.Label(new Rect(fPositionX, fPositionY - m_fTextLayoutOffset, m_fInputFieldWidth, m_fInputFieldHeight), "Server Title");
            m_sServerTitle = GUI.TextField(new Rect(fPositionX, fPositionY - m_fTextFieldOffset, m_fInputFieldWidth, m_fInputFieldHeight), m_sServerTitle, 32);
			m_fNumSlots = GUI.HorizontalSlider(new Rect(fScreenCenterX - 230, fScreenCenterY - 50, 200, 30), m_fNumSlots, 1.0f, 32.0f);
			GUI.Label(new Rect(fScreenCenterX - 158, fScreenCenterY - 80, 100, 30), "Slots: " + ((uint)m_fNumSlots).ToString());



			if (GUI.Button(new Rect(fScreenCenterX + 60, fScreenCenterY - 80, 160, 50), "Start Server") &&
				m_sServerTitle.Length > 1 &&
				m_fNumSlots > 0)
			{
				CNetwork.Server.Startup(kusServerPort, m_sServerTitle, m_sPlayerName, (uint)m_fNumSlots);
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


	void DrawLobbyGui()
	{
		float fViewWidth = 450;
		float fViewHeight = 150;
		float fPositionX = Screen.width / 2 - fViewWidth / 2;
		float fPositionY = Screen.height / 2 + 50;
		float fScreenCenterX = Screen.width / 2;
		float fScreenCenterY = Screen.height / 2;

		// Player Naming

        GUI.Label(new Rect(fPositionX + fViewWidth - m_fInputFieldWidth, fPositionY - m_fTextLayoutOffset, m_fInputFieldWidth, m_fInputFieldHeight), "Player Name");

        string sPlayerName = GUI.TextField(new Rect(fPositionX + fViewWidth - m_fInputFieldWidth, fPositionY - m_fTextFieldOffset, m_fInputFieldWidth, m_fInputFieldHeight), m_sPlayerName, 32);
		//PlayerName = GUI.TextField(new Rect(fScreenCenterX + 230, fScreenCenterY - 150, 200, 30), m_sPlayerName, 32);
		
		if(PlayerName != sPlayerName)
		{
			PlayerName = sPlayerName;
			EventNameChange(PlayerName);
		}

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

        // Text field for IP input
        m_strRemoteServerIP = GUI.TextField(new Rect(10, 200, 200, 20), m_strRemoteServerIP);

        // Text field for port input
        m_strRemoteServerPort = GUI.TextField(new Rect(10, 225, 200, 20), m_strRemoteServerPort);

        // Convert text string of port number to ushort
        ushort.TryParse(m_strRemoteServerPort, out m_usRemoteServerPort);

        // If the port is above 999
        if (m_usRemoteServerPort >= 1000)
        {
            // if GUI.Button "Connect" is pressed
            if (GUI.Button(new Rect(10, 250, 200, 20), "Connect"))
            {
                // Attempt to connect to server using the above text fields
                CNetwork.Connection.ConnectToServer(m_strRemoteServerIP, m_usRemoteServerPort, "");
            }
        }
	}


	void DebugProcessInputs()
	{
		// Lock Cursor toggle
		if(Input.GetKeyDown(KeyCode.F1))
		{
			Screen.lockCursor = !Screen.lockCursor;
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


	void OnServerStartup()
	{
        // DO FIRST (i.e. before anything in the game world is created), as the galaxy has no dependencies, but some objects depend on the galaxy.
		CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.Galaxy);    // The server manages the galaxy - the clients just receive notifications when stuff appears and disappears.
	}


    void OnConnect()
    {

    }


	void OnDisconnect()
	{
		if(!CNetwork.IsServer)
		{
			CUserInput.UnsubscribeAll();
		}
	}


	void OnApplicationFocus(bool _bFocused)
	{
	}


// Member Variables


	string m_sServerTitle = System.Environment.UserDomainName + ": " + System.Environment.UserName;
	string m_sPlayerName = "Enter name";
	string[] m_saTabTitles = { "Online Servers", "Lan Servers" };


	float m_fNumSlots = 16.0f;
	

	int m_iActiveTab = 1;

    // Manual server connection variables
    string m_strRemoteServerIP = "127.0.0.1";
    string m_strRemoteServerPort = "1337";
    ushort m_usRemoteServerPort = 0;

    const float m_fInputFieldWidth  = 200;
    const float m_fInputFieldHeight = 20;
    const float m_fTextFieldOffset  = 200;
    const float m_fTextLayoutOffset = 220;
	static CGame s_cInstance = null;

    // Dungeon Master
    private DungeonMaster m_DungeonMaster = null;

    // Debug
    public bool InitialiseDungeonMaster = true;
};