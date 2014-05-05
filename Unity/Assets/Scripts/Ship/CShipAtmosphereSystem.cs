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

	
	public float GenerationRate
	{
		get { return(m_fTotalGenerationRate.Value); }
	}
	

	public float Quality
	{
		get { return(m_fQuality.Get()); }
	}

	
// Member Methods


	public override void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar)
	{
		m_fQuality = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fMaxGenerationRate = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_fTotalGenerationRate = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}


    public void ChangeMaxGenerationRate(float _fValue)
    {
        m_fMaxGenerationRate.Value += _fValue;

        Debug.Log(string.Format("Ship atmosphere generation max change({0}) max generation({1})", _fValue, m_fMaxGenerationRate.Value));
    }


    public void ChangeGenerationRate(float _fValue)
    {
        m_fTotalGenerationRate.Value += _fValue;

        Debug.Log(string.Format("Ship atmosphere generation total change({0}) total generation({1})", _fValue, m_fTotalGenerationRate.Value));
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
    CNetworkVar<float> m_fMaxGenerationRate = null;
	CNetworkVar<float> m_fTotalGenerationRate = null;


}
