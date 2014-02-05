//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFireHazard.cs
//  Description :   --------------------------
//
//  Author  	:  Jade Abbott
//  Mail    	:  20chimps@gmail.com
//

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CActorHealth))]
[RequireComponent(typeof(Collider))]
public class CFireHazard : MonoBehaviour
{
	public bool burning = false;

	void Awake()
	{
		GetComponent<CActorHealth>().EventOnSetState += OnSetState;
		//GetComponent<CActorAtmosphericConsumer>().EventInsufficientAtmosphere += OnInsufficientAtmosphere;
	}

	void OnDestroy()
	{
		GetComponent<CActorHealth>().EventOnSetState -= OnSetState;
		//GetComponent<CActorAtmosphericConsumer>().EventInsufficientAtmosphere -= OnInsufficientAtmosphere;
	}

	void Update()
	{
		if(burning && CNetwork.IsServer)
		{
			CFacilityAtmosphere fa = GetComponent<CActorLocator>().LastEnteredFacility.GetComponent<CFacilityAtmosphere>();
			CActorHealth ah = GetComponent<CActorHealth>();

			float thresholdPercentage = 0.25f;
			if(fa.AtmospherePercentage < thresholdPercentage)
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
					ParticleSystem[] particleSystems = gameObject.GetComponentsInChildren<ParticleSystem>();
					foreach (ParticleSystem particleSystem in particleSystems)
					{
						particleSystem.Play();
					}

					gameObject.GetComponent<CFireHazard>().burning = true;
					gameObject.GetComponent<CActorAtmosphericConsumer>().SetAtmosphereConsumption(true);
				}
				break;

			case 2:	// End fire.
				{
					//gameObject.GetComponent<Collider>().enabled = false;
					ParticleSystem[] particleSystems = gameObject.GetComponentsInChildren<ParticleSystem>();
					foreach (ParticleSystem particleSystem in particleSystems)
					{
						particleSystem.Stop();
						particleSystem.Clear();
					}

					gameObject.GetComponent<CFireHazard>().burning = false;
					gameObject.GetComponent<CActorAtmosphericConsumer>().SetAtmosphereConsumption(false);
				}
				break;
		}
	}

//	void OnInsufficientAtmosphere()
//	{
//		CActorHealth ah = GetComponent<CActorHealth>();
//		ah.health = ah.health_max;
//	}

	void OnTriggerStay(Collider collider)
	{
		if (burning)
		{
			// Ignite players and other fires.
			CFireHazard otherFire = collider.GetComponent<CFireHazard>();
			if (otherFire != null)
				otherFire.GetComponent<CActorHealth>().health -= Time.fixedDeltaTime;
			else
			{
				CPlayerHealth otherPlayerhealth = collider.GetComponent<CPlayerHealth>();
				if (otherPlayerhealth != null)
					otherPlayerhealth.ApplyDamage(Time.fixedDeltaTime);
			}
		}
	}
}
