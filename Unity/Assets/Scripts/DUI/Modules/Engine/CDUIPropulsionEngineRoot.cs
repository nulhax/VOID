//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDUIPowerPropulsionRoot.cs
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


public class CDUIPropulsionEngineRoot : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public UIProgressBar m_PropulsionBar = null;
	public UILabel m_PropulsionRate = null;
	public UILabel m_PropulsionActive = null;
	public UILabel m_ErrorReport = null;
	public UILabel m_WarningReport = null;
	
	private GameObject m_Engine = null;
	private CPropulsionGeneratorBehaviour m_CachedPropulsionBehaviour = null;
	private CTestEngineBehaviour m_CachedEngine = null;
	
	// Member Properties
	
	
	// Member Methods
	public void RegisterPropulsionEngine(GameObject _Propulsion)
	{
		m_Engine = _Propulsion;
		m_CachedPropulsionBehaviour = m_Engine.GetComponent<CPropulsionGeneratorBehaviour>();
		m_CachedEngine = m_Engine.GetComponent<CTestEngineBehaviour>();
		
		// Register generation rate state chages
		m_CachedPropulsionBehaviour.EventPropulsionOutputChanged += HandlePropulsionStateChange;
		m_CachedPropulsionBehaviour.EventPropulsionPotentialChanged += HandlePropulsionStateChange;
		
		// Register for when the circuitry breaks/fixes
		m_CachedEngine.m_MechanicalComponent1.EventComponentBreak += HandleMechanicalStateChange;
		m_CachedEngine.m_MechanicalComponent2.EventComponentFix += HandleMechanicalStateChange;
		
		// Update initial values
		UpdateDUI();
	}
	
	private void HandlePropulsionStateChange(CPropulsionGeneratorBehaviour _Propulsion)
	{
		UpdateDUI();
	}
	
	private void HandleMechanicalStateChange(CComponentInterface _Component)
	{
		UpdateDUI();
	}
	
	private void UpdateDUI()
	{
		UpdatePropulsionVariables();
		UpdateStatus();
	}
	
	public void UpdatePropulsionVariables()
	{
		// Get the current generation current generation potential and detirmine the value
		float currentPropulsionRate = m_CachedPropulsionBehaviour.PropulsionForce;
		float currentPropulsionPotential = m_CachedPropulsionBehaviour.PropulsionPotential;
		float value = currentPropulsionRate/currentPropulsionPotential;
		
		// Update the generation value and bar color
		m_PropulsionBar.value = value;
		CDUIUtilites.LerpBarColor(value, m_PropulsionBar);
		
		// Update the lable
		m_PropulsionRate.color = CDUIUtilites.LerpColor(value);
		m_PropulsionRate.text = currentPropulsionRate.ToString() + " / " + currentPropulsionPotential.ToString();
		
		// Update the status report
		if(value <= 0.95f && value > 0.5f)
		{
			m_WarningReport.enabled = true;
			m_WarningReport.color = Color.yellow;
			m_WarningReport.text = "Warning: Mechanical maintenace required!";
		}
		else if(value <= 0.5f && value > 0.0f)
		{
			m_WarningReport.enabled = true;
			m_WarningReport.color = Color.red;
			m_WarningReport.text = "Warning: Mechanical maintenace required!";
		}
		else if(value == 0.0f)
		{
			m_WarningReport.enabled = true;
			m_WarningReport.color = Color.red;
			m_WarningReport.text = "Warning: Mechanical component defective!";
		}
		else
		{
			m_WarningReport.enabled = false;
		}
	}
	
	private void UpdateStatus()
	{
		if(!m_CachedEngine.m_MechanicalComponent1.IsFunctional && 
		   !m_CachedEngine.m_MechanicalComponent1.IsFunctional)
		{
			m_PropulsionActive.color = Color.red;
			m_PropulsionActive.text = "Status: Propulsion InActive";
			
			// Enable the status report
			m_ErrorReport.enabled = true;
			m_ErrorReport.text = "Warning: Mechanical components defective!";
		}
		else 
		{
			m_PropulsionActive.color = Color.green;
			m_PropulsionActive.text = "Status: Propulsion Active";
			
			// Disable the status report
			m_ErrorReport.enabled = false;
		}
	}
}
