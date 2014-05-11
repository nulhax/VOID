using UnityEngine;
using System.Collections;

public class CCannonProjectile : CNetworkMonoBehaviour
{
	[HideInInspector] public Rigidbody parent = null;
	float lifetime = 4.0f;
	float damage = 1.0f;
    bool destroyed = false;

	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{

	}

	void Awake()
	{
		transform.parent = CGalaxy.instance.transform;
	}

	void Start()
	{
        if (!CNetwork.IsServer)
            return;

		Physics.IgnoreCollision(collider, parent.collider);
		Collider[] parentColliders = parent.GetComponentsInChildren<Collider>();
		foreach (Collider parentCollider in parentColliders)
			Physics.IgnoreCollision(collider, parentCollider);
	}
	
	void Update()
	{
        if (!CNetwork.IsServer)
            return;

		lifetime -= Time.deltaTime;
        if (!destroyed &&
            lifetime <= 0.0f)
        {
            destroyed = true;
            CNetwork.Factory.DestoryGameObject(NetworkViewId);
        }	
	}

	void OnCollisionEnter(Collision collision)
	{
        if (!CNetwork.IsServer)
            return;

        if (destroyed)
            return;

		Rigidbody colliderRigidbody = collision.rigidbody;
		if(colliderRigidbody == null)
			colliderRigidbody = CUtility.FindInParents<Rigidbody>(collision.gameObject);

		if (colliderRigidbody != parent)
		{
			if (colliderRigidbody != null)
			{
				CActorHealth actorHealth = colliderRigidbody.GetComponent<CActorHealth>();
				if (actorHealth != null)
					actorHealth.health -= damage;
			}

            destroyed = true;
            CNetwork.Factory.DestoryGameObject(NetworkViewId);
		}
	}
}