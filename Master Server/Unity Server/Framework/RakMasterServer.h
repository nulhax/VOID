//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//  (c) 2013 Bryce Booth
//
//  File Name   :   RakMasterServer.h
//  Description :   --------------------------
//
//  Author      :   Bryce Booth
//  Mail        :   brycebooth@hotmail.com
//


#pragma once


#ifndef __RakMasterServer_H__
#define __RakMasterServer_H__


// Dependent Includes


// Local Includes
#include "Library/ProcessObject.h"


// Library Includes
#include <RakNet40802/RakPeer.h>
#include <RakNet40802/RakNetTypes.h>
#include <RakNet40802/MessageIdentifiers.h>
#include <string>
#include <map>


// Prototypes


class CRakMasterServer : public CProcessObject
{

	// Member Types
public:


	static const uint s_kuiMaxTitleLength;
	static const ushort s_kusPort;
	static const float s_kfRegistrationTimeout;


	enum EMessageId
	{
		MessageId_ServerList = ID_USER_PACKET_ENUM
	};


	struct TServerDetails
	{
		float fUnregisterTimer;
		RakNet::SystemAddress tAddress;
		RakNet::RakNetGUID tGuid;
	};


	// Member Functions
public:


	 CRakMasterServer();
	~CRakMasterServer();


	bool Initialise();
	void Process();


	void Shutdown();


	// Inline Functions


protected:


	void ProcessInboundPackets();
	void ProcessRegistrations();
	void ProcessCompilingServerList();


	void RegisterServer(RakNet::SystemAddress& _rServerAddress, RakNet::RakNetGUID& _rServerGuid, uchar* _ucpData, uint _uiLength);
	void HandlePlayerConnect(RakNet::SystemAddress _cSystemAddress, RakNet::RakNetGUID _cGuid);


	void UnregisterServer(uint64& _ui64rServerGuid);


private:


	CRakMasterServer(const CRakMasterServer& _krRakMasterServer);
	CRakMasterServer& operator = (const CRakMasterServer& _krRakMasterServer);


	// Member Variables
protected:


private:


	RakNet::BitStream m_CompiledServerList;


	float m_fListRecompileTimer;
	float m_fListRecompileInterval;


	RakNet::RakPeer* m_pRnPeer;


	std::map<uint64, TServerDetails*> m_mRegisteredServers;


};


#include "Inline/RakMasterServer.inl"


#endif //__RakMasterServer_H__