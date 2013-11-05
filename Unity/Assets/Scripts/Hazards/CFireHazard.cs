﻿//  Auckland
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


public class CFireHazard : MonoBehaviour {
    // Member Types


    // Member Delegates & Events
    

    // Member Properties


    // Member Functions


    public float Damage
    {
        get { return m_fDamage; }
        set { m_fDamage = value; }
    }

	// Use this for initialization
	public void Start () 
    {
	    
	}
    
    public void OnDestroy()
    {

    }

	// Update is called once per frame
	void Update () 
    {
        
	}

    // Get IsTrigger and get the collider of the player to check 
    // Send fire damage to player Hp 
    void OnTriggerStay(Collider _Entity)
    {
        Debug.Log("ontriggerstay function entered.");
        if (CNetwork.IsServer)
        {
            Debug.Log("Server detected in fire hazard");
             // is player actor, does the object return character motor
            if (_Entity.gameObject.name == "Player Actor(Clone)")
            {
                Debug.Log("Rigid body triggered. ---------------------------");
                //Get actor health
                float hp = _Entity.gameObject.GetComponent<CActorHealth>().m_fActorHp;
				
				if(hp <= 0.0f)
				{
					// Do nothing
				}
                else
				{
					// Set the damage fire will do to the character
               		 Damage = 40.0f * Time.deltaTime;

               		 //apply damage
               		 _Entity.gameObject.GetComponent<CActorHealth>().ApplyDamage(Damage, hp);
				}
            }
        }
    }
	
	//Members
	private float m_fDamage;
}