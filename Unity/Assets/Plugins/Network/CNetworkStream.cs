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


    public void WriteBits(byte[] _baByteArray, uint _uiNumBits)
    {
        m_cBitStream.WriteBits(_baByteArray, _uiNumBits);
    }


    public void WriteBits(object _cObject, uint _uiNumBits)
    {
        m_cBitStream.WriteBits(Converter.ToByteArray(_cObject, _cObject.GetType()), _uiNumBits); 
    }


    public void Write(CNetworkStream _cStream)
    {
		_cStream.SetReadOffset(0);
        m_cBitStream.Write(_cStream.BitStream);
        _cStream.SetReadOffset(0);
    }


	public void Write(TNetworkViewId _cNetworkViewId)
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


    public void Write(object _cObject)
    {
        Write(_cObject, _cObject.GetType());
    }


    public void Write(object _cObject, Type _cType)
    {
		if (_cType == typeof(TNetworkViewId))
		{
			if ((TNetworkViewId)_cObject == null)
			{
				this.Write((ushort)ushort.MaxValue);
				this.Write((byte)byte.MaxValue);
			}
			else
			{
				this.Write(((TNetworkViewId)_cObject).Id);
				this.Write(((TNetworkViewId)_cObject).ChildId);
			}
		}
		else
		{
            ABitSize[] aBitSize = _cType.GetCustomAttributes(typeof(ABitSize), false) as ABitSize[];

            if (aBitSize.Length > 0)
            {
                this.WriteBits(_cObject, aBitSize[0].BitCount);
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


	public void IgnoreBytes(int _iNumBytes)
	{
		IgnoreBytes((uint)_iNumBytes);
	}


	public void IgnoreBytes(uint _uiNumBytes)
	{
		m_cBitStream.IgnoreBytes(_uiNumBytes);
	}


    public void IgnoreBits(int _iNumBits)
    {
        IgnoreBits(_iNumBits);
    }


    public void IgnoreBits(uint _uiNumBits)
    {
        m_cBitStream.IgnoreBits(_uiNumBits);
    }


    public object[] ReadMethodParameters(MethodInfo _tMethodInfo)
    {
        // Extract the parameters from the method
        ParameterInfo[] caParameters = _tMethodInfo.GetParameters();

        object[] caParameterValues = new object[caParameters.Length];

        for (int i = 0; i < caParameters.Length; ++i)
        {
            caParameterValues[i] = ReadType(caParameters[i].ParameterType);
        }

        return (caParameterValues);
    }


    public object ReadType(Type _cType)
    {
		if (_cType == typeof(TNetworkViewId))
		{
            ushort usViewId = Read<ushort>();

            byte bSubViewId = Read<byte>();

            if (usViewId == ushort.MaxValue &&
                bSubViewId == byte.MaxValue)
            {
                return (null);
            }
            else
            {
                return (new TNetworkViewId(usViewId, bSubViewId));
            }
		}
		else
		{
            ABitSize[] aBitSize = _cType.GetCustomAttributes(typeof(ABitSize), false) as ABitSize[];

            if (aBitSize.Length > 0)
            {
                // Convert serialized data to object
                return (ReadBits(_cType, aBitSize[0].BitCount));
            }
            else
            {
                int iSize = Converter.GetSizeOf(_cType);

                if (_cType == typeof(string))
                {
                    iSize = Read<byte>();
                }

                // Convert serialized data to object
                return (Converter.ToObject(ReadBytes(iSize), _cType));
            }
		}
    }


    public TYPE Read<TYPE>()
    {
        return ((TYPE)ReadType(typeof(TYPE)));
    }


    public object ReadBits(Type _cType, uint _uiNumBits)
    {
        byte[] baData = new byte[Converter.GetSizeOf(_cType)];

        m_cBitStream.ReadBits(baData, _uiNumBits);

        return (Converter.ToObject(baData, _cType));
    }


    public TYPE ReadBits<TYPE>(uint _uiNumBits)
    {
        return ((TYPE)ReadBits(typeof(TYPE), _uiNumBits));
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


    public byte[] ReadBytes(int _iSize)
    {
        return (ReadBytes((uint)_iSize));
    }


	public RakNet.BitStream BitStream
	{
		get { return (m_cBitStream); }
	}


	public uint ByteSize
	{
		get { return (BitStream.GetNumberOfBytesUsed()); }
	}


    public uint BitSize
    {
        get { return (BitStream.GetNumberOfBitsUsed()); }
    }


	public uint NumReadByes
	{
		get { return (m_cBitStream.GetReadOffset() / 8); }
	}


    public uint NumReadBits
    {
        get { return (m_cBitStream.GetReadOffset()); }
    }


    public uint NumUnreadBytes
    {
        get { return (ByteSize - NumReadByes); }
    }


    public uint NumUnreadBits
    {
        get { return (this.m_cBitStream.GetNumberOfUnreadBits()); }
    }


	public bool HasUnreadData
	{
		get { return (this.m_cBitStream.GetNumberOfUnreadBits() > 0); }
	}


	// protected:


	// private:



// Member Variables

	// protected:


	// private:


	RakNet.BitStream m_cBitStream;


};
