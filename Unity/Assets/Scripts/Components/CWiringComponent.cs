//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CWiringComponent.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  scott.ipod@gmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */

[RequireComponent(typeof(CComponentInterface))]
public class CWiringComponent : CNetworkMonoBehaviour
{
	
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Properties
	public List<Transform> RatchetRepairPosition
	{
		get { return(m_RepairPositions);}
	}
	
	// Member Methods

	void OnBreak()
	{
		// TODO: swap between fixed to broken
	}

	void OnFix()
	{
		//TODO swap between broken to fixed
	}
	
	void Start()
	{
		// Find all the children which are component transforms
		foreach(Transform child in transform)
		{
			if(child.tag == "ComponentTransform")
				m_RepairPositions.Add(child);
		}

		// Register to event
		gameObject.GetComponent<CComponentInterface>().EventComponentBreak += OnBreak;
		gameObject.GetComponent<CComponentInterface>().EventComponentFix += OnFix;
	}
	
	
	void OnDestroy()
	{
	}
	
	
	void Update()
	{
	}

	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		
	}
	
	public override void InstanceNetworkVars (CNetworkViewRegistrar _cRegistrar)
	{
		
	}
	
	// Member Fields
	private List<Transform> m_RepairPositions = new List<Transform>();
	
};
