﻿//  Auckland
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

// Member Constants


	public const float k_fSendRate = 60; // 30ms
    public const int kiTitleMaxLength = 32;
	public const float kfRegistrationInterval = 5.0f;


// Member Types


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


// Member Delegates & Events


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


// Member Properties


	public uint SendCounter { get; set; }


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


// Member Methods


    public bool Startup(ushort _usPort, string _sTitle, string _sPlayerName, uint _uiNumSlots)
    {
        m_sTitle = _sTitle;
		m_sPlayerName = _sPlayerName;
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
		m_cRnPeer.Send(_cAudioDataStream.BitStream, RakNet.PacketPriority.IMMEDIATE_PRIORITY, RakNet.PacketReliability.UNRELIABLE_SEQUENCED, (char)1, FindNetworkPlayer(_ulPlayerId).SystemAddress, false);
		_cAudioDataStream.SetReadOffset(0);			
	}


	public CNetworkPlayer FindNetworkPlayer(ulong _ulPlayerId)
	{
		return (s_mNetworkPlayers[_ulPlayerId]);
	}


	public Dictionary<ulong, CNetworkPlayer> GetNetworkPlayers()
	{
		return (s_mNetworkPlayers);
	}


    void Start()
    {
        // Empty
    }


    void OnDestroy()
    {
        Shutdown();
    }


    void Update()
    {
        if (IsActive)
        {
            ProcessInboundPackets();
            ProcessMasterServerRegistration();
        }
    }


    void LateUpdate()
    {
        if (IsActive)
        {
            ProcessOutgoingPackets();
        }
    }


    void ProcessInboundPackets()
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
						Debug.Log("This should not be used");
                        HandlePlayerSerializedData(cRnPacket.guid, cRnPacket.data);
                    }
                    break;

                case (RakNet.DefaultMessageIDTypes)EPacketId.PlayerMicrophoneAudio:
                    {
                        HandlePlayerMicrophoneAudio(cRnPacket.guid, cRnPacket.data);
                    }
                    break;


				case RakNet.DefaultMessageIDTypes.ID_TIMESTAMP:
					HandlePlayerSerializedData(cRnPacket.guid, cRnPacket.data);
					break;
				
                default:
                    Logger.WriteError("Receieved unknown network message id ({0})", cRnPacket.data[0]);
                    break;
            }


            m_cRnPeer.DeallocatePacket(cRnPacket);
        }
    }


    void ProcessOutgoingPackets()
    {
        // Increment outbound timer
		m_fPacketOutboundTimer += Time.deltaTime;

        // Compile and send out packets if its time
		if (m_fPacketOutboundTimer > m_fPacketOutboundInterval)
		{
            // Decrement timer
            m_fPacketOutboundTimer -= m_fPacketOutboundInterval;

			++SendCounter;

            // Send player packets out
            foreach (KeyValuePair<ulong, CNetworkPlayer> tEntry in s_mNetworkPlayers)
            {
                CNetworkPlayer cNetworkPlayer = tEntry.Value;

                // Compile buffered outbound stream
				CNetworkStream cBufferedStream = new CNetworkStream();
				cBufferedStream.Write((byte)RakNet.DefaultMessageIDTypes.ID_TIMESTAMP);
				cBufferedStream.Write(RakNet.RakNet.GetTime());
				cBufferedStream.Write(cNetworkPlayer.BufferedStream);

                // Check stream has outbound data
                if (cNetworkPlayer.BufferedStream.ByteSize > 1)
                {
                    // Dispatch data to player
					m_cRnPeer.Send(cBufferedStream.BitStream, RakNet.PacketPriority.IMMEDIATE_PRIORITY, RakNet.PacketReliability.RELIABLE_ORDERED, (char)0, cNetworkPlayer.SystemAddress, false);

					cNetworkPlayer.ResetBufferedSteam();
                }

                // Compule unbuffered outbound stream
                CNetworkStream cUnbufferedStream = new CNetworkStream();
                cUnbufferedStream.Write((byte)RakNet.DefaultMessageIDTypes.ID_TIMESTAMP);
                cUnbufferedStream.Write(RakNet.RakNet.GetTime());
                cUnbufferedStream.Write(cNetworkPlayer.UnbufferedStream);

                // Check stream has outbound data
                if (cNetworkPlayer.UnbufferedStream.ByteSize > 1)
                {
                    // Dispatch data to player
                    m_cRnPeer.Send(cUnbufferedStream.BitStream, RakNet.PacketPriority.IMMEDIATE_PRIORITY, RakNet.PacketReliability.UNRELIABLE_SEQUENCED, (char)1, cNetworkPlayer.SystemAddress, false);

                    cNetworkPlayer.ResetUnbufferedSteam();
                }
            }
		}
    }


	void ProcessMasterServerRegistration()
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


    void HandlePlayerConnect(RakNet.SystemAddress _cSystemAddress, RakNet.RakNetGUID _cGuid)
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

		// Tell player that he has been given the initial game state
		cNetworkPlayer.SetDownloadingInitialGameStateComplete();
    }


    void HandlePlayerDisconnect(CNetworkPlayer _cPlayer, EDisconnectType _eDisconnectType)
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


	void HandlePlayerSerializedData(RakNet.RakNetGUID _cPlayerGuid, byte[] _baData)
	{
		CNetworkPlayer cNetworkPlayer = s_mNetworkPlayers[_cPlayerGuid.g];

		CNetworkConnection.ProcessPlayerSerializedData(cNetworkPlayer, _baData);
	}
	
	
	void HandlePlayerMicrophoneAudio(RakNet.RakNetGUID _cPlayerGuid, byte[] _baData)
	{
		if (EventRecievedPlayerMicrophoneAudio != null)
		{
			CNetworkStream cAudioDataStream = new CNetworkStream(_baData);
			cAudioDataStream.IgnoreBytes(1);
			
			EventRecievedPlayerMicrophoneAudio(FindNetworkPlayer(_cPlayerGuid.g), cAudioDataStream);
		}
	}


	void UpdateServerInfo()
	{
		byte[] baTitle = Converter.ToByteArray(m_sTitle, typeof(string));
		byte[] baTitlePadded = new byte[kiTitleMaxLength];
		baTitle.CopyTo(baTitlePadded, 0);


		m_cServerInfo.Clear();
		m_cServerInfo.Write((byte)m_cRnPeer.NumberOfConnections());
		m_cServerInfo.Write((byte)m_cRnPeer.GetMaximumIncomingConnections());
		m_cServerInfo.Write(baTitlePadded);


		m_cRnPeer.SetOfflinePingResponse(m_cServerInfo.BitStream.GetData(), (uint)m_cServerInfo.ByteSize);
	}


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
			m_cRnPeer.SetOccasionalPing(true);
			SendCounter = 0;
            bPeerStarted = true;


            Logger.Write("Server started with port ({0}) NumSlots({1})", m_usPort, _uiNumSlots);
        }

        return (bPeerStarted);
    }


    void OnGUI()
    {
        if (m_bShowStats &&
            IsActive)
        {
            RakNet.SystemAddress[] aRemoteSystems = null;
            ushort sSystemsCount = 100;
            m_cRnPeer.GetConnectionList(out aRemoteSystems, ref sSystemsCount);


            uint uiSendBufferCount = 0;
            uint uiSendBufferSize = 0;
            uint uiReceieveBufferCount = 0;
            uint uiReceieveBufferSize = 0;


            foreach (RakNet.SystemAddress cSystemAddress in aRemoteSystems)
            {
                RakNet.RakNetStatistics cStatistics = m_cRnPeer.GetStatistics(cSystemAddress);

                uiSendBufferCount += cStatistics.messageInSendBuffer[0];
                uiSendBufferSize += (uint)cStatistics.bytesInSendBuffer[0];
                uiReceieveBufferCount = cStatistics.messagesInResendBuffer;
                uiReceieveBufferSize = (uint)cStatistics.bytesInResendBuffer;
            }


            string sStatistics = "";
            sStatistics += string.Format("Server Statistics\n");
            sStatistics += string.Format("Player Count ({0})\n", aRemoteSystems.Length);
            sStatistics += string.Format("Send Buffer ({0} Messages) ({1}b)\n", uiSendBufferCount, uiSendBufferSize);
            sStatistics += string.Format("Resend Buffer ({0} Messages) ({1}b)\n", uiReceieveBufferCount, uiReceieveBufferSize);
            //sStatistics += string.Format("Packet Loss ({0}%/s) ({1}% Total)\n", cStatistics.packetlossLastSecond * 100.0f, cStatistics.packetlossTotal * 100.0f);
            // sStatistics += string.Format("Inbound ({0}B/s {1} Messages)\n", m_tInboundRateData.uiLastTotalBytes, m_tInboundRateData.uiLastTotalEntries);
            //sStatistics += string.Format("Outbound ({0}B/s {1} Messages)\n", m_tOutboundRateData.uiLastTotalBytes, m_tOutboundRateData.uiLastTotalEntries);


            GUI.Label(new Rect(Screen.width - 500, 0.0f, 250, 200), sStatistics);
        }
    }


// Member Fields


    RakNet.RakPeer m_cRnPeer = null;
	CNetworkStream m_cServerInfo = new CNetworkStream();


	float m_fRegistrationTimer = 5;
	float m_fPacketOutboundTimer = 0.0f;
	float m_fPacketOutboundInterval = 1.0f / k_fSendRate;


    string m_sTitle = "Untitled";
	string m_sPlayerName = "Untitled";


    ushort m_usPort = 0;


    bool m_bShowStats = false;


    static Dictionary<ulong, CNetworkPlayer> s_mNetworkPlayers = new Dictionary<ulong, CNetworkPlayer>();


};
