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


    public const string sMasterServerIp = "116.193.147.34"; // spacegamems.capturehub.net
	public const string sMasterServerPassword = "";
    public const ushort usMasterServerPort = 9896;


// Member Events & Delegates


    public delegate void NetworkUpdateHandler(float _fDeltatick);

    public static event NetworkUpdateHandler EventNetworkUpdate;


// Member Functions

	// public:


	public void Awake()
	{
		// Ensure there is only one of this instance type
		Logger.WriteErrorOn(s_cInstance != null, "There can only be one CNetwork instance. Two now exist");
		
		// Store instance
		s_cInstance = this;
		
		gameObject.AddComponent<CNetworkScanner>();
		gameObject.AddComponent<CNetworkFactory>();
		
		// Make sure the connection happens before the server
		gameObject.AddComponent<CNetworkConnection>();
		gameObject.AddComponent<CNetworkServer>();
		
		// Network view needs to be added last
		gameObject.AddComponent<CNetworkView>();
	}


	public void Start()
	{
		//gameObject.GetComponent<CNetworkView>().ViewId = 1;
	}


	public void OnDestroy()
	{
	}


	public void Update()
	{
        if (CNetwork.Connection.IsConnected ||
            CNetwork.IsServer)
        if (EventNetworkUpdate != null) EventNetworkUpdate(Time.deltaTime);
	}


	public static bool IsServer
	{
        get 
        { 
            return (CNetwork.Server != null && CNetwork.Server.IsActive); 
        }
	}


	public static bool IsConnectedToServer
	{
		get 
        {
            if (Connection == null) return false; 

            return(Connection.IsConnected); 
        }
	}


	public static CNetworkScanner Scanner
	{
		get 
        {
            if (s_cInstance == null) return null; 

            return (s_cInstance.GetComponent<CNetworkScanner>()); 
        }
	}


	public static CNetworkConnection Connection
	{
        get
        { 
            if (s_cInstance == null) return null; 
            
            return (s_cInstance.GetComponent<CNetworkConnection>()); 
        }
	}


	public static CNetworkServer Server
	{
		get 
        {
            if (s_cInstance == null) return null; 
            
            return (s_cInstance.GetComponent<CNetworkServer>()); 
        }
	}


	public static CNetworkFactory Factory
	{
		get 
        {
            if (s_cInstance == null) return null; 

            return (s_cInstance.GetComponent<CNetworkFactory>()); 
        }
	}


	public static ulong PlayerId
	{
        get 
        { 
            if (Connection == null) return (0); 
            
            return (Connection.RakPeer.GetMyGUID().g); 
        }
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
