
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

	public CModuleInterface.EType m_CellSlotType;

// Member Delegates & Events


// Member Properties
	
	
	
// Member Functions
	public override void InstanceNetworkVars()
	{
		m_bIsFunctionalityAllowed = new CNetworkVar<bool>(OnNetworkVarSync, true);
		m_bIsCellBroken = new CNetworkVar<bool>(OnNetworkVarSync, false);
		m_bIsCellMatchingSlot = new CNetworkVar<bool>(OnNetworkVarSync, true);
		m_usCurrentCell = new CNetworkVar<ushort>(OnNetworkVarSync, 0);
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
		if(_cVarInstance == m_usCurrentCell)
		{
			if(m_usCurrentCell.Get() != 0)
			{
				GameObject InsertedCell = CNetwork.Factory.FindObject(m_usCurrentCell.Get());
				InsertedCell.transform.position = transform.position + transform.up * 1.0f;
				InsertedCell.transform.rotation = transform.rotation;
				
				InsertedCell.GetComponent<CModuleFunctional>().EventModuleBroken += new CModuleFunctional.NotifyBroken(CellBroke);
				CellInserted();
				
				// Turn off  dynamic physics
				if(CNetwork.IsServer)
				{
                	InsertedCell.rigidbody.isKinematic = true;
				}

				// Disable dynamic actor
				InsertedCell.GetComponent<CDynamicActor>().enabled = false;
			}
		}
	}
	
	public ushort Insert (ushort _CellNetworkID)
	{
		ushort usCurrentCell = 0;
		if(CNetwork.IsServer)
		{
			usCurrentCell = m_usCurrentCell.Get();
			m_usCurrentCell.Set(_CellNetworkID);
		}
		return(usCurrentCell);
	}
	
	private void CellInserted()
	{
		m_bIsFunctionalityAllowed.Set(true);
	}
	
	private void CellBroke()
	{
		m_bIsFunctionalityAllowed.Set (false);
	}
	
	public ushort GetCell()
	{
		return(m_usCurrentCell.Get());
	}
	
// Member Fields

	CNetworkVar<bool> m_bIsFunctionalityAllowed;
	CNetworkVar<bool> m_bIsCellBroken;
	CNetworkVar<bool> m_bIsCellMatchingSlot;
	CNetworkVar<ushort> m_usCurrentCell;
};
