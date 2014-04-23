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


	public float ChargedAmount
	{ 
		get { return (m_fChargedAmount.Value); }
	}
	

	public float ChargeCapacity
	{ 
		get { return (m_fCapacity.Value); }
	}


	public bool IsFullyCharged
	{
		get { return (ChargedAmount == ChargeCapacity); }
	}
	

// Member Functions
	

	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
        m_fChargedAmount = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, m_fInitialCapacity * m_fInitialChargeRatio);
		m_fCapacity      = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, m_fInitialCapacity);
	}


    void Start()
    {
        // Empty
    }


    void OnNetworkVarSync(INetworkVar _VarInstance)
    {
        if (_VarInstance == m_fCapacity)
        {
            // Empty
        }
        else if (_VarInstance == m_fChargedAmount)
        {
            // Empty
        }
    }


// Member Fields


    public float m_fInitialCapacity = 0.0f;
    public float m_fInitialChargeRatio = 0.0f;


    CNetworkVar<float> m_fChargedAmount = null;
    CNetworkVar<float> m_fCapacity = null;


}
