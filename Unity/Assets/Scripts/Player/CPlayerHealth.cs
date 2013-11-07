//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CActorHealth.cs
//  Description :   --------------------------
//
//  Author  	:  Scott Emery
//  Mail    	:  scott.ipod@gmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* Implementation */

public class CPlayerHealth : MonoBehaviour 
{

    // Member Types


    // Member Delegates & Events


    // Member Properties


    // Member Functions


    // Get player HP from XML
    public float m_fActorHp = 100.0f;
	public bool m_bIsAlive = true;
	
	void Start () 
    {
		//Set everything to kinematic
		foreach(Transform child in transform.GetComponentsInChildren<Transform>())
		{		
			Rigidbody rigidBody;
			if(child.GetComponent<Rigidbody>() != null)
			{
				rigidBody = child.GetComponent<Rigidbody>();
				rigidBody.isKinematic = true;	
			}
			
			CapsuleCollider capCollider;
			if(child.GetComponent<CapsuleCollider>() != null)
			{
				capCollider = child.GetComponent<CapsuleCollider>();
				capCollider.isTrigger = true;	
			}
			
			BoxCollider boxCollider;
			if(child.GetComponent<BoxCollider>() != null)
			{
				boxCollider = child.GetComponent<BoxCollider>();
				boxCollider.isTrigger = true;	
			}			
		}			
	}

    public void OnDestroy()
    { 
    }

	// Update is called once per frame
	void Update () 
	{
	    // Get the player health from the XML
		if(m_bIsAlive == true)
		{
			if(m_fActorHp <= 0.0f || Input.GetKeyDown(KeyCode.Q))
			{			
				Debug.Log("Player is deaaad");
				
				foreach(Transform child in transform.GetComponentsInChildren<Transform>())
				{
					Rigidbody rigidBody;
					if(child.GetComponent<Rigidbody>() != null)
					{
						rigidBody = child.GetComponent<Rigidbody>();
						rigidBody.isKinematic = false;	
					}
					
					CapsuleCollider capCollider;
					if(child.GetComponent<CapsuleCollider>() != null)
					{
						capCollider = child.GetComponent<CapsuleCollider>();
						capCollider.isTrigger = false;	
					}
					
					BoxCollider boxCollider;
					if(child.GetComponent<BoxCollider>() != null)
					{
						boxCollider = child.GetComponent<BoxCollider>();
						boxCollider.isTrigger = false;	
					}
					
					transform.GetComponent<CharacterController>().enabled = false;
					transform.GetComponent<CPlayerBodyMotor>().enabled = false;
					transform.GetComponent<CPlayerHeadMotor>().enabled = false;
					
					Camera headCam = transform.GetComponent<CPlayerHeadMotor>().ActorHead.GetComponent<CPlayerCamera>().camera;
					Vector3 pos = headCam.transform.localPosition;
					pos.z -= 0.05f;
					headCam.transform.localPosition = pos;					
				}	
				
				m_bIsAlive = false;
			}	
		}
		else
		{
			transform.GetComponent<CPlayerHeadMotor>().ActorHead.GetComponent<CPlayerCamera>().camera.transform.LookAt(transform);
		// Do nothing at the moment
		}
        // Check if the player is in harmful atmosphere leading to suffocation
            // Decrease stamina first and incrimenting rate
            // Then decrease health

        // Check if player has been shot
            // Bullet does initial amount of damage but also applies more over time.
            // Head shot is 80-105% damage
            // Heart damage is 50% damage 50% damage applied over time
            // Knee damage is 10% but movement slowed 

        // Physical trauma of a projectile
            // Damage applied to player in relevence to the velocity travelling
            
        // Check health points

        // Send a complete loss of health to manager that handles death screen
}


    public void ApplyDamage(float _fDamage, float _fPlayerHealth)
    {
        m_fActorHp = _fPlayerHealth - _fDamage;
    }
    // Member Fields
	
}

