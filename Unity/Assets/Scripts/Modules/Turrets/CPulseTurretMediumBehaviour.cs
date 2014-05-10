//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   PulseTurretMediumBehaviour.cs
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


public class CPulseTurretMediumBehaviour : MonoBehaviour
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
        m_cTurretInterface = GetComponent<CTurretInterface>();

        m_cTurretInterface.EventPrimaryFire += OnEventFirePrimary;
        m_cTurretInterface.EventPrimaryFire += OnEventFireSecondary;
	}


	void OnDestroy()
	{
	}


	void Update()
	{
	}


    void OnEventFirePrimary(CTurretInterface _cSender)
    {
    }


    void OnEventFireSecondary(CTurretInterface _cSender)
    {
    }


// Member Fields


    CTurretInterface m_cTurretInterface = null;


};
