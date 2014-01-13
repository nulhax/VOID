//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRoomAtmosphere.cs
//  Description :   Atmosphere information for rooms
//
//  Author  	:  Nathan Boon
//  Mail    	:  Nathan.BooN@gmail.com
//

// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/* Implementation */


public class CFacilityAtmosphere : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Fields
		
	private CNetworkVar<float> m_AtmosphereTemperature;
	private CNetworkVar<float> m_AtmosphereQuantity;

	private CNetworkVar<float> m_AtmosphereRefillRate;
	private CNetworkVar<float> m_AtmosphereEmptyRate;

	private CNetworkVar<bool> m_AtmosphereLeaking;
	private CNetworkVar<bool> m_AtmosphereRefillingEnabled;
	
	private float m_fAtmosphereVolume = 1000.0f;


// Member Properties

	// If hull breach ocurrs, temp = zero. But, if the room has no pressure, the temp is unchanged.
    public float AtmosphereTemperature
    {
        get { return (m_AtmosphereTemperature.Get()); }
    }
	
	// Oxygen cannot be higher than the pressure multiplied by the area volume
    public float AtmosphereQuantity
    {
        get { return (m_AtmosphereQuantity.Get()); }
    }

	public float AtmosphereRefillRate
	{
		get { return (m_AtmosphereRefillRate.Get()); } 
	}

	public float AtmosphereEmptyRate
	{
		get { return (m_AtmosphereEmptyRate.Get()); } 
	}

    public float AtmospherePercentage
    {
		get { return (AtmosphereQuantity / m_fAtmosphereVolume); }
    }
	
    public bool IsAtmosphereRefillingEnabled
    { 
        get { return (m_AtmosphereRefillingEnabled.Get()); }
    }

	public bool IsAtmosphereLeaking
	{ 
		get { return (m_AtmosphereLeaking.Get()); }
	}

    public float AtmosphereVolume
    {
        get { return (m_fAtmosphereVolume); } 
    }


// Member Methods
	
    public override void InstanceNetworkVars()
    {
        m_AtmosphereTemperature = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_AtmosphereQuantity = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_AtmosphereRefillRate = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_AtmosphereEmptyRate = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_AtmosphereLeaking = new CNetworkVar<bool>(OnNetworkVarSync, true);
		m_AtmosphereRefillingEnabled = new CNetworkVar<bool>(OnNetworkVarSync, true);
    }

	void Awake()
	{
		gameObject.GetComponent<CFacilityHull>().EventBreached += new CFacilityHull.NotifyBreached(OnHullBreach);
	}
	
	void OnDestroy()
	{
		// Empty
	}
	
	void Update()
	{
		UpdateAtmosphereQuantity();
	}

    void UpdateAtmosphereQuantity()
    {
		if (!IsAtmosphereLeaking)
        {
			// Check atmosphere level is below area volume 
            if (AtmosphereQuantity < AtmosphereVolume)
            {
                float fNewQuantity = AtmosphereQuantity + AtmosphereRefillRate * Time.deltaTime;

                // Clamp atmosphere to gas capacity
				if (fNewQuantity > AtmosphereVolume)
                {
					fNewQuantity = AtmosphereVolume;
                }

                // Increase atmosphere amount
                m_AtmosphereQuantity.Set(fNewQuantity);
            }
        }
        else
        {
			// Check atmosphere is higher than 0
			if (AtmosphereQuantity > 0.0f)
            {
				float fNewQuantity = AtmosphereQuantity - AtmosphereEmptyRate * Time.deltaTime;
				
				// Clamp atmosphere to gas capacity
				if (fNewQuantity < 0.0f)
				{
					fNewQuantity = 0.0f;
				}

				// Increase atmosphere amount
				m_AtmosphereQuantity.Set(fNewQuantity);
            }
        }
    }


    void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        // Empty
    }


    void OnHullBreach()
    {
		if(CNetwork.IsServer)
		{
			m_AtmosphereLeaking.Set(true);
		}
    }

};
