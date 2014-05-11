//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPowerStorageBehaviour.cs
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
public class CPowerBatteryInterface : CNetworkMonoBehaviour 
{

// Member Types
	
	
// Member Delegates & Events
	
	
// Member Properties


    public float InitialCapacity
    {
        get { return (m_fInitialCapacity); }
    }
	

	public float ChargeCapacity
	{ 
		get { return (m_fCapacity.Value); }
	}


// Member Functions
	

	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
		m_fCapacity = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}


    void Awake()
    {
        m_cModuleInterface = GetComponent<CModuleInterface>();

        if (CNetwork.IsServer)
        {
            // Sign up for module events
            m_cModuleInterface.EventBuilt += OnEventBuilt;
            m_cModuleInterface.EventEnableChange += OnEventModuleEnableChange;
            m_cModuleInterface.EventFunctionalRatioChange += OnEventModuleFunctionalRatioChange;
        }
    }


    void Start()
    {
        // Empty
    }


    [AServerOnly]
    void OnEventBuilt(CModuleInterface _cSender)
    {
        CGameShips.Ship.GetComponent<CShipPowerSystem>().ChangeCapacityMax(m_fInitialCapacity);
    }


    [AServerOnly]
    void OnEventModuleEnableChange(CModuleInterface _cSender, bool _bEnabled)
    {
        if (_bEnabled)
        {
            m_fCapacity.Value = m_fInitialCapacity * m_cModuleInterface.FunctioanlRatio;
        }
        else
        {
            m_fCapacity.Value = 0.0f;
        }
    }


    [AServerOnly]
    void OnEventModuleFunctionalRatioChange(CModuleInterface _cSender, float _fPreviousRatio, float _fNewRatio)
    {
        if (m_cModuleInterface.IsEnabled)
        {
            m_fCapacity.Value = m_fInitialCapacity * _fNewRatio;
        }
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_fCapacity)
        {
            // Update ship power capacity
            if (CNetwork.IsServer)
            {
                CGameShips.Ship.GetComponent<CShipPowerSystem>().ChangeCapacityCurrent(m_fCapacity.Value - m_fCapacity.PreviousValue);
            }
        }
    }


// Member Fields


    public float m_fInitialCapacity = 0.0f;

    
    CNetworkVar<float> m_fCapacity = null;


    CModuleInterface m_cModuleInterface = null;


}
