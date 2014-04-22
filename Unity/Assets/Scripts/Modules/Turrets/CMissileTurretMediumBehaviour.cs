//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   MissileTurretMediumBehaviour.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CMissileTurretMediumBehaviour : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


    [ALocalOnly]
    public static void SerializeOutbound(CNetworkStream _cStream)
    {
        // Empty
    }


    [AServerOnly]
    public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        // Empty
    }


	void Start()
	{
        m_cTurretBehaviour = GetComponent<CTurretBehaviour>();

        m_cTurretBehaviour.EventPrimaryFire += OnEventFirePrimary;
        m_cTurretBehaviour.EventPrimaryFire += OnEventFireSecondary;
	}


	void OnDestroy()
	{
	}


	void Update()
	{
	}


    void OnEventFirePrimary(CTurretBehaviour _cSender)
    {
    }


    void OnEventFireSecondary(CTurretBehaviour _cSender)
    {
    }


// Member Fields


    CTurretBehaviour m_cTurretBehaviour = null;


};
