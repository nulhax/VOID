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


// Member Properties


	// If hull breach ocurrs, temp = zero. But, if the room has no pressure, the temp is unchanged.
    public float Temperature
    {
        get { return (m_fTemperature.Get()); }
    }
	
	// Oxygen cannot be higher than the pressure multiplied by the area volume
    public float Oxygen
    {
        get { return (m_fOxygen.Get()); }
    }

    public float OxygenPercent
    {
        get { return (Oxygen / m_fAreaVolume); }
    }

	// Pressure is between 0 and 1
    public float Pressure
    {
        get { return (m_fPressure.Get()); }
    }

    public bool IsOxygenRefillingEnabled
    { 
        get { return (m_bOxygenRefillingEnabled.Get()); }
    }

    public float AreaVolume
    {
        get { return (m_fAreaVolume); } 
    }


// Member Functions


    public override void InstanceNetworkVars()
    {
        m_fTemperature = new CNetworkVar<float>(OnNetworkVarSync);
        m_fOxygen = new CNetworkVar<float>(OnNetworkVarSync);
        m_fPressure = new CNetworkVar<float>(OnNetworkVarSync);
		m_bOxygenRefillingEnabled = new CNetworkVar<bool>(OnNetworkVarSync, true);
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
        if (CNetwork.IsServer)
        {
            bool bIsBreached = gameObject.GetComponent<CFacilityHull>().IsBreached;

            UpdatePressure(bIsBreached);
            UpdateOxygen(bIsBreached);
            UpdateTemperature(bIsBreached);
        }
	}


    void UpdatePressure(bool _bHullBreached)
    {
        if (!_bHullBreached)
        {
            // Check pressure is not at its maxium
            if (Pressure < 1.0f)
            {
                float fNewPressure = Pressure + (k_fPressurizingRate / m_fAreaVolume) * Time.deltaTime;

                // Clamp pressure to 1
                if (fNewPressure > 1.0f)
                {
                    fNewPressure = 1.0f;
                }

                m_fPressure.Set(fNewPressure);
            }
        }
        else
        {
            // Check pressure is not at its mininum
            if (Pressure > 0.0f)
            {
                float fNewPressure = Pressure - (k_fDepressurizingRate / m_fAreaVolume) * Time.deltaTime;

                // Clamp pressure to 0
                if (fNewPressure < 0.0f)
                {
                    fNewPressure = 0.0f;
                }

                m_fPressure.Set(fNewPressure);
            }
        }
    }


    void UpdateOxygen(bool _bHullBreached)
    {
        float fFacilityGasCapacity = Pressure * AreaVolume;

        if (!_bHullBreached)
        {
            // Check oxygen level is below area volume 
            if (Oxygen < fFacilityGasCapacity)
            {
                float fNewOxygen = Oxygen + k_fOxygenFillRate * Time.deltaTime;

                // Clamp oxygen to gas capacity
                if (fNewOxygen > fFacilityGasCapacity)
                {
                    fNewOxygen = fFacilityGasCapacity;
                }

                // Increase oxygen amount
                m_fOxygen.Set(fNewOxygen);
            }
        }
        else
        {
            // Check oxygen is higher then the gas capacity
            if (Oxygen > fFacilityGasCapacity)
            {
                // Set oxygen to gas capacity
                m_fOxygen.Set(fFacilityGasCapacity);
            }
        }
    }


    void UpdateTemperature(bool _bHullBreached)
    {
        if (!_bHullBreached)
        {
            if (Temperature < k_fPerfectTemperature)
            {
                float fNewTemperature = Temperature + 1 * Time.deltaTime;

                if (fNewTemperature > k_fPerfectTemperature)
                {
                    fNewTemperature = k_fPerfectTemperature;
                }

                m_fTemperature.Set(fNewTemperature);
            }
        }
        else
        {
            if (Temperature > 0.0f)
            {
                float fNewTemperature = Temperature - 3 * Time.deltaTime;

                if (fNewTemperature < 0.0f)
                {
                    fNewTemperature = 0.0f;
                }

                // Set oxygen to gas capacity
                m_fTemperature.Set(fNewTemperature);
            }
        }
    }


    void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        // Empty
    }


    void OnHullBreach()
    {
        // Empty
    }
	
	
// Member Fields


    public const float k_fOxygenFillRate        = 17.0f;
    public const float k_fO2DecrementRate       = 40.0f;
    public const float k_fPressurizingRate      = 34.0f;
    public const float k_fDepressurizingRate    = 60.0f;
    public const float k_fPerfectTemperature    = 21.0f;


    CNetworkVar<float> m_fTemperature;
    CNetworkVar<float> m_fOxygen;
    CNetworkVar<float> m_fPressure;
	CNetworkVar<bool> m_bOxygenRefillingEnabled;


	float m_fAreaVolume = 1000.0f;


};
