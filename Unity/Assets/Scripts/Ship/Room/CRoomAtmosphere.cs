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


/* Implementation */


public class CRoomAtmosphere : CNetworkMonoBehaviour
{

// Member Types
	const float k_fO2FillRate = 17.0f;
	const float k_fO2DecrementRate = 40.0f;
	const float k_fPressurizingRate = 34.0f;
	const float k_fDepressurizingRate = 60.0f;

    enum EPriority
    {
        Invalid = -1,
        Low,
        Medium,
        High,
        Critical,
        Max
    }


// Member Delegates & Events


// Member Properties

	// If hull breach ocurrs, temp = zero. But, if the room has no pressure, the temp is unchanged.
    float Temperature      { get { return (m_fTemperature.Get()); } }
	
	// Oxygen cannot be higher than the pressure multiplied by the volume
    float Oxygen           { get { return (m_fOxygen.Get()); } }
	
    float Radiation        { get { return (m_fRadiation.Get()); } }
	
	// Pressure is between 0 and 1
    float Pressure         { get { return (m_fPressure.Get()); } }
	
	bool  IsReceivingO2    { get { return (m_bO2ReceiveEnabled.Get()); } }
    EPriority Priority     { get { return (m_ePriority.Get()); } }
	
	float Volume		   { get { return (m_fVolume); } }


// Member Functions


    public override void InstanceNetworkVars()
    {
        m_fTemperature = new CNetworkVar<float>(OnNetworkVarSync);
        m_fOxygen = new CNetworkVar<float>(OnNetworkVarSync);
        m_fRadiation = new CNetworkVar<float>(OnNetworkVarSync);
        m_fPressure = new CNetworkVar<float>(OnNetworkVarSync);
		m_ePriority = new CNetworkVar<EPriority>(OnNetworkVarSync, EPriority.Medium);
		m_bO2ReceiveEnabled = new CNetworkVar<bool>(OnNetworkVarSync, true);
    }


	public void Start()
	{
		gameObject.GetComponent<CFacilityHull>().EventBreached += new CFacilityHull.NotifyBreached(OnHullBreach);

	}


	public void OnDestroy()
	{
	}


	public void Update()
	{
		bool bIsBreached = gameObject.GetComponent<CFacilityHull>().IsBreached;
		
		if(!bIsBreached)
		{
			// Increment pressure
			if(Pressure < 1.0f)
			{
				m_fPressure.Set(Pressure + k_fPressurizingRate * Time.deltaTime);
				
				if(Pressure > 1.0f)
				{
					m_fPressure.Set(1.0f);
				}
			}
			
			// Increment oxygen
			if(Oxygen < Volume)
			{
				
				float fPressureVolume = Pressure * Volume;
				
				if( Oxygen <  fPressureVolume)
				{
					m_fOxygen.Set(Oxygen + k_fO2FillRate * Time.deltaTime);
				}
				else
				{
					m_fOxygen.Set(fPressureVolume);
				}
			}
		}
		else
		{
			if(Pressure > 0.0f)
			{
				m_fPressure.Set(Pressure - k_fDepressurizingRate * Time.deltaTime);
			}
			
			if(Oxygen > Pressure * Volume)
			{
				m_fOxygen.Set ( Oxygen - k_fO2DecrementRate * Time.deltaTime);
			}
		}

	}


    void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
    }

	void OnHullBreach()
	{
		m_fTemperature.Set(0.0f);
	}
	
	
// Member Fields


    CNetworkVar<float> m_fTemperature;
    CNetworkVar<float> m_fOxygen;
    CNetworkVar<float> m_fRadiation;
    CNetworkVar<float> m_fPressure;
	CNetworkVar<bool> m_bO2ReceiveEnabled;
	float m_fVolume = 1000.0f;


    CNetworkVar<EPriority> m_ePriority;


};
