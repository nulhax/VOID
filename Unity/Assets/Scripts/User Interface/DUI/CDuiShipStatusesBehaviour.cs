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


public class CDuiShipStatusesBehaviour : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


	void Start()
	{
	}


	void OnDestroy()
	{
	}


	void Update()
	{
        if (CGameShips.Ship == null)
            return;

        UpdateAtmosphereStatus();
        UpdateNaniteStatus();
        UpdatePowerStatus();
        UpdatePropulsionStatus();
        UpdateShieldStatus();
	}


    void UpdateAtmosphereStatus()
    {
        CShipAtmosphereSystem cShipAtmosphereSystem = CGameShips.Ship.GetComponent<CShipAtmosphereSystem>();

        m_cProgressBarAtmosphere.value = cShipAtmosphereSystem.GenerationAvailableRatio;
        m_cLabelAtmosphereCurrent.text = Mathf.CeilToInt(cShipAtmosphereSystem.GenerationRateCurrent).ToString() + " /";
        m_cLabelAtmosphereMax.text = Mathf.CeilToInt(cShipAtmosphereSystem.GenerationRateMax).ToString();
    }


    void UpdateNaniteStatus()
    {
        CShipNaniteSystem cShipNaniteSystem = CGameShips.Ship.GetComponent<CShipNaniteSystem>();

        m_cProgressBarNanites.value = cShipNaniteSystem.NanaiteCapacityRatio;
        m_cLabelNanitesCurrent.text = Mathf.CeilToInt(cShipNaniteSystem.NanaiteQuanity).ToString() + " /";
        m_cLabelNanitesMax.text = Mathf.CeilToInt(cShipNaniteSystem.NanaiteCapacity).ToString();
    }


    void UpdatePowerStatus()
    {
        CShipPowerSystem cShipPowerSystem = CGameShips.Ship.GetComponent<CShipPowerSystem>();

        m_cProgressBarPowerGeneration.value = cShipPowerSystem.GenerationRateAvailableRatio;
        m_cLabelPowerGenerationCurrent.text = Mathf.CeilToInt(cShipPowerSystem.GenerationRateCurrent).ToString() + " /";
        m_cLabelPowerGenerationMax.text = Mathf.CeilToInt(cShipPowerSystem.GenerationRateMax).ToString();

        m_cProgressBarPowerCharge.value = cShipPowerSystem.ChargedRatio;
        m_cLabelPowerChargeCurrent.text = Mathf.CeilToInt(cShipPowerSystem.ChargeCurrent).ToString() + " /";
        m_cLabelPowerChargeMax.text = Mathf.CeilToInt(cShipPowerSystem.CapacityCurrent).ToString();
    }


    void UpdatePropulsionStatus()
    {
        CShipPropulsionSystem cShipPropulsionSystem = CGameShips.Ship.GetComponent<CShipPropulsionSystem>();

        m_cProgressBarPropulsion.value = cShipPropulsionSystem.PropulsionAvailableRation;
        m_cLabelPropulsionCurrent.text = Mathf.CeilToInt(cShipPropulsionSystem.PropulsionCurrent).ToString() + " /";
        m_cLabelPropulsionMax.text = Mathf.CeilToInt(cShipPropulsionSystem.PropulsionMax).ToString();
    }


    void UpdateShieldStatus()
    {
        CShipShieldSystem cShipShieldSystem = CGameShips.Ship.GetComponent<CShipShieldSystem>();

        m_cProgressBarShieldGeneration.value = cShipShieldSystem.GenerationRateAvailableRatio;
        m_cLabelShieldGenerationCurrent.text = Mathf.CeilToInt(cShipShieldSystem.GenerationRateCurrent).ToString() + " /";
        m_cLabelShieldGenerationMax.text = Mathf.CeilToInt(cShipShieldSystem.GenerationRateMax).ToString();

        m_cProgressBarShieldCharge.value = cShipShieldSystem.ChargedRatio;
        m_cLabelShieldChargeCurrent.text = Mathf.CeilToInt(cShipShieldSystem.ChargeCurrent).ToString() + " /";
        m_cLabelShieldChargeMax.text = Mathf.CeilToInt(cShipShieldSystem.CapacityCurrent).ToString();
    }


// Member Fields


    public UIProgressBar m_cProgressBarAtmosphere = null;
    public UILabel m_cLabelAtmosphereCurrent = null;
    public UILabel m_cLabelAtmosphereMax = null;

    public UIProgressBar m_cProgressBarNanites = null;
    public UILabel m_cLabelNanitesCurrent = null;
    public UILabel m_cLabelNanitesMax = null;

    public UIProgressBar m_cProgressBarPowerGeneration = null;
    public UILabel m_cLabelPowerGenerationCurrent = null;
    public UILabel m_cLabelPowerGenerationMax = null;

    public UIProgressBar m_cProgressBarPowerCharge = null;
    public UILabel m_cLabelPowerChargeCurrent = null;
    public UILabel m_cLabelPowerChargeMax = null;

    public UIProgressBar m_cProgressBarPropulsion = null;
    public UILabel m_cLabelPropulsionCurrent = null;
    public UILabel m_cLabelPropulsionMax = null;

    public UIProgressBar m_cProgressBarShieldGeneration = null;
    public UILabel m_cLabelShieldGenerationCurrent = null;
    public UILabel m_cLabelShieldGenerationMax = null;

    public UIProgressBar m_cProgressBarShieldCharge = null;
    public UILabel m_cLabelShieldChargeCurrent = null;
    public UILabel m_cLabelShieldChargeMax = null;


};
