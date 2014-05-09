//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CThrusterSmallBehaviour.cs
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


[RequireComponent(typeof(CThrusterInterface))]
public class CThrusterSmallBehaviour : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


	void Start()
	{
        m_cThrusterInterface = GetComponent<CThrusterInterface>();
        m_cGalaxyShipMotor = CGameShips.GalaxyShip.GetComponent<CGalaxyShipMotor>();
	}


	void OnDestroy()
	{
	}


	void Update()
	{
        UpdateActive();
        UpdateTargetRotation();
        UpdateRotation();
	}


    void UpdateActive()
    {
        
    }


    void UpdateTargetRotation()
    {
    }


    void UpdateRotation()
    {

    }


// Member Fields


    public Transform m_cTransBase = null;
    public Transform m_cTransHead = null;


    CThrusterInterface m_cThrusterInterface = null;
    CGalaxyShipMotor m_cGalaxyShipMotor = null;


};
