//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRatchetComponent.cs
//  Description :   --------------------------
//
//  Author  	:  Scott Emery
//  Mail    	:  scott.ipod@gmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */

[RequireComponent(typeof(CComponentInterface))]
public class CRatchetComponent : CNetworkMonoBehaviour
{

// Member Types
	
	
// Member Delegates & Events

	
// Member Properties


	public List<Transform> RatchetRepairPosition
	{
		get { return(m_RepairPositions);}
	}

	
// Member Methods


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        // Empty
    }


	void Start()
	{
		// Find all the children which are component transforms
		foreach(Transform child in transform)
		{
			if (child.tag == "ComponentTransform")
				m_RepairPositions.Add(child);
		}
	}
	

	void OnDestroy()
	{
        // Emptys
	}
	
	
	void Update()
	{
        // Empty
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
        // Empty
	}
	

// Member Fields


	List<Transform> m_RepairPositions = new List<Transform>();

	bool m_IsLerping = false;


};