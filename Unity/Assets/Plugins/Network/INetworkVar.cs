//  Auckland
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


	public abstract void InvokeSyncCallback();
	public abstract void SyncValue(object _cValue, float _fSyncTime);
	public abstract void SetNetworkViewOwner(byte _bNetworkVarId, CNetworkVar<object>.OnSetCallback _nSetCallback);


	public abstract object GetValueObject();
	public abstract Type GetValueType();
	public abstract float GetLastSyncedTime();


	public abstract bool IsDefault();


    // protected:


    // private:



// Member Variables

    // protected:


    // private:


};
