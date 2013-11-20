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
using System.IO;


/* Implementation */


public class CNetworkServer : MonoBehaviour
{


// Member Types


    public const int kiTitleMaxLength = 32;
	public const float kfRegistrationInterval = 5.0f;


    public enum EPacketId
    {
		PlayerSerializedData = RakNet.DefaultMessageIDTypes.ID_USER_PACKET_ENUM,
		PlayerMicrophoneAudio
    }


    public enum EDisconnectType
    {
        Invoked,
        Timeout,
        Kicked,
        Banned,
		ServerShutdown
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TServerInfo
    {
        public TServerInfo(byte[] _baMemory)
        {
            bNumAvaiableSlots = _baMemory[0];
            bNumSlots = _baMemory[1];
            sTitle = Encoding.UTF8.GetString(_baMemory, sizeof(byte) * 2, kiTitleMaxLength);
        }

        public string sTitle;
        public byte bNumAvaiableSlots;
        public byte bNumSlots;
    }


// Member Events


    public delegate void NotifyPlayerConnect(CNetworkPlayer _cPlayer);
    public event NotifyPlayerConnect EventPlayerConnect;


    public delegate void NotifyPlayerDisconnect(CNetworkPlayer _cPlayer);
    public event NotifyPlayerDisconnect EventPlayerDisconnect;

	public delegate void NotifyStartup();
	public event NotifyStartup EventStartup;


    public delegate void NotifyShutdown();
    public event NotifyShutdown EventShutdown;
	
	
	public delegate void NotifyRecievedPlayerMicrophoneAudio(CNetworkPlayer _cPlayer, CNetworkStream _cAudioDataStream);
	public event NotifyRecievedPlayerMicrophoneAudio EventRecievedPlayerMicrophoneAudio;


// Member Functions

    // public:


    public void Start()
    {
        // Empty
    }


	public void OnDestroy()
	{
		Shutdown();
	}


    public void Update()
    {
        if (IsActive)
        {
            ProcessInboundPackets();
            ProcessOutgoingPackets();
			ProcessMasterServerRegistration();
        }
    }


    public bool Startup(ushort _usPort, string _sTitle, uint _uiNumSlots)
    {
        m_sTitle = _sTitle;
		m_usPort = _usPort;
        bool bServerStarted = true;


        if (StartupPeer(_uiNumSlots))
        {
			if (EventStartup != null)
			{
				EventStartup();
			}

            UpdateServerInfo();
        }
        else
        {
            bServerStarted = false;
        }

        return (bServerStarted);
    }


    public void Shutdown()
    {
        if (m_cRnPeer != null)
        {
			foreach (KeyValuePair<ulong, CNetworkPlayer> tEntry in s_mNetworkPlayers)
			{
				Component.Destroy(tEntry.Value);
			}

			s_mNetworkPlayers.Clear();


			if (EventShutdown != null)
			{
				EventShutdown();
			}


            m_cRnPeer.Shutdown(200);
            m_cRnPeer = null;


            Logger.Write("Server shutdown");
        }
    }
	
	
	public void TransmitMicrophoneAudio(ulong _ulPlayerId, CNetworkStream _cAudioDataStream)
	{
		CNetworkStream cTransmitStream = new CNetworkStream();
		cTransmitStream.Write((byte)CNetworkConnection.EPacketId.MicrophoneAudio);
		cTransmitStream.Write(_cAudioDataStream);
		
		m_cRnPeer.Send(cTransmitStream.BitStream, RakNet.PacketPriority.IMMEDIATE_PRIORITY, RakNet.PacketReliability.UNRELIABLE_SEQUENCED, (char)1, FindNetworkPlayer(_ulPlayerId).SystemAddress, false);
	}


	public CNetworkPlayer FindNetworkPlayer(ulong _ulPlayerId)
	{
		return (s_mNetworkPlayers[_ulPlayerId]);
	}


	public Dictionary<ulong, CNetworkPlayer> FindNetworkPlayers()
	{
		return (s_mNetworkPlayers);
	}


    public bool IsActive
    {
		get
		{
			if (m_cRnPeer != null)
			{
				return (m_cRnPeer.IsActive());
			}
			else
			{
				return (false);
			}
		}
    }


    public RakNet.RakPeer RakPeer
    {
		get { return (m_cRnPeer); }
    }


	public ushort Port
	{
		get { return (m_usPort); }
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
                        HandlePlayerDisconnect(FindNetworkPlayer(cRnPacket.guid.g), EDisconnectType.Timeout);
                    }
                    break;

                case RakNet.DefaultMessageIDTypes.ID_REMOTE_DISCONNECTION_NOTIFICATION: // Fall through
                case RakNet.DefaultMessageIDTypes.ID_DISCONNECTION_NOTIFICATION:
                    {
                        HandlePlayerDisconnect(FindNetworkPlayer(cRnPacket.guid.g), EDisconnectType.Invoked);
                    }
                    break;

                case RakNet.DefaultMessageIDTypes.ID_REMOTE_NEW_INCOMING_CONNECTION:
                case RakNet.DefaultMessageIDTypes.ID_NEW_INCOMING_CONNECTION:
                    {
                        HandlePlayerConnect(cRnPacket.systemAddress, cRnPacket.guid);
                    }
                    break;

                case RakNet.DefaultMessageIDTypes.ID_UNCONNECTED_PING_OPEN_CONNECTIONS:
                case RakNet.DefaultMessageIDTypes.ID_UNCONNECTED_PING:
                    {
						// Empty
                    }
                    break;

                case (RakNet.DefaultMessageIDTypes)EPacketId.PlayerSerializedData:
                    {
                        HandlePlayerSerializedData(cRnPacket.guid, cRnPacket.data);
                    }
                    break;

                case (RakNet.DefaultMessageIDTypes)EPacketId.PlayerMicrophoneAudio:
                    {
                        HandlePlayerMicrophoneAudio(cRnPacket.guid, cRnPacket.data);
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
            foreach (KeyValuePair<ulong, CNetworkPlayer> tEntry in s_mNetworkPlayers)
            {
                CNetworkPlayer cNetworkPlayer = tEntry.Value;
				CNetworkStream cNetworkViewStream = cNetworkPlayer.NetworkViewStream;

                // Check stream has outbound data
				if (cNetworkViewStream.Size > 1)
                {
                    //Logger.WriteError("Sent packet to player id ({0}) system address ({1}) of size ({2}) MessageId ({3})", cNetworkPlayer.PlayerId, cNetworkPlayer.SystemAddress, cNetworkViewStream.GetSize(), cNetworkViewStream.ReadByte());
					//cNetworkViewStream.SetReadOffset(0);

                    // Dispatch data to player
					m_cRnPeer.Send(cNetworkViewStream.BitStream, RakNet.PacketPriority.IMMEDIATE_PRIORITY, RakNet.PacketReliability.RELIABLE_SEQUENCED, (char)0, cNetworkPlayer.SystemAddress, false);

					cNetworkPlayer.ResetNetworkViewSteam();
                }
            }
		}
    }


	protected void ProcessMasterServerRegistration()
	{
		m_fRegistrationTimer += Time.deltaTime;


		if (m_fRegistrationTimer >= kfRegistrationInterval)
		{
			if (CNetwork.IsServer)
			{
				CNetwork.Server.RakPeer.AdvertiseSystem(CNetwork.sMasterServerIp, CNetwork.usMasterServerPort, null, 0);
			}


			m_fRegistrationTimer = 0.0f;
		}
	}


    protected void HandlePlayerConnect(RakNet.SystemAddress _cSystemAddress, RakNet.RakNetGUID _cGuid)
    {
        // Create network player instance for new player
		CNetworkPlayer cNetworkPlayer = gameObject.AddComponent<CNetworkPlayer>();

		// Check is host by matching address and connection guid
		bool bIsHost = _cGuid.g == CNetwork.Connection.RakPeer.GetMyGUID().g;

		// Initialise player instance
		cNetworkPlayer.Initialise(_cSystemAddress, _cGuid, bIsHost);

        // Store network player instance
        s_mNetworkPlayers.Add(cNetworkPlayer.Guid.g, cNetworkPlayer);

		// Join notice
        Logger.Write("A player has joined. Instance id ({0}) System address ({1}) Guid ({2})", cNetworkPlayer.PlayerId, _cSystemAddress, _cGuid);

        // Notify event observers
        if (EventPlayerConnect != null)
        {
            EventPlayerConnect(cNetworkPlayer);
        }

        // Update server info, player count
        UpdateServerInfo();
    }


    protected void HandlePlayerDisconnect(CNetworkPlayer _cPlayer, EDisconnectType _eDisconnectType)
    {
        Logger.Write("A client has disconnected");

        // Notify event observers
        if (EventPlayerDisconnect != null)
        {
            EventPlayerDisconnect(_cPlayer);
        }

		Component.Destroy(s_mNetworkPlayers[_cPlayer.PlayerId]);

		// Remove and delete network player
		s_mNetworkPlayers.Remove(_cPlayer.PlayerId);

        // Update server info, player count
        UpdateServerInfo();
    }


	protected void HandlePlayerSerializedData(RakNet.RakNetGUID _cPlayerGuid, byte[] _baData)
	{
		CNetworkPlayer cNetworkPlayer = s_mNetworkPlayers[_cPlayerGuid.g];


		CNetworkConnection.ProcessPlayerSerializedData(cNetworkPlayer, _baData);
	}
	
	
	protected void HandlePlayerMicrophoneAudio(RakNet.RakNetGUID _cPlayerGuid, byte[] _baData)
	{
		if (EventRecievedPlayerMicrophoneAudio != null)
		{
			CNetworkStream cAudioDataStream = new CNetworkStream(_baData);
			cAudioDataStream.IgnoreBytes(1);
			
			
			EventRecievedPlayerMicrophoneAudio(FindNetworkPlayer(_cPlayerGuid.g), cAudioDataStream);
		}
	}


	protected void UpdateServerInfo()
	{
		byte[] baTitle = Converter.ToByteArray(m_sTitle, typeof(string));
		byte[] baTitlePadded = new byte[kiTitleMaxLength];
		baTitle.CopyTo(baTitlePadded, 0);


		m_cServerInfo.Clear();
		m_cServerInfo.Write((byte)m_cRnPeer.NumberOfConnections());
		m_cServerInfo.Write((byte)m_cRnPeer.GetMaximumIncomingConnections());
		m_cServerInfo.Write(baTitlePadded);


		m_cRnPeer.SetOfflinePingResponse(m_cServerInfo.BitStream.GetData(), (uint)m_cServerInfo.Size);
	}


    // private:


    bool StartupPeer(uint _uiNumSlots)
    {
        //m_cRnPeer = RakNet.RakPeerInterface.GetInstance();
        m_cRnPeer = new RakNet.RakPeer();


        RakNet.SocketDescriptor tSocketDesc = new RakNet.SocketDescriptor((ushort)m_usPort, "");
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


            Logger.Write("Server started with port ({0}) NumSlots({1})", m_usPort, _uiNumSlots);
        }

        return (bPeerStarted);
    }


// Member Variables

    // protected:


    // private:


    RakNet.RakPeer m_cRnPeer = null;
	CNetworkStream m_cServerInfo = new CNetworkStream();


	float m_fRegistrationTimer = 5;
	float m_fPacketOutboundTimer = 0.0f;
	float m_fPacketOutboundInterval = 1.0f / 100.0f;


    string m_sTitle = "Untitled";


    ushort m_usPort = 0;


    static Dictionary<ulong, CNetworkPlayer> s_mNetworkPlayers = new Dictionary<ulong, CNetworkPlayer>();


};
