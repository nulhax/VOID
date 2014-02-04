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


    void Start()
    {
        m_RatchetComponent1.EventComponentBreak += OnComponentDamaged;
        m_RatchetComponent2.EventComponentBreak += OnComponentDamaged;
        m_RatchetComponent1.EventComponentFix += OnComponentRepaired;
        m_RatchetComponent2.EventComponentFix += OnComponentRepaired;
    }


    void OnDestroy() { }


    void Update()
    {
        
    }


    void OnComponentDamaged(CComponentInterface _Component)
    {
        GetComponent<CPropulsionGeneratorSystem>().DeactivatePropulsionGeneration();
    }

    void OnComponentRepaired(CComponentInterface _Component)
    {
        GetComponent<CPropulsionGeneratorSystem>().ActivatePropulsionGeneration();
    }


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar) { }


    public CComponentInterface m_RatchetComponent1 = null;
    public CComponentInterface m_RatchetComponent2 = null;

    static CModuleEngineBehaviour m_Instance;
};