//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//  (c) 2013 Bryce Booth
//
//  File Name :   ProcessObject.cpp
//
//  Author    :   Bryce Booth
//  Mail      :   brycebooth@hotmail.com
//


#include "ProcessObject.h"


// Local Includes
#include "Framework/Application.h"


// Library Includes


// Static Variables
std::list<CProcessObject*> CProcessObject::s_ProcessObjects;


// Implementation


/********************************
            Public
*********************************/


CProcessObject::CProcessObject()
: m_uiProcessOrderIndex(0)
, m_bProcessingEnabled(true)
{
	s_ProcessObjects.push_back(this);
}


CProcessObject::~CProcessObject()
{
	s_ProcessObjects.remove(this);
}


void 
CProcessObject::SetProcessingEnabled(bool _bEnabled)
{
	m_bProcessingEnabled = _bEnabled;
}


/********************************
            Protected
*********************************/


/********************************
            Private
*********************************/

