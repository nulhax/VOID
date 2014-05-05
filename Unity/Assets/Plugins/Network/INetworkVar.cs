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


    public enum EReliabilityType
    {
        INVALID,

        // Guaranteed to make it, all packets are recived and processed
        Reliable_Ordered,

        // Might not make it, old packets are thrown away
        Unreliable_Sequenced
    }


// Member Methods


    public abstract void Initialise(CNetworkView _cOwnerNetworkView, byte _bNetworkVarId, EReliabilityType _eReliabilityType);
	public abstract void InvokeSyncCallback();
	public abstract void SyncValue(object _cValue, float _fSyncTime);
    public abstract void SetSendInterval(float _fInterval);


	public abstract object GetValueObject();
	public abstract Type GetValueType();
	public abstract float GetLastSyncedTick();
    public abstract EReliabilityType GetReliabilityType();


	public abstract bool IsDefault();


// Member Fields


};
