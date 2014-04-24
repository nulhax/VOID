using UnityEngine;
using System.Collections;

public class CFireHazard : CNetworkMonoBehaviour
{
	private int audioClipIndex = -1;
	private float spreadRadius = 3.0f;
	private float emissionsPerUnitOfSurfaceArea = 1;
	private float emissionsPerUnitOfSurfaceAreaDiscrepancy = 0.05f;	// Variance percentage in particle emission rate.
	private float particleLifetime = 1.0f;
	private float particlelifetimeDiscrepancy = 0.05f;	// Variance percentage in particle lifetime.
	private System.Collections.Generic.List<GameObject> particleSystems = new System.Collections.Generic.List<GameObject>();
	private GameObject particleEmitterTemplate = null;
	float[] healthStateTransitions = { 1.0f, 20.0f };
	private CActorHealth_Embedded fireHealth = null;
	public CActorHealth_Embedded health { get { return fireHealth; } }

	private CFacilityAtmosphere cache_FacilityAtmosphere = null;
	private System.Collections.Generic.List<CActorHealth> cache_ActorHealths = new System.Collections.Generic.List<CActorHealth>();

	public bool burning { get { return burning_internal; } }
	private bool burning_internal = false;

	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		fireHealth = new CActorHealth_Embedded(gameObject, true, false, false, true, false, true, 25, 0, 25, 2, healthStateTransitions, 0.1f);
		fireHealth.InstanceNetworkVars(_cRegistrar);
	}

	void Awake()
	{
		gameObject.AddMissingComponent<CActorLocator>();
		gameObject.AddMissingComponent<CActorAtmosphericConsumer>();
		gameObject.AddMissingComponent<CNetworkView>();

		GetComponent<CActorAtmosphericConsumer>().AtmosphericConsumptionRate += 25.0f;

		particleEmitterTemplate = Resources.Load<GameObject>("Prefabs/Hazards/ParticleEmitter");
		AttachEmitterToChildren(gameObject);

		CAudioCue audioCue = gameObject.AddComponent<CAudioCue>();
		audioClipIndex = audioCue.AddSound("Audio/Fire/Fire", 0.0f, 0.0f, true);
	}

	void Start()
	{
		if (CNetwork.IsServer)
		{
			CActorLocator actorLocator = GetComponent<CActorLocator>();
			if (actorLocator != null)
			{
				GameObject currentFacility = actorLocator.CurrentFacility;
				if (currentFacility != null)
					cache_FacilityAtmosphere = currentFacility.GetComponent<CFacilityAtmosphere>();
			}

			cache_ActorHealths.AddRange(GetComponentsInChildren<CActorHealth>());
			cache_ActorHealths.AddRange(GetComponents<CActorHealth>());
		}

		fireHealth.Start();

		fireHealth.EventOnSetState += OnSetState;
		GetComponent<CActorAtmosphericConsumer>().EventInsufficientAtmosphere += OnInsufficientAtmosphere;
	}

	void OnDestroy()
	{
		fireHealth.EventOnSetState -= OnSetState;
		GetComponent<CActorAtmosphericConsumer>().EventInsufficientAtmosphere -= OnInsufficientAtmosphere;
	}
	
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.L))
			fireHealth.health = fireHealth.health_min;

		if (CNetwork.IsServer && burning)
		{
			if(cache_FacilityAtmosphere != null)
			{
				float thresholdPercentage = 0.25f;
				if (cache_FacilityAtmosphere.QuantityPercent < thresholdPercentage)
					fireHealth.health += (1.0f / (cache_FacilityAtmosphere.QuantityPercent / thresholdPercentage)) * Time.deltaTime;
			}

			fireHealth.health -= Time.deltaTime;	// Self damage over time. Seek help.

			foreach (CActorHealth actorHealth in cache_ActorHealths)
				actorHealth.health -= Time.deltaTime;
		}

		fireHealth.Update();
	}

	//void FixedUpdate()
	//{
	//    // Adjust particles.
	//    foreach (GameObject go in particleSystems)
	//    {
	//        Particle[] particles = go.particleEmitter.particles;
	//        foreach (Particle particle in particles)
	//        {
	//            // Particle.
	//        }

	//        go.particleEmitter.particles = particles;
	//    }
	//}

	void OnCollisionEnter(Collision collision)
	{
		fireHealth.OnCollisionEnter(collision);
	}

	void OnSetState(byte prevState, byte currState)
	{
		switch (currState)
		{
			case 0:	// Begin fire.
				{
					burning_internal = true;

					GetComponent<CAudioCue>().Play(transform, 1.0f, true, audioClipIndex);
					GetComponent<CActorAtmosphericConsumer>().SetAtmosphereConsumption(true);

					foreach (GameObject go in particleSystems)
						if (go != null)
							go.particleEmitter.emit = true;

				}
				break;

			case 2:	// End fire.
				{
					burning_internal = false;

					GetComponent<CAudioCue>().StopAllSound();
					GetComponent<CActorAtmosphericConsumer>().SetAtmosphereConsumption(false);

					foreach (GameObject go in particleSystems)
						if (go != null)
							go.particleEmitter.emit = false;
				}
				break;
		}
	}

	void OnInsufficientAtmosphere()
	{
		fireHealth.health = fireHealth.health_max;
	}

	void AttachEmitterToChildren(GameObject go)
	{
		foreach (Transform child in go.transform)
			AttachEmitterToChildren(child.gameObject);

		MeshFilter mf = go.GetComponent<MeshFilter>();
		if (mf == null)
			return;

		float emissionRate = emissionsPerUnitOfSurfaceArea * CUtility.GetMeshSurfaceArea(mf.sharedMesh, go.transform.lossyScale);

		// Manual creation/initialisation of particle system.
		for (int loopParticleSystem = 0; loopParticleSystem < 3; ++loopParticleSystem)
		{
			GameObject newParticleSystem = GameObject.Instantiate(particleEmitterTemplate) as GameObject;
			newParticleSystem.transform.parent = go.transform;
			newParticleSystem.transform.localPosition = Vector3.zero;
			newParticleSystem.transform.localRotation = Quaternion.identity;
			newParticleSystem.transform.localScale = Vector3.one;

			MeshFilter newMeshFilter = newParticleSystem.GetComponent<MeshFilter>();
			newMeshFilter.sharedMesh = mf.sharedMesh;

			{
				// ParticleRenderer.
				ParticleRenderer particleRenderer = newParticleSystem.GetComponent<ParticleRenderer>(); if (particleRenderer == null) particleRenderer = newParticleSystem.AddComponent<ParticleRenderer>();	// Get or create ParticleRenderer (there must be one).
				particleRenderer.castShadows = false;
				particleRenderer.receiveShadows = true;
				particleRenderer.sharedMaterial = Resources.Load<Material>("Materials/FireMaterial" + (loopParticleSystem + 1).ToString());	// One particle renderer per emitter.
				particleRenderer.useLightProbes = false;
				particleRenderer.cameraVelocityScale = 0.0f;
				particleRenderer.particleRenderMode = ParticleRenderMode.SortedBillboard;
				particleRenderer.lengthScale = 0.0f;
				particleRenderer.velocityScale = 0.0f;
				particleRenderer.maxParticleSize = 1e+10f;
				particleRenderer.maxPartileSize = 1e+10f;
				particleRenderer.uvAnimationCycles = 1.0f;
				particleRenderer.uvAnimationXTile = 1;
				particleRenderer.uvAnimationYTile = 1;
				//particleRenderer.uvTiles;0.333333333333333333333
			}

			{
				// ParticleAnimator.
				ParticleAnimator particleAnimator = newParticleSystem.GetComponent<ParticleAnimator>(); if (particleAnimator == null) particleAnimator = newParticleSystem.AddComponent<ParticleAnimator>();	// Get or create ParticleAnimator (there must be one).
				Color[] newColourAnimation = new Color[5];
				newColourAnimation[0] = new Color(1, 0, 0, 1);
				newColourAnimation[1] = new Color(0, 1, 0, 1);
				newColourAnimation[2] = new Color(0, 0, 1, 1);
				newColourAnimation[3] = new Color(1, 0, 1, 1);
				newColourAnimation[4] = new Color(1, 1, 0, 0);
				particleAnimator.colorAnimation = newColourAnimation;
				particleAnimator.doesAnimateColor = true;
			}

			{
				ParticleEmitter particleEmitter = newParticleSystem.GetComponent<ParticleEmitter>(); if (particleEmitter == null) particleEmitter = (ParticleEmitter)newParticleSystem.AddComponent("MeshParticleEmitter");	// Get or create ParticleEmitter (there must be one).
				particleEmitter.angularVelocity = 0.0f;
				particleEmitter.emit = false;	// Toggle this to control emission of particles.
				particleEmitter.emitterVelocityScale = 0.333f;	// Inherit ⅓ of the emitter's velocity.
				particleEmitter.enabled = true;
				particleEmitter.localVelocity = Vector3.zero;
				particleEmitter.maxEmission = emissionRate + emissionRate * emissionsPerUnitOfSurfaceAreaDiscrepancy;
				particleEmitter.maxEnergy = particleLifetime + particleLifetime * particlelifetimeDiscrepancy;
				particleEmitter.maxSize = /*1e+10f*/1.0f;
				particleEmitter.minEmission = emissionRate - emissionRate * emissionsPerUnitOfSurfaceAreaDiscrepancy;
				particleEmitter.minEnergy = particleLifetime - particleLifetime * particlelifetimeDiscrepancy;
				particleEmitter.minSize = /*0.0f*/1.0f;
				particleEmitter.rndAngularVelocity = 90.0f;
				particleEmitter.rndRotation = true;
				particleEmitter.rndVelocity = new Vector3(1.0f, 1.0f, 1.0f);
				particleEmitter.useWorldSpace = true;
				// Interpolating triangles is not accessible by script, which is why a ParticleEmitter should already exist on ParticleEmitter.prefab with the setting enabled.
			}

			particleSystems.Add(newParticleSystem);
		}
	}
}
