//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShipPropulsionSystem.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//

// Namespaces
using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CShipPropulsionSystem : CNetworkMonoBehaviour 
{
// Member Types
	
	
// Member Delegates & Events


// Member Properties


    public float PropulsionAvailableRation
    {
        get 
        {
            if (PropulsionMax <= 0.0f) return (0.0f);

            return (PropulsionCurrent / PropulsionMax); 
        }
    }


	public float PropulsionCurrent
	{
        get { return (m_fPropulsionCurrent.Value); } 
	}


    public float PropulsionMax
    {
        get { return (m_fPropulsionMax.Value); }
    }


// Member Methods


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
        m_fPropulsionCurrent = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fPropulsionMax = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}


    [AServerOnly]
    public void ChangeMaxPropolsion(float _fAmount)
    {
        m_fPropulsionMax.Value += _fAmount;

        Debug.Log(string.Format("Ship propulsion change({0}) max propulsion({1})", _fAmount, m_fPropulsionMax.Value));
    }


    [AServerOnly]
    public void ChangePropulsion(float _fAmount)
    {
        m_fPropulsionCurrent.Value += _fAmount;

        Debug.Log(string.Format("Ship propulsion total change({0}) total propulsion({1})", _fAmount, m_fPropulsionCurrent.Value));
    }

	
	void Start() 
	{
		// Empty
	}

	void Update() 
	{
        // Empty
	}


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        // Empty
    }


// Member Fields


    CNetworkVar<float> m_fPropulsionCurrent = null;
    CNetworkVar<float> m_fPropulsionMax = null;


}
