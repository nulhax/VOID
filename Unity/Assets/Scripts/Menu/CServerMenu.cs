//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CServerMenu.cs
//  Description :   Allows the users to select and control server connection properties
//
//  Author  	:  Scott Emery
//  Mail    	:  scott.ipod@gmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Xml.Linq;

/* Implementation */
public class CServerMenu : CNetworkMonoBehaviour 
{
	public GameObject ServerTable;
	public GameObject ServerItem;
	GameObject ServerClone;

	public const ushort kusServerPort = 1337;

	// Text inputs from NGUI
	public void ServerName()
	{
		m_sServerTitle = UIInput.current.value;
	}

	public void ServerIPAddress()
	{
		m_strRemoteServerIP = UIInput.current.value;
	}

	public void PlayerName()
	{
		m_sPlayerName = UIInput.current.value;
	}

	public void RefreshButton()
	{
		CNetwork.Scanner.RefreshLanServers(kusServerPort);

		aServerList = CNetwork.Scanner.GetLanServers();
	}

	// User select Inputs from NGUI
	void RefreshServerList(CNetworkScanner.TServer _tServer)
	{
		ServerClone = NGUITools.AddChild(ServerTable, ServerItem);
		ServerClone.SetActive(true);

		ServerClone.GetComponent<CServerItem>().sServerIP = _tServer.sIp;
		ServerClone.GetComponent<CServerItem>().usServerPort = _tServer.usPort;
		ServerClone.GetComponent<CServerItem>().m_ServerMenu = this;

		UILabel Label = ServerClone.GetComponentInChildren<UILabel>();
		Label.text = "Title: " + _tServer.tServerInfo.sTitle + "\t\t" + _tServer.tServerInfo.bNumSlots + "/" + _tServer.tServerInfo.bNumAvaiableSlots;

		ServerTable.GetComponent<UITable>().Reposition();
	}

	public void CreateServer()
	{
		if (!CNetwork.Connection.IsConnected && 
		    !CNetwork.Server.IsActive)
		{
			// Host creates server
			CNetwork.Server.Startup(kusServerPort, m_sServerTitle, m_sPlayerName, (uint)m_fNumSlots);

			string ip = m_strRemoteServerIP;
			ushort port = kusServerPort;
			string pw = "";
			
			string sPort = port.ToString();

			// Save variables for the CGame to use when starting the default s
			PlayerPrefs.SetString("IP Address", ip);
			PlayerPrefs.SetString("Server Port", sPort); 
			PlayerPrefs.SetString("Server Password", pw);

			if(PlayerPrefs.HasKey("IP Address") && PlayerPrefs.HasKey("Server Port") && PlayerPrefs.HasKey("Server Password"))
			{
				Application.LoadLevel("Default");
			}
		}

	}

	public void ShutdownServer()
	{
		CNetwork.Server.Shutdown();
	}

	public void Connect()
	{
		UIButton current = UIButton.current;
		CServerItem serverItem = CUtility.FindInParents<CServerItem>(current.gameObject);

		string ip = serverItem.sServerIP;
		ushort port = serverItem.usServerPort;
		string pw = serverItem.sServerPassword;

		CNetwork.Connection.ConnectToServer(ip, port, pw);

		string sPort = port.ToString();

		// Save variables for the CGame to use when starting the default s
		PlayerPrefs.SetString("IP Address", ip);
		PlayerPrefs.SetString("Server Port", sPort); 
		PlayerPrefs.SetString("Server Password", pw); 

		if(PlayerPrefs.HasKey("IP Address") && PlayerPrefs.HasKey("Server Port") && PlayerPrefs.HasKey("Server Password"))
		{
			Application.LoadLevel("Default");
		}
	}

	public void DisconnectServer()
	{
		CNetwork.Connection.Disconnect();
		OnDisconnect();
	}

	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
		// Empty
	}

	// Use this for initialization
	void Start () 
	{
		Debug.Log(gameObject.name);

		List<CNetworkScanner.TServer> aServerList = new List<CNetworkScanner.TServer>();

		CNetwork.Scanner.EventFoundServer += RefreshServerList;
//		CNetwork.Server.EventStartup += new CNetworkServer.NotifyStartup(OnServerStartup);
		// CNetwork.Connection.EventConnectionAccepted += new CNetworkConnection.OnConnect(OnConnect);
		CNetwork.Connection.EventDisconnect += new CNetworkConnection.OnDisconnect(OnDisconnect);
	}

	void OnServerStartUp()
	{

	}

	void OnConnect()
	{

	}

	void ConnectToServer(CNetworkScanner.TServer _tServer)
	{

	}

	void OnDisconnect()
	{
		if(!CNetwork.IsServer)
		{
			CUserInput.UnsubscribeAll();
		}
	}

	// Update is called once per frame
	void Update () 
	{
	}

	// Private members
	List<CNetworkScanner.TServer> aServerList = null;

	string m_sServerTitle = System.Environment.UserDomainName + ": " + System.Environment.UserName;
	string m_sPlayerName = "Enter name";
	string[] m_saTabTitles = { "Online Servers", "Lan Servers" };

	int m_iServerListSize = 0;
	bool m_bIsDisplayed = false;

	bool m_bConnectPressed = false;

	float m_fNumSlots = 16.0f;

	// Manual server connection variables
	string m_strRemoteServerIP = "127.0.0.1";
	string m_strRemoteServerPort = "1337";
	ushort m_usRemoteServerPort = 0;
}

// NGUI help
// http://www.tasharen.com/forum/index.php?topic=1501.0
// http://www.tasharen.com/forum/index.php?topic=6752.0
// http://www.tasharen.com/?page_id=693