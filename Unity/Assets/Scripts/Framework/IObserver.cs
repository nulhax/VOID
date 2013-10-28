//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   Observer.h
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


#pragma warning disable 0649


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public interface IObserver<TYPE> where TYPE : class
{

// Member Types


// Member Functions

    // public:


    void Notify(TYPE _rSender, short _sSubject, byte[] _baData);


    // protected:


    // private:


// Member Variables

    // protected:


    // private:


};
