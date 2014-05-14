using UnityEngine;
using System.Collections;

public class CServerItem : MonoBehaviour {

	public UILabel m_PortIpLabel = null;
	public UIButton m_ConnectButton = null;

	public CServerMenu m_ServerMenu = null;

	public string sServerIP
	{
		get { return(m_ServerIP); }
		set {m_ServerIP = value; }
	}

	public ushort usServerPort
	{
		get { return(m_ServerPort); }
		set {m_ServerPort = value; }
	}

	public string sServerPassword
	{
		get { return(m_ServerPassword); }
		set {m_ServerPassword = value; }
	}

	// Use this for initialization
	void Start () 
	{
		m_PortIpLabel.text = "IP: " + m_ServerIP + " | " + m_ServerPort;

		EventDelegate.Add(m_ConnectButton.onClick, m_ServerMenu.Connect);
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	private string m_ServerIP;
	private ushort m_ServerPort;
	private string m_ServerPassword;
}
