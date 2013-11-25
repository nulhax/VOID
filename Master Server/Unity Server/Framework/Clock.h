//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//  (c) 2013 Bryce Booth
//
//  File Name   :   WinClock.h
//  Description :   --------------------------
//
//  Author      :   Bryce Booth
//  Mail        :   brycebooth@hotmail.com
//


#pragma once


#ifndef __WinClock_H__
#define __WinClock_H__


// Dependent Includes
#include "Framework/Application.h"
#include "Defines/DataTypes.h"


// Local Includes


// Library Includes


// Defines
#define CLOCK	APPLICATION.GetClock()


// Prototypes


class CClock
{

	// Member Types
public:


	// Member Functions
public:


	 CClock();
	~CClock();


	bool Initialise();
	bool ProcessFrame();


	// Inline Functions
	inline float GetDeltaTick() const;
	inline float GetFramesPerSecond() const;


protected:


	void UpdateDeltatick();
	void UpdateFps();
	void ProcessObjects();


private:


	CClock(const CClock& _krWinClock);
	CClock& operator = (const CClock& _krWinClock);


	// Member Variables
protected:


private:


	int64 m_i64CurrentTime;
	int64 m_i64LastTime;
	int64 m_i64LastTimeAfterSleep;
	int64 m_i64CountsPerSecond;


	float m_fDeltaTick;
	float m_fPerfCounterTimeScale;
	float m_fTimeElasped;
	float m_fFrameCount;
	float m_fFramesPerSecond;


	static const float s_kfMaxFrames;


};


#include "Inline/Clock.inl"


#endif //__WinClock_H__