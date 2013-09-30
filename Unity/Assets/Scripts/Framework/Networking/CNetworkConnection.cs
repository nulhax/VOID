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


    public delegate void HandleConnect(object _cSender, params object[] _caParams);
    public event HandleConnect EventClientConnect;


    public delegate void HandleDisconnect(object _cSender, params object[] _caParams);
    public event HandleDisconnect EventClientDisconnect;


    public delegate void HandleShutdown(object _cSender, params object[] _caParams);
    public event HandleShutdown EventShutdown;


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
        if (IsConnected())
        {
            ProcessIncomingPackets();
            ProcessOutgoingPackets();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            Disconnect();
        }
    }


    public bool ConnectToServer(uint _uiListenPort, string _sServerIp, ushort _usServerPort, string _sServerPassword)
    {
        Disconnect();


        bool bConnectionStarted = false;


        if (StartupPeer(_uiListenPort))
        {
            RakNet.ConnectionAttemptResult eConnectionAttempResult = m_cRnPeer.Connect(_sServerIp, _usServerPort, _sServerPassword, _sServerPassword.Length);


            if (eConnectionAttempResult != RakNet.ConnectionAttemptResult.CONNECTION_ATTEMPT_STARTED)
            {
                Debug.LogError(string.Format("Connection to server attempt failed. ErrorCode({0})", eConnectionAttempResult));
            }
            else
            {
                bConnectionStarted = true;


                Debug.Log(string.Format("Connection request sent. ServerIp({0}) ServerPort({1}) ServerPassword({2})", _sServerIp, _usServerPort, _sServerPassword));
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


            Debug.LogError("Connection disconnected");
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
                        Debug.Log("Connection established with server");
                    }//HandleConnectSuccess(pRnPacket->systemAddress);
                    break;

                case RakNet.DefaultMessageIDTypes.ID_NO_FREE_INCOMING_CONNECTIONS:
                    {
                        Debug.Log("No free incoming connects");
                    }//HandleConnectFailServerFull();
                    break;

                case RakNet.DefaultMessageIDTypes.ID_DISCONNECTION_NOTIFICATION:
                    {
                        Debug.Log("Disconnect notification");
                    }//HandleDisconnectServerShutdown();
                    break;

                case RakNet.DefaultMessageIDTypes.ID_CONNECTION_LOST:
                    {
                        Debug.Log("Connection lost");
                    }//HandleTimout();
                    break;

                case RakNet.DefaultMessageIDTypes.ID_CONNECTION_ATTEMPT_FAILED:
                    {
                        Debug.Log("Failed to connect to server");
                    }
                    break;

                case (RakNet.DefaultMessageIDTypes)EPacketId.NetworkView:
                    {
						CPacketStream cTransmissionStream = new CPacketStream(cRnPacket.data);
						cTransmissionStream.IgnoreBytes(1); // Ignore packet id


                        CNetworkView.HandleInboundData(cTransmissionStream);
                    }
                    break;

                default:
                    Debug.LogError(string.Format("Receieved unknown network message id ({0})", cRnPacket.data[0]));
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
            if (CNetworkView.HasOutboundData())
            {
                CPacketStream cNetworkViewStream = new CPacketStream(new RakNet.BitStream());


                if (CGame.PlayerController.HasOutboundData())
                {
                    CGame.PlayerController.CompileOutboundData(cNetworkViewStream);
                }
            }


            m_fPacketOutboundTimer -= m_fPacketOutboundInterval;
        }
    }


    public void HandleNetworkViewPacket()
    {

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
            Debug.LogError(string.Format("Raknet peer failed to start. ErrorCode({0}) Port({1})", eStartupResult, _uiPort));
        }
        else
        {
            bPeerStarted = true;


            Debug.Log(string.Format("Raknet peer started. Port({0})", _uiPort));
        }


        return (bPeerStarted);
    }


// Member Variables
    
    // protected:


    // private:


    RakNet.RakPeer m_cRnPeer = null;
    CTimer m_cPacketOutgoingTimer = new CTimer();


    float m_fPacketOutboundTimer = 0.0f;
    float m_fPacketOutboundInterval = 1.0f / 10.0f;


    uint m_uiPort = 0;


};
