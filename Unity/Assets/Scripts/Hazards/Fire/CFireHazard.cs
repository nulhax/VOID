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
	}

	void OnDestroy()
	{
		GetComponent<CActorHealth>().EventOnSetState -= OnSetState;
	}

	static void OnSetState(GameObject gameObject, byte prevState, byte currState)
	{
		switch (currState)
		{
			case 0:	// Begin fire.
				//gameObject.GetComponent<Collider>().enabled = true;
				gameObject.GetComponent<ParticleSystem>().Play();
				gameObject.GetComponent<CFireHazard>().burning = true;
				break;

			case 2:	// End fire.
				//gameObject.GetComponent<Collider>().enabled = false;
				ParticleSystem particleSystem = gameObject.GetComponent<ParticleSystem>();
				particleSystem.Stop();
				particleSystem.Clear();
				gameObject.GetComponent<CFireHazard>().burning = false;
				break;
		}
	}

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
