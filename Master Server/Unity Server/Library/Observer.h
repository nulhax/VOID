//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//  (c) 2011 Bryce Booth
//
//  File Name   :   Observver.h
//  Description :   --------------------------
//
//  Author      :   Bryce Booth
//  Mail        :   bryce.booth@mediadesign.school.nz
//


#pragma once


#ifndef __Observver_H__
#define __Observver_H__


// Library Includes


// Local Includes
#include "Defines/DataTypes.h"


// Types


// Prototypes


template <class SUBJECT_TYPE>
class IObserver
{

	// Member Functions
public:


			 IObserver() {};
	virtual ~IObserver() {};


	virtual void Notify(SUBJECT_TYPE& _rSender, short _sSubject, void* _pData) = 0;


	// Inline Functions


protected:


private:


	// Member Variables
protected:


private:


};


#endif //__Observver_H__