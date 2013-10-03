//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   NetworkServer.h
//  Description :   --------------------------
//
//  Author  	:  Programming Team
//  Mail    	:  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;
using System.Text;


/* Implementation */


public class CNetworkServer : MonoBehaviour
{

// Member Types


    const int kiTitleMaxLength = 32;


    public enum EPacketId : byte
    {
        PlayerController = RakNet.DefaultMessageIDTypes.ID_USER_PACKET_ENUM,
    }


    public enum EDisconnectType
    {
        Invoked,
        Timeout,
        Kicked,
        Banned
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TServerInfo
    {
        public TServerInfo(string _sTitle, byte _bNumOpenSlots)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] baConvertedTitle = encoding.GetBytes(_sTitle);

            baTitle = new byte[kiTitleMaxLength];
            baConvertedTitle.CopyTo(baTitle, 0);

            bNumOpenSlots = _bNumOpenSlots;
        }

        public byte[] baTitle;
        public byte bNumOpenSlots;
    }


// Member Events


    public delegate void NotifyPlayerConnect(CNetworkPlayer _cPlayer);
    public event NotifyPlayerConnect EventPlayerConnect;


    public delegate void NotifyPlayerDisconnect(CNetworkPlayer _cPlayer);
    public event NotifyPlayerDisconnect EventPlayerDisconnect;


    public delegate void NotifyShutdown(object _cSender, params object[] _caParams);
    public event NotifyShutdown EventShutdown;


// Member Functions

    // public:


    public void Start()
    {
        // Empty
    }


    public void OnDestory()
    {
        Shutdown();
    }


    public void Update()
    {
        if (IsActive())
        {
            ProcessInboundPackets();
            ProcessOutgoingPackets();
        }
    }


    public bool HostServer(string _sTitle, uint _uiNumSlots, uint _uiPort)
    {
        m_sTitle = _sTitle;
        bool bServerStarted = false;


        if (StartupPeer(_uiPort, _uiNumSlots))
        {
            UpdateServerInfo();
        }


        return (bServerStarted);
    }


    protected void UpdateServerInfo()
    {
        TServerInfo tServerInfo = new TServerInfo(m_sTitle,
                                                  (byte)m_cRnPeer.GetMaximumIncomingConnections());


        byte[] baServerInfoData = Converter.ToByteArray(tServerInfo);


        m_cRnPeer.SetOfflinePingResponse(baServerInfoData, (uint)baServerInfoData.Length);
    }


    public void Shutdown()
    {
        if (m_cRnPeer != null)
        {
            m_cRnPeer.Shutdown(200);
            m_cRnPeer = null;
            //RakNet.RakPeerInterface.DestroyInstance(m_cRnPeer);


            Logger.Write("Server shutdown");
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


    public RakNet.RakPeer GetPeer()
    {
        return (m_cRnPeer);
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
                case RakNet.DefaultMessageIDTypes.ID_REMOTE_CONNECTION_LOST: // Fall through
                case RakNet.DefaultMessageIDTypes.ID_CONNECTION_LOST:
                    {
                        HandlePlayerDisconnect(CNetworkPlayer.FindUsingGuid(cRnPacket.guid), EDisconnectType.Timeout);
                    }
                    break;

                case RakNet.DefaultMessageIDTypes.ID_REMOTE_DISCONNECTION_NOTIFICATION: // Fall through
                case RakNet.DefaultMessageIDTypes.ID_DISCONNECTION_NOTIFICATION:
                    {
                        HandlePlayerDisconnect(CNetworkPlayer.FindUsingGuid(cRnPacket.guid), EDisconnectType.Invoked);
                    }
                    break;

                case RakNet.DefaultMessageIDTypes.ID_REMOTE_NEW_INCOMING_CONNECTION:
                case RakNet.DefaultMessageIDTypes.ID_NEW_INCOMING_CONNECTION:
                    {
                        HandlePlayerConnect(cRnPacket.systemAddress, cRnPacket.guid);
                    }
                    break;

                case (RakNet.DefaultMessageIDTypes)EPacketId.PlayerController:
                    {
                        HandlePlayerControllerPacket(CNetworkPlayer.FindUsingGuid(cRnPacket.guid), cRnPacket.data);
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
        // Increment outbound timer
		m_fPacketOutboundTimer += Time.deltaTime;

        // Compile and send out packets if its time
		if (m_fPacketOutboundTimer > m_fPacketOutboundInterval)
		{
            // Decrement timer
            m_fPacketOutboundTimer -= m_fPacketOutboundInterval;

            // Send player packets out
            foreach (KeyValuePair<uint, CNetworkPlayer> tEntry in CNetworkPlayer.FindAll())
            {
                CNetworkPlayer cNetworkPlayer = tEntry.Value;

                // Send out packet if stream contains more then the identifier
                if (!cNetworkPlayer.IsHost())
                {
                    if (cNetworkPlayer.HasOutboundData())
                    {
                        Logger.Write("Sent packet to player id ({0}) system address ({1}) of size ({2})", cNetworkPlayer.PlayerId, cNetworkPlayer.SystemAddress, cNetworkPlayer.PacketStream.GetSize());

                        m_cRnPeer.Send(cNetworkPlayer.PacketStream.GetBitStream(), RakNet.PacketPriority.IMMEDIATE_PRIORITY, RakNet.PacketReliability.RELIABLE_SEQUENCED, (char)0, cNetworkPlayer.SystemAddress, false);

                        cNetworkPlayer.ResetPacketStream();
                    }
                }
            }
		}
    }


    protected void HandlePlayerConnect(RakNet.SystemAddress _cSystemAddress, RakNet.RakNetGUID _cGuid)
    {
        // Create network player instance for new player
        CNetworkPlayer cNetworkPlayer = gameObject.AddComponent<CNetworkPlayer>();
        cNetworkPlayer.SystemAddress = new RakNet.SystemAddress(_cSystemAddress.ToString());
        cNetworkPlayer.Guid = new RakNet.RakNetGUID(_cGuid.g);

        // Notify other components
        if (EventPlayerConnect != null)
        {
            EventPlayerConnect(cNetworkPlayer);
        }

        Logger.Write("A player has joined. Instance id ({0}) System address ({1}) Guid ({2})", cNetworkPlayer.PlayerId, _cSystemAddress, _cGuid);
    }


    protected void HandlePlayerDisconnect(CNetworkPlayer _cPlayer, EDisconnectType _eDisconnectType)
    {
        Logger.Write("A client has disconnected");
    }


    protected void HandlePlayerControllerPacket(CNetworkPlayer _cPlayer, byte[] _baData)
    {
        // Create packet stream
        CPacketStream cPacketStream = new CPacketStream(_baData);

        // Ignore packet id
        cPacketStream.IgnoreBytes(1);

        // Have the network player controller process the data
        CNetworkPlayerController.ProcessNetworkInboundData(_cPlayer, cPacketStream);
    }


    // private:


    bool StartupPeer(uint _uiPort, uint _uiNumSlots)
    {
        //m_cRnPeer = RakNet.RakPeerInterface.GetInstance();
        m_cRnPeer = new RakNet.RakPeer();
        m_uiPort = _uiPort;


        RakNet.SocketDescriptor tSocketDesc = new RakNet.SocketDescriptor((ushort)m_uiPort, "");
        RakNet.StartupResult eStartupResult = m_cRnPeer.Startup(_uiNumSlots, tSocketDesc, 1);
        bool bPeerStarted = false;


        if (eStartupResult != RakNet.StartupResult.RAKNET_STARTED)
        {
            Logger.WriteError("Raknet peer failed to start. ErrorCode({0})", eStartupResult);
        }
        else
        {
            m_cRnPeer.SetMaximumIncomingConnections((ushort)_uiNumSlots);
            bPeerStarted = true;


            Logger.Write("Server started with port ({0}) NumSlots({1})", _uiPort, _uiNumSlots);
        }


        return (bPeerStarted);
    }


// Member Variables

    // protected:


    // private:


    RakNet.RakPeer m_cRnPeer = null;


	float m_fPacketOutboundTimer = 0.0f;
	float m_fPacketOutboundInterval = 1.0f / 100.0f;


    string m_sTitle = "Untitled";


    uint m_uiPort = 0;


};
