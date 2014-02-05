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
        m_RatchetComponent1.EventComponentFix   += OnComponentRepaired;
        m_RatchetComponent2.EventComponentFix   += OnComponentRepaired;

        GetComponent<CActorInteractable>().EventHover += OnHover;
    }


    void OnDestroy()
    {
        m_RatchetComponent1.EventComponentBreak -= OnComponentDamaged;
        m_RatchetComponent2.EventComponentBreak -= OnComponentDamaged;
        m_RatchetComponent1.EventComponentFix   -= OnComponentRepaired;
        m_RatchetComponent2.EventComponentFix   -= OnComponentRepaired;

        GetComponent<CActorInteractable>().EventHover -= OnHover;
    }


    void Update()
    {
        
    }


    // TEMPORARY //
    //
    // Hover text logic that needs revision. OnGUI + Copy/Paste code = Terribad
    //
    // TEMPORARY //
    bool bShowName = false;
    bool bOnGUIHit = false;
    void OnHover(RaycastHit _RayHit, CNetworkViewId _cPlayerActorViewId)
    {
        bShowName = true;
    }


    public void OnGUI()
    {
        float fScreenCenterX = Screen.width / 2;
        float fScreenCenterY = Screen.height / 2;
        float fWidth = 100.0f;
        float fHeight = 20.0f;
        float fOriginX = fScreenCenterX + 25.0f;
        float fOriginY = fScreenCenterY - 10.0f;

        if (bShowName && !bOnGUIHit)
        {
            GUI.Label(new Rect(fOriginX, fOriginY, fWidth, fHeight), "Engine Module");
            bOnGUIHit = true;
        }
        else if (bShowName && bOnGUIHit)
        {
            GUI.Label(new Rect(fOriginX, fOriginY, fWidth, fHeight), "Engine Module");
            bShowName = false;
            bOnGUIHit = false;
        }
    }
    // TEMPORARY //
    //
    // 
    //
    // TEMPORARY //


    void OnComponentDamaged(CComponentInterface _Component)
    {
        GetComponent<CPropulsionGeneratorSystem>().DeactivatePropulsionGeneration();
    }


    void OnComponentRepaired(CComponentInterface _Component)
    {
        GetComponent<CPropulsionGeneratorSystem>().ActivatePropulsionGeneration();
    }


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar) { }


    // Member Fields
    public CComponentInterface m_RatchetComponent1 = null;
    public CComponentInterface m_RatchetComponent2 = null;

    static CModuleEngineBehaviour m_Instance;
};