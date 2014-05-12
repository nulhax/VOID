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
        if (CNetwork.IsServer)
        {
            // Notify parent
            transform.parent.GetComponent<CCannonProjectile>().NotifyHitShipShield(_cCollider);

            // Turn off collider
            gameObject.collider.enabled = false;
        }
    }


// Member Fields


};
