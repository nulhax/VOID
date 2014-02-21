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
		GetComponent<CFacilityPower>().EventFacilityInsufficientPower += OnInsufficientPower;
	}

	void OnDestroy()
	{
		GetComponent<CActorHealth>().EventOnSetState -= OnSetState;
		GetComponent<CFacilityPower>().EventFacilityInsufficientPower -= OnInsufficientPower;
	}

	[AServerOnly]
	void Update()
	{
		if (CNetwork.IsServer && shorting)
		{
			CFacilityAtmosphere fa = GetComponent<CActorLocator>().LastEnteredFacility.GetComponent<CFacilityAtmosphere>();
			CActorHealth ah = GetComponent<CActorHealth>();

			ah.health -= Time.deltaTime;	// Self damage over time. Seek help.

			float thresholdPercentage = 0.25f;
			if (fa.AtmospherePercentage < thresholdPercentage)
				ah.health += (1.0f / (fa.AtmospherePercentage / thresholdPercentage)) * Time.deltaTime;
		}
	}

	static void OnSetState(GameObject gameObject, byte prevState, byte currState)
	{
		switch (currState)
		{
			case 0:	// Begin fire.
				{
					//gameObject.GetComponent<Collider>().enabled = true;
					gameObject.particleSystem.Play();
					gameObject.GetComponent<CShortCircuit>().shorting_internal = true;
					gameObject.GetComponent<CActorPowerConsumer>().SetAtmosphereConsumption(true);
				}
				break;

			case 2:	// End fire.
				{
					//gameObject.GetComponent<Collider>().enabled = false;
					gameObject.particleSystem.Stop();
					gameObject.GetComponent<CShortCircuit>().shorting_internal = false;
					gameObject.GetComponent<CActorPowerConsumer>().SetAtmosphereConsumption(false);
				}
				break;
		}
	}

	void OnInsufficientAtmosphere()
	{
		CActorHealth ah = GetComponent<CActorHealth>();
		ah.health = ah.health_max;
	}

	[AServerOnly]
	void OnTriggerStay(Collider collider)
	{
		if (CNetwork.IsServer)
		{
			if (shorting)
			{
				// Damage everything within radius that is flammable.
				CActorHealth victimHealth = collider.GetComponent<CActorHealth>();
				if (victimHealth != null)
					if (victimHealth.flammable)
						victimHealth.health -= Time.fixedDeltaTime;
					else
					{
						// Damage players - they use their own health script.
						CPlayerHealth otherPlayerhealth = collider.GetComponent<CPlayerHealth>();
						if (otherPlayerhealth != null)
							otherPlayerhealth.ApplyDamage(Time.fixedDeltaTime);
					}
			}
		}
	}

	void OnParticleCollision(GameObject other)
	{
		if (CNetwork.IsServer)
		{
			CActorHealth otherHealth = other.GetComponent<CActorHealth>();
			if (otherHealth != null)
				if (otherHealth.flammable)
					otherHealth.health -= 10.0f;
		}
	}
}
