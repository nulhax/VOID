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


public class CDUIPowerCapacitorRoot : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public UIProgressBar m_CapacityBar = null;
	public UILabel m_Charge = null;
	public UILabel m_CapacitorStatus = null;
	public UILabel m_ErrorReport = null;
	public UILabel m_WarningReport = null;
	
	private bool[] m_CircuitryBroken = new bool[2];
	private float m_InitialCapacity = 0.0f;
	private float m_CurrentCharge = 0.0f;
	private float m_CurrentMaximumCapacity = 0.0f;
	
	// Member Properties
	public float InitialCapacity
	{
		set { m_InitialCapacity = value; }
	}
	
	// Member Methods
	public void Start()
	{
		m_CircuitryBroken[0] = true;
		m_CircuitryBroken[1] = true;
	}

	public void UpdateCapacitorVariables(float _CurrentCharge, float _MaximumCapacity)
	{
		m_CurrentCharge = _CurrentCharge;
		m_CurrentMaximumCapacity = _MaximumCapacity;

		float value = m_CurrentMaximumCapacity/m_InitialCapacity;
		
		// Update the bar colors
		m_CapacityBar.backgroundWidget.color = Color.Lerp(Color.red * 0.8f, Color.cyan * 0.8f, value);
		m_CapacityBar.foregroundWidget.color = Color.Lerp(Color.red, Color.cyan, value);

		// Update the charge value
		m_CapacityBar.value = _CurrentCharge/_MaximumCapacity;
		
		// Update the label
		m_Charge.text = _CurrentCharge.ToString() + " / " + _MaximumCapacity.ToString();
	}

	public void SetCircuitryStateChange(int _Index, bool _State)
	{
		m_CircuitryBroken[_Index] = _State;

		int active = 0;
		foreach(bool b in m_CircuitryBroken)
			if(b) active += 1;

		if(active == 0)
		{
			m_Charge.color = Color.red;
			m_CapacitorStatus.color = Color.red;
			m_CapacitorStatus.text = "Status: Charge InActive";

			m_WarningReport.UpdateVisibility(false);
			m_ErrorReport.UpdateVisibility(true);

			m_ErrorReport.color = Color.red;
			m_ErrorReport.text = "Error: All circuitry components defective!";
			m_ErrorReport.GetComponent<UITweener>().Play();
		}
		else if(active == 1)
		{
			m_Charge.color = Color.yellow;
			m_CapacitorStatus.color = Color.yellow;
			m_CapacitorStatus.text = "Status: Charge NonOptimal";

			m_WarningReport.UpdateVisibility(true);
			m_ErrorReport.UpdateVisibility(false);

			m_WarningReport.color = Color.yellow;
			m_WarningReport.text = "Warning: Circuitry component defective!";
			m_WarningReport.GetComponent<UITweener>().Play();
		}
		else
		{
			m_Charge.color = Color.cyan;
			m_CapacitorStatus.color = Color.green;
			m_CapacitorStatus.text = "Status: Charge Active";

			m_ErrorReport.UpdateVisibility(false);
			m_WarningReport.UpdateVisibility(false);
		}
	}
}
