//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   NetworkMasterServer.cs
//  Description :   --------------------------
//
//  Author  	:  Programming Team
//  Mail    	:  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;


/* Implementation */


public class CNetworkScanner : MonoBehaviour
{

// Member Types


	enum EPacketId
	{
		ServerList = RakNet.DefaultMessageIDTypes.ID_USER_PACKET_ENUM
	}


    public struct TServer
    {
        public string sIp;
        public ushort usPort;
		public uint uiLatency;
        public RakNet.RakNetGUID cGuid;
        public CNetworkServer.TServerInfo tServerInfo;
    }


	public delegate void NotifyFoundServer(TServer _tServer);
	public event NotifyFoundServer EventFoundServer;


	public delegate void NotifyRefresh();
	public event NotifyRefresh EventRefresh;


	public delegate void NotifyConnectionError();
	public event NotifyConnectionError EventConnectionError;


// Member Functions
    
    // public:


    public void Start()
    {
		StartupPeer();
    }
    
    
    public void OnDestroy()
    {
		// Empty
    }


    public void Update()
    {
		ProcessInboundPackets();
    }


	public void RefreshOnlineServers()
	{
		if (m_cMasterServerAddress == null)
		{
			m_aOnlineServers.Clear();


			if (EventRefresh != null)
			{
				EventRefresh();
			}


			ConnectToMasterServer();
		}
	}


	public void RefreshLanServers(ushort _usServerPort)
	{
		m_aLanServers.Clear();
		m_cRnPeer.Ping("255.255.255.255", _usServerPort, false);
	}


	public void QuickRefreshOnlineServers()
	{
		foreach (TServer tOnlineServer in m_aOnlineServers)
		{
			
			m_cRnPeer.Ping(tOnlineServer.sIp, tOnlineServer.usPort, false);
		}
	}


	public void QuickRefreshLanServers()
	{
		foreach (TServer tLanServer in m_aLanServers)
		{
			m_cRnPeer.Ping(tLanServer.sIp, tLanServer.usPort, false);
		}
	}


	public List<TServer> GetOnlineServers()
	{
		return (m_aOnlineServers);
	}


	public List<TServer> GetLanServers()
	{
		return (m_aLanServers);
	}


    // protected:


	protected void ProcessInboundPackets()
	{
		RakNet.Packet cRnPacket = null;

		// Iterate through every packet in queue
		while ((cRnPacket = m_cRnPeer.Receive()) != null)
		{
			// Extract packet id
			int iPacketId = cRnPacket.data[0];

			// Handle packet
			switch ((RakNet.DefaultMessageIDTypes)iPacketId)
			{
				case RakNet.DefaultMessageIDTypes.ID_CONNECTION_REQUEST_ACCEPTED:
					{
						HandleConnectionAccepted(cRnPacket.systemAddress);
					}
					break;

				case RakNet.DefaultMessageIDTypes.ID_UNCONNECTED_PONG:
					{
						HandleUnconnectPong(cRnPacket.data, cRnPacket.systemAddress, cRnPacket.guid);
					}
					break;

				case (RakNet.DefaultMessageIDTypes)EPacketId.ServerList:
					{
						HandleOnlineServerList(cRnPacket.data);
						Disconnect();
					}
					break;

				case RakNet.DefaultMessageIDTypes.ID_NO_FREE_INCOMING_CONNECTIONS:
				case RakNet.DefaultMessageIDTypes.ID_DISCONNECTION_NOTIFICATION:
				case RakNet.DefaultMessageIDTypes.ID_CONNECTION_LOST:
				case RakNet.DefaultMessageIDTypes.ID_CONNECTION_ATTEMPT_FAILED:
					{
						 // Empty
					}
					break;

				default:
					Logger.WriteError("Receieved unknown network message id ({0})", cRnPacket.data[0]);
					break;
			}


			m_cRnPeer.DeallocatePacket(cRnPacket);
		}
	}


	protected void HandleConnectionAccepted(RakNet.SystemAddress _cServerSystemAddress)
	{
		m_cMasterServerAddress = _cServerSystemAddress;


		//Logger.WriteError("Our master server connection was accepted");
	}


	protected void HandleOnlineServerList(byte[] _baData)
	{
		string sServerList = ASCIIEncoding.ASCII.GetString(_baData, 1, _baData.Length - 1);


		string[] sServer = sServerList.Split('&');


		foreach (string sMyString in sServer)
		{
			if (sMyString.Length > 0)
			{
				RakNet.SystemAddress cServerAddress = new RakNet.SystemAddress(sMyString);


				m_cRnPeer.Ping(cServerAddress.ToString(), cServerAddress.GetPort(), false);
			}
		}
	}


	protected void HandleUnconnectPong(byte[] _baData, RakNet.SystemAddress _cServerSystemAddress, RakNet.RakNetGUID _cServerGuid)
	{
		// Create stream
		CNetworkStream cStream = new CNetworkStream(_baData);

		// Ignore message identifier
		cStream.IgnoreBytes(1);

		// Read time
		uint uiTime = cStream.ReadUInt();

		// Read response data
		byte[] baOfflinePingResponse = cStream.ReadBytes(cStream.NumUnreadBytes);

		// Create server info
		TServer tLanServerInfo = new TServer();
		tLanServerInfo.sIp = _cServerSystemAddress.ToString();
		tLanServerInfo.usPort = _cServerSystemAddress.GetPort();
		tLanServerInfo.uiLatency = RakNet.RakNet.GetTimeMS() - uiTime;
		tLanServerInfo.cGuid = new RakNet.RakNetGUID(_cServerGuid.g);

		// Convert response data to server info
		tLanServerInfo.tServerInfo = new CNetworkServer.TServerInfo(baOfflinePingResponse);
	
		
		uint uiLocalNumberOfAddresses = m_cRnPeer.GetNumberOfAddresses();
		string sIpPrefix = _cServerSystemAddress.ToString().Substring(0, _cServerSystemAddress.ToString().IndexOf('.'));
		bool bIsLanServer = false;


		for (uint i = 0; i < uiLocalNumberOfAddresses; ++ i)
		{
			string sMyLanIp = m_cRnPeer.GetLocalIP(i);


			string sLanIpPrefix = sMyLanIp.Substring(0, sMyLanIp.IndexOf('.'));


			if (sLanIpPrefix == sIpPrefix)
			{
				bIsLanServer = true;
				break;
			}
		}


		if (!bIsLanServer)
		{
			m_aOnlineServers.Add(tLanServerInfo);
		}
		else
		{
			m_aLanServers.Add(tLanServerInfo);
		}


		//Logger.WriteError("Added server ({0}:({1}) lan ({2})", tLanServerInfo.sIp, tLanServerInfo.usPort, bIsLanServer);


		// Notify event observers
		if (EventFoundServer != null)
		{
			EventFoundServer(tLanServerInfo);
		}
	}


    // private:


	void StartupPeer()
	{
		m_cRnPeer = new RakNet.RakPeer();
		m_usPort = (ushort)UnityEngine.Random.Range(10000, 30000);


		RakNet.SocketDescriptor tSocketDesc = new RakNet.SocketDescriptor((ushort)m_usPort, "");
		RakNet.StartupResult eStartupResult = m_cRnPeer.Startup(1, tSocketDesc, 1);


		if (eStartupResult != RakNet.StartupResult.RAKNET_STARTED)
		{
			Logger.WriteError("Master server peer failed to start. ErrorCode({0}) Port({1})", eStartupResult, m_usPort);
		}
	}


	void ShutdownPeer()
	{
		if (m_cRnPeer == null)
		{
			m_cRnPeer.Shutdown(200);
			m_cRnPeer = null;
		}
	}


	void Disconnect()
	{
		m_cRnPeer.CloseConnection(m_cMasterServerAddress, true);
		m_cMasterServerAddress = null;
	}


	void ConnectToMasterServer()
	{
		// Terminate current connection
		ShutdownPeer();

		// Connect the server
		RakNet.ConnectionAttemptResult eConnectionAttempResult = m_cRnPeer.Connect(CNetwork.sMasterServerIp, CNetwork.usMasterServerPort, 
																				   CNetwork.sMasterServerPassword, CNetwork.sMasterServerPassword.Length);

		// Ensure connection started
		if (eConnectionAttempResult != RakNet.ConnectionAttemptResult.CONNECTION_ATTEMPT_STARTED)
		{
			Logger.WriteError("Master server connection to server attempt failed. ErrorCode({0})", eConnectionAttempResult);

			if (EventConnectionError != null)
			{
				// Notify observer's
				EventConnectionError();
			}
		}
		else
		{
			//Logger.Write("Master server connection request sent. ServerIp({0}) ServerPort({1}) ServerPassword({2})", CNetwork.sMasterServerIp, CNetwork.usMasterServerPort, CNetwork.sMasterServerPassword);
		}
	}


// Member Variables
    
    // protected:


    // private:


	ushort m_usPort = 0;


	RakNet.RakPeer m_cRnPeer = null;
	RakNet.SystemAddress m_cMasterServerAddress = null;
	

	List<TServer> m_aOnlineServers = new List<TServer>();
	List<TServer> m_aLanServers = new List<TServer>();


};
