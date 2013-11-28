//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//  (c) 2013 Bryce Booth
//
//  File Name   :   ProcessObject.h
//  Description :   --------------------------
//
//  Author      :   Bryce Booth
//  Mail        :   brycebooth@hotmail.com
//


#pragma once


#ifndef __ProcessObject_H__
#define __ProcessObject_H__


// Dependent Includes


// Local Includes
#include "Defines/DataTypes.h"


// Library Includes
#include <list>


// Prototypes


class CProcessObject
{

	// Member Types
public:


	// Member Functions
public:


			 CProcessObject();
	virtual ~CProcessObject();

	/*------------------------------------------------------------------
	@Description: Allows the object to process
	------------------------------------------------------------------*/
	virtual void Process() = 0;

	/*------------------------------------------------------------------
	@Description: Enables/disabled processing for this object
	------------------------------------------------------------------*/
	void SetProcessingEnabled(bool _bEnabled);


	// Inline Functions

	/*------------------------------------------------------------------
	@Description: Returns the process order index
	------------------------------------------------------------------*/
	inline uint GetProcessOrderIndex() const;

	/*------------------------------------------------------------------
	@Description: Returns if this object is processing or not
	------------------------------------------------------------------*/
	inline bool IsProcessingEnabled() const;

	/*------------------------------------------------------------------
	@Description: Returns list that contains all process objects
	------------------------------------------------------------------*/
	static inline std::list<CProcessObject*>& GetProcessObjectsList();


protected:


private:


	CProcessObject(const CProcessObject& _krProcessObject);
	CProcessObject& operator = (const CProcessObject& _krProcessObject);


	// Member Variables
protected:


private:


	uint m_uiProcessOrderIndex;


	uchar m_ucOwnerSceneIndex;


	bool m_bProcessingEnabled;


	static std::list<CProcessObject*> s_ProcessObjects;


};


#include "Inline/ProcessObject.inl"


#endif //__ProcessObject_H__