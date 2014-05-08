//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   TNetworkViewId.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class TNetworkViewId
{

// Member Types


	public const int k_iSerializedSize = 3;


// Member Delegates & Events


// Member Properties


	public ushort Id
	{
		set
		{
			m_usViewId = value;
		}

		get
		{ 
			//Logger.WriteErrorOn(m_usViewId == 0, "View id has not be registered for a view id");

			return (m_usViewId); 
		}
	}


	public byte ChildId
	{
		get { return (m_bChildViewId);}
	}


	public bool IsChildViewId
	{
		get { return (m_bChildViewId != 0); }
	}


	public GameObject GameObject
	{
		get 
		{ 
			return (CNetwork.Factory.FindGameObject(this)); 
		}
	}


// Member Methods


	public TNetworkViewId()
	{
	}


	public TNetworkViewId(ushort _usViewId, byte _bSubViewId)
	{
		m_usViewId = _usViewId;
		m_bChildViewId = _bSubViewId;
	}


	public override bool Equals(object other)
	{
		var cRight = other as TNetworkViewId;
		
		if (cRight == null)
		{
			return (false);
		}
		
		return (m_usViewId == cRight.m_usViewId &&
		        m_bChildViewId == cRight.m_bChildViewId);
	}
	
	public override int GetHashCode()
	{
		return ((int)(Mathf.Pow(m_usViewId, 2) + Mathf.Pow(m_bChildViewId, 3)));
	}


// Member Fields


	ushort m_usViewId = 0;


	byte m_bChildViewId = 0;


};
