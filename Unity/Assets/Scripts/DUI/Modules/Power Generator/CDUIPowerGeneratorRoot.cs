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


	// Member Properties

	
	// Member Methods
	[AServerOnly]
	public void SetPowerGenerationRate(float _GenerationRate, float _MaximumGenerationRate)
	{
		float value = _GenerationRate/_MaximumGenerationRate;

		// Update the bar
		m_GenerationBar.backgroundWidget.color = Color.Lerp(Color.red * 0.8f, Color.cyan * 0.8f, value);
		m_GenerationBar.foregroundWidget.color = Color.Lerp(Color.red, Color.cyan, value);
		m_GenerationBar.value = value;

		// Update the lable
		m_GenerationRate.color = Color.Lerp(Color.red, Color.cyan, value);
		m_GenerationRate.text = _GenerationRate.ToString() + " / " + _MaximumGenerationRate.ToString();
	}
}
