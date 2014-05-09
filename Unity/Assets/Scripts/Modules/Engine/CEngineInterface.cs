//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPropulsionGeneratorSystem.cs
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
public class CEngineInterface : CNetworkMonoBehaviour 
{

// Member Types
	
	
// Member Delegates & Events
			

// Member Properties


	public float PropulsionForce
	{
        get { return (m_fPropulsion.Value); }
	}

	public float Propulsion
	{
        get { return (m_fPropulsion.Value); }
	}

	
// Member Functions
	

	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
		m_fPropulsion = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}


    void Awake()
    {
        m_cModuleInterface = GetComponent<CModuleInterface>();

        if (CNetwork.IsServer)
        {
            // Signup for module events
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
        CGameShips.Ship.GetComponent<CShipPropulsionSystem>().ChangeMaxPropolsion(m_fInitialPropulsion);
    }


    [AServerOnly]
    void OnEventModuleEnableChange(CModuleInterface _cSender, bool _bEnabled)
    {
        if (_bEnabled)
        {
            m_fPropulsion.Value = m_fInitialPropulsion * m_cModuleInterface.FunctioanlRatio;
        }
        else
        {
            m_fPropulsion.Value = 0.0f;
        }
    }


    [AServerOnly]
    void OnEventModuleFunctionalRatioChange(CModuleInterface _cSender, float _fPreviousRatio, float _fNewRatio)
    {
        if (m_cModuleInterface.IsEnabled)
        {
            m_fPropulsion.Value = m_fInitialPropulsion * _fNewRatio;
        }
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_fPropulsion)
        {
            // Update ship power system
            if (CNetwork.IsServer)
            {
                CGameShips.Ship.GetComponent<CShipPropulsionSystem>().ChangePropulsion(m_fPropulsion.Value - m_fPropulsion.PreviousValue);
            }
        }
    }


// Member Fields


    public float m_fInitialPropulsion = 0.0f;


    CNetworkVar<float> m_fPropulsion = null;


    CModuleInterface m_cModuleInterface = null;


}
