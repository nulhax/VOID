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


public class CShipAtmosphereSystem : CNetworkMonoBehaviour 
{

// Member Types


// Member Delegates & Events


// Member Properties


    public float GenerationAvailableRatio
    {
        get 
        {
            if (GenerationRateMax == 0.0f) return (0.0f);

            return (GenerationRateCurrent / GenerationRateMax); 
        }
    }

	
	public float GenerationRateCurrent
	{
		get { return(m_fGenerationRateCurrent.Value); }
	}


    public float GenerationRateMax
    {
        get { return (m_fGenerationRateMax.Value); }
    }
	

	public float Quality
	{
		get { return(m_fQuality.Get()); }
	}

	
// Member Methods


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
		m_fQuality = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fGenerationRateMax = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_fGenerationRateCurrent = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}


    public void ChangeMaxGenerationRate(float _fValue)
    {
        m_fGenerationRateMax.Value += _fValue;

        Debug.Log(string.Format("Ship atmosphere generation max change({0}) max generation({1})", _fValue, m_fGenerationRateMax.Value));
    }


    public void ChangeGenerationRate(float _fValue)
    {
        m_fGenerationRateCurrent.Value += _fValue;

        Debug.Log(string.Format("Ship atmosphere generation total change({0}) total generation({1})", _fValue, m_fGenerationRateCurrent.Value));
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


    CNetworkVar<float> m_fQuality = null;
    CNetworkVar<float> m_fGenerationRateMax = null;
	CNetworkVar<float> m_fGenerationRateCurrent = null;


}
