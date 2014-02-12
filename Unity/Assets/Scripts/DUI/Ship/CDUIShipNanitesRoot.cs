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


[RequireComponent(typeof(CNetworkView))]
public class CDUIShipNanitesRoot : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public UISprite m_NanitesNegative = null;
	public UISprite m_NanitesPositive = null;
	public UISprite m_NanitesIdle = null;
	
	public UIProgressBar m_NanitesBar = null;
	public UILabel m_NanitesRate = null;
	
	private float m_LastNanitesValue = 0.0f;
	
	
	// Member Properties
	
	
	// Member Methods
	public void Update()
	{
		UpdateDUI();
	}
	
	public void UpdateDUI()
	{
		UpdateNanitesformation();
	}
	
	public void UpdateNanitesformation()
	{
		// Get the ship generation and generation potential
		int shipNanites = CGameShips.Ship.GetComponent<CShipNaniteSystem>().ShipCurentNanites;
		int shipNanitesPotential = CGameShips.Ship.GetComponent<CShipNaniteSystem>().ShipNanitesPotential;
		
		// Calculate the value ratio
		float value = (float)shipNanites/(float)shipNanitesPotential;
		
		// Update the bar
		CDUIUtilites.LerpBarColor(value, m_NanitesBar);
		m_NanitesBar.value = value;
		
		// Update the lable
		m_NanitesRate.color = CDUIUtilites.LerpColor(value);
		m_NanitesRate.text = shipNanites + " / " + shipNanitesPotential;
		
		// Update the positive/negative report
		if(value < m_LastNanitesValue)
		{
			m_NanitesNegative.enabled = true;
			m_NanitesPositive.enabled = false;
			m_NanitesIdle.enabled = false;
		}
		else if(value > m_LastNanitesValue)
		{
			m_NanitesNegative.enabled = false;
			m_NanitesPositive.enabled = true;
			m_NanitesIdle.enabled = false;
		}
		else
		{
			m_NanitesNegative.enabled = false;
			m_NanitesPositive.enabled = false;
			m_NanitesIdle.enabled = true;
			m_NanitesIdle.color = CDUIUtilites.LerpColor(value);
		}
		
		m_LastNanitesValue = value;
	}
}