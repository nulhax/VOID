//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLASSNAME.cs
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


public class CAirlockFacilityBehaviour : CNetworkMonoBehaviour
{

// Member Types





// Member Delegates & Events


// Member Properties


// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
    }


	void Start()
	{
        m_cDoorExternal.GetComponent<CDoorBehaviour>();
        m_cDoorFacility.GetComponent<CDoorBehaviour>();

        m_cDuiInternal.GetComponent<CDUIConsole>().DUI.GetComponent<CDuiFacilityDoorBehaviour>();
        m_cDuiFacility.GetComponent<CDUIConsole>().DUI.GetComponent<CDuiFacilityDoorBehaviour>();
        m_cDuiExternal.GetComponent<CDUIConsole>().DUI.GetComponent<CDuiFacilityDoorBehaviour>();
	}


	void OnDestroy()
	{
	}


	void Update()
	{
	}


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
    }


// Member Fields


    public GameObject m_cDoorExternal = null;
    public GameObject m_cDoorFacility = null;

    public GameObject m_cDuiInternal = null;
    public GameObject m_cDuiFacility = null;
    public GameObject m_cDuiExternal = null;


    //CNetworkVar 


};
