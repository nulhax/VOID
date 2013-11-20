//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CNaniteReplicator.cs
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

public class CNaniteReplicator : CNetworkMonoBehaviour 
{
	
// Member Types


// Member Delegates & Events


// Member Properties


// Member Functions

	public override void InstanceNetworkVars()
	{

	}
	
	// Use this for initialization
	void Start () 
	{
		m_fTotalNanites = 1000.0f;
	}
	

	public void OnDestroy()
	{
		
	}


	// Update is called once per frame
	void Update () 
	{

	}
	
	void OnTriggerEnter(Collider _Object)
	{
		m_fObjectSize = _Object.bounds.size.magnitude * 50.0f;
		
		m_fTotalNanites = m_fTotalNanites + m_fObjectSize;
		
		Debug.Log("Nanites are " + m_fTotalNanites.ToString());
		
		// Check for player entity
		if(_Object.gameObject.name != "Player Actor(Clone)")
		{
			// If the object is not a player, just deleted it.
			Destroy(_Object.gameObject);
			
			// Display particles for nanite conversion
			gameObject.GetComponentInChildren<ParticleSystem>().Play();

		}
		else
		{
			// If the object is a player actor, kill it.
			float fDamage = 1000.0f;
			
			// Kill player
			_Object.gameObject.GetComponent<CPlayerHealth>().ApplyDamage(fDamage, 0.0f);
		}
	}
	
	void OnNetworkVarSync(INetworkVar _cVarInstance)
	{
		
	}
	// Member Fields
	float m_fObjectSize;
	float m_fTotalNanites;
}
