//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDUIAtmosphereGeneratorRoot.cs
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


public class CDUIAtmosphereGeneratorRoot : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public UIProgressBar m_GenerationBar = null;
	public UILabel m_GenerationRate = null;
	public UILabel m_GenerationActive = null;
	public UILabel m_ErrorReport = null;
	public UILabel m_WarningReport = null;
	
	private GameObject m_AtmosphereGenerator = null;
	private CAtmosphereGeneratorBehaviour m_CachedAtmosphereGeneratorBehaviour = null;
	private CTestAtmosphereGenerator m_CachedAtmosphereGenerator = null;


	// Member Properties
	private bool IsCircuitryFunctional
	{
		get { return(m_CachedAtmosphereGenerator.m_CircuitryComponent.IsFunctional); }
	}

	
	// Member Methods
	public void RegisterAtmosphereGenerator(GameObject _AtmosphereGenerator)
	{
		m_AtmosphereGenerator = _AtmosphereGenerator;
		m_CachedAtmosphereGeneratorBehaviour = m_AtmosphereGenerator.GetComponent<CAtmosphereGeneratorBehaviour>();
		m_CachedAtmosphereGenerator = m_AtmosphereGenerator.GetComponent<CTestAtmosphereGenerator>();
		
		// Register generation rate state chages
		m_CachedAtmosphereGeneratorBehaviour.EventGenerationRateChanged += HandleGenerationRateStateChange;
		
		// Register for when the circuitry breaks/fixes
		m_CachedAtmosphereGenerator.m_CircuitryComponent.EventComponentBreak += HandleCircuitryStateChange;
		m_CachedAtmosphereGenerator.m_CircuitryComponent.EventComponentFix += HandleCircuitryStateChange;
		
		// Update initial values
		UpdateDUI();
	}
	
	private void HandleGenerationRateStateChange(CAtmosphereGeneratorBehaviour _Generator)
	{
		UpdateDUI();
	}
	
	private void HandleCircuitryStateChange(CComponentInterface _Component)
	{
		UpdateDUI();
	}
	
	private void UpdateDUI()
	{
		UpdateGeneratorVariables();
		UpdateCircuitryState();
	}

	private void UpdateGeneratorVariables()
	{
		// Get the current generation current generation potential and detirmine the value
		float currentGenerationRate = m_CachedAtmosphereGeneratorBehaviour.AtmosphereGenerationRate;
		float currentGenerationRatePotential = m_CachedAtmosphereGenerator.m_MaxAtmosphereGenerationRate;
		float value = currentGenerationRate/currentGenerationRatePotential;
		
		// Update the generation value and bar color
		m_GenerationBar.value = value;
		CDUIUtilites.LerpBarColor(value, m_GenerationBar);
		
		// Update the lable
		m_GenerationRate.color = CDUIUtilites.LerpColor(value);
		m_GenerationRate.text = currentGenerationRate.ToString() + " / " + currentGenerationRatePotential.ToString();
		
		// Update the status report
		if(value <= 0.95f && value > 0.5f)
		{
			m_WarningReport.enabled = true;
			m_WarningReport.color = Color.yellow;
			m_WarningReport.text = "Warning: Fluid maintenace required!";
		}
		else if(value <= 0.5f && value > 0.0f)
		{
			m_WarningReport.enabled = true;
			m_WarningReport.color = Color.red;
			m_WarningReport.text = "Warning: Fluid maintenace required!";
		}
		else if(value == 0.0f)
		{
			m_WarningReport.enabled = true;
			m_WarningReport.color = Color.red;
			m_WarningReport.text = "Warning: Fluid component defective!";
		}
		else
		{
			m_WarningReport.enabled = false;
		}
	}
	
	private void UpdateCircuitryState()
	{
		if(IsCircuitryFunctional)
		{
			m_GenerationActive.color = Color.green;
			m_GenerationActive.text = "Status: Generation Active";
			
			// Disable the status reporting
			m_ErrorReport.enabled = false;
		}
		else 
		{
			m_GenerationActive.color = Color.red;
			m_GenerationActive.text = "Status: Generation InActive";
			
			// Enable the status report
			m_ErrorReport.enabled = true;
			m_ErrorReport.text = "Warning: Circuitry component defective!";
		}
	}
}
