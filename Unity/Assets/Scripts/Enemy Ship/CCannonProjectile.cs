using UnityEngine;
using System.Collections;

public class CCannonProjectile : CNetworkMonoBehaviour
{
	[HideInInspector]
	public Rigidbody parent = null;
	float lifetime = 4.0f;
	float damage = 1.0f;
	bool homingMissile = false;
	bool initialised_internal = false;
	bool initialised
	{
		get { return initialised_internal; }
		set
		{
			if (value != initialised)
			{
				initialised_internal = GetComponentInChildren<MeshRenderer>().enabled = collider.enabled = value;
			}
		}
	}

	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
		_cRegistrar.RegisterRpc(this, "RpcInitialise");
	}

	void Awake()
	{
		transform.parent = CGalaxy.instance.transform;
	}

	void Update()
	{
		if (!CNetwork.IsServer || !initialised)
			return;

		lifetime -= Time.deltaTime;
		if (lifetime <= 0.0f)
		{
			Destroy();
		}
	}

	public void Destroy()
	{
		if (!CNetwork.IsServer)
			return;

		CNetwork.Factory.DestoryGameObject(NetworkViewId);
		initialised = false;
	}

	void OnCollisionEnter(Collision collision)
	{
		if (!initialised)
			return;

		Rigidbody colliderRigidbody = collision.rigidbody;
		if (colliderRigidbody == null)
			colliderRigidbody = CUtility.FindInParents<Rigidbody>(collision.gameObject);

		if (colliderRigidbody != parent)
		{
			if (CNetwork.IsServer)
			{
				if (colliderRigidbody != null)
				{
					CActorHealth actorHealth = colliderRigidbody.GetComponent<CActorHealth>();
					if (actorHealth != null)
						actorHealth.health -= damage;
				}

				CNetwork.Factory.DestoryGameObject(NetworkViewId);
			}

			initialised = false;
		}
	}

	[AServerOnly]
	public void RpcInitialise(TNetworkViewId _parent, Vector3 _position, Vector3 _velocity, bool _homingMissile)
	{
		parent = _parent.GameObject.rigidbody;
		transform.position = _position;
		homingMissile = _homingMissile;

		if (homingMissile)
		{

		}
		else	// Not a homing missile.
		{

		}

		transform.rotation = Quaternion.LookRotation(_velocity);

		initialised = true;

		rigidbody.AddForce(_velocity, ForceMode.VelocityChange);	// 4) Velocity.

		Collider[] parentChildColliders = parent.GetComponentsInChildren<Collider>();
		Collider[] childColliders = GetComponentsInChildren<Collider>();

		Physics.IgnoreCollision(collider, parent.collider);	// This vs that.

		foreach (Collider childCollider in childColliders)
			Physics.IgnoreCollision(childCollider, parent.collider);	// This children vs that.

		foreach (Collider parentChildCollider in parentChildColliders)
		{
			Physics.IgnoreCollision(collider, parentChildCollider);	// This vs that children.

			foreach (Collider childCollider in childColliders)
				Physics.IgnoreCollision(childCollider, parent.collider);	// This children vs that children.
		}
	}
}