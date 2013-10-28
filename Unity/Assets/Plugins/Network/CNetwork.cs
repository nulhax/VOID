//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   Network.cs
//  Description :   --------------------------
//
//  Author  	:  Programming Team
//  Mail    	:  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public class CNetwork : MonoBehaviour
{

// Member Types


	public const string sMasterServerIp = "localhost"; // spacegamems.capturehub.net
	public const string sMasterServerPassword = "";
	public const ushort usMasterServerPort = 40000;


// Member Functions

	// public:


	public void Awake()
	{
		// Ensure there is only one of this instance type
		Logger.WriteErrorOn(s_cInstance != null, "There can only be one CNetwork instance. Two now exist");

		// Store instance
		s_cInstance = this;


		gameObject.AddComponent<CNetworkScanner>();
		gameObject.AddComponent<CNetworkConnection>();
		gameObject.AddComponent<CNetworkFactory>();
		gameObject.AddComponent<CNetworkServer>();
		gameObject.AddComponent<CNetworkView>();
	}


	public void Start()
	{
	}


	public void OnDestroy()
	{
	}


	public void Update()
	{
	}


	public static bool IsServer
	{
		get { return (CNetwork.Server.IsActive); }
	}


	public static bool IsConnectedToServer()
	{
		return (CNetwork.Connection.IsConnected);
	}


	public static CNetworkScanner Scanner
	{
		get { return (s_cInstance.GetComponent<CNetworkScanner>()); }
	}


	public static CNetworkConnection Connection
	{
		get { return (s_cInstance.GetComponent<CNetworkConnection>()); }
	}


	public static CNetworkServer Server
	{
		get { return (s_cInstance.GetComponent<CNetworkServer>()); }
	}


	public static CNetworkFactory Factory
	{
		get { return (s_cInstance.GetComponent<CNetworkFactory>()); }
	}


	public static CNetwork Instance
	{
		get { return (s_cInstance); }
	}


	// protected:


	// private:



// Member Variables

	// protected:


	// private:


	static CNetwork s_cInstance = null;


};
