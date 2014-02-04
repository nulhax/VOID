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
	public delegate void NotifyDUIEvent();
	public event NotifyDUIEvent EventBuildModuleButtonPressed;
	
	
	// Member Fields
	public UIProgressBar m_GenerationBar = null;
	public UILabel m_GenerationRate = null;
	public UILabel m_GenerationActive = null;
	public UILabel m_ErrorReport = null;
	public UILabel m_WarningReport = null;

	private bool m_CircuitryBroken = false;
	private float m_CalibrationValue = 1.0f;

	// Member Properties

	
	// Member Methods
	public void SetPowerGenerationRate(float _GenerationRate, float _MaximumGenerationRate)
	{
		float value = _GenerationRate/_MaximumGenerationRate;
		m_CalibrationValue = value;

		// Update the bar
		m_GenerationBar.backgroundWidget.color = Color.Lerp(Color.red * 0.8f, Color.cyan * 0.8f, value);
		m_GenerationBar.foregroundWidget.color = Color.Lerp(Color.red, Color.cyan, value);
		m_GenerationBar.value = value;

		// Update the lable
		m_GenerationRate.color = Color.Lerp(Color.red, Color.cyan, value);
		m_GenerationRate.text = _GenerationRate.ToString() + " / " + _MaximumGenerationRate.ToString();

		// Update the status report
		if(value <= 0.95f && value > 0.5f)
		{
			m_WarningReport.UpdateVisibility(true);
			m_WarningReport.color = Color.yellow;
			m_WarningReport.text = "Warning: Calibration maintenace required!";
			m_WarningReport.GetComponent<TweenScale>().enabled = false;
		}
		else if(value <= 0.5f && value > 0.0f)
		{
			m_WarningReport.UpdateVisibility(true);
			m_WarningReport.color = Color.yellow;
			m_WarningReport.text = "Warning: Calibration maintenace required!";
			m_WarningReport.GetComponent<TweenScale>().enabled = true;
		}
		else if(value == 0.0f)
		{
			m_WarningReport.UpdateVisibility(true);
			m_WarningReport.color = Color.red;
			m_WarningReport.text = "Error: Calibration component defective!";
			m_WarningReport.GetComponent<TweenScale>().enabled = true;
		}
		else
		{
			m_WarningReport.UpdateVisibility(false);
			m_WarningReport.GetComponent<TweenScale>().enabled = false;
		}

		UpdateActiveLabel();
	}
	
	public void SetPowerGenerationActive(bool _Active)
	{
		m_CircuitryBroken = !_Active;

		if(!m_CircuitryBroken)
		{
			// Disable the status report
			m_ErrorReport.UpdateVisibility(false);
			
			//m_StatusReport.GetComponent<TweenScale>().
			m_ErrorReport.GetComponent<TweenScale>().enabled = false;
		}
		else 
		{
			// Enable the status report
			m_ErrorReport.UpdateVisibility(true);
			m_ErrorReport.color = Color.red;
			m_ErrorReport.text = "Error: Circuitry component defective!";
			m_ErrorReport.GetComponent<TweenScale>().enabled = true;
		}

		UpdateActiveLabel();
	}

	private void UpdateActiveLabel()
	{
		if(m_CircuitryBroken)
		{
			m_GenerationActive.color = Color.red;
			m_GenerationActive.text = "Status: Generation InActive";
		}
		else
		{
			if(m_CalibrationValue < 0.95f)
			{
				m_GenerationActive.color = Color.yellow;
				m_GenerationActive.text = "Status: Generation UnOptimal";

			}
			else if(m_CalibrationValue == 0.0f)
			{
				m_GenerationActive.color = Color.red;
				m_GenerationActive.text = "Status: Generation InActive";
			}
			else
			{
				m_GenerationActive.color = Color.green;
				m_GenerationActive.text = "Status: Generation Optimal";
			}
		}
	}
}
