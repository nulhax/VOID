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

[RequireComponent(typeof(CActorAtmosphericConsumer))]
[RequireComponent(typeof(CActorHealth))]
[RequireComponent(typeof(Collider))]
public class CFireHazard : MonoBehaviour
{
	//private static System.Collections.Generic.List<CFireHazard> s_AllFires = new System.Collections.Generic.List<CFireHazard>();

	//public float spreadRadius = 3.0f;

	//public float timeBetweenSpreadProcess = 1.0f;
	//private float timeUntilNextSpreadProcess = 0.0f;

	public bool burning { get { return burning_internal; } }
	private bool burning_internal = false;

	void Awake()
	{
		if (GetComponentsInChildren<ParticleSystem>() != null)
			Debug.LogError("Replace Fire hazard on " + transform.parent.gameObject.name);
	}

	void Start()
	{
		//s_AllFires.Add(this);

		GetComponent<CActorHealth>().EventOnSetState += OnSetState;
		GetComponent<CActorAtmosphericConsumer>().EventInsufficientAtmosphere += OnInsufficientAtmosphere;
	}

	void OnDestroy()
	{
		//s_AllFires.Remove(this);

		GetComponent<CActorHealth>().EventOnSetState -= OnSetState;
		GetComponent<CActorAtmosphericConsumer>().EventInsufficientAtmosphere -= OnInsufficientAtmosphere;
	}

	[AServerOnly]
	void Update()
	{
		if(CNetwork.IsServer && burning)
		{
			CFacilityAtmosphere fa = GetComponent<CActorLocator>().LastEnteredFacility.GetComponent<CFacilityAtmosphere>();
			CActorHealth ah = GetComponent<CActorHealth>();

			ah.health -= Time.deltaTime;	// Self damage over time. Seek help.

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
					gameObject.GetComponent<ParticleSystem>().Play();
					gameObject.GetComponent<CFireHazard>().burning_internal = true;
					gameObject.GetComponent<CActorAtmosphericConsumer>().SetAtmosphereConsumption(true);
				}
				break;

			case 2:	// End fire.
				{
					//gameObject.GetComponent<Collider>().enabled = false;
					gameObject.GetComponent<ParticleSystem>().Stop();
					gameObject.GetComponent<CFireHazard>().burning_internal = false;
					gameObject.GetComponent<CActorAtmosphericConsumer>().SetAtmosphereConsumption(false);
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
		if(CNetwork.IsServer)
		{
			if (burning)
			{
				// Damage everything within radius that is flammable.
				CActorHealth victimHealth = collider.GetComponent<CActorHealth>();
				if (victimHealth != null)
					if(victimHealth.flammable)
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
