using UnityEngine;
using System.Collections;

public class CFireHazard_New : MonoBehaviour
{
	public float spreadRadius = 3.0f;

	void Awake()
	{
		// Manual creation/initialisation of particle system.
		{
			// MeshFilter used by ParticleEmitter for emitting particles along the surface of a mesh.
			MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>(); if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();	// Get or create MeshFilter (there must be one).

			// Inherit mesh from parent if there isn't one already.
			if (meshFilter.sharedMesh == null/* && inheritMesh*/)
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
				meshFilter.sharedMesh = Resources.Load<Mesh>("Quad");	// Todo: Rotate by 90 degrees if quad is vertical.
		}

		{
			// ParticleRenderer.
			ParticleRenderer particleRenderer = gameObject.GetComponent<ParticleRenderer>(); if (particleRenderer == null) particleRenderer = gameObject.AddComponent<ParticleRenderer>();	// Get or create ParticleRenderer (there must be one).
			particleRenderer.castShadows = false;
			particleRenderer.receiveShadows = true;
			particleRenderer.sharedMaterial = Resources.Load<Material>("Materials/FireMaterial1");	// One particle renderer per emitter.
			particleRenderer.useLightProbes = false;
			particleRenderer.cameraVelocityScale = 0.0f;
			particleRenderer.particleRenderMode = ParticleRenderMode.SortedBillboard;
			particleRenderer.lengthScale = 1.0f;
			particleRenderer.velocityScale = 0.0f;
			particleRenderer.maxParticleSize = 1e+10f;
			particleRenderer.maxPartileSize = 1e+10f;
			particleRenderer.uvAnimationCycles = 1.0f;
			particleRenderer.uvAnimationXTile = 1;
			particleRenderer.uvAnimationYTile = 3;
			//particleRenderer.uvTiles;0.333333333333333333333
		}

		{
			// ParticleAnimator.
			ParticleAnimator particleAnimator = gameObject.GetComponent<ParticleAnimator>(); if (particleAnimator == null) particleAnimator = gameObject.AddComponent<ParticleAnimator>();	// Get or create ParticleAnimator (there must be one).
			particleAnimator.doesAnimateColor = true;
			particleAnimator.colorAnimation = new Color[3];
			particleAnimator.colorAnimation[0] = Color.red;
			particleAnimator.colorAnimation[1] = Color.green;
			particleAnimator.colorAnimation[2] = Color.blue;
		}
	}

	void Start()
	{
	
	}
	
	void Update()
	{
		// Todo: Emit.
	}
}
