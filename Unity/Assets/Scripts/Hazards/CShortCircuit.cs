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

[RequireComponent(typeof(CActorPowerConsumer))]
[RequireComponent(typeof(CActorHealth))]
[RequireComponent(typeof(Collider))]
public class CShortCircuit : MonoBehaviour
{
	public bool shorting { get { return shorting_internal; } }
	private bool shorting_internal = false;

	void Start()
	{
		GetComponent<CActorHealth>().EventOnSetState += OnSetState;
		GetComponent<CActorPowerConsumer>().EventConsumptionToggle += OnConsumptionToggle;
	}

	void OnDestroy()
	{
		GetComponent<CActorHealth>().EventOnSetState -= OnSetState;
		GetComponent<CActorPowerConsumer>().EventConsumptionToggle -= OnConsumptionToggle;
	}

	void OnSetState(byte prevState, byte currState)
	{
		switch (currState)
		{
			case 0:	// Begin short circuit.
				{
					particleSystem.Play();
					GetComponent<CShortCircuit>().shorting_internal = true;
					//GetComponent<CActorPowerConsumer>().SetAtmosphereConsumption(true);
				}
				break;

			case 2:	// End short circuit.
				{
					particleSystem.Stop();
					GetComponent<CShortCircuit>().shorting_internal = false;
					//GetComponent<CActorPowerConsumer>().SetAtmosphereConsumption(false);
				}
				break;
		}
	}

	void OnConsumptionToggle(bool consuming)
	{
		// Todo: Toggle short circuit.
	}

	void OnCollisionStay(Collision collisionInfo)
	{
		// Todo: Zap things with CActorHealth.
	}
}
