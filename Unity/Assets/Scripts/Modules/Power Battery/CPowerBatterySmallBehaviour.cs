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


[RequireComponent(typeof(CPowerBatteryInterface))]
public class CPowerBatterySmallBehaviour: MonoBehaviour 
{
// Member Types


// Member Delegates & Events


// Member Properties


	public int NumWorkingCircuitryComponents
	{
		get
		{
			int num = 0;

			num += m_Circuitry1.IsBroken ? 1 : 0;
			num += m_Circuitry2.IsBroken ? 1 : 0;

			return(num);
		}
	}


// Member Methods


	public void Start()
	{
		m_PowerStorage = gameObject.GetComponent<CPowerBatteryInterface>();

		// Register for when the circuitry breaks/fixes
		m_Circuitry1.EventBreakStateChange += OnEventComponentBreakStateChange;
		m_Circuitry2.EventBreakStateChange += OnEventComponentBreakStateChange;

        //begin the initial ambient sound
        gameObject.GetComponent<CAudioCue>().Play(0.13f, true, 0);
	}


	void OnEventComponentBreakStateChange(CComponentInterface _cSender, bool _bBroken)
	{
		if (CNetwork.IsServer)
		{
            /*
			int numWorkingComponents = NumWorkingCircuitryComponents;

			// Calculate the charge capacity
			m_PowerStorage.ChargeCapacity = m_MaxPowerBatteryCapacity * (float)numWorkingComponents / 2.0f;

			// Deactive the charge availablity
			if(numWorkingComponents == 0)
			{
				m_PowerStorage.DeactivateBatteryChargeAvailability();

                //stop the ambient sound from playing as the capacitor no longer works
                gameObject.GetComponent<CAudioCue>().StopAllSound();

                //turn off the light, should be a better place to do this. For example: Engine
                gameObject.GetComponentInChildren<Light>().intensity = (0.0f);
			}
			else
			{
                if (!m_PowerStorage.IsBatteryChargeAvailable)
                {
                    m_PowerStorage.ActivateBatteryChargeAvailability();

                    //change lighting depending on power
                    //gameObject.GetComponentInChildren<Light>().intensity = (m_PowerStorage.BatteryCharge / 150.0f);

                    //restart playing the ambient sound
                    gameObject.GetComponent<CAudioCue>().Play(0.13f, true, 0);
                }
			}
             * */
		}
	}


// Member Fields


    public float m_MaxPowerBatteryCapacity = 1000.0f;
    public CDUIConsole m_DUIConsole = null;

    public CComponentInterface m_Circuitry1 = null;
    public CComponentInterface m_Circuitry2 = null;


    CPowerBatteryInterface m_PowerStorage = null;
    CDUIPowerCapacitorRoot m_DUIPowerCapacitor = null;


}
