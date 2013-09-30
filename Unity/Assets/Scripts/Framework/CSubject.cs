//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   Subject.h
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


#pragma warning disable 0649


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CSubject<TYPE> where TYPE : class
{

// Member Types


// Member Functions

    // public:


    public void Subscribe(IObserver<TYPE> _pObserver, short _sSubject)
    {
        while (_sSubject >= m_aaObservers.Count)
        {
            m_aaObservers.Add(new List<IObserver<TYPE>>());
        }


        m_aaObservers[_sSubject].Add(_pObserver);
    }


    public void Unsubscribe(IObserver<TYPE> _pObserver, short _sSubject)
    {
        m_aaObservers[_sSubject].Remove(_pObserver);
    }


    public void UnsubscribeAll(IObserver<TYPE> _pObserver)
    {
        foreach (List<IObserver<TYPE>> aSubjectObservers in m_aaObservers)
        {
            aSubjectObservers.Remove(_pObserver);
        }
    }


    // protected:


    protected void NotifySubscribers(short _sSubject)
    {
        foreach (IObserver<TYPE> aSubjectObservers in m_aaObservers[_sSubject])
        {
            aSubjectObservers.Notify(null, _sSubject, null);
        }
        
    }


    // private:


// Member Variables

    // protected:


    // private:


    List<List<IObserver<TYPE>>> m_aaObservers = new List<List<IObserver<TYPE>>>();


};
