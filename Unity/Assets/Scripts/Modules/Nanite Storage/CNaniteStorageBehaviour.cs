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

    public const int m_kiAbsoluteMaximumCapacity = 500;
	public int m_iCurrentMaximumCapacity         = 500;
	
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
	
	public bool HasAvailableNanites
	{
		get { return (m_bNanitesAvailable.Get() && m_iStoredNanites.Get() != 0.0f); }
	}
	
	// Member Functions
	
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_iStoredNanites = _cRegistrar.CreateNetworkVar<int>(OnNetworkVarSync, 0);
		m_iNaniteCapacity = _cRegistrar.CreateNetworkVar<int>(OnNetworkVarSync, m_iCurrentMaximumCapacity);
		m_bNanitesAvailable = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, false);
	}
	
	void OnNetworkVarSync(INetworkVar _VarInstance)
	{

	}
	
	public void Start()
	{
        m_CompCalib.EventComponentBreak += OnComponentStateChange;
        m_CompFluid.EventComponentBreak += OnComponentStateChange;
        m_CompCalib.EventComponentFix   += OnComponentStateChange;
        m_CompFluid.EventComponentFix   += OnComponentStateChange;
        
		CGameShips.Ship.GetComponent<CShipNaniteSystem>().RegisterNaniteSilo(gameObject);
		
		if(CNetwork.IsServer)
		{
			ActivateNaniteAvailability();
		}
	}

    void OnDestroy()
    {
        m_CompCalib.EventComponentBreak -= OnComponentStateChange;
        m_CompFluid.EventComponentBreak -= OnComponentStateChange;
        m_CompCalib.EventComponentFix   -= OnComponentStateChange;
        m_CompFluid.EventComponentFix   -= OnComponentStateChange;
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

    [AServerOnly]
    void OnComponentStateChange(CComponentInterface _Sender)
    {
        // Local variables
        float fMultiplicationCoefficient = 0.0f;
        byte NumFunctionalComponents     = 0;

        // Count the number of functional components
        if (m_CompCalib.IsFunctional) { ++NumFunctionalComponents; }
        if (m_CompFluid.IsFunctional) { ++NumFunctionalComponents; }

        // Determine nanite multiplication coefficient
        fMultiplicationCoefficient = 0.5f * NumFunctionalComponents;

        // Halve the maximum amount of nanites that can be stored by this module
        m_iCurrentMaximumCapacity = ((int)(m_kiAbsoluteMaximumCapacity * fMultiplicationCoefficient));

        // Update maximum capacity network var
        m_iNaniteCapacity.Set(m_iCurrentMaximumCapacity);

        // If the amount of nanites currently stored exceeds the storage maximum
        if (StoredNanites > m_iCurrentMaximumCapacity)
        {
            // Set the amount of stored nanites to the maximum
            // Deduct all surplus nanites from the ship's total
            CGameShips.Ship.GetComponent<CShipNaniteSystem>().DeductNanites(StoredNanites - m_iCurrentMaximumCapacity);

            // Cause an explosion as excess nanites are released
            // TODO: Add an explosion here
        }
    }

    public CComponentInterface m_CompCalib;
    public CComponentInterface m_CompFluid;
}
