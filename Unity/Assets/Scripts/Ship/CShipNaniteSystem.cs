//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShipNaniteSystem.cs
//  Description :   Keeps track of ship-wide nanites and is responsible for adding and subtracting nanites
//
//  Author  	:  Daniel Langsford
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/* Implementation */

public class CShipNaniteSystem : CNetworkMonoBehaviour 
{

// Member Types
	
	
// Member Delegates & Events


    public delegate void QuantityChangeHandler(float _fPreviousQuantity, float _fNewQuantity);
    public event QuantityChangeHandler EventQuantityChange;


    public delegate void CapacityChangeHandler(float _fPreviousQuantity, float _fNewQuantity);
    public event CapacityChangeHandler EventCapacityChange;



// Member Properties


    public float NanaiteMaxRatio
    {
        get { return (NanaiteQuanity / NanaiteCapacity); }
    }


    public float NanaiteQuanity
	{
        get { return (m_fNanaiteQuanity.Value); } 
	}


	public float NanaiteCapacity
	{
        get { return (m_fNanaiteCapacity.Value); } 
	}


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
        m_fNanaiteQuanity  = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fNanaiteCapacity = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}


	public void Update()
	{
        // Empty
	}


	[AServerOnly]
	public float ChangeQuanity(float _fNumNanites)
	{
        float fChangeQuanity = 0.0f;
        float fNewQuanity = m_fNanaiteQuanity.Value + _fNumNanites;

        // Check new quantity will be higher then capacity
        if (fNewQuanity > m_fNanaiteCapacity.Value)
        {
            fChangeQuanity = fNewQuanity - m_fNanaiteCapacity.Value;
        }

        // Check new quanity will be lower then zero
        else if (fNewQuanity < 0.0f)
        {
            fChangeQuanity = m_fNanaiteQuanity.Value;
        }

        // All nanites accepted
        else
        {
            fChangeQuanity = _fNumNanites;
        }

        // Make quanity change
        if (fChangeQuanity != 0.0f)
        {
            m_fNanaiteQuanity.Value += fChangeQuanity;
        }

        return (fChangeQuanity);
	}


    [AServerOnly]
    public void ChangeCapacity(float _fCapacity)
    {
        m_fNanaiteCapacity.Value += _fCapacity;
    }


    public void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_fNanaiteQuanity)
        {
            if (EventQuantityChange != null) EventQuantityChange(m_fNanaiteQuanity.PreviousValue, m_fNanaiteQuanity.Value);
        }
        else if (_cSyncedVar == m_fNanaiteCapacity)
        {
            if (EventCapacityChange != null) EventCapacityChange(m_fNanaiteQuanity.PreviousValue, m_fNanaiteQuanity.Value);
        }
    }


	public void OnGUI()
	{
        if (CNetwork.IsConnectedToServer)
        {
            float boxWidth = 150;
            float boxHeight = 40;

            GUI.Box(new Rect(Screen.width - boxWidth - 10, Screen.height - boxHeight - 110, boxWidth, boxHeight),
                             "[Ship Nanites]\n" + m_fNanaiteQuanity.Value.ToString() + " / " + m_fNanaiteCapacity.Value);
        }
	}


// Member Fields


    CNetworkVar<float> m_fNanaiteQuanity  = null;
    CNetworkVar<float> m_fNanaiteCapacity = null;


}
