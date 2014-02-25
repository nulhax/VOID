//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShieldEventHandler.cs
//  Description :   Simple event to send message to CGalaxyShipShield of collision.
//
//  Author  	:  Scott Emery
//  Mail    	:  scott.ipod@gmail.com
//

/* Implementation */

// Namespaces
using UnityEngine;
using System.Collections;

public class CShieldEventHandler : MonoBehaviour {
	
	// Member Delegates & Events

	// Ship Shield Collider
	public delegate void NotifyShieldCollider(Collider _Collider);
	public event NotifyShieldCollider EventShieldCollider;

	// Ship Shield Damage
	public delegate void NotifyShieldDamage(Collider _Collider);
	public event NotifyShieldDamage EventShieldDamage;

	// Shield Recharge
	public delegate void NotifyShieldRecharge();
	public event NotifyShieldRecharge EventShieldRecharge;


	// Member Properties
	
	
	// Member Methods


	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(EventShieldRecharge != null)
			EventShieldRecharge();
	}

	void OnTriggerEnter(Collider _Collider)
	{
		if(EventShieldCollider != null)
			EventShieldCollider(_Collider);
		
		if(EventShieldDamage != null)
			EventShieldDamage(_Collider);
	}


}
