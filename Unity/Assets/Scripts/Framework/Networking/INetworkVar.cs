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


public enum ENetworkVarType
{
    Invalid,

    Value_1_Byte,
    Value_2_Bytes,
    Value_4_Bytes,

    Struct,
    String
}


public interface INetworkVar
{

// Member Types


// Member Functions

    // public:


	void Sync(object _cValue);


    void Set(object _cNewValue);
	void SetNetworkViewOwner(CNetworkView _cNetworkView, byte _bNetworkVarId);


    object GetAsObject();
	int GetSize();
    ENetworkVarType GetVarType();
    Type GetValueType();


    // protected:


    // private:



// Member Variables

    // protected:


    // private:


};
