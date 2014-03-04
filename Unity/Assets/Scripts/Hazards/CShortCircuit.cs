//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShortCircuit.cs
//  Description :   --------------------------
//
//  Author  	:  Jade Abbott
//  Mail    	:  20chimps@gmail.com
//

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CModulePowerConsumption))]
[RequireComponent(typeof(CActorHealth))]
public class CShortCircuit : MonoBehaviour
{
	public bool shorting { get { return shorting_internal; } }
	private bool shorting_internal = false;
	private int audioClipIndex = -1;

	void Awake()
	{
		// Add components at runtime instead of updating all the prefabs.
		{
			// CAudioCue
			CAudioCue audioCue = gameObject.AddComponent<CAudioCue>();
			audioClipIndex = audioCue.AddSound("Audio/ShortCircuit", 0.0f, 0.0f, true);
		}
	}

	void Start()
	{
		GetComponent<CActorHealth>().EventOnSetState += OnSetState;
		//GetComponent<CModulePowerConsumption>().EventInsufficientPower += OnConsumptionToggle;
	}

	void OnDestroy()
	{
		GetComponent<CActorHealth>().EventOnSetState -= OnSetState;
		//GetComponent<CModulePowerConsumption>().EventInsufficientPower -= OnConsumptionToggle;
	}

	void OnSetState(byte prevState, byte currState)
	{
		switch (currState)
		{
			case 0:	// Begin short circuit.
				if(!shorting)
				{
					GetComponent<CAudioCue>().Play(transform, 1.0f, true, audioClipIndex);
					particleSystem.Play();
					shorting_internal = true;
					//GetComponent<CActorPowerConsumer>().SetAtmosphereConsumption(true);
				}
				break;

			case 2:	// End short circuit.
				if(shorting)
				{
					GetComponent<CAudioCue>().StopAllSound();
					particleSystem.Stop();
					shorting_internal = false;
					//GetComponent<CActorPowerConsumer>().SetAtmosphereConsumption(false);
				}
				break;
		}
	}

	void OnConsumptionToggle()
	{
		// Todo: Toggle short circuit.
	}

	void OnCollisionStay(Collision collisionInfo)
	{
		// Todo: Zap things with CActorHealth.
	}
}
