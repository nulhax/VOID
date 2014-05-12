using UnityEngine;
using System.Collections;

public class CServerMenu : MonoBehaviour 
{
	public void ServerName()
	{
		m_sServerTitle = UIInput.current.value;
	}

	public void Port()
	{
		m_strRemoteServerPort = UIInput.current.value;
	}

	public void PlayerName()
	{
		m_sPlayerName = UIInput.current.value;
	}
	

	// Use this for initialization
	void Start () 
	{

	}
	
	// Update is called once per frame
	void Update () 
	{

	}

	void OnClick()
	{

	}

	string m_sServerTitle = System.Environment.UserDomainName + ": " + System.Environment.UserName;
	string m_sPlayerName = "Enter name";
	string[] m_saTabTitles = { "Online Servers", "Lan Servers" };
	
	
	float m_fNumSlots = 16.0f;
	
	
	int m_iActiveTab = 1;

	// Manual server connection variables
	string m_strRemoteServerIP = "127.0.0.1";
	string m_strRemoteServerPort = "1337";
	ushort m_usRemoteServerPort = 0;
}

// http://www.tasharen.com/forum/index.php?topic=1501.0
// http://www.tasharen.com/forum/index.php?topic=6752.0
// http://www.tasharen.com/?page_id=693