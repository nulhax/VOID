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
            transform.parent.GetComponent<CCannonProjectile>().Destroy();

            gameObject.collider.enabled = false;
        }
    }


// Member Fields


};
