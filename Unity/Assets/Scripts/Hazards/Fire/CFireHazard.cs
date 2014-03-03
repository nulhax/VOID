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
	//private System.Collections.Generic.List<GameObject> m_ThingsToBurn = new System.Collections.Generic.List<GameObject>();

	public bool burning { get { return burning_internal; } }
	private bool burning_internal = false;
	private int audioClipIndex = -1;
	
	void Awake()
	{
		if (particleSystem == null)
			Debug.LogError("FIX FIRE ON " + transform.parent.name);

		// Add components at runtime instead of updating all the prefabs.
		{
			// CAudioCue
			CAudioCue audioCue = gameObject.AddComponent<CAudioCue>();
			audioClipIndex = audioCue.AddSound("Audio/Fire/Fire", 0.0f, 0.0f, true);
		}
	}

	void Start()
	{
		GetComponent<CActorHealth>().EventOnSetState += OnSetState;
		GetComponent<CActorAtmosphericConsumer>().EventInsufficientAtmosphere += OnInsufficientAtmosphere;
	}

	void OnDestroy()
	{
		GetComponent<CActorHealth>().EventOnSetState -= OnSetState;
		GetComponent<CActorAtmosphericConsumer>().EventInsufficientAtmosphere -= OnInsufficientAtmosphere;
	}

	[AServerOnly]
	void Update()
	{
		if(CNetwork.IsServer && burning)
		{
			CFacilityAtmosphere fa = GetComponent<CActorLocator>().CurrentFacility.GetComponent<CFacilityAtmosphere>();
			CActorHealth ah = GetComponent<CActorHealth>();

			ah.health -= Time.deltaTime;	// Self damage over time. Seek help.

			float thresholdPercentage = 0.25f;
			if(fa.AtmosphereQuantityPercentage < thresholdPercentage)
                ah.health += (1.0f / (fa.AtmosphereQuantityPercentage / thresholdPercentage)) * Time.deltaTime;
		}
	}

	void OnSetState(byte prevState, byte currState)
	{
		switch (currState)
		{
			case 0:	// Begin fire.
				{
					GetComponent<CAudioCue>().Play(transform, 1.0f, true, audioClipIndex);
					particleSystem.Play();
					GetComponent<CFireHazard>().burning_internal = true;
					GetComponent<CActorAtmosphericConsumer>().SetAtmosphereConsumption(true);
				}
				break;

			case 2:	// End fire.
				{
					GetComponent<CAudioCue>().StopAllSound();
					particleSystem.Stop();
					GetComponent<CFireHazard>().burning_internal = false;
					GetComponent<CActorAtmosphericConsumer>().SetAtmosphereConsumption(false);
				}
				break;
		}
	}

	void OnInsufficientAtmosphere()
	{
		CActorHealth ah = GetComponent<CActorHealth>();
		ah.health = ah.health_max;
	}

	//[AServerOnly]
	//void OnTriggerEnter(Collider other)
	//{
	//    CActorHealth ah = other.GetComponent<CActorHealth>();
	//    if (ah != null)
	//        if (ah.flammable)
	//            m_ThingsToBurn.Add(other.gameObject);


	//}

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
