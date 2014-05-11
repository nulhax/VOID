//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CGalaxyShipShield.cs
//  Description :   --------------------------
//
//  Author      :  
//  Mail        :  
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CShipShieldSystem : CNetworkMonoBehaviour 
{

// Member Types


	public enum EShieldState
	{
		INVALID,
		
		MAX
	}


// Member Delegates & Events


// Member Properties


    public float GenerationRate
    {
        get { return (m_fCurrentGenerationRate.Value); }
    }


    public float GenerationRateMax
    {
        get { return (m_fMaxGenerationRate.Value); }
    }


    public float Capacity
    {
        get { return (m_fCurrentCapacity.Value); }
    }


    public float CapacityMax
    {
        get { return (m_fMaxCapacity.Value); }
    }


    public float Charge
    {
        get { return (m_fCurrentCharge.Value); }
    }


    public bool IsGenerating
    {
        get { return (GenerationRate > 0.0f); }
    }


    public bool IsFullyCharged
    {
        get { return (Charge == Capacity); }
    }


    public bool HasCapacity
    {
        get { return (Capacity > 0.0f); }
    }
	

// Member Methods


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        m_fMaxGenerationRate = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fCurrentGenerationRate = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fMaxCapacity = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fCurrentCapacity = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fCurrentCharge = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
    }


    public void ChangeMaxGenerationRate(float _fValue)
    {
        m_fMaxGenerationRate.Value += _fValue;

        Debug.Log(string.Format("Ship shield generation max change({0}) max generation({1})", _fValue, m_fMaxGenerationRate.Value));
    }


    public void ChangeGenerationRate(float _fValue)
    {
        m_fCurrentGenerationRate.Value += _fValue;

        Debug.Log(string.Format("Ship shield generation total change({0}) total generation({1})", _fValue, m_fCurrentGenerationRate.Value));
    }


    public void ChangeMaxCapacity(float _fValue)
    {
        m_fMaxCapacity.Value += _fValue;

        Debug.Log(string.Format("Ship shield capacity max change({0}) max capacity({1})", _fValue, m_fMaxCapacity.Value));
    }


    public void ChangeCapacity(float _fValue)
    {
        m_fCurrentCapacity.Value += _fValue;

        Debug.Log(string.Format("Ship shield capacity total change({0}) total capacity({1})", _fValue, m_fCurrentCapacity.Value));
    }


    public bool ReduceCharge(float _fAmount)
    {
        bool bReduced = false;

        if (Charge > _fAmount)
        {
            m_fCurrentCharge.Value -= _fAmount;

            bReduced = true;
        }

        return (bReduced);
    }


	void Start()
	{

	}


    void Update()
    {
        UpdateCharge();
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
        float fNewCharge = GenerationRate * Time.deltaTime;

        // Set new capacity if not bigger to available capacity
        if (fNewCharge < Capacity)
        {
            m_fCurrentCharge.Value = fNewCharge;
        }

        // Set to full available capacity
        else
        {
            m_fCurrentCharge.Value = Capacity;
        }
    }


	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
        // Empty
	}
	
	
// Member Fields


    CNetworkVar<float> m_fMaxGenerationRate = null;
    CNetworkVar<float> m_fCurrentGenerationRate = null;
    CNetworkVar<float> m_fMaxCapacity = null;
    CNetworkVar<float> m_fCurrentCapacity = null;
    CNetworkVar<float> m_fCurrentCharge = null;


}
