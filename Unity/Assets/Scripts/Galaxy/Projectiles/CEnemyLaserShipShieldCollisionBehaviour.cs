//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CEnemyLaserShipShieldCollisionBehaviour.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CEnemyLaserShipShieldCollisionBehaviour : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods

	void Awake()
	{
		if (CNetwork.IsServer)
		{
//			m_SphereCollider = gameObject.AddComponent<SphereCollider>();
//			m_SphereCollider.radius = 10.0f;
//			m_SphereCollider.isTrigger = true;
		}
	}


	void Start()
	{
	}


	void OnDestroy()
	{
	}


	void Update()
	{
	}


    [AServerOnly]
    void OnTriggerEnter(Collider _cCollider)
    {
        if (CNetwork.IsServer &&
            _cCollider.gameObject.transform.parent != null &&
            _cCollider.gameObject.transform.parent.parent != null &&
            _cCollider.gameObject.transform.parent.parent.GetComponent<CGalaxyShipFacilities>() != null)
        {
			bool bAbsorbed = CGameShips.Ship.GetComponent<CShipShieldSystem>().ProjectileHit(5.0f, transform.position, Quaternion.LookRotation((transform.position - _cCollider.gameObject.transform.position).normalized).eulerAngles);

			if (bAbsorbed)
			{
				transform.parent.GetComponent<CCannonProjectile>().Destroy();
			}

            // Turn off collider
			collider.enabled = false;
        }
    }


// Member Fields

};
