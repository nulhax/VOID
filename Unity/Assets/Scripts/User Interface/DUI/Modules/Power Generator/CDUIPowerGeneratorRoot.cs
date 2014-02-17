//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDUIPowerGeneratorRoot.cs
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


public class CDUIPowerGeneratorRoot : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public UIProgressBar m_GenerationBar = null;
	public UILabel m_GenerationRate = null;
	public UILabel m_GenerationActive = null;
	public UILabel m_ErrorReport = null;
	public UILabel m_WarningReport = null;

	private GameObject m_PowerGenerator = null;
	private CPowerGenerationBehaviour m_CachedPowerGeneratorBehaviour = null;
	private CTestPowerGenerator m_CachedPowerGenerator = null;

	// Member Properties

	
	// Member Methods
	public void RegisterPowerGenerator(GameObject _PowerGenerator)
	{
		m_PowerGenerator = _PowerGenerator;
		m_CachedPowerGeneratorBehaviour = m_PowerGenerator.GetComponent<CPowerGenerationBehaviour>();
		m_CachedPowerGenerator = m_PowerGenerator.GetComponent<CTestPowerGenerator>();
		
		// Register generation rate state chages
		m_CachedPowerGeneratorBehaviour.EventGenerationRateChanged += HandleGenerationStateChange;
		m_CachedPowerGeneratorBehaviour.EventGenerationRatePotentialChanged += HandleGenerationStateChange;

		// Register for when the circuitry breaks/fixes
		m_CachedPowerGenerator.m_CircuitryComponent.EventComponentBreak += HandleCircuitryStateChange;
		m_CachedPowerGenerator.m_CircuitryComponent.EventComponentFix += HandleCircuitryStateChange;
		
		// Update initial values
		UpdateDUI();
	}

	private void HandleGenerationStateChange(CPowerGenerationBehaviour _Generator)
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

	public void UpdateGeneratorVariables()
	{
		// Get the current generation current generation potential and detirmine the value
		float currentGenerationRate = m_CachedPowerGeneratorBehaviour.PowerGenerationRate;
		float currentGenerationRatePotential = m_CachedPowerGeneratorBehaviour.PowerGenerationRatePotential;
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
			m_WarningReport.text = "Warning: Calibration maintenace required!";
		}
		else if(value <= 0.5f && value > 0.0f)
		{
			m_WarningReport.enabled = true;
			m_WarningReport.color = Color.red;
			m_WarningReport.text = "Warning: Calibration maintenace required!";
		}
		else if(value == 0.0f)
		{
			m_WarningReport.enabled = true;
			m_WarningReport.color = Color.red;
			m_WarningReport.text = "Warning: Calibration component defective!";
		}
		else
		{
			m_WarningReport.enabled = false;
		}
	}
	
	private void UpdateCircuitryState()
	{
		if(m_CachedPowerGenerator.m_CircuitryComponent.IsFunctional)
		{
			m_GenerationActive.color = Color.green;
			m_GenerationActive.text = "Status: Generation Active";

			// Disable the status report
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
