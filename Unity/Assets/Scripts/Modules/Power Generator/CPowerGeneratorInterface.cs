//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPowerGenerationBehaviour.cs
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
public class CPowerGeneratorInterface : CNetworkMonoBehaviour 
{
	
// Member Types


// Member Delegates & Events


// Member Properties


	public float PowerGenerationRate
	{ 
		get { return (m_fGenerationRate.Value); }
	}


// Member Functions


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
        m_fGenerationRate = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}


    void Awake()
    {
        m_cModuleInterface = GetComponent<CModuleInterface>();

        if (CNetwork.IsServer)
        {
            // Signup for module events
            m_cModuleInterface.EventEnableChange += OnEventModuleEnableChange;
            m_cModuleInterface.EventFunctionalRatioChange += OnEventModuleFunctionalRatioChange;
        }
    }


	void Start()
	{
        if (CNetwork.IsServer)
        {
            // Increase the ships max generation rate
            CGameShips.Ship.GetComponent<CShipPowerSystem>().ChangeMaxGenerationRate(m_fInitialGenerationRate);
        }
	}


    [AServerOnly]
    void OnEventModuleEnableChange(CModuleInterface _cSender, bool _bEnabled)
    {
        if (_bEnabled)
        {
            m_fGenerationRate.Value = m_fInitialGenerationRate * m_cModuleInterface.FunctioanlRatio;
        }
        else
        {
            m_fGenerationRate.Value = 0.0f;
        }
    }


    [AServerOnly]
    void OnEventModuleFunctionalRatioChange(CModuleInterface _cSender, float _fPreviousRatio, float _fNewRatio)
    {
        if (m_cModuleInterface.IsEnabled)
        {
            m_fGenerationRate.Value = m_fInitialGenerationRate * _fNewRatio;
        }
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_fGenerationRate)
        {
            // Update ship power system
            if (CNetwork.IsServer)
            {
                CGameShips.Ship.GetComponent<CShipPowerSystem>().ChangeGenerationRate(m_fGenerationRate.Value - m_fGenerationRate.PreviousValue);
            }
        }
    }


// Member Fields


    public float m_fInitialGenerationRate = 0.0f;


    CNetworkVar<float> m_fGenerationRate = null;


    CModuleInterface m_cModuleInterface = null;


}
