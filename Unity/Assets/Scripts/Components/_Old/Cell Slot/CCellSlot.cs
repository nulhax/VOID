
//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CCellSlot.cs
//  Description :   --------------------------
//
//  Author  	:  Scott Emery	
//  Mail    	:  scott.ipod@gmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/* Implementation */


public class CCellSlot : CNetworkMonoBehaviour
{

// Member Types

	public CPartInterface.EType m_CellSlotType;

// Member Delegates & Events


// Member Properties
	
	
	
// Member Functions
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_bIsFunctionalityAllowed = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, true);
		m_bIsCellBroken = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, false);
		m_bIsCellMatchingSlot = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, true);
		m_cCurrentCell = _cRegistrar.CreateNetworkVar<CNetworkViewId>(OnNetworkVarSync, null);
	}

// Member Methods
	public void Awake()
	{
		
	}
	
	
	public void Start()
	{
		// Empty
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
		string CellName = m_CellSlotType.ToString();
		
		if(gameObject.name == CellName)
		{
			
		}
		// Check the slot type
		// Check it's the right cell
		// Check it's not broken
		// Attach cell to replicator in correct orientation 
		// If it's broken, stop the production of resource
	}

	void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
		if(_cVarInstance == m_cCurrentCell)
		{
			if(m_cCurrentCell.Get() != null)
			{
				GameObject InsertedCell = CNetwork.Factory.FindObject(m_cCurrentCell.Get());
				InsertedCell.transform.position = transform.position + transform.up * 1.0f;
				InsertedCell.transform.rotation = transform.rotation;
				
				InsertedCell.GetComponent<CPartBehaviour>().EventModuleBroken += new CPartBehaviour.NotifyBroken(CellBroke);
				CellInserted();
				
				// Turn off  dynamic physics
				if(CNetwork.IsServer)
				{
                	InsertedCell.rigidbody.isKinematic = true;
				}
			}
		}
	}
	
	public CNetworkViewId Insert (CNetworkViewId _CellNetworkID)
	{
		CNetworkViewId cCurrentCell = null;
		if(CNetwork.IsServer)
		{
			cCurrentCell = m_cCurrentCell.Get();
			m_cCurrentCell.Set(_CellNetworkID);
		}
		return(cCurrentCell);
	}
	
	private void CellInserted()
	{
		m_bIsFunctionalityAllowed.Set(true);
	}
	
	private void CellBroke()
	{
		m_bIsFunctionalityAllowed.Set (false);
	}
	
	public CNetworkViewId GetCell()
	{
		return(m_cCurrentCell.Get());
	}
	
// Member Fields

	CNetworkVar<bool> m_bIsFunctionalityAllowed;
	CNetworkVar<bool> m_bIsCellBroken;
	CNetworkVar<bool> m_bIsCellMatchingSlot;
	CNetworkVar<CNetworkViewId> m_cCurrentCell;
};
