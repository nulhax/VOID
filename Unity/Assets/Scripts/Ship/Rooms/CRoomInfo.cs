//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRoomInfo.cs
//  Description :   Information class for rooms
//
//  Author  	:  Nathan Boon
//  Mail    	:  Nathan.BooN@gmail.com
//

// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CRoomInfo : CNetworkMonoBehaviour
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
    float PowerConsumption { get { return (m_fPowerConsumption.Get()); } }

    bool HasPowerStorage   { get { return (m_bHasPowerStorage.Get()); } }

    EPriority Priority     { get { return (m_ePriority.Get()); } }


// Member Functions


    public override void InitialiseNetworkVars()
    {
        m_fTemperature = new CNetworkVar<float>(OnNetworkVarSync);
        m_fOxygen = new CNetworkVar<float>(OnNetworkVarSync);
        m_fRadiation = new CNetworkVar<float>(OnNetworkVarSync);
        m_fPressure = new CNetworkVar<float>(OnNetworkVarSync);
        m_fPowerConsumption = new CNetworkVar<float>(OnNetworkVarSync);

        m_bHasPowerStorage = new CNetworkVar<bool>(OnNetworkVarSync);

        m_ePriority = new CNetworkVar<EPriority>(OnNetworkVarSync);
        m_ePriority.Set(EPriority.Medium);
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
    CNetworkVar<float> m_fPowerConsumption;

    CNetworkVar<bool> m_bHasPowerStorage;

    CNetworkVar<EPriority> m_ePriority;


};
