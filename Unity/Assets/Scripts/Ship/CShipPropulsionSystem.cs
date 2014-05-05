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


	public float TotalPropulsion
	{
        get { return (m_fTotalPropulsion.Value); } 
	}


    public float MaxPropulsion
    {
        get { return (m_fMaxPropulsion.Value); }
    }


// Member Methods


	public override void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar)
	{
        m_fTotalPropulsion = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fMaxPropulsion = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}


    [AServerOnly]
    public void ChangeMaxPropolsion(float _fAmount)
    {
        m_fMaxPropulsion.Value += _fAmount;

        Debug.Log(string.Format("Ship propulsion change({0}) max propulsion({1})", _fAmount, m_fMaxPropulsion.Value));
    }


    [AServerOnly]
    public void ChangePropulsion(float _fAmount)
    {
        m_fTotalPropulsion.Value += _fAmount;

        Debug.Log(string.Format("Ship propulsion total change({0}) total propulsion({1})", _fAmount, m_fTotalPropulsion.Value));
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


    CNetworkVar<float> m_fTotalPropulsion = null;
    CNetworkVar<float> m_fMaxPropulsion = null;


}
