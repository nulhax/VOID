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
	public GameObject ServerChild;

	public const ushort kusServerPort = 1337;

	// Text inputs from NGUI
	public void ServerName()
	{
		m_sServerTitle = UIInput.current.value;
	}

	public void Port()
	{
		m_strRemoteServerPort = UIInput.current.value;
		
		ushort.TryParse(m_strRemoteServerPort, out m_usRemoteServerPort);
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
	}

	// User select Inputs from NGUI
	void RefreshServerList(CNetworkScanner.TServer _tServer)
	{
		if(ServerItem == null)
		{
			// do nothing
		}
		else
		{
			ServerChild = NGUITools.AddChild(ServerTable, ServerItem);
			ServerChild.SetActive(true);

			UILabel Label = ServerChild.GetComponentInChildren<UILabel>();

			Label.text = "ServerInfo " + _tServer.tServerInfo.sTitle;
		
			ServerTable.GetComponent<UITable>().Reposition();
		}
	}

	public void CreateServer()
	{
		ServerChild = NGUITools.AddChild(ServerTable, ServerItem);
		ServerChild.SetActive(true);

		UILabel Label = ServerChild.GetComponentInChildren<UILabel>();
		Label.text = "Server/Port " + m_sServerTitle + m_strRemoteServerPort;

		ServerTable.GetComponent<UITable>().Reposition();

		CNetwork.Server.Startup(kusServerPort, m_sServerTitle, m_sPlayerName, (uint)m_fNumSlots);
	}

	public void ShutdownServer()
	{
		CNetwork.Server.Shutdown();
	}

	public void Connect()
	{
		CNetwork.Server.Startup(kusServerPort, m_sServerTitle, m_sPlayerName, (uint)m_fNumSlots);

		Application.LoadLevel("Default");

	}

	public void DisconnectServer()
	{
		CNetwork.Connection.Disconnect();
	}

	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
		// Empty
	}

	// Use this for initialization
	void Start () 
	{
		List<CNetworkScanner.TServer> aServerList = null;

		CNetwork.Scanner.EventFoundServer += RefreshServerList;
	}
	
	// Update is called once per frame
	void Update () 
	{
	}


	void ServerSetUp()
	{
		// Set the active server list to draw

		// Set to online servers tab

		// Set to lan servers

		// Title
		
		// Slots
		
		// Latency
		
		// Connect button

		// If the port is above 999
	}

	// Private members
	List<CNetworkScanner.TServer> aServerList = null;

	string m_sServerTitle = System.Environment.UserDomainName + ": " + System.Environment.UserName;
	string m_sPlayerName = "Enter name";
	string[] m_saTabTitles = { "Online Servers", "Lan Servers" };

	int m_iServerListSize = 0;
	bool m_bIsDisplayed = false;

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