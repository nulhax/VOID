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
using System.Collections;


/* Implementation */


public class CPacketStream
{

// Member Types


// Member Functions

	// public:


    public CPacketStream()
    {
        m_cBitStream = new RakNet.BitStream();
    }


	public CPacketStream(RakNet.BitStream _cBitStream)
	{
		m_cBitStream = _cBitStream;
	}


	public CPacketStream(byte[] _baData)
	{
		m_cBitStream = new RakNet.BitStream(_baData, (uint)_baData.Length, true);
	}


	public void Clear()
	{
		m_cBitStream.Reset();
	}


    public void Write(CPacketStream _cStream)
    {
        m_cBitStream.Write(_cStream.GetBitStream());
    }


	public void Write(byte[] _baData)
	{
		m_cBitStream.Write(_baData, (uint)_baData.Length);
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


	public void IgnoreBytes(int _iNumBytes)
	{
		IgnoreBytes((uint)_iNumBytes);
	}


	public void IgnoreBytes(uint _uiNumBytes)
	{
		m_cBitStream.IgnoreBytes(_uiNumBytes);
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
			Debug.LogError("Could not read bytes");
		}


		return (baBytes);
	}


	public byte ReadByte()
	{
		byte bByte = 0;


		if (!m_cBitStream.Read(out bByte))
		{
			Debug.LogError("Could not read byte");
		}


		return (bByte);
	}


	public short ReadShort()
	{
		short sValue = 0;


		if (!m_cBitStream.Read(out sValue))
		{
			Debug.LogError("Could not read short");
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
			Debug.LogError("Could not read int");
		}

		
		return (iValue);
	}


	public uint ReadUInt()
	{
		return ((uint)ReadInt());
	}


	public uint GetNumReadByes()
	{
		return (m_cBitStream.GetReadOffset() / 8);
	}


	public uint GetSize()
	{
		return (m_cBitStream.GetNumberOfBytesUsed());
	}


	public RakNet.BitStream GetBitStream()
	{
		return (m_cBitStream);
	}


	public bool HasUnreadData()
	{
        return (GetNumReadByes() < GetSize());
	}


	// protected:


	// private:



// Member Variables

	// protected:


	// private:


	RakNet.BitStream m_cBitStream;


};
