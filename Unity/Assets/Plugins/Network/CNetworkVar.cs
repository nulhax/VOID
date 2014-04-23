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


	public delegate void SyncedHandler(INetworkVar _cVarInstance);


// Member Properties


	public TYPE Value
	{
		get { return(Get()); }
		set { Set(value); }
	}


	public TYPE PreviousValue
	{
		get {return(GetPrevious());}
	}


    public bool IsSyncEnabled
    {
        get { return (m_bSyncEnabled); }
    }


// Member Methods


	public CNetworkVar()
	{
        // Empty
	}


	public CNetworkVar(SyncedHandler _cSyncedObserver)
    {
		m_nSyncedHandler = _cSyncedObserver;
    }


	public CNetworkVar(SyncedHandler _cSyncedObserver, TYPE _DefaultValue)
    {
		m_nSyncedHandler = _cSyncedObserver;
		m_Value = _DefaultValue;
		m_DefaultValue = _DefaultValue;
    }


    public virtual void Set(TYPE _NewValue, bool _bAllowSameValue)
    {
        if (!CNetwork.IsServer)
			Logger.WriteError("Clients are not allowed to set network variables!");

        bool bSetValue = false;

        // Check new value is not null
        if (_NewValue == null)
        {
            // Check current value is not null 
            if (m_Value != null)
            {
                bSetValue = true;
            }
        }

        // Check same value setting allowed
        // Or check values are not the same
        else if ( _bAllowSameValue ||
                 !(_NewValue).Equals(m_Value))
        {
            bSetValue = true;
        }
        
        // Check value has been allowed to be set
		if (bSetValue)
		{
			m_PreviousValue = m_Value;
			m_Value = _NewValue;

            // Check sending is only done by intervals
            if (m_bSendIntervalEnabled)
            {
                m_bValueDirty = true;
            }

            // Send change straight away
            else
            {
                m_cOwnerNetworkView.SyncNetworkVar(0, m_bNetworkVarId);
            }
		}
    }


    public virtual void Set(TYPE _NewValue)
    {
        Set(_NewValue, false);
    }


    public virtual void SetSendInterval(float _fInterval)
    {
        if (CNetwork.IsServer)
        {
            m_fSendInterval = _fInterval;

            if (m_fSendInterval > 0.0f)
            {
                CNetwork.EventNetworkUpdate += UpdateSendInterval;

                m_bSendIntervalEnabled = true;
            }
            else
            {
                if (m_bSendIntervalEnabled)
                {
                    CNetwork.EventNetworkUpdate -= UpdateSendInterval;
                }

                m_bSendIntervalEnabled = false;
            }
        }
    }


    public void SetSyncEnabled(bool _bEnabled)
    {
        m_bSyncEnabled = _bEnabled;
    }


	public override void InvokeSyncCallback()
	{
		Logger.WriteErrorOn(m_nSyncedHandler == null, "This network var does not have a OnSyncCallback defined!!");

		// Notify observer
		m_nSyncedHandler(this);
	}


	public override void SyncValue(object _cValue, float _fSyncTick)
	{
		// Previous values are already set on the server
		if (!CNetwork.IsServer)
		{
			m_PreviousValue = m_Value;
		}

		m_Value = (TYPE)_cValue;
		m_fSyncedTick = _fSyncTick;
	}


    public override void Initialise(CNetworkView _cOwnerNetworkView, byte _bNetworkVarId, EReliabilityType _eReliabilityType)
	{
		Logger.WriteErrorOn(m_bNetworkVarId != 0, "You should not change a network var's network view owner once set. Undefined behaviour may occur");

        m_cOwnerNetworkView = _cOwnerNetworkView;
		m_bNetworkVarId     = _bNetworkVarId;
        m_eReliabilityType  = _eReliabilityType;
	}


    public virtual TYPE Get()
    {
        return (m_Value);
    }


    public virtual TYPE GetPrevious()
	{
		return (m_PreviousValue);
	}


	public override object GetValueObject()
    {
        return (m_Value);
    }


	public override Type GetValueType()
    {
		return (typeof(TYPE));
    }


	public override float GetLastSyncedTick()
	{
		return (m_fSyncedTick);
	}


	public override bool IsDefault()
	{
		return ((m_DefaultValue == null && m_Value == null) || 
		        m_Value.Equals(m_DefaultValue));
	}


    public override EReliabilityType GetReliabilityType()
    {
        return (m_eReliabilityType);
    }


    void UpdateSendInterval(float _fDeltatick)
    {
        if (!m_bSendIntervalEnabled)
            return;

        m_fSendTimer += _fDeltatick;

        if (m_fSendTimer > m_fSendInterval)
        {
            m_fSendTimer -= m_fSendInterval;

            if (m_bSyncEnabled)
            {
                m_cOwnerNetworkView.SyncNetworkVar(0, m_bNetworkVarId);

                //m_bValueDirty = false;
            }
        }
    }


// Member Fields


    TYPE m_Value;
    TYPE m_DefaultValue;
    TYPE m_PreviousValue;


    CNetworkView m_cOwnerNetworkView = null;
    SyncedHandler m_nSyncedHandler = null;


    EReliabilityType m_eReliabilityType = EReliabilityType.INVALID;


    float m_fSyncedTick     = 0.0f;
    float m_fSendInterval   = 0.0f;
    float m_fSendTimer      = 0.0f;


    uint m_uiSendCount        = 0;
    uint m_uiSendPacketOffset = 0;


	byte m_bNetworkVarId = 0;
    bool m_bSendIntervalEnabled = false;
    bool m_bValueDirty = false;
    bool m_bSyncEnabled = true;


};
