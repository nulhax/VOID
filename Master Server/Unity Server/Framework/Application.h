//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//  (c) 2013 Bryce Booth
//
//  File Name   :   Application.h
//  Description :   --------------------------
//
//  Author      :   Bryce Booth
//  Mail        :   bryce.booth@mediadesign.school.nz
//


#pragma once


#ifndef __Application_H__
#define __Application_H__


// Dependent Includes


// Local Includes
#include "Library/Subject.h"
#include "Defines/DataTypes.h"


// Library Includes


// Defines
#define APPLICATION			CApplication::GetInstance()


// Prototypes
class CRakMasterServer;
class CLogger;
class CClock;


class CApplication : public CSubject<CApplication>
{

	// Member Types
public:


	enum ESubject
	{
		SUBJECT_INIT_COMPLETE,
		SUBJECT_QUIT_CALLED,
	};


	// Member Functions
public:


	 CApplication();
	~CApplication();

	/*------------------------------------------------------------------
	@Description: Runs one frame on the application
	------------------------------------------------------------------*/
	bool Initialise();

	/*------------------------------------------------------------------
	@Description: Runs one frame on the application
	------------------------------------------------------------------*/
	bool ExecuteOneFrame();

	/*------------------------------------------------------------------
	@Description: Calls application to quit at end of current frame
	------------------------------------------------------------------*/
	void Quit();

	/*------------------------------------------------------------------
	@Description: Return logger instance
	------------------------------------------------------------------*/
	inline CLogger& GetLogger();

	/*------------------------------------------------------------------
	@Description: Return clock instance
	------------------------------------------------------------------*/
	inline CClock& GetClock();

	/*------------------------------------------------------------------
	@Description: Return network instance.
	------------------------------------------------------------------*/
	inline CRakMasterServer& GetMasterServer();

	/*------------------------------------------------------------------
	@Description: Returns if the application will quit or not at the
				  end of the current frame
	------------------------------------------------------------------*/
	inline bool IsQuitting();


	// Inline Functions

	/*------------------------------------------------------------------
	@Description: Return application singleton instance
	------------------------------------------------------------------*/
	static inline CApplication& GetInstance();


protected:


private:


	CApplication(const CApplication& _krApplication);
	CApplication& operator = (const CApplication& _krApplication);


	// Member Variables
protected:


private:


	CLogger* m_pLogger;
	CClock* m_pClock;
	CRakMasterServer* m_pMasterServer;


	bool m_bQuit;

	static CApplication* s_pInstance;


};


#include "Inline/Application.inl"


#endif //__Application_H__