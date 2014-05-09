//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShipWarningSystem.cs
//  Description :   Atmosphere information for rooms
//
//  Author  	:  
//  Mail    	:  
//

// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public enum EWarningType : int
{
	INVALID,
	
	Atmosphere,
	Power,
	Engine,
	ProximityObject,
	
	MAX
}

public enum EWarningSeverity : int
{
	INVALID,
	
	Minor,
	Major,
	Critical,
	
	MAX
}

public struct TWarningInstance
{
	public TWarningInstance(EWarningType _WarningType, EWarningSeverity _WarningSeverity)
	{
		m_WarningType = _WarningType;
		m_WarningSeverity = _WarningSeverity;
	}
	
	public EWarningType m_WarningType;
	public EWarningSeverity m_WarningSeverity;
}


public class CShipWarningSystem : CNetworkMonoBehaviour
{
	// Member Types

	
	// Member Delegates & Events
	
	
	// Member Fields
	
	
	// Member Properties
	
	
	// Member Methods
	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
		
	}
	
	private void Start()
	{
		if(CNetwork.IsServer)
		{
			
		}
	}
	
	
	private void Update()
	{
		if(CNetwork.IsServer)
		{
		}
	}
	
	void OnNetworkVarSync(INetworkVar _cSynedVar)
	{
		// Empty
	}
};
