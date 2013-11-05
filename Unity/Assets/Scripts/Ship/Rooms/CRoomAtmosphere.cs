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


    float Temperature      { get { return (m_fTemperature.Get()); } }
    float Oxygen           { get { return (m_fOxygen.Get()); } }
    float Radiation        { get { return (m_fRadiation.Get()); } }
    float Pressure         { get { return (m_fPressure.Get()); } }

    EPriority Priority     { get { return (m_ePriority.Get()); } }


// Member Functions


    public override void InstanceNetworkVars()
    {
        m_fTemperature = new CNetworkVar<float>(OnNetworkVarSync);
        m_fOxygen = new CNetworkVar<float>(OnNetworkVarSync);
        m_fRadiation = new CNetworkVar<float>(OnNetworkVarSync);
        m_fPressure = new CNetworkVar<float>(OnNetworkVarSync);
		m_ePriority = new CNetworkVar<EPriority>(OnNetworkVarSync, EPriority.Medium);
    }


	public void Start()
	{
	}


	public void OnDestroy()
	{
	}


	public void Update()
	{
	}


    void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
    }


// Member Fields


    CNetworkVar<float> m_fTemperature;
    CNetworkVar<float> m_fOxygen;
    CNetworkVar<float> m_fRadiation;
    CNetworkVar<float> m_fPressure;


    CNetworkVar<EPriority> m_ePriority;


};
