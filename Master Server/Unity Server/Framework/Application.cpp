//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//  (c) 2013 Bryce Booth
//
//  File Name :   Application.cpp
//
//  Author    :   Bryce Booth
//  Mail      :   bryce.booth@mediadesign.school.nz
//


#include "Application.h"


// Local Includes
#include "Framework/Logger.h"
#include "Framework/Clock.h"
#include "Framework/RakMasterServer.h"
#include "Defines/Macros.h"


// Library Includes


// Static Variables
CApplication* CApplication::s_pInstance = null;


// Implementation


/********************************
            Public
*********************************/


CApplication::CApplication()
: m_pLogger(null)
, m_pClock(null)
, m_pMasterServer(null)
, m_bQuit(false)
{
	s_pInstance = this;
}


CApplication::~CApplication()
{
	FW_DELETE(m_pLogger);
	FW_DELETE(m_pClock);
	FW_DELETE(m_pMasterServer);


	s_pInstance = 0;
}


bool 
CApplication::Initialise()
{
	m_pLogger = new CLogger();
	VALIDATE(m_pLogger->Initialise());


	m_pClock = new CClock();
	VALIDATE(m_pClock->Initialise());


	m_pMasterServer = new CRakMasterServer();
	VALIDATE(m_pMasterServer->Initialise());


	return (true);
}


bool 
CApplication::ExecuteOneFrame()
{
	m_pClock->ProcessFrame();


	return (true);
}


void 
CApplication::Quit()
{
	m_bQuit = true;
}


/********************************
            Protected
*********************************/


/********************************
            Private
*********************************/