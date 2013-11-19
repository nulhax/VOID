﻿//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   NetworkVar.h
//  Description :   --------------------------
//
//  Author  	:  Programming Team
//  Mail    	:  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System;


/* Implementation */


public abstract class INetworkVar
{

// Member Types


// Member Functions

    // public:


	public abstract void Sync(object _cValue);
	public abstract void SetNetworkViewOwner(byte _bNetworkVarId, CNetworkVar<object>.OnSetCallback _nSetCallback);


	public abstract object GetValueObject();
	public abstract Type GetValueType();


	public abstract bool IsDefault();


    // protected:


    // private:



// Member Variables

    // protected:


    // private:


};
