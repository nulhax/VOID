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


    public float GenerationRateAvailableRatio
    {
        get 
        {
            if (GenerationRateMax <= 0.0f) return (0.0f); 

            return (GenerationRateCurrent / GenerationRateMax); 
        }
    }


    public float GenerationRateCurrent
    {
        get { return (m_fGenerationRateCurrent.Value); }
    }


    public float GenerationRateMax
    {
        get { return (m_fGenerationRateMax.Value); }
    }


    public float CapacityAvailableRatio
    {
        get 
        {
            if (CapacityMax <= 0.0f) return (0.0f); 
            
            return (CapacityCurrent / CapacityMax); 
        }
    }


    public float CapacityCurrent
    {
        get { return (m_fCapacityCurrent.Value); }
    }


    public float CapacityMax
    {
        get { return (m_fCapacityMax.Value); }
    }


    public float ChargedRatio
    {
        get
        {
            if (CapacityCurrent <= 0.0f) return (0.0f);

            return (ChargeCurrent / CapacityCurrent);
        }
    }


    public float ChargeCurrent
    {
        get { return (m_fCurrentCharge.Value); }
    }


    public float ConsumptionRate
    {
        get { return (m_fCurrentConsumptionRate.Value); }
    }


    public bool IsGenerating
    {
        get { return (GenerationRateCurrent > 0.0f); }
    }


    public bool IsFullyCharged
    {
        get { return (ChargeCurrent == CapacityCurrent); }
    }


    public bool HasCapacity
    {
        get { return (CapacityCurrent > 0.0f); }
    }

	
// Member Methods


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
        m_fGenerationRateMax        = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fCurrentConsumptionRate   = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fGenerationRateCurrent    = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fCapacityMax              = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fCapacityCurrent          = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fCurrentCharge            = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}


    public void ChangeGenerationRateMax(float _fValue)
    {
        m_fGenerationRateMax.Value += _fValue;

        Debug.Log(string.Format("Ship power generation max change({0}) max generation({1})", _fValue, m_fGenerationRateMax.Value));
    }


    public void ChangeGenerationRateCurrent(float _fValue)
    {
        m_fGenerationRateCurrent.Value += _fValue;

        Debug.Log(string.Format("Ship power generation total change({0}) total generation({1})", _fValue, m_fGenerationRateCurrent.Value));
    }


    public void ChangeConsumptionRate(float _fValue)
    {
        m_fCurrentConsumptionRate.Value += _fValue;

        Debug.Log(string.Format("Ship power consumption total change({0}) total consumption({1})", _fValue, m_fCurrentConsumptionRate.Value));
    }


    public void ChangeCapacityMax(float _fValue)
    {
        m_fCapacityMax.Value += _fValue;

        Debug.Log(string.Format("Ship power capacity max change({0}) max capacity({1})", _fValue, m_fCapacityMax.Value));
    }


    public void ChangeCapacityCurrent(float _fValue)
    {
        m_fCapacityCurrent.Value += _fValue;

        Debug.Log(string.Format("Ship power capacity total change({0}) total capacity({1})", _fValue, m_fCapacityCurrent.Value));
    }


	void Update()
	{
        if (CNetwork.IsServer)
        {
            UpdateCharge();
        }
	}


    void UpdateCharge()
    {
        // Check already fully charged
        if (IsFullyCharged)
            return;

        // Check no generation
        if (!IsGenerating)
            return;

        // Check no capacity to store charge
        if (!HasCapacity)
            return;

        // Calculate new charge
        float fNewCharge = ChargeCurrent + GenerationRateCurrent * Time.deltaTime;

        // Set new capacity if not bigger to available capacity
        if (fNewCharge < CapacityCurrent)
        {
            m_fCurrentCharge.Value = fNewCharge;
        }

        // Set to full available capacity
        else
        {
            m_fCurrentCharge.Value = CapacityCurrent;
        }
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        // Empty
    }
	

// Member Fields


    CNetworkVar<float> m_fGenerationRateMax = null;
    CNetworkVar<float> m_fCurrentConsumptionRate = null;
    CNetworkVar<float> m_fGenerationRateCurrent = null;
    CNetworkVar<float> m_fCapacityMax = null;
    CNetworkVar<float> m_fCapacityCurrent = null;
    CNetworkVar<float> m_fCurrentCharge = null;





}



