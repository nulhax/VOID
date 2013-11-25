//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//  (c) 2013 Bryce Booth
//
//  File Name :   WinClock.cpp
//
//  Author    :   Bryce Booth
//  Mail      :   brycebooth@hotmail.com
//


#include "Clock.h"


// Local Includes
#include "Library/ProcessObject.h"


// Library Includes
#include <Windows.h>
#include <iostream>


// Static Variables
const float CClock::s_kfMaxFrames = 60.0f;


// Implementation


/********************************
            Public
*********************************/


CClock::CClock()
: m_i64CurrentTime(0)
, m_i64LastTime(0)
, m_i64LastTimeAfterSleep(0)
, m_i64CountsPerSecond(0)
, m_fDeltaTick(0)
, m_fPerfCounterTimeScale(0)
, m_fTimeElasped(0)
, m_fFrameCount(0)
, m_fFramesPerSecond(0)
{
	// Empty
}


CClock::~CClock()
{
	// Empty
}


bool
CClock::Initialise()
{
	bool bReturn = true;


	bool bQueryReturn = (QueryPerformanceFrequency(reinterpret_cast<LARGE_INTEGER*>(&m_i64CountsPerSecond)) != 0);


	if (!bQueryReturn)
	{
		bReturn = false;
		//TODO: Update error. ("Could not query the performance frequency from the CPU. Feature may not be supported.");
	}
	else
	{
		m_fPerfCounterTimeScale = 1.0f / static_cast<float>(m_i64CountsPerSecond);


		bQueryReturn = (QueryPerformanceCounter(reinterpret_cast<LARGE_INTEGER*>(&m_i64LastTime)) != 0);
	}


	return (bReturn);
}


bool 
CClock::ProcessFrame()
{
	UpdateDeltatick();
	UpdateFps();
	ProcessObjects();


	return (true);
}


/********************************
            Protected
*********************************/


void 
CClock::UpdateDeltatick()
{
	QueryPerformanceCounter(reinterpret_cast<LARGE_INTEGER*>(&m_i64CurrentTime));


	m_fDeltaTick  = static_cast<float>(m_i64CurrentTime - m_i64LastTime);
	m_fDeltaTick *= m_fPerfCounterTimeScale;


	m_i64LastTime = m_i64CurrentTime;


	float fDeltatickUncapped  = static_cast<float>(m_i64CurrentTime - m_i64LastTimeAfterSleep);
	fDeltatickUncapped *= m_fPerfCounterTimeScale;


	float fWaitPerFrame = 1.0f / s_kfMaxFrames;
	float fWaitThisFrame = fWaitPerFrame - fDeltatickUncapped;


	if (fWaitThisFrame > 0.0f)
	{
		Sleep((uint)(fWaitThisFrame * 1000.0f));
	}


	QueryPerformanceCounter(reinterpret_cast<LARGE_INTEGER*>(&m_i64LastTimeAfterSleep));
}


void
CClock::UpdateFps()
{
	m_fTimeElasped += GetDeltaTick();


	if (m_fTimeElasped > 1.0f)
	{
		m_fFramesPerSecond = m_fFrameCount + 1.0f * (m_fTimeElasped - 1.0f);
		m_fFrameCount = 0.0f;
		m_fTimeElasped = 0.0f;


		//std::cout << m_fFramesPerSecond << "(" <<  m_fDeltaTick << ")" << std::endl;
	}


	++ m_fFrameCount;
}


void 
CClock::ProcessObjects()
{
	std::list<CProcessObject*> aProcessObjects = CProcessObject::GetProcessObjectsList();
	std::list<CProcessObject*>::iterator Current = aProcessObjects.begin();
	std::list<CProcessObject*>::iterator End = aProcessObjects.end();

	while (Current != End)
	{
		(*Current)->Process();

		++ Current;
	}
}


/********************************
            Private
*********************************/

