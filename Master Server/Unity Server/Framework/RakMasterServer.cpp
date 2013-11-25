//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//  (c) 2013 Bryce Booth
//
//  File Name :   RakMasterServer.cpp
//
//  Author    :   Bryce Booth
//  Mail      :   brycebooth@hotmail.com
//


#include "RakMasterServer.h"


// Local Includes
#include "Framework/Logger.h"
#include "Framework/Clock.h"
#include "Defines/DataTypes.h"
#include "Defines/Macros.h"


// Library Includes
#include <iostream>


// Static Variables
const uint CRakMasterServer::s_kuiMaxTitleLength = 32;
const ushort CRakMasterServer::s_kusPort = 9896;
const float CRakMasterServer::s_kfRegistrationTimeout = 10.0f;


// Implementation


/********************************
            Public
*********************************/


CRakMasterServer::CRakMasterServer()
: m_pRnPeer(null)
, m_fListRecompileTimer(0)
, m_fListRecompileInterval(2.0f)
{
	// Empty
}


CRakMasterServer::~CRakMasterServer()
{
	FW_DELETE(m_pRnPeer);
}


bool
CRakMasterServer::Initialise()
{
	m_pRnPeer = new RakNet::RakPeer();
	bool bSuccessful = false;


	RakNet::SocketDescriptor tSocketDesc(s_kusPort, "");
	RakNet::StartupResult eStartupResult = m_pRnPeer->Startup(1000, &tSocketDesc, 1);


	if (eStartupResult != RakNet::RAKNET_STARTED)
	{
		LOG_ERROR("NetworkServer failed to start. ErrorCode(%d)", eStartupResult);
	}
	else
	{
		m_pRnPeer->SetMaximumIncomingConnections(1000);


		LOG_MESSAGE("Network client successfully started. Port(%d)", s_kusPort);


		bSuccessful = true;
	}


	LOG_MESSAGE("Ready!");


	return (true);
}


void 
CRakMasterServer::Process()
{
	ProcessInboundPackets();
	ProcessRegistrations();
}


void 
CRakMasterServer::Shutdown()
{
	m_pRnPeer->Shutdown(200);
}


/********************************
            Protected
*********************************/


void 
CRakMasterServer::ProcessInboundPackets()
{
	RakNet::Packet* pRnPacket = m_pRnPeer->Receive();


	while (pRnPacket != null)
	{
		switch (pRnPacket->data[0])
		{
		case ID_ADVERTISE_SYSTEM:
			RegisterServer(pRnPacket->systemAddress, pRnPacket->guid, pRnPacket->data, pRnPacket->length);
			break;

        case ID_REMOTE_NEW_INCOMING_CONNECTION:
        case ID_NEW_INCOMING_CONNECTION:
            {
                HandlePlayerConnect(pRnPacket->systemAddress, pRnPacket->guid);
				std::cout << "Player Connected. Sending server list..." << std::endl;
            }
            break;

		case ID_REMOTE_CONNECTION_LOST: // Fall through
		case ID_CONNECTION_LOST:
		case ID_REMOTE_DISCONNECTION_NOTIFICATION: // Fall through
		case ID_DISCONNECTION_NOTIFICATION:
			{
				std::cout << "Player disconnected." << std::endl;
			}
			break;

		default:
			LOG_MESSAGE("Unknown message with identifier %i has arrived.\n", pRnPacket->data[0]);
			break;
		}


		m_pRnPeer->DeallocatePacket(pRnPacket);
		pRnPacket = m_pRnPeer->Receive();
	}
}


void 
CRakMasterServer::ProcessRegistrations()
{
	std::map<uint64, TServerDetails*>::iterator Current = m_mRegisteredServers.begin();
	std::map<uint64, TServerDetails*>::iterator End = m_mRegisteredServers.end();
	m_CompiledServerList.Reset();
	m_CompiledServerList.Write((uchar)MessageId_ServerList);


	std::vector<uint64> aUnregisterList;
	std::string sIp = "";;


	while (Current != End)
	{
		TServerDetails* tpServerDetails = (*Current).second;
		tpServerDetails->fUnregisterTimer -= CLOCK.GetDeltaTick();


		if (tpServerDetails->fUnregisterTimer < 0.0f)
		{
			aUnregisterList.push_back(tpServerDetails->tGuid.g);
		}
		else
		{
			sIp = tpServerDetails->tAddress.ToString();


			m_CompiledServerList.Write(sIp.c_str(), sIp.length());
			m_CompiledServerList.Write("&", 1);
		}


		++ Current;
	}


	//LOG_MESSAGE("Proccess (%d) registrees", m_mRegisteredServers.size());


	for (uint i = 0; i < aUnregisterList.size(); ++ i)
	{
		UnregisterServer(aUnregisterList[i]);

		LOG_MESSAGE("Unregistered a server");
	}
}


void 
CRakMasterServer::HandlePlayerConnect(RakNet::SystemAddress _cSystemAddress, RakNet::RakNetGUID _cGuid)
{
	LOG_MESSAGE("Recieved Connection");


	m_pRnPeer->Send(&m_CompiledServerList, IMMEDIATE_PRIORITY, RELIABLE, 0, _cSystemAddress, false);
}


void
CRakMasterServer::RegisterServer(RakNet::SystemAddress& _rServerAddress, RakNet::RakNetGUID& _rServerGuid, uchar* _ucpData, uint _uiLength)
{
	std::map<uint64, TServerDetails*>::iterator Entry = m_mRegisteredServers.find(_rServerGuid.g);


	if (Entry == m_mRegisteredServers.end())
	{
		TServerDetails* tpServerDetails = new TServerDetails();


		char sServerIp[256];
		_rServerAddress.ToString_Old(false, sServerIp);


		tpServerDetails->tAddress = _rServerAddress;
		tpServerDetails->tGuid = _rServerGuid;
		tpServerDetails->fUnregisterTimer = s_kfRegistrationTimeout;


		m_mRegisteredServers.insert( std::pair<uint64, TServerDetails*>(_rServerGuid.g, tpServerDetails) );


		LOG_MESSAGE("Registered Server. IP(%s) GUID(%llu)", _rServerAddress.ToString(), _rServerGuid.g);
	}
	else
	{
		Entry->second->fUnregisterTimer = s_kfRegistrationTimeout;


		LOG_MESSAGE("Re-registered Server. IP(%s) GUID(%llu)", _rServerAddress.ToString(), _rServerGuid.g);
	}
}


void 
CRakMasterServer::UnregisterServer(uint64& _ui64rServerGuid)
{
	std::map<uint64, TServerDetails*>::iterator Entry = m_mRegisteredServers.find(_ui64rServerGuid);


	FW_DELETE(Entry->second);


	m_mRegisteredServers.erase(Entry);
}


/********************************
            Private
*********************************/

