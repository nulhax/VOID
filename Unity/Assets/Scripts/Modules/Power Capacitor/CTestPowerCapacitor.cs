//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CTestPowerCapacitor.cs
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


[RequireComponent(typeof(CPowerStorageBehaviour))]
public class CTestPowerCapacitor: MonoBehaviour 
{
	// Member Types


	// Member Delegates & Events


	// Member Fields
	public float m_MaxPowerBatteryCapacity = 1000.0f;
	public CDUIConsole m_DUIConsole = null;

	public CComponentInterface m_Circuitry1 = null;
	public CComponentInterface m_Circuitry2 = null;

	private CPowerStorageBehaviour m_PowerStorage = null;
	private CDUIPowerCapacitorRoot m_DUIPowerCapacitor = null;


	// Member Properties
	public int NumWorkingCircuitryComponents
	{
		get
		{
			int num = 0;
			num += m_Circuitry1.IsFunctional ? 1 : 0;
			num += m_Circuitry2.IsFunctional ? 1 : 0;
			return(num);
		}
	}


	// Member Methods
	public void Start()
	{
		m_PowerStorage = gameObject.GetComponent<CPowerStorageBehaviour>();

		// Register for when the circuitry breaks/fixes
		m_Circuitry1.EventComponentBreak += HandleCircuitryStateChange;
		m_Circuitry1.EventComponentFix += HandleCircuitryStateChange;
		m_Circuitry2.EventComponentBreak += HandleCircuitryStateChange;
		m_Circuitry2.EventComponentFix += HandleCircuitryStateChange;

		// Get the DUI of the power generator
		m_DUIPowerCapacitor = m_DUIConsole.DUI.GetComponent<CDUIPowerCapacitorRoot>();
		m_DUIPowerCapacitor.RegisterPowerCapacitor(gameObject);

        //begin the initial ambient sound
        gameObject.GetComponent<CAudioCue>().Play(0.08f, true, 0);

		// Debug: Set the charge to half its total capacity
		if(CNetwork.IsServer)
		{
			m_PowerStorage.BatteryCapacity = m_MaxPowerBatteryCapacity;
			m_PowerStorage.BatteryCharge = m_PowerStorage.BatteryCapacity / 2;
		}
	}

	private void HandleCircuitryStateChange(CComponentInterface _Component)
	{
		if(CNetwork.IsServer)
		{
			int numWorkingComponents = NumWorkingCircuitryComponents;

			// Calculate the charge capacity
			m_PowerStorage.BatteryCapacity = m_MaxPowerBatteryCapacity * (float)numWorkingComponents / 2.0f;

			// Deactive the charge availablity
			if(numWorkingComponents == 0)
			{
				m_PowerStorage.DeactivateBatteryChargeAvailability();

                //stop the ambient sound from playing as the capacitor no longer works
                gameObject.GetComponent<CAudioCue>().StopAllSound();

                //turn off the light, should be a better place to do this. See: Engine
                gameObject.GetComponentInChildren<Light>().intensity = (0.0f);
			}
			else
			{
                if (!m_PowerStorage.IsBatteryChargeAvailable)
                {
                    m_PowerStorage.ActivateBatteryChargeAvailability();

                    //change lighting depending on power
                    //gameObject.GetComponentInChildren<Light>().intensity = (m_PowerStorage.BatteryCharge / 150.0f);

                    //rebegin playing the ambient sound
                    gameObject.GetComponent<CAudioCue>().Play(0.08f, true, 0);
                }
			}
		}
	}
}
