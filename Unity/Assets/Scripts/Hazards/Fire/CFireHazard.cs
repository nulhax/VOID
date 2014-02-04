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
	void Start ()
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
				gameObject.GetComponent<Collider>().enabled = true;
				gameObject.GetComponent<ParticleSystem>().Play();
				break;

			case 2:	// End fire.
				gameObject.GetComponent<Collider>().enabled = false;
				ParticleSystem particleSystem = gameObject.GetComponent<ParticleSystem>();
				particleSystem.Stop();
				particleSystem.Clear();
				break;
		}
	}

	void OnTriggerStay(Collider collider)
	{
		CActorHealth otherHealth = collider.GetComponent<CActorHealth>();
		if (otherHealth != null)
			otherHealth.health -= Time.fixedDeltaTime;
	}
}
