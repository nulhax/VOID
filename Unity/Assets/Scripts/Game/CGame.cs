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


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
		// Empty
	}


    public void Awake()
    {
		// Load in variables from Main menu server connection
		m_strRemoteServerIP = PlayerPrefs.GetString("IP Address");
		string port = PlayerPrefs.GetString("Server Port");
		ushort.TryParse(port, out m_usRemoteServerPort);

		m_Server = PlayerPrefs.GetInt("Server");

		Application.runInBackground = true;

        s_cInstance = this;
    }


    public void Start()
    {
		// Sign up to events
		CNetwork.Server.EventStartup += new CNetworkServer.NotifyStartup(OnServerStartup);
        CNetwork.Connection.EventConnectionAccepted += new CNetworkConnection.OnConnect(OnConnect);
        CNetwork.Connection.EventDisconnect += new CNetworkConnection.OnDisconnect(OnDisconnect);
	
		// Start server Host only
		if(m_Server == 1)
		{
			// Host join
			CNetwork.Server.Startup(m_usRemoteServerPort, m_sServerTitle, "DefaultName", 16);
			CNetwork.Connection.ConnectToServer(m_strRemoteServerIP, m_usRemoteServerPort, "");
		}
		else
		{
			// Client join
			CNetwork.Connection.ConnectToServer(m_strRemoteServerIP, m_usRemoteServerPort, "");
		}

        // Initialise the dungeon master
        // Note: This may need to be moved should the lobby system change
        if (CNetwork.IsServer &&
            InitialiseDungeonMaster)
        {
            m_DungeonMaster = gameObject.AddComponent<DungeonMaster>();
        }
    }
	

	public void Update()
	{
		DebugProcessInputs();
	}
	
	void DebugProcessInputs()
	{
		// Quick quit game
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			CNetwork.Connection.Disconnect();

			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#endif
		}
	}


	void OnServerStartup()
	{
        // DO FIRST (i.e. before anything in the game world is created), as the galaxy has no dependencies, but some objects depend on the galaxy.
		CNetwork.Factory.CreateGameObject(CGameRegistrator.ENetworkPrefab.Galaxy);    // The server manages the galaxy - the clients just receive notifications when stuff appears and disappears.
	}


    void OnConnect()
    {

    }


	void OnDisconnect()
	{

		if(!CNetwork.IsServer)
		{
			CUserInput.UnsubscribeAll();
			Application.LoadLevel("MainMenu");
		}

		if(CNetwork.IsServer)
		{
			CNetwork.Server.Shutdown();
			Application.LoadLevel("MainMenu");
		}
	}


// Member Variables


	string m_sServerTitle = System.Environment.UserDomainName + ": " + System.Environment.UserName;
	string m_sPlayerName = "Enter name";
	string[] m_saTabTitles = { "Online Servers", "Lan Servers" };

	float m_fNumSlots = 16.0f;
	

	int m_iActiveTab = 1;

	// Can't save bool in PlayerPref, use int 0 - 1
	int m_Client = 0;
	int m_Server = 0;

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