//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   TransmissionStream.h
//  Description :   --------------------------
//
//  Author  	:  Programming Team
//  Mail    	:  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using System.Reflection;


/* Implementation */


public class CNetworkStream
{

// Member Types


// Member Functions

	// public:


    public CNetworkStream()
    {
        m_cBitStream = new RakNet.BitStream();
    }


	public CNetworkStream(byte[] _baData)
	{
		m_cBitStream = new RakNet.BitStream(_baData, (uint)_baData.Length, false);
	}


	public CNetworkStream(RakNet.BitStream _cBitStream)
	{
		m_cBitStream = _cBitStream;
	}


	public void Clear()
	{
		m_cBitStream.Reset();
		m_cBitStream.SetWriteOffset(0);
		m_cBitStream.SetReadOffset(0);
	}


    public void SetReadOffset(uint _uiBytes)
    {
        m_cBitStream.SetReadOffset(_uiBytes * 8);
    }


    public void Write(CNetworkStream _cStream)
    {
		_cStream.SetReadOffset(0);
        m_cBitStream.Write(_cStream.BitStream);
        _cStream.SetReadOffset(0);
    }


	public void Write(CNetworkViewId _cNetworkViewId)
	{
		if (_cNetworkViewId == null)
		{
			this.Write(ushort.MaxValue);
			this.Write(byte.MaxValue);
		}
		else
		{
			this.Write(_cNetworkViewId.Id);
			this.Write(_cNetworkViewId.ChildId);
		}
	}


    public void Write(object _cObject, Type _cType)
    {
		if (_cType == typeof(CNetworkViewId))
		{
			if ((CNetworkViewId)_cObject == null)
			{
				this.Write((ushort)ushort.MaxValue);
				this.Write((byte)byte.MaxValue);
			}
			else
			{
				this.Write(((CNetworkViewId)_cObject).Id);
				this.Write(((CNetworkViewId)_cObject).ChildId);
			}
		}
		else
		{
	        // Serialize the parameter value
	        byte[] baValueSerialized = Converter.ToByteArray(_cObject, _cType);

	        // Write string length if type is string
	        if (_cType == typeof(string))
	        {
	            this.Write((byte)((string)_cObject).Length);
	        }

	        // Write parameter value
	        this.Write(baValueSerialized);
		}
    }


    public void Write(MethodInfo _tMethodInfo, object[] _caParameterValues)
    {
        // Extract the parameters from the method
        ParameterInfo[] caParameters = _tMethodInfo.GetParameters();
		
        for (int i = 0; i < caParameters.Length; ++i)
        {
            Write(_caParameterValues[i], caParameters[i].ParameterType);
        }
    }


	public void WriteString(string _sString)
	{
		Write(_sString, typeof(string));
	}


	public void Write(byte[] _baData)
	{
		m_cBitStream.Write(_baData, (uint)_baData.Length);
	}


	public void Write(byte[] _baData, uint _uiLength)
	{
		m_cBitStream.Write(_baData, _uiLength);
	}


	public void Write(byte[] _baData, int _iLength)
	{
		Write(_baData, (uint)_iLength);
	}


	public void Write(byte _bValue)
	{
		m_cBitStream.Write(_bValue);
	}


	public void Write(short _sValue)
	{
		m_cBitStream.Write(_sValue);
	}


	public void Write(ushort _usValue)
	{
		m_cBitStream.Write(_usValue);
	}


	public void Write(int _iValue)
	{
		m_cBitStream.Write(_iValue);
	}


	public void Write(uint _uiValue)
	{
		m_cBitStream.Write((int)_uiValue);
	}


	public void Write(long _lValue)
	{
		m_cBitStream.Write(_lValue);
	}


	public void Write(ulong _ulValue)
	{
		m_cBitStream.Write((long)_ulValue);
	}
	
	
	public void Write(float _fValue)
	{
		m_cBitStream.Write(_fValue);
	}


	public void IgnoreBytes(int _iNumBytes)
	{
		IgnoreBytes((uint)_iNumBytes);
	}


	public void IgnoreBytes(uint _uiNumBytes)
	{
		m_cBitStream.IgnoreBytes(_uiNumBytes);
	}


	public CNetworkViewId ReadNetworkViewId()
	{
		ushort usViewId = ReadUShort();
		byte bSubViewId = ReadByte();
		
		if (usViewId == ushort.MaxValue &&
		    bSubViewId == byte.MaxValue)
		{
			return (null);
		}
		else
		{
			return (new CNetworkViewId(usViewId, bSubViewId));
		}
	}


    public object[] ReadMethodParameters(MethodInfo _tMethodInfo)
    {
        // Extract the parameters from the method
        ParameterInfo[] caParameters = _tMethodInfo.GetParameters();


        object[] caParameterValues = new object[caParameters.Length];


        for (int i = 0; i < caParameters.Length; ++i)
        {
			if (caParameters[i].ParameterType == typeof(CNetworkViewId))
			{
				caParameterValues[i] = ReadType(typeof(CNetworkViewId));
			}
			else
			{
	            int iSize = Converter.GetSizeOf(caParameters[i].ParameterType);

	            // Read string length if type is string
	            if (caParameters[i].ParameterType == typeof(string))
	            {
	                iSize = this.ReadByte();
	            }
				else if (caParameters[i].ParameterType == typeof(CNetworkViewId))
				{
					iSize = CNetworkViewId.k_iSerializedSize;
				}

	            byte[] baSerializedValue = this.ReadBytes(iSize);

	            caParameterValues[i] = Converter.ToObject(baSerializedValue, caParameters[i].ParameterType);
			}
        }


        return (caParameterValues);
    }


    public object ReadType(Type _cType)
    {
		if (_cType == typeof(CNetworkViewId))
		{
			return (ReadNetworkViewId());
		}
		else
		{
	        int iSize = Converter.GetSizeOf(_cType);

	        if (_cType == typeof(string))
	        {
	            iSize = ReadByte();
	        }

			// Convert serialized data to object
			return (Converter.ToObject(ReadBytes(iSize), _cType));
		}
    }


	public byte[] ReadBytes(int _iSize)
	{
		return (ReadBytes((uint)_iSize));
	}


	public byte[] ReadBytes(uint _uiSize)
	{
		byte[] baBytes = new byte[_uiSize];


		if (!m_cBitStream.Read(baBytes, _uiSize))
		{
			Logger.WriteError("Could not read bytes");
		}


		return (baBytes);
	}


	public byte ReadByte()
	{
		byte bByte = 0;


		if (!m_cBitStream.Read(out bByte))
		{
			Logger.WriteError("Could not read byte");
		}


		return (bByte);
	}


	public short ReadShort()
	{
		short sValue = 0;


		if (!m_cBitStream.Read(out sValue))
		{
			Logger.WriteError("Could not read short");
		}


		return (sValue);
	}


	public ushort ReadUShort()
	{
		return ((ushort)ReadShort());
	}


	public int ReadInt()
	{
		int iValue = 0;


		if (!m_cBitStream.Read(out iValue))
		{
			Logger.WriteError("Could not read int");
		}

		
		return (iValue);
	}


	public uint ReadUInt()
	{
		return ((uint)ReadInt());
	}


	public long ReadLong()
	{
		long lValue = 0;


		if (!m_cBitStream.Read(out lValue))
		{
			Logger.WriteError("Could not read int");
		}


		return (lValue);
	}


	public ulong ReadULong()
	{
		return ((ulong)ReadLong());
	}
	
	
	public float ReadFloat()
	{
		float fValue = 0;


		if (!m_cBitStream.Read(out fValue))
		{
			Logger.WriteError("Could not read float");
		}

		
		return (fValue);
	}


	public string ReadString()
	{
		int	iSize = this.ReadByte();
		
		byte[] baSerializedValue = this.ReadBytes(iSize);
		
		return ((string)Converter.ToObject(baSerializedValue, typeof(string)));
	}


	public RakNet.BitStream BitStream
	{
		get { return (m_cBitStream); }
	}


	public uint Size
	{
		get { return (BitStream.GetNumberOfBytesUsed()); }
	}


	public uint NumReadByes
	{
		get { return (m_cBitStream.GetReadOffset() / 8); }
	}


    public uint NumUnreadBytes
    {
        get { return (Size - NumReadByes); }
    }


	public bool HasUnreadData
	{
		get { return (this.NumReadByes < this.Size); }
	}


	// protected:


	// private:



// Member Variables

	// protected:


	// private:


	RakNet.BitStream m_cBitStream;


};
