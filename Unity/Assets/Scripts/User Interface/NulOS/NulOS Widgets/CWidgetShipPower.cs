//  Auckland
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


[RequireComponent(typeof(CNOSWidget))]
public class CWidgetShipPower : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public UIProgressBar m_GenerationBar = null;
	public UILabel m_GenerationRate = null;

	public UIProgressBar m_ChargeBar = null;
	public UILabel m_ChargeRate = null;

	public UILabel m_Consumption = null;

	public List<UISprite> m_ErrorIcons = new List<UISprite>();

	private float m_LastGenerationValue = 0.0f;
	private float m_LastChargeValue = 0.0f;
	private int m_LastConsumptionValue = 0;

	private bool m_ShowErrors = false;


	// Member Properties
	
	
	// Member Methods
	public void Update()
	{
		UpdateDUI();
	}

	public void UpdateDUI()
	{
		UpdateGenerationInformation();
		UpdateChargeInformation();
		UpdateConsumptionInformation();
		UpdateErrors();
	}

	public void UpdateGenerationInformation()
	{
		// Get the ship generation and generation potential
		float shipGeneration = CGameShips.Ship.GetComponent<CShipPowerSystem>().CapacityCurrent;
		float shipGenerationPotential = CGameShips.Ship.GetComponent<CShipPowerSystem>().GenerationRateCurrent;

		// Calculate the value ratio
		float value = shipGeneration/shipGenerationPotential;

		// Update the bar
		CDUIUtilites.LerpBarColor(value, m_GenerationBar);
		m_GenerationBar.value = value;
		
		// Update the lable
		m_GenerationRate.color = CDUIUtilites.LerpColor(value);
		m_GenerationRate.text = Mathf.RoundToInt(shipGeneration) + " / " + Mathf.RoundToInt(shipGenerationPotential);

//		// Update the positive/negative report
//		if(value < m_LastGenerationValue)
//		{
//			m_GenerationNegative.enabled = true;
//			m_GenerationPositive.enabled = false;
//			m_GenerationIdle.enabled = false;
//		}
//		else if(value > m_LastGenerationValue)
//		{
//			m_GenerationNegative.enabled = false;
//			m_GenerationPositive.enabled = true;
//			m_GenerationIdle.enabled = false;
//		}
//		else
//		{
//			m_GenerationNegative.enabled = false;
//			m_GenerationPositive.enabled = false;
//			m_GenerationIdle.enabled = true;
//			m_GenerationIdle.color = CDUIUtilites.LerpColor(value);
//		}

		m_LastGenerationValue = value;
	}

	public void UpdateChargeInformation()
	{
		// Get the ship charge, charge capacity and charge capacity potential
		float shipCharge = CGameShips.Ship.GetComponent<CShipPowerSystem>().CapacityCurrent;
		float shipChargeCapacity = CGameShips.Ship.GetComponent<CShipPowerSystem>().ChargeCurrent;
		
		// Calculate the value ratio
		float value = shipCharge/shipChargeCapacity;
		if(float.IsNaN(value)) 
			value = 0.0f;

		// Update the bar
		CDUIUtilites.LerpBarColor(value, m_ChargeBar);
		m_ChargeBar.value = value;
		
		// Update the lable
		m_ChargeRate.color = CDUIUtilites.LerpColor(value);
		m_ChargeRate.text = Mathf.RoundToInt(shipCharge) + " / " + Mathf.RoundToInt(shipChargeCapacity);
		
//		// Update the positive/negative report
//		if(value < m_LastChargeValue)
//		{
//			m_ChargeNegative.enabled = true;
//			m_ChargePositive.enabled = false;
//			m_ChargeIdle.enabled = false;
//		}
//		else if(value > m_LastChargeValue)
//		{
//			m_ChargeNegative.enabled = false;
//			m_ChargePositive.enabled = true;
//			m_ChargeIdle.enabled = false;
//		}
//		else
//		{
//			m_ChargeNegative.enabled = false;
//			m_ChargePositive.enabled = false;
//			m_ChargeIdle.enabled = true;
//			m_ChargeIdle.color = CDUIUtilites.LerpColor(value);
//		}

		m_LastChargeValue = value;
	}

	private void UpdateConsumptionInformation()
	{
		// Get the ship consumption and generation rate
        float shipConsumptionRate = CGameShips.Ship.GetComponent<CShipPowerSystem>().ConsumptionRate;
		float shipGenerationRate = CGameShips.Ship.GetComponent<CShipPowerSystem>().CapacityCurrent;
		int finalValue = Mathf.RoundToInt(shipGenerationRate - shipConsumptionRate);

		if(finalValue > 0)
		{
			// Update the text to be positive
			m_Consumption.text = "+" + finalValue.ToString();
			m_Consumption.color = Color.green;
		}
		else if(finalValue < 0)
		{
			// Update the text to be negative
			m_Consumption.text = finalValue.ToString();
			m_Consumption.color = Color.red;
		}
		else
		{
			// Update the text to be neutral
			m_Consumption.text = finalValue.ToString();
			m_Consumption.color = Color.cyan;
		}

		m_LastConsumptionValue = finalValue;
	}

	private void UpdateErrors()
	{
		// Update the error icons
		bool showingErrors = m_ShowErrors;
		if(m_LastChargeValue < 0.2f || 
		   m_LastGenerationValue < 0.2f)
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