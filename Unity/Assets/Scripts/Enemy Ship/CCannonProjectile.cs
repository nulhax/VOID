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


    [AServerOnly]
    public void NotifyHitShip(Collider _cOther)
    {
        if (destroyed)
            return;

        CGameShips.Ship.GetComponent<CShipShieldSystem>().ProjectileHitNoShield(transform.position, Quaternion.LookRotation((transform.position - _cOther.gameObject.transform.position).normalized).eulerAngles);

        Destroy();
    }
//
//    
//    [AServerOnly]
//    public void NotifyHitShipShield(Collider _cOther)
//    {
//        if (destroyed)
//            return;
//
//        bool bAbsorbed = CGameShips.Ship.GetComponent<CShipShieldSystem>().ProjectileHit(5.0f, transform.position, Quaternion.LookRotation((transform.position - _cOther.gameObject.transform.position).normalized).eulerAngles);
//
//        if (bAbsorbed)
//        {
//            Destroy();
//        }
//    }


	void Awake()
	{
		transform.parent = CGalaxy.instance.transform;
	}

	void Start()
	{
        if (!CNetwork.IsServer)
            return;

        foreach (Collider cChildCollider in GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(cChildCollider, parent.collider);

            Collider[] parentColliders = parent.GetComponentsInChildren<Collider>();
            foreach (Collider parentCollider in parentColliders)
                Physics.IgnoreCollision(cChildCollider, parentCollider);
        }
	}
	
	void Update()
	{
        if (!CNetwork.IsServer)
            return;

		lifetime -= Time.deltaTime;
        if (!destroyed &&
            lifetime <= 0.0f)
        {
            Destroy();
        }	
	}

    public void Destroy()
    {
        if (!CNetwork.IsServer)
            return;

        if (destroyed)
            return;

        destroyed = true;
        CNetwork.Factory.DestoryGameObject(NetworkViewId);
    }

    /*
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
     * */
}