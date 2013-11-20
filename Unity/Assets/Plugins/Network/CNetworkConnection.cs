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


public class CNetworkConnection : MonoBehaviour
{

// Member Types


	public delegate void OnConnect();
	public event OnConnect EventConnectionAccepted;


	public delegate void OnDisconnect();
	public event OnDisconnect EventDisconnect;
	
	
	public delegate void HandleRecievedMicrophoneAudio(CNetworkStream _cAudioDataStream);
	public event HandleRecievedMicrophoneAudio EventRecievedMicrophoneAudio;


	public delegate void SerializeMethod(CNetworkStream _cStream);
	public delegate void UnserializeMethod(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream);


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


// Member Functions
    
    // public:


    public void Awake()
    {
        StartupPeer();


		m_cOutboundSerializationStream.Write((byte)CNetworkServer.EPacketId.PlayerSerializedData);
    }


    public void OnDestory()
    {
        Disconnect();
        ShutdownPeer();
    }


    public void Update()
    {
        // Process packets
        if (this.IsActive)
        {
            ProcessInboundPackets();

			if (this.IsConnected)
            {
				ProcessOutboundPackets();
			}

			ProcessRates();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            m_bShowStats = !m_bShowStats;
        }
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


	public void OnGUI()
	{
		if (IsConnected &&
            m_bShowStats)
		{
			RakNet.RakNetStatistics cStatistics = m_cRnPeer.GetStatistics(m_cServerSystemAddress);

			string sStatistics = "";
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


    public RakNet.RakPeer RakPeer
    {
        get { return (m_cRnPeer); }
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

		// Ignore packet id
		cStream.IgnoreBytes(1);

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


    // protected:


    protected void ProcessInboundPackets()
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
                        HandleNetworkViewPacket(cRnPacket.data);
                    }
                    break;
				
                case (RakNet.DefaultMessageIDTypes)EPacketId.MicrophoneAudio:
                    {
                        HandleMicrophoneAudio(cRnPacket.data);
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


    protected void ProcessOutboundPackets()
    {
		CompileSerializeTargetsOutboundData(m_cOutboundSerializationStream);

        // Increment outbound timer
		m_fPacketOutboundTimer += Time.deltaTime;

        if (m_fPacketOutboundTimer > m_fPacketOutboundInterval)
		{
			CompileThrottledSerializeTargetsOutboundData(m_cOutboundSerializationStream);

            // Check player has data to be sent to the server
			if (m_cOutboundSerializationStream.Size > 1)
            {
				m_tOutboundRateData.uiBytes += m_cOutboundSerializationStream.Size;
				m_tOutboundRateData.uiNumEntries += 1;

                // Dispatch data to the server
				m_cRnPeer.Send(m_cOutboundSerializationStream.BitStream, RakNet.PacketPriority.IMMEDIATE_PRIORITY, RakNet.PacketReliability.RELIABLE_ORDERED, (char)0, m_cServerSystemAddress, false);

				// Reset stream
				m_cOutboundSerializationStream.Clear();
				m_cOutboundSerializationStream.Write((byte)CNetworkServer.EPacketId.PlayerSerializedData);
            }

            // Decrement timer by interval
            m_fPacketOutboundTimer -= m_fPacketOutboundInterval;
		}
    }


	protected void ProcessRates()
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


    protected void HandleConnectionAccepted(RakNet.SystemAddress _cServerSystemAddress)
    {
        // Save server address
        m_cServerSystemAddress = new RakNet.SystemAddress(_cServerSystemAddress.ToString(), _cServerSystemAddress.GetPort());

        // Notify event observers
        if (EventConnectionAccepted != null)
        {
            EventConnectionAccepted();
        }

        Logger.Write("Connection established with server");
    }


    protected void HandleNetworkViewPacket(byte[] _baData)
    {
		if (IsConnected)
		{
			// Create stream with data
			CNetworkStream cStream = new CNetworkStream(_baData);

			// Ignore packet id
			cStream.IgnoreBytes(1);

			// Process packet data
			CNetworkView.ProcessInboundStream(cStream);

			// Logger.WriteError("Processed Inbound Data of size ({0})", cStream.GetSize());
		}
    }


    protected void HandleDisconnect(EDisconnectType _eDisconnectType)
    {
        // Notify event observers
        if (EventDisconnect != null)
        {
            EventDisconnect();
        }

        Logger.Write("Disconnect notification ({0})", _eDisconnectType);
    }
	
	
	protected void HandleMicrophoneAudio(byte[] _bAudioData)
	{
		if (EventRecievedMicrophoneAudio != null)
		{
			CNetworkStream cAudioDataStream = new CNetworkStream(_bAudioData);
			cAudioDataStream.IgnoreBytes(1);
			
			EventRecievedMicrophoneAudio(cAudioDataStream);
		}
	}


	protected static void CompileSerializeTargetsOutboundData(CNetworkStream _cOutboundStream)
	{
		// Create packet stream
		CNetworkStream cSerializedDataStream = new CNetworkStream();


		foreach (KeyValuePair<byte, TSerializationMethods> tEntry in s_mSerializeTargets)
		{
			tEntry.Value.nSerializeMethod(cSerializedDataStream);


			if (cSerializedDataStream.Size > 0)
			{
				// Write the control identifier
				_cOutboundStream.Write(tEntry.Key);

				// Write the serializing target type
				_cOutboundStream.Write((byte)ESerializeTargetType.Unthrottled);

				// Write the size of the data
				_cOutboundStream.Write((byte)cSerializedDataStream.Size);

				// Write the data
				_cOutboundStream.Write(cSerializedDataStream);

				// Clear target stream
				cSerializedDataStream.Clear();
			}
		}
	}


	protected static void CompileThrottledSerializeTargetsOutboundData(CNetworkStream _cOutboundStream)
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


    // private:


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

            Logger.Write("Raknet peer started. Port({0})", m_usPort);
        }


        return (bPeerStarted);
    }


    void ShutdownPeer()
    {
        m_cRnPeer.Shutdown(200);
        m_cRnPeer = null;
    }


// Member Variables
    
    // protected:


    // private:


    RakNet.RakPeer m_cRnPeer = null;
    RakNet.SystemAddress m_cServerSystemAddress = null;
	CNetworkStream m_cOutboundSerializationStream = new CNetworkStream();
	CNetworkStream m_cThrottledSerializationStream = new CNetworkStream();
	TRateData m_tInboundRateData = new TRateData();
	TRateData m_tOutboundRateData = new TRateData();


    float m_fPacketOutboundTimer = 0.0f;
    float m_fPacketOutboundInterval = 1.0f / 30.0f;


    ushort m_usPort = 0;


    bool m_bShowStats = true;


	static Dictionary<byte, TSerializationMethods> s_mSerializeTargets = new Dictionary<byte, TSerializationMethods>();
	static Dictionary<byte, TSerializationMethods> s_mThrottledSerializeTargets = new Dictionary<byte, TSerializationMethods>();


};
