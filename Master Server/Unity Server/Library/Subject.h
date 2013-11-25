//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//  (c) 2011 Bryce Booth
//
//  File Name   :   Subject.h
//  Description :   --------------------------
//
//  Author      :   Bryce Booth
//  Mail        :   bryce.booth@mediadesign.school.nz
//


#pragma once


#ifndef __Subject_H__
#define __Subject_H__


// Library Includes
#include <vector>
#include <algorithm>


// Local Includes
#include "Library/Observer.h"


// Types


// Prototypes


template <typename SUBJECT_TYPE>
class CSubject
{

	// Member Types


	typedef IObserver<SUBJECT_TYPE> ObserverType;
	#define ObserverIter typename std::vector< ObserverType* >::iterator


	// Member Functions
public:


			 CSubject();
	virtual ~CSubject();


	virtual void Subscribe(ObserverType* _pObserver, short _sSubject);
	virtual void Unsubscribe(ObserverType* _pObserver, short _sSubject);
	virtual void UnsubscribeAll(ObserverType* _pObserver);


	// Inline Functions


protected:


	void NotifySubscribers(short _sSubject, void* _pData = 0);


private:


	// Member Variables
protected:


private:


	int m_iMaxSubjectId;

	
	std::vector< std::vector<ObserverType*> > m_aObservers;


};


template <class SUBJECT_TYPE>
CSubject<SUBJECT_TYPE>::CSubject()
: m_iMaxSubjectId(-1)
{
	// Empty
}


template <class SUBJECT_TYPE>
CSubject<SUBJECT_TYPE>::~CSubject()
{
	// Empty
}


template <class SUBJECT_TYPE>
void
CSubject<SUBJECT_TYPE>::Subscribe(ObserverType* _pObserver, short _sSubject)
{
	if (_sSubject > m_iMaxSubjectId)
	{
		m_iMaxSubjectId = _sSubject;
		m_aObservers.resize(m_iMaxSubjectId + 1);
	}


	m_aObservers[_sSubject].push_back(_pObserver);
}


template <class SUBJECT_TYPE>
void
CSubject<SUBJECT_TYPE>::Unsubscribe(ObserverType* _pObserver, short _sSubject)
{
	ObserverIter Observer;
	ObserverIter Begin = m_aObservers[_sSubject].begin();
	ObserverIter End = m_aObservers[_sSubject].end();


	Observer = std::find(Begin, End, _pObserver);


	if (Observer != End)
	{
		(*Observer) = 0;
	}
}


template <class SUBJECT_TYPE>
void
CSubject<SUBJECT_TYPE>::UnsubscribeAll(ObserverType* _pObserver)
{
	for (unsigned int uiSubject = 0; uiSubject < m_aObservers.size(); ++ uiSubject)
	{
		Unsubscribe(_pObserver, uiSubject);
	}
}


template <class SUBJECT_TYPE>
void
CSubject<SUBJECT_TYPE>::NotifySubscribers(short _sSubject, void* _pData)
{
	if (_sSubject <= m_iMaxSubjectId)
	{
		ObserverIter Current = m_aObservers[_sSubject].begin();
		ObserverIter End = m_aObservers[_sSubject].end();


		for (; Current != End; ++ Current)
		{
			if ((*Current) != 0)
			{
				(*Current)->Notify(reinterpret_cast<SUBJECT_TYPE&>(*this), _sSubject, _pData);
			}
		}
	}
	else
	{
		// Don't send
	}
}


#endif //__Subject_H__