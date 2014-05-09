//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPowerBatterySmallBheaviour.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


/* Implementation */


[RequireComponent(typeof(CPowerBatteryInterface))]
public class CPowerBatterySmallBehaviour: MonoBehaviour 
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


	public void Start()
	{
		m_cPowerStorageInterface = gameObject.GetComponent<CPowerBatteryInterface>();

        //begin the initial ambient sound
        gameObject.GetComponent<CAudioCue>().Play(0.13f, true, 0);
	}


// Member Fields


    CPowerBatteryInterface m_cPowerStorageInterface = null;


}
