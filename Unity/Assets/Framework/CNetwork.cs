//  Auckland
//  New Zealand
//
//  (c) 2013 S.H.I.T
//
//  File Name   :   Network.h
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


class CNetwork : MonoBehaviour
{

    // Member Types
	
	
	enum ESettings
	{
		Port	= 30001	
	}


    // Member Functions
    
// public:


    public void Start()
    {
		 m_cRnPeer 	= RakNet.RakPeerInterface.GetInstance();
    }
    
    
    public void OnDestroy()
    {
		Disconnect();
    }
	
	
	public void Update()
	{
		ProcessIncomingPackets();
		ProcessOutgoingPackets();
	}
	
	
	public bool Connect(string _sServerIp, uint _uiPort, string _sPassword)
	{
		RakNet.SocketDescriptor tSocketDesc = new RakNet.SocketDescriptor((ushort)ESettings.Port, "");
		RakNet.StartupResult eStartupResult = m_cRnPeer.Startup(1, tSocketDesc, 1);
		bool bSuccessful = false;
				
		
		RakNet.ConnectionAttemptResult eConnectionAttempResult = m_cRnPeer.Connect(_sServerIp, (ushort)_uiPort,
																				   _sPassword, _sPassword.Length);
		
		
		
		if (eConnectionAttempResult != RakNet.ConnectionAttemptResult.CONNECTION_ATTEMPT_STARTED)
		{
			Debug.LogError(string.Format("Connection to server attempt failed. ErrorId{0}", eConnectionAttempResult));
		}
		else
		{
			Debug.Log(string.Format("Connection to server attempt started. Ip{0} Port{1}", _sServerIp, ESettings.Port));
			bSuccessful = true;
		}
		
		
		return (bSuccessful);
	}
	
	
	public void Disconnect()
	{
		
		RakNet.RakPeerInterface.DestroyInstance(m_cRnPeer);
	}
	
	
	public void SendData()
	{	
	}
	
	
	public void SendInt()
	{
	}
	
	
	public void SendString()
	{
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
				//HandleConnectSuccess(pRnPacket->systemAddress);
				break;
	
			case RakNet.DefaultMessageIDTypes.ID_NO_FREE_INCOMING_CONNECTIONS:
				//HandleConnectFailServerFull();
				break;
	
			case RakNet.DefaultMessageIDTypes.ID_DISCONNECTION_NOTIFICATION:
				//HandleDisconnectServerShutdown();
				break;
	
			case RakNet.DefaultMessageIDTypes.ID_CONNECTION_LOST:
				//HandleTimout();
				break;
	
			//case PACKET_ID_APPLICATION:
				//HandleApplicationPacket(&pRnPacket->data[1], pRnPacket->length - 1);
				//break;
	
			default:
				//FW_LOG_MESSAGE("Message with identifier %i has arrived.\n", pRnPacket->data[0]);
				break;
			}
	
	
			m_cRnPeer.DeallocatePacket(cRnPacket);
		}
	}
	
	
	protected void ProcessOutgoingPackets()
	{
		m_fOutgoingTimer += Time.deltaTime;
		
		
		if (m_fOutgoingTimer > 0.0f)
		{
			
			
			
			m_fOutgoingTimer = 0.0f;
		}
	}
	

// private:


    // Member Variables
    
// protected:


// private:
	
	
	RakNet.RakPeerInterface m_cRnPeer 	= null;
	
	
	float m_fOutgoingFrequency 			= 30.0f;
	float m_fOutgoingTimer				= 0.0f;


};
