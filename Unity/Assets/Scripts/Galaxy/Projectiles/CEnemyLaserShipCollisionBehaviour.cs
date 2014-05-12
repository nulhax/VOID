//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CEnemyLaserShipCollisionBehaviour.cs
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


public class CEnemyLaserShipCollisionBehaviour : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


	void Start()
	{
        // Empty
	}


	void OnDestroy()
	{
        // Empty
	}


	void Update()
	{
        // Empty
	}


    [AServerOnly]
    void OnTriggerEnter(Collider _cCollider)
    {
        if (CNetwork.IsServer &&
            _cCollider.gameObject.transform.parent != null &&
            _cCollider.gameObject.transform.parent.parent != null &&
            _cCollider.gameObject.transform.parent.parent.GetComponent<CGalaxyShipFacilities>() != null)
        {
            transform.parent.GetComponent<CCannonProjectile>().NotifyHitShip(_cCollider);

            gameObject.collider.enabled = false;
        }
    }


// Member Fields


};
