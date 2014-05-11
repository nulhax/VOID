//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShipAtmosphere.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

/* Implementation */


public class CShipPowerSystem : CNetworkMonoBehaviour 
{

// Member Types
	
	
// Member Delegates & Events


// Member Properties


    public float MaxGenerationRate
    {
        get { return (m_fMaxGenerationRate.Value); }
    }


	public float TotalGenerationRate
	{
        get { return (m_fCurrentGenerationRate.Value); } 
	}


	public float TotalCapacity
	{
        get { return (m_fCurrentCapacity.Value); } 
	}


    public float TotalCharge
    {
        get { return (m_fCurrentCharge.Value); }
    }


    public float TotalConsumptionRate
    {
        get { return (m_fCurrentConsumptionRate.Value); }
    }

	
// Member Methods


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
        m_fMaxGenerationRate        = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fCurrentConsumptionRate   = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fCurrentGenerationRate    = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fMaxCapacity              = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fCurrentCapacity          = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fCurrentCharge            = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}


    public void ChangeMaxGenerationRate(float _fValue)
    {
        m_fMaxGenerationRate.Value += _fValue;

        Debug.Log(string.Format("Ship power generation max change({0}) max generation({1})", _fValue, m_fMaxGenerationRate.Value));
    }


    public void ChangeGenerationRate(float _fValue)
    {
        m_fCurrentGenerationRate.Value += _fValue;

        Debug.Log(string.Format("Ship power generation total change({0}) total generation({1})", _fValue, m_fCurrentGenerationRate.Value));
    }


    public void ChangeConsumptionRate(float _fValue)
    {
        m_fCurrentConsumptionRate.Value += _fValue;

        Debug.Log(string.Format("Ship power consumption total change({0}) total consumption({1})", _fValue, m_fCurrentConsumptionRate.Value));
    }


    public void ChangeMaxCapacity(float _fValue)
    {
        m_fMaxCapacity.Value += _fValue;

        Debug.Log(string.Format("Ship power capacity max change({0}) max capacity({1})", _fValue, m_fMaxCapacity.Value));
    }


    public void ChangeCapacity(float _fValue)
    {
        m_fCurrentCapacity.Value += _fValue;

        Debug.Log(string.Format("Ship power capacity total change({0}) total capacity({1})", _fValue, m_fCurrentCapacity.Value));
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


    CNetworkVar<float> m_fMaxGenerationRate = null;
    CNetworkVar<float> m_fCurrentConsumptionRate = null;
    CNetworkVar<float> m_fCurrentGenerationRate = null;
    CNetworkVar<float> m_fMaxCapacity = null;
    CNetworkVar<float> m_fCurrentCapacity = null;
    CNetworkVar<float> m_fCurrentCharge = null;





}



