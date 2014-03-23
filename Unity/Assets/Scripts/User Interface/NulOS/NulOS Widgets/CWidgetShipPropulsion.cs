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
public class CWidgetShipPropulsion : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public UIProgressBar m_PropulsionBar = null;
	public UILabel m_PropulsionRate = null;
	
	public List<UISprite> m_ErrorIcons = new List<UISprite>();
	
	private float m_LastPropulsionValue = 0.0f;
	
	private bool m_ShowErrors = false;
	
	
	// Member Properties
	
	
	// Member Methods
	public void Update()
	{
		UpdateDUI();
	}
	
	public void UpdateDUI()
	{
		UpdatePropulsionformation();
		UpdateErrors();
	}
	
	public void UpdatePropulsionformation()
	{
		// Get the ship generation and generation potential
		float shipGeneration = CGameShips.Ship.GetComponent<CShipPropulsionSystem>().ShipCurentPropulsion;
		float shipGenerationPotential = CGameShips.Ship.GetComponent<CShipPropulsionSystem>().ShipPropulsionPotential;
		
		// Calculate the value ratio
		float value = shipGeneration/shipGenerationPotential;
		if(float.IsNaN(value)) 
			value = 0.0f;

		// Update the bar
		CDUIUtilites.LerpBarColor(value, m_PropulsionBar);
		m_PropulsionBar.value = value;
		
		// Update the lable
		m_PropulsionRate.color = CDUIUtilites.LerpColor(value);
		m_PropulsionRate.text = Mathf.RoundToInt(shipGeneration) + " / " + Mathf.RoundToInt(shipGenerationPotential);
		
//		// Update the positive/negative report
//		if(value < m_LastPropulsionValue)
//		{
//			m_PropulsionNegative.enabled = true;
//			m_PropulsionPositive.enabled = false;
//			m_PropulsionIdle.enabled = false;
//		}
//		else if(value > m_LastPropulsionValue)
//		{
//			m_PropulsionNegative.enabled = false;
//			m_PropulsionPositive.enabled = true;
//			m_PropulsionIdle.enabled = false;
//		}
//		else
//		{
//			m_PropulsionNegative.enabled = false;
//			m_PropulsionPositive.enabled = false;
//			m_PropulsionIdle.enabled = true;
//			m_PropulsionIdle.color = CDUIUtilites.LerpColor(value);
//		}
		
		m_LastPropulsionValue = value;
	}
	
	private void UpdateErrors()
	{
		// Update the error icons
		bool showingErrors = m_ShowErrors;
		if(m_LastPropulsionValue < 0.2f)
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