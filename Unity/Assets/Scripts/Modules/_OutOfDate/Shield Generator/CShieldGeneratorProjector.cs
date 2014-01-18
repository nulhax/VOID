//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRoomShieldGeneratorProjector.cs
//  Description :   --------------------------
//
//  Author  	:  Matt
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CShieldGeneratorProjector : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Functions


	public void Start()
	{
        //calls CheckShipBounds to find the current size/shape of ship
        //and sets the shield to match that

        CheckShipBounds();
	}


	public void OnDestroy()
	{
	}


	public void Update()
	{
        //Turn ShieldSphere into an actual Sphere, and give it all of it's cool looking effects and things.
        //Alternatively, have the Shield itself as a separate class entirely,
        //this could be useful for enemy ships having shields without shield generators.

        //call CheckShipBounds

        //check if something has hit the shield, if so, render it.
        //gameObject;
	}


    public void CheckShipBounds()
    {
        //check the shape of the ship, and whether it is different to what it was
        //if it is different, call UpdateShield, and pass in whatever information 
        //is required for it, which is currently unknown

        //Vector3 Position = new Vector3(100.0f, 0.0f, 100.0f);

        //gameObject.rigidbody.isKinematic = false;

         //gameObject.GetComponent<SphereCollider>().
    }


    public void UpdateShield()
    {
        //change the ShieldSphere to match the new shape of the ship
    }


// Member Fields
    GameObject ShieldSphere;

    CNetworkVar<uint> m_uiCurrentShieldCapacity;
    CNetworkVar<uint> m_uiCurrentShieldStrength;
    CNetworkVar<uint> m_uiShieldRegenRate;

};
