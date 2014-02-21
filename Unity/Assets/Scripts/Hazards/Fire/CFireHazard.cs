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

[RequireComponent(typeof(CActorPowerConsumer))]
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
		//if (Random.Range(0, 1) == 0)
		//    return;

		//// Manual creation/initialisation of particle system.
		//{
		//    // MeshFilter used by ParticleEmitter for emitting particles along the surface of a mesh.
		//    MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>(); if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();	// Get or create MeshFilter (there must be one).

		//    // Inherit mesh from parent if there isn't one already.
		//    if (meshFilter.sharedMesh == null/* && inheritMesh*/)
		//    {
		//        Transform parentTransform = transform.parent;
		//        if (parentTransform != null)
		//        {
		//            MeshFilter parentMeshFilter = parentTransform.GetComponent<MeshFilter>();
		//            if (parentMeshFilter != null)
		//                meshFilter.sharedMesh = parentMeshFilter.sharedMesh;
		//        }
		//    }

		//    // If no current mesh and no parent mesh - use quad.
		//    if (meshFilter.sharedMesh == null)
		//        meshFilter.sharedMesh = Resources.Load<Mesh>("Quad");	// Todo: Rotate by 90 degrees if quad is vertical.
		//}

		//{
		//    // ParticleRenderer.
		//    ParticleRenderer particleRenderer = gameObject.GetComponent<ParticleRenderer>(); if (particleRenderer == null) particleRenderer = gameObject.AddComponent<ParticleRenderer>();	// Get or create ParticleRenderer (there must be one).
		//    particleRenderer.castShadows = false;
		//    particleRenderer.receiveShadows = true;
		//    particleRenderer.sharedMaterial = Resources.Load<Material>("Materials/FireMaterial");
		//    particleRenderer.useLightProbes = false;
		//    particleRenderer.cameraVelocityScale = 0.0f;
		//    particleRenderer.particleRenderMode = ParticleRenderMode.SortedBillboard;
		//    particleRenderer.lengthScale = 1.0f;
		//    particleRenderer.velocityScale = 0.0f;
		//    particleRenderer.maxParticleSize = 1e+10f;
		//    particleRenderer.maxPartileSize = 1e+10f;
		//    particleRenderer.uvAnimationCycles = 1.0f;
		//    particleRenderer.uvAnimationXTile = 1;
		//    particleRenderer.uvAnimationYTile = 3;
		//    //particleRenderer.uvTiles;0.333333333333333333333
		//}

		//{
		//    // ParticleAnimator.
		//    ParticleAnimator particleAnimator = gameObject.GetComponent<ParticleAnimator>(); if (particleAnimator == null) particleAnimator = gameObject.AddComponent<ParticleAnimator>();	// Get or create ParticleAnimator (there must be one).
		//    particleAnimator.doesAnimateColor = true;
		//    particleAnimator.colorAnimation = new Color[3];
		//    particleAnimator.colorAnimation[0] = Color.red;
		//    particleAnimator.colorAnimation[1] = Color.green;
		//    particleAnimator.colorAnimation[2] = Color.blue;
		//}
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

	void OnSetState(byte prevState, byte currState)
	{
		switch (currState)
		{
			case 0:	// Begin fire.
				{
					//GetComponent<Collider>().enabled = true;
					particleSystem.Play();
					GetComponent<CFireHazard>().burning_internal = true;
					GetComponent<CActorAtmosphericConsumer>().SetAtmosphereConsumption(true);
				}
				break;

			case 2:	// End fire.
				{
					//GetComponent<Collider>().enabled = false;
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
