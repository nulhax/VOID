﻿//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   Game.cs
//  Description :   --------------------------
//
//  Author  	:  Programming Team
//  Mail    	:  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CGame : CNetworkMonoBehaviour
{

// Member Types


	public const ushort kusServerPort = 30001;


	public enum EPrefab
	{
		PlayerActor,
	}


// Member Functions
    
    // public:


	public override void InitialiseNetworkVars()
	{
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
		CNetwork.Server.EventShutdown += new CNetworkServer.NotifyShutdown(OnServerShutdown);
		CNetwork.Connection.EventDisconnect +=new CNetworkConnection.OnDisconnect(OnDisconnect);

		// Register prefabs
		CNetwork.Factory.RegisterPrefab((ushort)EPrefab.PlayerActor, "Player Actor");

		// Register serialization targets
        CNetworkConnection.RegisterSerializationTarget(CActorMotor.SerializePlayerInput, CActorMotor.UnserializePlayerInput);


		CNetwork.Server.Startup(kusServerPort, "Developer Server", 8);
		CNetwork.Connection.ConnectToServer("localhost", kusServerPort, "");
    }
    

	public void Update()
	{
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


	public static GameObject Actor
	{
		get { return (CNetwork.Factory.FindObject(Instance.m_usActorNetworkViewId)); }
	}


	public static GameObject FindPlayerActor(ulong _ulPlayerId)
	{
		return (CNetwork.Factory.FindObject(s_cInstance.m_mPlayersActor[_ulPlayerId]));
	}


	public static CGame Instance
	{
		get { return (s_cInstance); }
	}


    // protected:


	protected void OnPlayerJoin(CNetworkPlayer _cPlayer)
	{
		// Send created objects to new player
		CNetwork.Factory.SyncPlayer(_cPlayer);

		// Create new player's actor
		GameObject cPlayerActor = CNetwork.Factory.CreateObject((ushort)EPrefab.PlayerActor);

		// Get actor network view id
		ushort usActorNetworkViewId = cPlayerActor.GetComponent<CNetworkView>().ViewId;

		// Save which player owns which actor
		m_mPlayersActor.Add(_cPlayer.PlayerId, usActorNetworkViewId);

		// Tell connecting player to update their network player id 
		InvokeRpc(_cPlayer.PlayerId, "SetActorNetworkViewId", usActorNetworkViewId);

		// Set start position for new player's actor
		//cPlayerActor.GetComponent<CActorMotor>().PositionX = -14 + 2 * UnityEngine.Random.Range(1, 10);


		Logger.Write("Added New Player Actor for Player Id ({0})", _cPlayer.PlayerId);
	}


	protected void OnPlayerDisconnect(CNetworkPlayer _cPlayer)
	{
		ushort usPlayerActorNetworkViewId = m_mPlayersActor[_cPlayer.PlayerId];


		CNetwork.Factory.DestoryObject(usPlayerActorNetworkViewId);


		m_mPlayersActor.Remove(_cPlayer.PlayerId);


		Logger.Write("Removed Player Actor for Player Id ({0})", _cPlayer.PlayerId);
	}


	protected void OnServerShutdown()
	{
		m_mPlayersActor.Clear();
		m_usActorNetworkViewId = 0;
	}


	protected void OnDisconnect()
	{
		GameObject.Find("Main Camera").camera.enabled = true;
		m_usActorNetworkViewId = 0;
	}


    // private:


	[ANetworkRpc]
	void SetActorNetworkViewId(ushort _usActorId)
	{
		m_usActorNetworkViewId = _usActorId;

		// Notice
		Logger.Write("My actor network view id is ({0})", m_usActorNetworkViewId);

		CNetwork.Factory.FindObject(m_usActorNetworkViewId).AddComponent<CActorCamera>();
	}


// Member Variables
    
    // public:


    // private:


	float m_fNumSlots = 16.0f;
	string m_sServerTitle = "Default Title";
	int m_iActiveTab = 1;
	string[] m_saTabTitles = { "Online Servers", "Lan Servers" };


	ushort m_usActorNetworkViewId = 0;


	Dictionary<ulong, ushort> m_mPlayersActor = new Dictionary<ulong, ushort>();

	
	static CGame s_cInstance = null;


};