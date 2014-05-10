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


public class CHud2dInterface : MonoBehaviour
{

// Member Types


    public enum EHud
    {
        INVALID,

        TurretCockpitMenu,
        ModuleMenu,
        PilotOverlay
    }


// Member Delegates & Events


// Member Properties


    public EHud ActiveHud
    {
        get { return (m_eActiveHud); }
    }


    public CHudModuleBuildMenuInterface ModuleBuildMenuInterface
    {
        get { return (m_cPanelModuleMenu.GetComponent<CHudModuleBuildMenuInterface>()); }
    }


    public CHudTurretCockpitInterface TurretCockpitControlInterface
    {
        get { return (m_cPanelTurretCockpitMenu.GetComponent<CHudTurretCockpitInterface>()); }
    }


    public CHudModuleBuildMenuInterface PilotOverlay
    {
        get { Debug.LogError("TODO"); return (m_cPanelPilotOverlay.GetComponent<CHudModuleBuildMenuInterface>()); }
    }


// Member Methods


    public void ToggleHud(EHud _eHud)
    {
        if (ActiveHud == _eHud)
        {
            HideHud(_eHud);
        }
        else
        {
            ShowHud(_eHud);
        }
    }


    public void ShowHud(EHud _eHud)
    {
        HideAllHuds();

        switch (_eHud)
        {
            case EHud.ModuleMenu:
                m_cPanelModuleMenu.gameObject.SetActive(true);
                m_cActiveHudPanel = m_cPanelModuleMenu;
                break;

            case EHud.TurretCockpitMenu:
                m_cPanelTurretCockpitMenu.gameObject.SetActive(true);
                m_cActiveHudPanel = m_cPanelTurretCockpitMenu;
                break;

            case EHud.PilotOverlay:
                m_cPanelPilotOverlay.gameObject.SetActive(true);
                m_cActiveHudPanel = m_cPanelPilotOverlay;
                break;

            default:
                Debug.LogError("Unknown hud: " + _eHud);
                break;
        }

        m_eActiveHud = _eHud;
        m_cCamera.gameObject.SetActive(true);
    }


    public void HideHud(EHud _eHud)
    {
        if (ActiveHud == _eHud)
        {
            CloseActiveHud();
        }
    }


    public void CloseActiveHud()
    {
        if (m_eActiveHud != EHud.INVALID)
        {
            m_cActiveHudPanel.gameObject.SetActive(false);
            m_eActiveHud = EHud.INVALID;
            m_cCamera.gameObject.SetActive(false);
        }
    }


	void Start()
	{
        HideAllHuds();
	}


	void OnDestroy()
	{
	}


	void Update()
	{
	}


    void HideAllHuds()
    {
        m_cPanelModuleMenu.gameObject.SetActive(false);
        m_cPanelTurretCockpitMenu.gameObject.SetActive(false);
        m_cPanelPilotOverlay.gameObject.SetActive(false);
        m_cCamera.gameObject.SetActive(false);
    }


// Member Fields


    public UIPanel m_cPanelModuleMenu = null;
    public UIPanel m_cPanelTurretCockpitMenu = null;
    public UIPanel m_cPanelPilotOverlay = null;
    public Camera  m_cCamera = null;


    UIPanel m_cActiveHudPanel = null;
    EHud m_eActiveHud = EHud.INVALID;


};
