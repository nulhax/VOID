using UnityEngine;
using System.Collections;

public class CCannonProjectile : MonoBehaviour
{
	[HideInInspector] public Rigidbody parent = null;
	float lifetime = 4.0f;
	float damage = 1000.0f;

	void Awake()
	{
		transform.parent = CGalaxy.instance.transform;
	}

	void Start()
	{
		Physics.IgnoreCollision(collider, parent.collider);
		Collider[] parentColliders = parent.GetComponentsInChildren<Collider>();
		foreach (Collider parentCollider in parentColliders)
			Physics.IgnoreCollision(collider, parentCollider);
	}
	
	void Update()
	{
		lifetime -= Time.deltaTime;
		if (lifetime <= 0.0f)
			Destroy(gameObject);
	}

	void OnCollisionEnter(Collision collision)
	{
		Rigidbody colliderRigidbody = CUtility.FindInParents<Rigidbody>(collision.gameObject);

		if (colliderRigidbody != parent)
		{
			if (colliderRigidbody != null)
			{
				CActorHealth actorHealth = colliderRigidbody.GetComponent<CActorHealth>();
				if (actorHealth != null)
					actorHealth.health -= damage;
			}

			Destroy(gameObject);
		}
	}
}