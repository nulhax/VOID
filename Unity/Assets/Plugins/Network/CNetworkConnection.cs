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
using System;


/* Implementation */


public class CNetworkConnection : CNetworkMonoBehaviour
{

// Member Constants


	public const uint k_uiOutboundRate = 30; // 50ms


// Member Types


	public enum EPacketId
	{
		NetworkView = RakNet.DefaultMessageIDTypes.ID_USER_PACKET_ENUM,
		MicrophoneAudio
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


	public enum ESerializeTargetType : byte
	{
		Unthrottled,
		Throttled
	}


	public enum EState
	{
		Unconnected,
		RecievingInitialGameData,
		Connected,
	}


	public struct TRateData
	{
		public float fTimer;
		public uint uiBytes;
		public uint uiNumEntries;
		public uint uiLastTotalEntries;
		public uint uiLastTotalBytes;
	}


	public struct TSerializationMethods
	{
		public TSerializationMethods(SerializeMethod _nSerializeMethod, UnserializeMethod _nUnserializeMethod)
		{
			nSerializeMethod = _nSerializeMethod;
			nUnserializeMethod = _nUnserializeMethod;
		}

		public SerializeMethod nSerializeMethod;
		public UnserializeMethod nUnserializeMethod;
	}


// Member Delegates & Events


	public delegate void OnConnect();
	public event OnConnect EventConnectionAccepted;


	public delegate void OnDisconnect();
	public event OnDisconnect EventDisconnect;


	public delegate void HandleRecievedMicrophoneAudio(CNetworkStream _cAudioDataStream);
	public event HandleRecievedMicrophoneAudio EventRecievedMicrophoneAudio;


	public delegate void HandleInitialGameStateDownloaded();
	public event HandleInitialGameStateDownloaded EventInitialGameStateDownloaded;


	public delegate void SerializeMethod(CNetworkStream _cStream);
	public delegate void UnserializeMethod(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream);


// Member Properties


	public RakNet.RakPeer RakPeer
	{
		get { return (m_cRnPeer); }
	}


	public float ConnectionElapsedTime
	{
		get { return (m_fConnectionElapsedTime); }
	}


	public float TickTimer
	{
		get { return (m_fTickTimer); }
	}


	public float Tick
	{
		get { return (m_fConnectionElapsedTime - Mathf.Floor(m_fConnectionElapsedTime)) * CNetworkServer.k_fSendRate; }
	}


	public float TickTotal
	{
		get { return (m_fTickTotal); }
	}


	public ushort Port
	{
		get { return (m_usPort); }
	}


	public bool IsActive
	{
		get { return (m_cRnPeer.IsActive()); }
	}


	public bool IsConnected
	{
		get
		{
			bool bConnected = false;

			// Ensure server address is set
			if (m_cServerSystemAddress != null)
			{
				// Check we are connected to the server address
				if (m_cRnPeer.GetConnectionState(m_cServerSystemAddress) == RakNet.ConnectionState.IS_CONNECTED)
				{
					bConnected = true;
				}
			}

			return (bConnected);
		}
	}


	public bool IsDownloadingInitialGameData
	{
		get
		{
			return (m_bDownloadingInitialGameState);
		}
	}



// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		// Empty
        _cRegistrar.RegisterRpc(this, "NotifyDownloadingInitialGameStateComplete");
	}


    public bool ConnectToServer(string _sServerIp, ushort _usServerPort, string _sServerPassword)
    {
        bool bConnectionStarted = false;

        // Set empty password for null password
        if (_sServerPassword == null)
        {
            _sServerPassword = "";
        }

        // Terminate current connection
        Disconnect();

        // Connect the server
        RakNet.ConnectionAttemptResult eConnectionAttempResult = m_cRnPeer.Connect(_sServerIp, _usServerPort, _sServerPassword, _sServerPassword.Length);

        // Ensure connection started
        if (eConnectionAttempResult != RakNet.ConnectionAttemptResult.CONNECTION_ATTEMPT_STARTED)
        {
            Logger.WriteError("Connection to server attempt failed. ErrorCode({0})", eConnectionAttempResult);
        }
        else
        {
            bConnectionStarted = true;

            Logger.Write("Connection request sent. ServerIp({0}) ServerPort({1}) ServerPassword({2})", _sServerIp, _usServerPort, _sServerPassword);
        }


        return (bConnectionStarted);
    }


    public void Disconnect()
    {
        // Ensure a connection already exists
        if (IsConnected)
        {
            // Close the connection
            m_cRnPeer.CloseConnection(m_cServerSystemAddress, true);

            // Forget the server address
            m_cServerSystemAddress = null;

            Logger.Write("Connection disconnected");
        }
    }
	
	
	public void TransmitMicrophoneAudio(CNetworkStream _cAudioDataStream)
	{

		CNetworkStream cTransmitStream = new CNetworkStream();
		cTransmitStream.Write((byte)CNetworkServer.EPacketId.PlayerMicrophoneAudio);
		cTransmitStream.Write(_cAudioDataStream);
		
		m_cRnPeer.Send(cTransmitStream.BitStream, RakNet.PacketPriority.IMMEDIATE_PRIORITY, RakNet.PacketReliability.UNRELIABLE_SEQUENCED, (char)1, m_cServerSystemAddress, false);
	}


	public static void RegisterSerializationTarget(SerializeMethod _nSerializeMethod, UnserializeMethod _nUnserializeMethod)
	{
		int iTargetId = s_mSerializeTargets.Count + 1;


		s_mSerializeTargets.Add((byte)iTargetId, new TSerializationMethods(_nSerializeMethod, _nUnserializeMethod));
	}


	public static void RegisterThrottledSerializationTarget(SerializeMethod _nSerializeMethod, UnserializeMethod _nUnserializeMethod)
	{
		int iTargetId = s_mThrottledSerializeTargets.Count + 1;


		s_mThrottledSerializeTargets.Add((byte)iTargetId, new TSerializationMethods(_nSerializeMethod, _nUnserializeMethod));
	}


	public static void ProcessPlayerSerializedData(CNetworkPlayer _cPlayer, byte[] _baData)
	{
		// Create packet stream
		CNetworkStream cStream = new CNetworkStream(_baData);

		cStream.IgnoreBytes(1); // Packet id

		// Retrieve latency
		ulong ulLatency = RakNet.RakNet.GetTime() - cStream.ReadULong();

		cStream.IgnoreBytes(1); // Packet id

		// Iterate through the packet data
		while (cStream.HasUnreadData)
		{
			// Extract the target identifier
			byte bTargetIdentifier = cStream.ReadByte();

			// Extract the target type
			ESerializeTargetType eTargetType = (ESerializeTargetType)cStream.ReadByte();

			// Extract the size of the data
			byte bSize = cStream.ReadByte();

			// Extract the data
			byte[] baData = cStream.ReadBytes(bSize);

			// Create stream for the control
			CNetworkStream cTargetStream = new CNetworkStream(baData);

			// Have the target process its data
			if (eTargetType == ESerializeTargetType.Unthrottled)
			{
				s_mSerializeTargets[bTargetIdentifier].nUnserializeMethod(_cPlayer, cTargetStream);
			}
			else if (eTargetType == ESerializeTargetType.Throttled)
			{
				s_mThrottledSerializeTargets[bTargetIdentifier].nUnserializeMethod(_cPlayer, cTargetStream);
			}
			else
			{
				Logger.WriteError("Unknown serialize target type");
			}
		}
	}




    void Awake()
    {
        StartupPeer();
    }


    void OnDestory()
    {
        Disconnect();
        ShutdownPeer();
    }


    void Update()
    {
        // Process packets
        if (this.IsActive)
        {
            ProcessInboundPackets();

            if (this.IsConnected)
            {
                UpdateTicksAndTimes();
            }
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            m_bShowStats = !m_bShowStats;
        }
    }


    void LateUpdate()
    {
        // Process packets
        if (this.IsActive)
        {
            if (this.IsConnected)
            {
                ProcessOutboundPackets();
            }
        }
    }


	void UpdateTicksAndTimes()
	{
		m_fTickTimer += Time.deltaTime;
		m_fTickTimer -= Mathf.Floor(m_fTickTimer);
		m_fTick = CNetworkServer.k_fSendRate * m_fTickTimer;


		m_fConnectionElapsedTime += Time.deltaTime;
	}


    void ProcessInboundPackets()
    {
        RakNet.Packet cRnPacket = null;

        // Iterate through queued packets
        while ((cRnPacket = m_cRnPeer.Receive()) != null)
        {
            // Extract packet id
            int iPacketId = cRnPacket.data[0];


            // Process message
            switch ((RakNet.DefaultMessageIDTypes)iPacketId)
            {
                case RakNet.DefaultMessageIDTypes.ID_CONNECTION_REQUEST_ACCEPTED:
                    {
                        HandleConnectionAccepted(cRnPacket.systemAddress);
                    }
                    break;

                case RakNet.DefaultMessageIDTypes.ID_NO_FREE_INCOMING_CONNECTIONS:
                    {
                        Logger.WriteError("No free incoming connects");
                    }
                    break;

                case RakNet.DefaultMessageIDTypes.ID_DISCONNECTION_NOTIFICATION:
                    {
                        HandleDisconnect(EDisconnectType.Kicked);
                    }
                    break;

                case RakNet.DefaultMessageIDTypes.ID_CONNECTION_LOST:
                    {
                        HandleDisconnect(EDisconnectType.Timedout);
                    }
                    break;

                case RakNet.DefaultMessageIDTypes.ID_CONNECTION_ATTEMPT_FAILED:
                    {
                        Logger.WriteError("Failed to connect to server");
                    }
                    break;

                case (RakNet.DefaultMessageIDTypes)EPacketId.NetworkView:
                    {
						Logger.WriteError("This method should not have been called.");
                    }
                    break;
				
                case (RakNet.DefaultMessageIDTypes)EPacketId.MicrophoneAudio:
                    {
                        HandleMicrophoneAudio(cRnPacket.data);
                    }
                    break;

				case RakNet.DefaultMessageIDTypes.ID_TIMESTAMP:
					{
						// Get actual subject of message, dismissing RakNet.DefaultMessageIDTypes.ID_TIMESTAMP
						// and the following 8 byte timestamp value
						switch (cRnPacket.data[sizeof(byte) + sizeof(ulong)])
						{
							case (byte)EPacketId.NetworkView:
								HandleNetworkViewPacket(cRnPacket.data);
								break;

							default:
								Logger.WriteError("Receieved unknown network message id ({0})");
								break;
						}
					}
					break;

                default:
                    Logger.WriteError("Receieved unknown network message id ({0})", cRnPacket.data[0]);
                    break;
            }


			m_tInboundRateData.uiBytes += (uint)cRnPacket.data.Length;
			m_tInboundRateData.uiNumEntries += 1;


            m_cRnPeer.DeallocatePacket(cRnPacket);
        }
    }


    void ProcessOutboundPackets()
    {
		if (!m_bDownloadingInitialGameState)
		{
			CompileSerializeTargetsOutboundData();

			// Increment outbound timer
			m_fPacketOutboundTimer += Time.deltaTime;

			if (m_fPacketOutboundTimer > m_fPacketOutboundInterval)
			{
				CNetworkStream cOutboundStream = new CNetworkStream();
				cOutboundStream.Write((byte)RakNet.DefaultMessageIDTypes.ID_TIMESTAMP);
				cOutboundStream.Write(RakNet.RakNet.GetTime());
				cOutboundStream.Write((byte)CNetworkServer.EPacketId.PlayerSerializedData);
				cOutboundStream.Write(s_cUntrottledSerializationStream);

				CompileThrottledSerializeTargetsOutboundData(cOutboundStream);

				// Check player has data to be sent to the server
				if (cOutboundStream.Size > 10)
				{
					m_tOutboundRateData.uiBytes += cOutboundStream.Size;
					m_tOutboundRateData.uiNumEntries += 1;

					// Dispatch data to the server
					
					m_cRnPeer.Send(cOutboundStream.BitStream, RakNet.PacketPriority.IMMEDIATE_PRIORITY, RakNet.PacketReliability.RELIABLE_ORDERED, (char)0, m_cServerSystemAddress, false);

					// Reset stream
					s_cUntrottledSerializationStream.Clear();
				}

				// Decrement timer by interval
				m_fPacketOutboundTimer -= m_fPacketOutboundInterval;
			}
		}
    }


	void ProcessInboundOutboundRates()
	{
		// Timer increment
		m_tInboundRateData.fTimer += Time.deltaTime;
		m_tOutboundRateData.fTimer += Time.deltaTime;


		if (m_tInboundRateData.fTimer > 1.0f)
		{
			m_tInboundRateData.uiLastTotalEntries = m_tInboundRateData.uiNumEntries;
			m_tInboundRateData.uiLastTotalBytes = m_tInboundRateData.uiBytes;

			m_tInboundRateData.uiNumEntries = 0;
			m_tInboundRateData.uiBytes = 0;
			m_tInboundRateData.fTimer = 0.0f;
		}


		if (m_tOutboundRateData.fTimer > 1.0f)
		{
			m_tOutboundRateData.uiLastTotalEntries = m_tOutboundRateData.uiNumEntries;
			m_tOutboundRateData.uiLastTotalBytes = m_tOutboundRateData.uiBytes;

			m_tOutboundRateData.uiNumEntries = 0;
			m_tOutboundRateData.uiBytes = 0;
			m_tOutboundRateData.fTimer = 0.0f;
		}
	}


    void HandleConnectionAccepted(RakNet.SystemAddress _cServerSystemAddress)
    {
		m_fConnectionElapsedTime = 0.0f;
		m_fTick = 0;
		m_fTickTotal = 0;
		m_bDownloadingInitialGameState = true;

        // Save server address
        m_cServerSystemAddress = new RakNet.SystemAddress(_cServerSystemAddress.ToString(), _cServerSystemAddress.GetPort());

        // Notify event observers
        if (EventConnectionAccepted != null)
        {
            EventConnectionAccepted();
        }

        Logger.Write("Connection established with server");
    }


    void HandleNetworkViewPacket(byte[] _baData)
    {
		if (IsConnected)
		{
			// Create stream with data
			CNetworkStream cStream = new CNetworkStream(_baData);

			// Ignore ID_TIME
			cStream.IgnoreBytes(1);
			
			// Retrieve latency
			ulong ulLatency = RakNet.RakNet.GetTime() - cStream.ReadULong();
			// Ignore EPacketId.NetworkView identifier
			cStream.IgnoreBytes(1);
			
			// Process packet data
			CNetworkView.ProcessInboundStream(ulLatency, cStream);

			// Logger.WriteError("Processed Inbound Data of size ({0})", cStream.GetSize());
		}
    }


    void HandleDisconnect(EDisconnectType _eDisconnectType)
    {
        // Notify event observers
        if (EventDisconnect != null)
        {
            EventDisconnect();
        }

        Logger.Write("Disconnect notification ({0})", _eDisconnectType);
    }
	
	
	void HandleMicrophoneAudio(byte[] _bAudioData)
	{
		if (EventRecievedMicrophoneAudio != null)
		{
			CNetworkStream cAudioDataStream = new CNetworkStream(_bAudioData);
			cAudioDataStream.IgnoreBytes(1);
			
			EventRecievedMicrophoneAudio(cAudioDataStream);
		}
	}


    bool StartupPeer()
    {
        m_cRnPeer = new RakNet.RakPeer();
        m_usPort = (ushort)UnityEngine.Random.Range(10000, 30000);


        RakNet.SocketDescriptor tSocketDesc = new RakNet.SocketDescriptor((ushort)m_usPort, "");
        RakNet.StartupResult eStartupResult = m_cRnPeer.Startup(1, tSocketDesc, 1);
        bool bPeerStarted = false;


        if (eStartupResult != RakNet.StartupResult.RAKNET_STARTED)
        {
            Logger.WriteError("Raknet peer failed to start. ErrorCode({0}) Port({1})", eStartupResult, m_usPort);
        }
        else
        {
            bPeerStarted = true;
			m_cRnPeer.SetOccasionalPing(true);

            Logger.Write("Raknet peer started. Port({0})", m_usPort);
        }


        return (bPeerStarted);
    }


    void ShutdownPeer()
    {
        m_cRnPeer.Shutdown(200);
        m_cRnPeer = null;
    }


	[ANetworkRpc]
	void NotifyDownloadingInitialGameStateComplete()
	{
		m_bDownloadingInitialGameState = false;

		if (EventInitialGameStateDownloaded != null)
		{
			EventInitialGameStateDownloaded();
		}
	}


    void OnGUI()
    {
        if (IsConnected &&
            m_bShowStats)
        {
            RakNet.RakNetStatistics cStatistics = m_cRnPeer.GetStatistics(m_cServerSystemAddress);

            string sStatistics = "";
            sStatistics += string.Format("Connection Statistics\n");
            sStatistics += string.Format("Server ({0})\n", m_cServerSystemAddress.ToString());
            sStatistics += string.Format("Ping ({0}) Average ({0})\n", m_cRnPeer.GetLastPing(m_cServerSystemAddress), m_cRnPeer.GetAveragePing(m_cServerSystemAddress));
            sStatistics += string.Format("Send Buffer ({0} Messages) ({1}b)\n", cStatistics.messageInSendBuffer[0], cStatistics.bytesInSendBuffer[0]);
            sStatistics += string.Format("Resend Buffer ({0} Messages) ({1}b)\n", cStatistics.messagesInResendBuffer, cStatistics.bytesInResendBuffer);
            sStatistics += string.Format("Packet Loss ({0}%/s) ({1}% Total)\n", cStatistics.packetlossLastSecond * 100.0f, cStatistics.packetlossTotal * 100.0f);
            sStatistics += string.Format("Inbound ({0}B/s {1} Messages)\n", m_tInboundRateData.uiLastTotalBytes, m_tInboundRateData.uiLastTotalEntries);
            sStatistics += string.Format("Outbound ({0}B/s {1} Messages)\n", m_tOutboundRateData.uiLastTotalBytes, m_tOutboundRateData.uiLastTotalEntries);


            GUI.Label(new Rect(Screen.width - 250, 0.0f, 250, 200), sStatistics);
        }
    }


    static void CompileSerializeTargetsOutboundData()
    {
        // Create packet stream
        CNetworkStream cSerializedDataStream = new CNetworkStream();


        foreach (KeyValuePair<byte, TSerializationMethods> tEntry in s_mSerializeTargets)
        {
            tEntry.Value.nSerializeMethod(cSerializedDataStream);


            if (cSerializedDataStream.Size > 0)
            {
                // Write the control identifier
                s_cUntrottledSerializationStream.Write(tEntry.Key);

                // Write the serializing target type
                s_cUntrottledSerializationStream.Write((byte)ESerializeTargetType.Unthrottled);

                // Write the size of the data
                s_cUntrottledSerializationStream.Write((byte)cSerializedDataStream.Size);

                // Write the data
                s_cUntrottledSerializationStream.Write(cSerializedDataStream);

                // Clear target stream
                cSerializedDataStream.Clear();
            }
        }
    }


    static void CompileThrottledSerializeTargetsOutboundData(CNetworkStream _cOutboundStream)
    {
        // Create packet stream
        CNetworkStream cSerializedDataStream = new CNetworkStream();


        foreach (KeyValuePair<byte, TSerializationMethods> tEntry in s_mThrottledSerializeTargets)
        {
            tEntry.Value.nSerializeMethod(cSerializedDataStream);


            if (cSerializedDataStream.Size > 0)
            {
                // Write the control identifier
                _cOutboundStream.Write(tEntry.Key);

                // Write the serializing target type
                _cOutboundStream.Write((byte)ESerializeTargetType.Throttled);

                // Write the size of the data
                _cOutboundStream.Write((byte)cSerializedDataStream.Size);

                // Write the data
                _cOutboundStream.Write(cSerializedDataStream);

                // Clear target stream
                cSerializedDataStream.Clear();
            }
        }
    }


// Member Fields
    

    RakNet.RakPeer m_cRnPeer = null;
    RakNet.SystemAddress m_cServerSystemAddress = null;
	TRateData m_tInboundRateData = new TRateData();
	TRateData m_tOutboundRateData = new TRateData();


	float m_fConnectionElapsedTime = 0.0f;
	float m_fTickTimer = 0.0f;
    float m_fPacketOutboundTimer = 0.0f;
	float m_fPacketOutboundInterval = 1.0f / k_uiOutboundRate;
	float m_fTick = 0;
	float m_fTickTotal = 0;


    ushort m_usPort = 0;


    bool m_bShowStats = true;
	bool m_bDownloadingInitialGameState = true;


	static CNetworkStream s_cUntrottledSerializationStream = new CNetworkStream();
	static Dictionary<byte, TSerializationMethods> s_mSerializeTargets = new Dictionary<byte, TSerializationMethods>();
	static Dictionary<byte, TSerializationMethods> s_mThrottledSerializeTargets = new Dictionary<byte, TSerializationMethods>();


};
