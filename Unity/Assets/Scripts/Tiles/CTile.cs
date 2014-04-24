//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CTile.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;


/* Implementation */


public abstract class CTile : MonoBehaviour 
{
	// Member Types
	public enum EType
	{
		INVALID = -1,
		
		Floor,
		Wall_Ext,
		Wall_Int,
		Ceiling,
		Wall_Ext_Cap,
		Wall_Int_Cap,
		
		MAX
	}

	[System.Serializable]
	public struct TMeta
	{
		public static TMeta Default
		{
			get { return(new TTileMeta(0, -1)); }
		}

		public TMeta(int _IdentifierMask, int _MetaType)
		{
			m_IdentifierMask = _IdentifierMask;
			m_NeighbourExemptionMask = 0;
			m_MetaType = _MetaType;
			m_Rotations = 0;
			m_Variant = 0;
		}

		public int m_IdentifierMask;
		public int m_NeighbourExemptionMask;
		public int m_MetaType;
		public int m_Rotations;
		public int m_Variant;
	}


	// Member Delegates & Events
	
	
	// Member Fields
	public EType m_TileType = EType.INVALID;

	public CTile.TMeta m_ActiveTileMeta = TTileMeta.Default;
	protected CTile.TMeta m_CurrentTileMeta = TTileMeta.Default;

	protected CTileRoot m_TileRoot = null;

	protected List<EDirection> m_NeighbourExemptions = new List<EDirection>();
	protected bool m_IsDirty = false;


	// Member Properties
	public CTileRoot TileRoot
	{
		set { m_TileRoot = value; }
	}


	// Member Methods
	protected static CTile.TMeta CreateMetaEntry(int _MetaType, EDirection[] _MaskNeighbours)
	{
		// Define the mask neighbours into a int mask
		int mask = 0;
		foreach(EDirection direction in _MaskNeighbours)
		{
			mask |= 1 << (int)direction;
		}

		return(new CTile.TMeta(mask, (int)_MetaType));
	}

	public abstract void UpdateCurrentMeta();
}