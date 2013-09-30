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


    public enum ESubject
	{
		OnSync
	}


// Member Functions

    // public:


    public CNetworkVar(IObserver<INetworkVar> _cSyncObserver)
    {
        UpdateValueType();


        m_cSyncObserver = _cSyncObserver;
    }


    public CNetworkVar(IObserver<INetworkVar> _cSyncObserver, TYPE _Right)
    {
        UpdateValueType();


		m_cSyncObserver = _cSyncObserver;
        m_Value = _Right;
    }


	public void SyncSerialized(byte[] _baValueSerialized)
	{
        if (m_eType != ENetworkVarType.String)
        {
            int iValueSize = GetSize();


            // Allocate raw buffer
            IntPtr ipRawBuffer = Marshal.AllocHGlobal(iValueSize);


            // Copy byte array into raw buffer
            Marshal.Copy(_baValueSerialized, 0, ipRawBuffer, iValueSize);


            // Copy value to raw buffer
            m_Value = (TYPE)Marshal.PtrToStructure(ipRawBuffer, typeof(TYPE));


            // Free raw buffer
            Marshal.FreeHGlobal(ipRawBuffer);
        }
        else
        {
            char[] chars = new char[_baValueSerialized.Length / sizeof(char)];
            System.Buffer.BlockCopy(_baValueSerialized, 0, chars, 0, _baValueSerialized.Length);


            m_Value = (TYPE)Convert.ChangeType(new string(chars), typeof(TYPE));
        }


        // Notify observers
        m_cSyncObserver.Notify(this, (short)ESubject.OnSync, null);


        Debug.LogError(string.Format("Network var was synced with value ({0})", m_Value));
	}


    public void Set(object _cValue)
    {
        if (!CGame.IsServer())
        {
			Debug.LogError("Clients are not allowed to set network variables!");
        }
        else
        {
            m_Value = (TYPE)Convert.ChangeType(_cValue, typeof(TYPE));
			m_cNetworkView.OnNetworkVarChange(m_bNetworkVarId);
        }
    }


	public void SetNetworkViewOwner(CNetworkView _cNetworkView, byte _bNetworkVarId)
	{
		m_cNetworkView = _cNetworkView;
		m_bNetworkVarId = _bNetworkVarId;
	}


    public TYPE Get()
    {
        return (m_Value);
    }


	public int GetSize()
	{
        return (Converter.GetSizeOf(typeof(TYPE)));
	}


    public ENetworkVarType GetVarType()
    {
        return (m_eType);
    }


	public byte[] GetSerialized()
	{
		int iValueSize = GetSize();
        byte[] baSerializedValue = null;


        if (m_eType != ENetworkVarType.String)
        {
            baSerializedValue = new byte[iValueSize];


            // Allocate raw buffer
            IntPtr ipRawBuffer = Marshal.AllocHGlobal(iValueSize);


            // Copy value to raw buffer
            Marshal.StructureToPtr(m_Value, ipRawBuffer, true);


            // Copy raw buffer contents to byte array
            Marshal.Copy(ipRawBuffer, baSerializedValue, 0, iValueSize);


            // Free raw buffer
            Marshal.FreeHGlobal(ipRawBuffer);
        }
        else
        {
            string sValue = m_Value as string;


            baSerializedValue = new byte[sValue.Length * sizeof(char)];
            System.Buffer.BlockCopy(sValue.ToCharArray(), 0, baSerializedValue, 0, baSerializedValue.Length);
        }


		return (baSerializedValue);
	}


    // protected:


    // private:


    void UpdateValueType()
    {
        // Value type
        if ((typeof(TYPE).IsValueType ||
             typeof(TYPE).IsEnum) && 
             typeof(TYPE).IsPrimitive)
        {
            switch ((uint)Marshal.SizeOf(typeof(TYPE)))
            {
            case 1:
                m_eType = ENetworkVarType.Value_1_Byte;
                break;

            case 2:
                m_eType = ENetworkVarType.Value_2_Bytes;
                break;

            case 4:
                m_eType = ENetworkVarType.Value_4_Bytes;
                break;

            default:
                Debug.LogError(string.Format("Could not validate the actual size of Network Var value type ({0})", typeof(TYPE)));
                break;
            }
        }

        // Struct type
        else if ( typeof(TYPE).IsValueType &&
                 !typeof(TYPE).IsEnum &&
                 !typeof(TYPE).IsPrimitive)
        {
            m_eType = ENetworkVarType.Struct;
        }

        // String
        else if (typeof(TYPE) == typeof(string))
        {
            m_eType = ENetworkVarType.String;
        }

        else
        {
            Debug.LogError(string.Format("Network var type ({0}) is invalid. Type must be a value type or string.", typeof(TYPE).Name));
        }



        //Debug.LogError(m_eType);
    }


// Member Variables

    // protected:


    // private:


    TYPE m_Value;
	IObserver<INetworkVar> m_cSyncObserver = null;
	CNetworkView m_cNetworkView = null;
    ENetworkVarType m_eType = ENetworkVarType.Invalid;
	byte m_bNetworkVarId = 0;


    static uint s_uiIdCount = 0;


};
