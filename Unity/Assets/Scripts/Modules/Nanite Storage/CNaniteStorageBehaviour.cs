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

	public int m_MaximumCapacity = 250;
	
	// Member Properties
	public int StoredNanites
	{ 
		get { return (m_iStoredNanites.Get()); }
		
		[AServerOnly]
		set { m_iStoredNanites.Set(value); }
	}

	public int NaniteCapacity
	{ 
		get { return (m_iNaniteCapacity.Get()); }
		
		[AServerOnly]
		set
		{ 
			m_iNaniteCapacity.Set(value); 

			if(value < StoredNanites)
				StoredNanites = value;
		}
	}
	
	public int AvailableNaniteCapacity
	{ 
		get { return (m_iNaniteCapacity.Get() - m_iStoredNanites.Get()); }
		
		[AServerOnly]
		set { m_iNaniteCapacity.Set(value); }
	}
	
	public bool IsStorageAvailable
	{
		get { return (m_bNanitesAvailable.Get()); }
	}
	
	// Member Functions
	
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_iStoredNanites = _cRegistrar.CreateNetworkVar<int>(OnNetworkVarSync, 0);
		m_iNaniteCapacity = _cRegistrar.CreateNetworkVar<int>(OnNetworkVarSync, m_MaximumCapacity);
		m_bNanitesAvailable = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, false);
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
