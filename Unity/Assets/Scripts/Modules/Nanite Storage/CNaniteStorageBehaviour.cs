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
	public delegate void NotifyStateChange(CNaniteStorageBehaviour _Self);
	
	public event NotifyStateChange EventNaniteStorageChanged;
	public event NotifyStateChange EventNaniteCapacityChanged;


	// Member Fields
	private CNetworkVar<float> m_fStoredNanites = null;
	private CNetworkVar<float> m_fNaniteCapacity = null;
	private CNetworkVar<bool> m_bNanitesAvailable = null;


	// Member Properties
	public float StoredNanites
	{ 
		get { return (m_fStoredNanites.Get()); }
		
		[AServerOnly]
		set { m_fStoredNanites.Set(value); }
	}

	public float NaniteCapacity
	{ 
		get { return (m_fNaniteCapacity.Get()); }
		
		[AServerOnly]
		set
		{ 
			m_fNaniteCapacity.Set(value); 

			if(value < StoredNanites)
				StoredNanites = value;
		}
	}
	
	public float AvailableNaniteCapacity
	{ 
		get { return (m_fNaniteCapacity.Get() - m_fStoredNanites.Get()); }
		
		[AServerOnly]
		set { m_fNaniteCapacity.Set(value); }
	}
	
	public bool IsStorageAvailable
	{
		get { return (m_bNanitesAvailable.Get()); }
	}
	
	// Member Functions
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_fStoredNanites = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0);
        m_fNaniteCapacity = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0);
		m_bNanitesAvailable = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, false);
	}
	
	void OnNetworkVarSync(INetworkVar _VarInstance)
	{
		if(_VarInstance == m_fStoredNanites)
		{
			if(EventNaniteStorageChanged != null)
				EventNaniteStorageChanged(this);
		}
		else if(_VarInstance == m_fNaniteCapacity)
		{
			if(EventNaniteCapacityChanged != null)
				EventNaniteCapacityChanged(this);
		}
	}
	
	public void Start()
	{
		CGameShips.Ship.GetComponent<CShipNaniteSystem>().RegisterNaniteSilo(gameObject);
		
		if(CNetwork.IsServer)
		{
			SetNaniteAvailability(true);

			// Register power activation/deactivation events
			//gameObject.GetComponent<CModuleInterface>().ParentFacility.GetComponent<CFacilityPower>().EventFacilityPowerActivated += OnPowerActivated;
			//gameObject.GetComponent<CModuleInterface>().ParentFacility.GetComponent<CFacilityPower>().EventFacilityPowerDeactivated += OnPowerDeactivated;
		}
	}
	
	[AServerOnly]
	public void SetNaniteAvailability(bool _State)
	{
		m_bNanitesAvailable.Set(_State);
	}

	[AServerOnly]
    public void DeductNanites(float _fNanites)
	{
        StoredNanites = StoredNanites - _fNanites;
	}

	[AServerOnly]
	public void OnPowerDeactivated()
	{
		SetNaniteAvailability(false);
	}

	[AServerOnly]
	public void OnPowerActivated()
	{
		SetNaniteAvailability(true);
	}
}
