//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   NetworkConnection.h
//  Description :   --------------------------
//
//  Author  	:  Programming Team
//  Mail    	:  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Text;


/* Implementation */


public class CNetworkConnection : MonoBehaviour
{

// Member Types


    public enum EPacketId : byte
    {
        NetworkView = RakNet.DefaultMessageIDTypes.ID_USER_PACKET_ENUM,
    }


    public enum EDisconnectType
    {
        Invoked,
        Timedout,
        Kicked,
        Banned
    }


    public enum EConnectFailType
    {
        Full,
        NotFound
    }


    public delegate void OnConnect(object _cSender, params object[] _caParams);
    public event OnConnect EventPlayerConnect;


    public delegate void OnDisconnect(object _cSender, params object[] _caParams);
    public event OnDisconnect EventPlayerDisconnect;


    public delegate void OnShutdown(object _cSender, params object[] _caParams);
    public event OnShutdown EventShutdown;


// Member Functions
    
    // public:


    public void Start()
    {
        // Empty
    }


    public void OnDestory()
    {
        Disconnect();
    }


    public void Update()
    {
        // Process packets
        if (IsConnected())
        {
            ProcessIncomingPackets();
            ProcessOutgoingPackets();
        }
    }


    public bool ConnectToServer(uint _uiListenPort, string _sServerIp, ushort _usServerPort, string _sServerPassword)
    {
        // Terminate current connection
        Disconnect();


        bool bConnectionStarted = false;


        if (StartupPeer(_uiListenPort))
        {
            RakNet.ConnectionAttemptResult eConnectionAttempResult = m_cRnPeer.Connect(_sServerIp, _usServerPort, _sServerPassword, _sServerPassword.Length);


            if (eConnectionAttempResult != RakNet.ConnectionAttemptResult.CONNECTION_ATTEMPT_STARTED)
            {
                Logger.WriteError("Connection to server attempt failed. ErrorCode({0})", eConnectionAttempResult);
            }
            else
            {
                bConnectionStarted = true;


                Logger.Write("Connection request sent. ServerIp({0}) ServerPort({1}) ServerPassword({2})", _sServerIp, _usServerPort, _sServerPassword);
            }
        }


        return (bConnectionStarted);
    }


    public void Disconnect()
    {
        if (m_cRnPeer != null)
        {
            m_cRnPeer.Shutdown(500);
            m_cRnPeer = null;
            //RakNet.RakPeerInterface.DestroyInstance(m_cRnPeer);


            Logger.WriteError("Connection disconnected");
        }
    }


    public bool IsActive()
    {
        bool bActive = false;


        if (m_cRnPeer != null)
        {
            bActive = m_cRnPeer.IsActive();
        }


        return (bActive);
    }


    public bool IsConnected()
    {
        return (m_cRnPeer != null &&
				m_cRnPeer.NumberOfConnections() >= 1);
    }


    // protected:


    protected void ProcessIncomingPackets()
    {
        RakNet.Packet cRnPacket = null;


        while ((cRnPacket = m_cRnPeer.Receive()) != null)
        {
            int iPacketId = cRnPacket.data[0];
            RakNet.DefaultMessageIDTypes eMessageId = (RakNet.DefaultMessageIDTypes)iPacketId;


            switch (eMessageId)
            {
                case RakNet.DefaultMessageIDTypes.ID_CONNECTION_REQUEST_ACCEPTED:
                    {
                        m_cServerSystemAddress = new RakNet.SystemAddress(cRnPacket.systemAddress.ToString());
                        Logger.Write("Connection established with server");
                    }
                    break;

                case RakNet.DefaultMessageIDTypes.ID_NO_FREE_INCOMING_CONNECTIONS:
                    {
                        Logger.Write("No free incoming connects");
                    }
                    break;

                case RakNet.DefaultMessageIDTypes.ID_DISCONNECTION_NOTIFICATION:
                    {
                        Logger.Write("Disconnect notification");
                    }
                    break;

                case RakNet.DefaultMessageIDTypes.ID_CONNECTION_LOST:
                    {
                        Logger.Write("Connection lost");
                    }
                    break;

                case RakNet.DefaultMessageIDTypes.ID_CONNECTION_ATTEMPT_FAILED:
                    {
                        Logger.Write("Failed to connect to server");
                    }
                    break;

                case (RakNet.DefaultMessageIDTypes)EPacketId.NetworkView:
                    {
                        HandleNetworkViewPacket(cRnPacket.data);
                    }
                    break;

                default:
                    Logger.WriteError("Receieved unknown network message id ({0})", cRnPacket.data[0]);
                    break;
            }


            m_cRnPeer.DeallocatePacket(cRnPacket);
        }
    }


    protected void ProcessOutgoingPackets()
    {
		m_fPacketOutboundTimer += Time.deltaTime;


        if (m_fPacketOutboundTimer > m_fPacketOutboundInterval)
        {
            CNetworkPlayerController cPlayerController = CGame.PlayerController;


            if (cPlayerController.HasOutboundData())
            {
                Logger.Write("Sent player controller to server of size ({0})", cPlayerController.PacketStream.GetSize());


                m_cRnPeer.Send(cPlayerController.PacketStream.GetBitStream(), RakNet.PacketPriority.IMMEDIATE_PRIORITY, RakNet.PacketReliability.RELIABLE_ORDERED, (char)0, m_cServerSystemAddress, false);


                cPlayerController.ClearOutboundData();
            }


            m_fPacketOutboundTimer -= m_fPacketOutboundInterval;
        }
    }


    public void HandleNetworkViewPacket(byte[] _baData)
    {
        // Create stream with data
        CPacketStream cPacketStream = new CPacketStream(_baData);

        // Ignore packet id
        cPacketStream.IgnoreBytes(1);

        // Process packet data
        CNetworkView.ProcessInboundNetworkData(cPacketStream);

        Logger.Write("Processed Inbound Data of size ({0})", cPacketStream.GetSize());
    }


    // private:


    bool StartupPeer(uint _uiPort)
    {
        //m_cRnPeer = RakNet.RakPeerInterface.GetInstance();
        m_cRnPeer = new RakNet.RakPeer();
        m_uiPort = _uiPort;


        RakNet.SocketDescriptor tSocketDesc = new RakNet.SocketDescriptor((ushort)m_uiPort, "");
        RakNet.StartupResult eStartupResult = m_cRnPeer.Startup(1, tSocketDesc, 1);
        bool bPeerStarted = false;


        if (eStartupResult != RakNet.StartupResult.RAKNET_STARTED)
        {
            Logger.WriteError("Raknet peer failed to start. ErrorCode({0}) Port({1})", eStartupResult, _uiPort);
        }
        else
        {
            bPeerStarted = true;


            Logger.Write("Raknet peer started. Port({0})", _uiPort);
        }


        return (bPeerStarted);
    }


// Member Variables
    
    // protected:


    // private:


    RakNet.RakPeer m_cRnPeer = null;
    RakNet.SystemAddress m_cServerSystemAddress = null;
    CTimer m_cPacketOutgoingTimer = new CTimer();


    float m_fPacketOutboundTimer = 0.0f;
    float m_fPacketOutboundInterval = 1.0f / 30.0f;


    uint m_uiPort = 0;


};
