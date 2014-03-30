//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CTestNaniteCapsule.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


/* Implementation */


[RequireComponent(typeof(CNaniteStorageBehaviour))]
public class CTestNaniteCapsule: MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public int m_MaxNaniteCapacity = 1000;
	public CDUIConsole m_DUIConsole = null;
	
	public CComponentInterface m_CircuitryComp = null;
	public CComponentInterface m_CalibratorComp = null;
	
	private CNaniteStorageBehaviour m_NaniteStorage = null;
	private CDUINaniteCapsuleRoot m_DUINaniteCapsule = null;
	
	
	// Member Properties
	public int NumWorkingComponents
	{
		get
		{
			int num = 0;
			num += m_CircuitryComp.IsFunctional ? 1 : 0;
			num += m_CalibratorComp.IsFunctional ? 1 : 0;
			return(num);
		}
	}
	
	
	// Member Methods
	public void Start()
	{
		m_NaniteStorage = gameObject.GetComponent<CNaniteStorageBehaviour>();
		
		// Register for when the circuitry breaks/fixes
		m_CircuitryComp.EventComponentBreak += HandleComponentStateChange;
		m_CircuitryComp.EventComponentFix += HandleComponentStateChange;
		m_CalibratorComp.EventComponentBreak += HandleComponentStateChange;
		m_CalibratorComp.EventComponentFix += HandleComponentStateChange;
		
		// Get the DUI of the power generator
		m_DUINaniteCapsule = m_DUIConsole.DUIRoot.GetComponent<CDUINaniteCapsuleRoot>();
		m_DUINaniteCapsule.RegisterNaniteCapsule(gameObject);

		if(CNetwork.IsServer)
		{
			m_NaniteStorage.NaniteCapacity = m_MaxNaniteCapacity;
			m_NaniteStorage.StoredNanites = 0;
		}
	}
	
	private void HandleComponentStateChange(CComponentInterface _Component)
	{
		if(CNetwork.IsServer)
		{
			int numWorkingComponents = NumWorkingComponents;
			
			// Calculate the charge capacity
			// commented this out because it just adds nanites and not sure what it's supposed to be doing...
			// m_NaniteStorage.StoredNanites = m_MaxNaniteCapacity * (int)((float)numWorkingComponents / 2.0f);
			
			// Deactive the charge availablity
			if(numWorkingComponents == 0)
			{
				m_NaniteStorage.SetNaniteAvailability(false);
			}
			else
			{
				if(!m_NaniteStorage.IsStorageAvailable)
					m_NaniteStorage.SetNaniteAvailability(true);
			}
		}
	}
}
