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


public interface INetworkVar
{

// Member Types


// Member Functions

    // public:


	void Sync(object _cValue);
	void SetNetworkViewOwner(byte _bNetworkVarId, CNetworkVar<object>.OnSetCallback _nSetCallback);


	object GetValueObject();
    Type GetValueType();


	bool IsDefault();


    // protected:


    // private:



// Member Variables

    // protected:


    // private:


};
