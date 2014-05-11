//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShieldGeneratorInterface.cs
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


public class CShieldGeneratorInterface : CNetworkMonoBehaviour
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


    public float GenerationRate
    {
        get { return (m_fGenerationRate.Value); }
    }


// Member Methods


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        m_fGenerationRate = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
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
        CGameShips.Ship.GetComponent<CShipShieldSystem>().ChangeMaxGenerationRate(m_fInitialGenerationRate);

        CGameShips.Ship.GetComponent<CShipShieldSystem>().ChangeMaxCapacity(m_fInitialCapacity);
    }


    [AServerOnly]
    void OnEventModuleEnableChange(CModuleInterface _cSender, bool _bEnabled)
    {
        if (_bEnabled)
        {
            m_fGenerationRate.Value = m_fInitialGenerationRate * m_cModuleInterface.FunctioanlRatio;

            m_fCapacity.Value = m_fInitialCapacity * m_cModuleInterface.FunctioanlRatio;
        }
        else
        {
            m_fGenerationRate.Value = 0.0f;

            m_fCapacity.Value = 0.0f;
        }
    }


    [AServerOnly]
    void OnEventModuleFunctionalRatioChange(CModuleInterface _cSender, float _fPreviousRatio, float _fNewRatio)
    {
        if (m_cModuleInterface.IsEnabled)
        {
            m_fGenerationRate.Value = m_fInitialGenerationRate * _fNewRatio;

            m_fCapacity.Value = m_fInitialCapacity * _fNewRatio;
        }
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_fGenerationRate)
        {
            // Update ship power system
            if (CNetwork.IsServer)
            {
                CGameShips.Ship.GetComponent<CShipShieldSystem>().ChangeGenerationRate(m_fGenerationRate.Value - m_fGenerationRate.PreviousValue);

                CGameShips.Ship.GetComponent<CShipShieldSystem>().ChangeCapacity(m_fCapacity.Value - m_fCapacity.PreviousValue);
            }
        }
    }


// Member Fields


    public float m_fInitialGenerationRate = 0.0f;
    public float m_fInitialCapacity = 0.0f;


    CNetworkVar<float> m_fGenerationRate = null;
    CNetworkVar<float> m_fCapacity = null;


    CModuleInterface m_cModuleInterface = null;


};
