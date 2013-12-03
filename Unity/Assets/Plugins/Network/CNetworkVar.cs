//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   NetworkVariable.h
//  Description :   --------------------------
//
//  Author  	:  Programming Team
//  Mail    	:  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;


/* Implementation */


public class CNetworkVar<TYPE> : INetworkVar
{

// Member Types


// Member Events


	public delegate void OnSetCallback(byte _bIdentifier);
	public delegate void OnSyncCallback(INetworkVar _cVarInstance);


// Member Functions

    // public:


	public CNetworkVar()
	{

	}


	public CNetworkVar(OnSyncCallback _cSyncObserver)
    {
		m_nSyncNotifyCallback = _cSyncObserver;
    }


	public CNetworkVar(OnSyncCallback _cSyncObserver, TYPE _DefaultValue)
    {
		m_nSyncNotifyCallback = _cSyncObserver;
		m_Value = _DefaultValue;
		m_StartValue = _DefaultValue;
    }


    public void Set(TYPE _NewValue)
    {
        if (!CNetwork.IsServer)
        {
			Logger.WriteError("Clients are not allowed to set network variables!");
        }
        else
        {
			if (!(_NewValue).Equals(m_Value))
			{
				m_Value = _NewValue;
				m_nSetNotifyCallback(m_bNetworkVarId);
			}
        }
    }


	public override void InvokeSyncCallback()
	{
		Logger.WriteErrorOn(m_nSyncNotifyCallback == null, "This network var does not have a OnSyncCallback defined!!");

		// Notify observer
		m_nSyncNotifyCallback(this);
	}


	public override void SyncValue(object _cValue, float _fSyncTick)
	{
		m_Value = (TYPE)_cValue;
		m_fSyncedTick = _fSyncTick;
	}


	public override void SetNetworkViewOwner(byte _bNetworkVarId, CNetworkVar<object>.OnSetCallback _nSetCallback)
	{
		Logger.WriteErrorOn(m_bNetworkVarId != 0, "You should not change a network var's network view owner once set. Undefined behaviour may occur");

		m_nSetNotifyCallback = _nSetCallback;
		m_bNetworkVarId = _bNetworkVarId;
	}


    public TYPE Get()
    {
        return (m_Value);
    }


	public override object GetValueObject()
    {
        return (m_Value);
    }


	public override Type GetValueType()
    {
        return (m_Value.GetType());
    }


	public override float GetLastSyncedTick()
	{
		return (m_fSyncedTick);
	}


	public override bool IsDefault()
	{
		return (m_Value.Equals(m_StartValue));
	}


    // protected:


    // private:


// Member Variables

    // protected:


    // private:


    TYPE m_Value;
	TYPE m_StartValue;


	CNetworkVar<object>.OnSetCallback m_nSetNotifyCallback = null;
	OnSyncCallback m_nSyncNotifyCallback = null;
	float m_fSyncedTick = 0;
	uint m_uiSendCount = 0;
	uint m_uiSendPacketOffset = 0;


	byte m_bNetworkVarId = 0;


};
