//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CNaniteStorageBehaviour.cs
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


[RequireComponent(typeof(CModuleInterface))]
public class CNaniteStorageBehaviour : CNetworkMonoBehaviour 
{
	
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	CNetworkVar<int> m_iStoredNanites = null;
	CNetworkVar<int> m_iNaniteCapacity = null;
	CNetworkVar<bool> m_bNanitesAvailable = null;
	
	
	// Member Properties
	public int StoredNanites
	{ 
		get { return (m_iStoredNanites.Get()); }
		
		[AServerOnly]
		set { m_iStoredNanites.Set(value); }
	}

	public int MaxNaniteCapacity
	{ 
		get { return (m_iNaniteCapacity.Get()); }
		
		[AServerOnly]
		set { m_iNaniteCapacity.Set(value); }
	}
	
	public int AvailableNaniteCapacity
	{ 
		get { return (m_iNaniteCapacity.Get() - m_iStoredNanites.Get()); }
		
		[AServerOnly]
		set { m_iNaniteCapacity.Set(value); }
	}
	
	public bool HasAvailableNanites
	{
		get { return (m_bNanitesAvailable.Get() && m_iStoredNanites.Get() != 0.0f); }
	}
	
	// Member Functions
	
	public override void InstanceNetworkVars()
	{
		m_iStoredNanites = new CNetworkVar<int>(OnNetworkVarSync, 0);
		m_iNaniteCapacity = new CNetworkVar<int>(OnNetworkVarSync, 5000);
		m_bNanitesAvailable = new CNetworkVar<bool>(OnNetworkVarSync, false);
	}
	
	void OnNetworkVarSync(INetworkVar _VarInstance)
	{

	}
	
	public void Start()
	{
		CGameShips.Ship.GetComponent<CShipNaniteSystem>().RegisterNaniteSilo(gameObject);
		
		if(CNetwork.IsServer)
		{
			ActivateNaniteAvailability();
		}
	}
	
	[AServerOnly]
	public void ActivateNaniteAvailability()
	{
		m_bNanitesAvailable.Set(true);
	}
	
	[AServerOnly]
	public void Deactivate()
	{
		m_bNanitesAvailable.Set(false);
	}

	[AServerOnly]
	public void DeductNanites(int _iNanites)
	{
		StoredNanites = StoredNanites - _iNanites;
	}
}
