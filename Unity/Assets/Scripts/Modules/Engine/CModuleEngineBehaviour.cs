//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CModuleEngineBehaviour.cs
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


public class CModuleEngineBehaviour : CNetworkMonoBehaviour
{
    // Member Types


    // Member Delegates & Events


    // Member Properties


    // Member Methods
    public void Awake()
    {
        // Save a static reference to this class
        m_Instance = this;
    }


    void Start() { }


    void OnDestroy() { }


    void Update() { }


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar) { }


    static CModuleEngineBehaviour m_Instance;
};