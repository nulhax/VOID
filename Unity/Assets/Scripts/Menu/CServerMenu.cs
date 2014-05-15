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

	public GameObject AmbientSlider;
	public GameObject EffectSlider;
	public GameObject VoiceSlider;
	public GameObject MusicSlider;

	public const ushort kusServerPort = 1337;

	// Input from Sound Options
	public void Ambient()
	{
		UISlider slider = AmbientSlider.GetComponent<UISlider>();

		if(slider.value < 0)
		{
			slider.value = 0.75f;
		}

		PlayerPrefs.SetFloat("AmbienceVolume", slider.value);
	}
	
	public void Effects()
	{
		UISlider slider = EffectSlider.GetComponent<UISlider>();

		if(slider.value < 0)
		{
			slider.value = 0.75f;
		}

		PlayerPrefs.SetFloat("EffectsVolume", slider.value);
	}
	
	public void Voice()
	{
		UISlider slider = VoiceSlider.GetComponent<UISlider>();

		if(slider.value < 0)
		{
			slider.value = 0.75f;
		}

		PlayerPrefs.SetFloat("VoiceVolume", slider.value);
	}
	
	public void Music()
	{
		UISlider slider = MusicSlider.GetComponent<UISlider>();

		if(slider.value < 0)
		{
			slider.value = 0.75f;
		}

		PlayerPrefs.SetFloat("MusicVolume", slider.value);

		GetComponent<AudioSource>().volume = slider.value;
	}

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
		List<Transform> children = ServerTable.GetComponent<UITable>().children;
		foreach(Transform child in children)
		{
			Destroy(child.gameObject);
		}
		ServerTable.GetComponent<UITable>().children.Clear();
		ServerTable.GetComponent<UITable>().Reposition();

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

			// Save variables for the CGame to use when starting the default scene
			PlayerPrefs.SetString("IP Address", ip);
			PlayerPrefs.SetString("Server Port", sPort); 
			PlayerPrefs.SetString("Server Password", pw);

			m_Server = 1;

			PlayerPrefs.SetInt("Server", m_Server);
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

		//CNetwork.Connection.ConnectToServer(ip, port, pw);

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

		CNetwork.Connection.EventDisconnect += new CNetworkConnection.OnDisconnect(OnDisconnect);

		UISlider slider = AmbientSlider.GetComponent<UISlider>();
		if (PlayerPrefs.HasKey ("AmbienceVolume")) 
		{
            slider.value = PlayerPrefs.GetFloat("AmbienceVolume");
		}
		else
        {
			PlayerPrefs.SetFloat("AmbienceVolume", 0.75f);
			slider.value = 0.75f;
		}

		slider = EffectSlider.GetComponent<UISlider>();

        if (PlayerPrefs.HasKey ("EffectsVolume")) 
        {
            slider.value = PlayerPrefs.GetFloat("EffectsVolume");
        }
        else
        {
            PlayerPrefs.SetFloat("EffectsVolume", 0.75f);
            slider.value = 0.75f;
        }

		slider = MusicSlider.GetComponent<UISlider>();	
        if (PlayerPrefs.HasKey ("MusicVolume")) 
        {
            slider.value = PlayerPrefs.GetFloat("MusicVolume");
        }
        else
        {
            PlayerPrefs.SetFloat("MusicVolume", 0.75f);
            slider.value = 0.75f;
        }

		GetComponent<AudioSource>().volume = slider.value;

		slider = VoiceSlider.GetComponent<UISlider>();	
        if (PlayerPrefs.HasKey ("VoiceVolume")) 
        {
            slider.value = PlayerPrefs.GetFloat("VoiceVolume");
        }
        else
        {
            PlayerPrefs.SetFloat("VoiceVolume", 1.0f);
            slider.value = 0.75f;
        }
		
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
		if (Input.GetKeyDown (KeyCode.T))
		{
			PlayerPrefs.DeleteAll();
		}
	}

	// Private members

	// Transfer this accross scenes to load either client or host server connection
	int m_Server = 0;

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