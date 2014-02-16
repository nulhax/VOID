﻿//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDUINaniteGeneratorRoot.cs
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


public class CDUINaniteCapsuleRoot : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public UIProgressBar m_CapacityBar = null;
	public UILabel m_Nanites = null;
	public UILabel m_CapsuleStatus = null;
	public UILabel m_ErrorReport = null;
	public UILabel m_WarningReport = null;
	
	private GameObject m_NaniteCapsule = null;
	private CNaniteStorageBehaviour m_CachedNaniteStorageBehaviour = null;
	private CTestNaniteCapsule m_CachedNaniteCapsule = null;
	
	
	// Member Properties
	
	
	// Member Methods
	public void RegisterNaniteCapsule(GameObject _NaniteCapacitor)
	{
		m_NaniteCapsule = _NaniteCapacitor;
		m_CachedNaniteStorageBehaviour = m_NaniteCapsule.GetComponent<CNaniteStorageBehaviour>();
		m_CachedNaniteCapsule = m_NaniteCapsule.GetComponent<CTestNaniteCapsule>();

		// Register charge/capacity state chages
		m_CachedNaniteStorageBehaviour.EventNaniteStorageChanged += HandleCapsuleStateChange;
		m_CachedNaniteStorageBehaviour.EventNaniteCapacityChanged += HandleCapsuleStateChange;
		
		// Register for when the circuitry breaks/fixes
		m_CachedNaniteCapsule.m_CircuitryComp.EventComponentBreak += HandleComponentStateChange;
		m_CachedNaniteCapsule.m_CircuitryComp.EventComponentFix += HandleComponentStateChange;
		m_CachedNaniteCapsule.m_CalibratorComp.EventComponentBreak += HandleComponentStateChange;
		m_CachedNaniteCapsule.m_CalibratorComp.EventComponentFix += HandleComponentStateChange;
		
		// Update initial values
		UpdateDUI();
	}
	
	private void HandleCapsuleStateChange(CNaniteStorageBehaviour _Capsule)
	{
		UpdateDUI();
	}
	
	private void HandleComponentStateChange(CComponentInterface _Component)
	{
		UpdateDUI();
	}
	
	private void UpdateDUI()
	{
		UpdateCapacitorVariables();
		UpdateCircuitryStates();
	}
	
	private void UpdateCapacitorVariables()
	{
		// Get the current charge, intial capacity and current capacity
		float currentNanites = m_CachedNaniteStorageBehaviour.StoredNanites;
		float currentCapacity = m_CachedNaniteStorageBehaviour.NaniteCapacity;
		float initialCapacity = m_CachedNaniteCapsule.m_MaxNaniteCapacity;
		
		// Update the charge value
		m_CapacityBar.value = currentNanites/currentCapacity;
		
		// Update the bar colors
		float value = currentCapacity/initialCapacity;
		CDUIUtilites.LerpBarColor(value, m_CapacityBar);
		
		// Update the label
		m_Nanites.text = currentNanites.ToString() + " / " + currentCapacity.ToString();
	}
	
	private void UpdateCircuitryStates()
	{
		int numWorkingComponents = m_CachedNaniteCapsule.NumWorkingComponents;
		
		if(numWorkingComponents == 0)
		{
			m_Nanites.color = Color.red;
			m_CapsuleStatus.color = Color.red;
			m_CapsuleStatus.text = "Status: Storage Unavailable";
			
			m_WarningReport.enabled = false;
			m_ErrorReport.enabled = true;
			
			m_ErrorReport.color = Color.red;
			m_ErrorReport.text = "Warning: Fluid & calibration component defective!";
		}
		else if(numWorkingComponents == 1)
		{
			m_Nanites.color = Color.yellow;
			m_CapsuleStatus.color = Color.yellow;
			m_CapsuleStatus.text = "Status: Storage NonOptimal";
			
			m_WarningReport.enabled = true;
			m_ErrorReport.enabled = false;
			
			m_WarningReport.color = Color.yellow;
			m_WarningReport.text = "Warning: Component defective!";
		}
		else
		{
			m_Nanites.color = Color.cyan;
			m_CapsuleStatus.color = Color.green;
			m_CapsuleStatus.text = "Status: Storage Available";
			
			m_WarningReport.enabled = false;
			m_ErrorReport.enabled = false;
		}
	}
}