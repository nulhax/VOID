using UnityEngine;
using System.Collections;

public class CFireHazard_New : MonoBehaviour
{
	public bool inheritMeshFromParent = false;

	private float spreadRadius = 3.0f;
	private float particleEmissionsPerSecond = 30;
	private float particleEmissionsPerSecondDiscrepancy = 0.05f;	// Variance percentage in particle emission rate.
	private float particleLifetime = 1.0f;
	private float particlelifetimeDiscrepancy = 0.05f;	// Variance percentage in particle lifetime.
	System.Collections.Generic.List<GameObject> particleSystems = new System.Collections.Generic.List<GameObject>();

	void Awake()
	{
		GameObject newParticleSystemTemplate = Resources.Load<GameObject>("Prefabs/Hazards/ParticleEmitter");

		// Manual creation/initialisation of particle system.
		for (int loopParticleSystem = 0; loopParticleSystem < 3; ++loopParticleSystem)
		{
			GameObject newParticleSystem = GameObject.Instantiate(newParticleSystemTemplate) as GameObject;
			newParticleSystem.transform.parent = transform;
			newParticleSystem.transform.localPosition = Vector3.zero;
			newParticleSystem.transform.localRotation = Quaternion.identity;
			newParticleSystem.transform.localScale = Vector3.zero;

			{
				// MeshFilter used by ParticleEmitter for emitting particles along the surface of a mesh.
				MeshFilter meshFilter = newParticleSystem.GetComponent<MeshFilter>(); if (meshFilter == null) meshFilter = newParticleSystem.AddComponent<MeshFilter>();	// Get or create MeshFilter (there must be one).
				MeshFilter firehazardMeshFilter = GetComponent<MeshFilter>(); if (firehazardMeshFilter != null) meshFilter.sharedMesh = firehazardMeshFilter.sharedMesh;	// Assign mesh where the CFireHazard script is.

				// Inherit mesh from parent if there isn't one already.
				if (meshFilter.sharedMesh == null && inheritMeshFromParent)
				{
					Transform parentTransform = transform.parent;
					if (parentTransform != null)
					{
						MeshFilter parentMeshFilter = parentTransform.GetComponent<MeshFilter>();
						if (parentMeshFilter != null)
							meshFilter.sharedMesh = parentMeshFilter.sharedMesh;
					}
				}

				// If no current mesh and no parent mesh - use quad.
				if (meshFilter.sharedMesh == null)
				{
					meshFilter.sharedMesh = Resources.Load<Mesh>("Quad");
					newParticleSystem.transform.Rotate(90, 0, 0);
				}
			}

			{
				// ParticleRenderer.
				ParticleRenderer particleRenderer = newParticleSystem.GetComponent<ParticleRenderer>(); if (particleRenderer == null) particleRenderer = newParticleSystem.AddComponent<ParticleRenderer>();	// Get or create ParticleRenderer (there must be one).
				particleRenderer.castShadows = false;
				particleRenderer.receiveShadows = true;
				particleRenderer.sharedMaterial = Resources.Load<Material>("Materials/FireMaterial" + (loopParticleSystem+1).ToString());	// One particle renderer per emitter.
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
				particleEmitter.maxEmission = particleEmissionsPerSecond + particleEmissionsPerSecond * particleEmissionsPerSecondDiscrepancy;
				particleEmitter.maxEnergy = particleLifetime + particleLifetime * particlelifetimeDiscrepancy;
				particleEmitter.maxSize = /*1e+10f*/1.0f;
				particleEmitter.minEmission = particleEmissionsPerSecond - particleEmissionsPerSecond * particleEmissionsPerSecondDiscrepancy;
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

	void Start()
	{
		foreach (GameObject go in particleSystems)
			go.particleEmitter.emit = true;
	}
	
	void Update()
	{

	}

	void FixedUpdate()
	{
		//// Adjust particles.
		//foreach (GameObject go in particleSystems)
		//{
		//    Particle[] particles = go.particleEmitter.particles;
		//    foreach (Particle particle in particles)
		//    {
		//        // Particle.
		//    }

		//    go.particleEmitter.particles = particles;
		//}
	}
}
