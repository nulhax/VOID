//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CNetworkViewId.cs
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


public class CNetworkViewId
{

// Member Types


	public const int k_iSerializedSize = 3;


// Member Delegates & Events


// Member Properties


	public ushort Id
	{
		get
		{ 
			Logger.WriteErrorOn(m_usViewId == 0, "View id has not be registered for a view id");

			return (m_usViewId); 
		}
	}


	public byte SubId
	{
		get 
		{
			//Logger.WriteErrorOn(m_bSubViewId == 0, "View id has not be registered for a sub view id");

			return (m_bSubViewId);
		}
	}


	public bool IsSubViewId
	{
		get { return (m_bSubViewId != 0); }
	}


// Member Methods


	public CNetworkViewId()
	{
	}


	public CNetworkViewId(ushort _usViewId, byte _bSubViewId)
	{
		m_usViewId = _usViewId;
		m_bSubViewId = _bSubViewId;
	}


	public override bool Equals(object other)
	{
		var cRight = other as CNetworkViewId;
		
		if (cRight == null)
		{
			return (false);
		}
		
		return (m_usViewId == cRight.m_usViewId &&
		        m_bSubViewId == cRight.m_bSubViewId);
	}
	
	public override int GetHashCode()
	{
		return ((int)(Mathf.Pow(m_usViewId, 2) + Mathf.Pow(m_bSubViewId, 3)));
	}


// Member Fields


	List<CNetworkViewId> m_aSubViewIds = null;


	ushort m_usViewId = 0;


	byte m_bSubViewId = 0;


};
