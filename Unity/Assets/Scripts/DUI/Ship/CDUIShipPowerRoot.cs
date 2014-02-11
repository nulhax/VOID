﻿//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CButtonSelectFacility.cs
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


[RequireComponent(typeof(CNetworkView))]
public class CDUIShipPowerRoot : CNetworkMonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public UISprite m_GenerationNegative = null;
	public UISprite m_GenerationPositive = null;
	public UISprite m_GenerationIdle = null;

	public UIProgressBar m_GenerationBar = null;
	public UILabel m_GenerationRate = null;
	
	public UISprite m_ChargeNegative = null;
	public UISprite m_ChargePositive = null;
	public UISprite m_ChargeIdle = null;

	public UIProgressBar m_ChargeBar = null;
	public UILabel m_ChargeRate = null;

	public List<UISprite> m_ErrorIcons = new List<UISprite>();

	private float m_LastGenerationValue = 0.0f;
	private float m_LastChargeValue = 0.0f;

	private bool m_ShowErrors = false;

	// Member Properties
	
	
	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		
	}
	
	private void OnNetworkVarSync(INetworkVar _SyncedNetworkVar)
	{

	}

	public void Update()
	{
		UpdateGenerationInformation();
		UpdateChargeInformation();
		UpdateErrors();
	}

	public void UpdateGenerationInformation()
	{
		// Get the ship generation and generation potential
		float shipGeneration = CGameShips.Ship.GetComponent<CShipPowerSystem>().ShipCurentGenerationRate;
		float shipGenerationPotential = CGameShips.Ship.GetComponent<CShipPowerSystem>().ShipGenerationRatePotential;

		// Calculate the value ratio
		float value = shipGeneration/shipGenerationPotential;

		// Update the bar
		Color fromColor = value > 0.5f ? Color.yellow : Color.red;
		Color toColor = value > 0.5f ? Color.cyan : Color.yellow;
		float barValue = value > 0.5f ? (value - 0.5f) / 0.5f : value / 0.5f;
		m_GenerationBar.backgroundWidget.color = Color.Lerp(fromColor * 0.8f, toColor * 0.8f, barValue);
		m_GenerationBar.foregroundWidget.color = Color.Lerp(fromColor, toColor, barValue);
		m_GenerationBar.value = value;
		
		// Update the lable
		m_GenerationRate.color = Color.Lerp(Color.red, Color.cyan, value);
		m_GenerationRate.text = shipGeneration + " / " + shipGenerationPotential;

		// Update the positive/negative report
		if(value < m_LastGenerationValue)
		{
			m_GenerationNegative.enabled = true;
			m_GenerationPositive.enabled = false;
			m_GenerationIdle.enabled = false;
		}
		else if(value > m_LastGenerationValue)
		{
			m_GenerationNegative.enabled = false;
			m_GenerationPositive.enabled = true;
			m_GenerationIdle.enabled = false;
		}
		else
		{
			m_GenerationNegative.enabled = false;
			m_GenerationPositive.enabled = false;
			m_GenerationIdle.enabled = true;
		}

		// Update the error icons
		if(value < 0.2f)
		{
			foreach(UISprite s in m_ErrorIcons)
			{
				s.enabled = true;
			}
		}
		else
		{
			foreach(UISprite s in m_ErrorIcons)
			{
				s.enabled = false;
			}
		}

		m_LastGenerationValue = value;
	}

	public void UpdateChargeInformation()
	{
		// Get the ship charge, charge capacity and charge capacity potential
		float shipCharge = CGameShips.Ship.GetComponent<CShipPowerSystem>().ShipCurrentCharge;
		float shipChargeCapacity = CGameShips.Ship.GetComponent<CShipPowerSystem>().ShipCurrentChargeCapacity;
		
		// Calculate the value ratio
		float value = shipCharge/shipChargeCapacity;
		
		// Update the bar
		Color fromColor = m_ChargeBar.value > 0.5f ? Color.yellow : Color.red;
		Color toColor = m_ChargeBar.value > 0.5f ? Color.cyan : Color.yellow;
		float barValue = value > 0.5f ? (value - 0.5f) / 0.5f : value / 0.5f;
		m_ChargeBar.backgroundWidget.color = Color.Lerp(fromColor * 0.8f, toColor * 0.8f, barValue);
		m_ChargeBar.foregroundWidget.color = Color.Lerp(fromColor, toColor, barValue);
		m_ChargeBar.value = value;
		
		// Update the lable
		m_ChargeRate.color = Color.Lerp(Color.red, Color.cyan, value);
		m_ChargeRate.text = shipCharge + " / " + shipChargeCapacity;
		
		// Update the positive/negative report
		if(value < m_LastChargeValue)
		{
			m_ChargeNegative.enabled = true;
			m_ChargePositive.enabled = false;
			m_ChargeIdle.enabled = false;
		}
		else if(value > m_LastChargeValue)
		{
			m_ChargeNegative.enabled = false;
			m_ChargePositive.enabled = true;
			m_ChargeIdle.enabled = false;
		}
		else
		{
			m_ChargeNegative.enabled = false;
			m_ChargePositive.enabled = false;
			m_ChargeIdle.enabled = true;
		}

		m_LastChargeValue = value;
	}

	private void UpdateErrors()
	{
		// Update the error icons
		bool showingErrors = m_ShowErrors;
		if(m_LastChargeValue < 0.2f || m_LastGenerationValue < 0.2f)
		{
			m_ShowErrors = true;
		}
		else
		{
			m_ShowErrors = false;
		}

		if(m_ShowErrors != showingErrors)
		{
			foreach(UISprite s in m_ErrorIcons)
			{
				s.enabled = m_ShowErrors;
			}
		}
	}
}