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
	public float fSizeOfObject
	{ 
		get { return (m_fObjectSize.Get()); }
		set { m_fObjectSize.Set(value); }
	}
	public float fNaniteTotal
	{ 
		get { return (m_fTotalNanites.Get()); }
		set { m_fTotalNanites.Set(value); }
	}
	
// Member Functions

	public override void InstanceNetworkVars()
	{
		m_fObjectSize = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_fTotalNanites = new CNetworkVar<float>(OnNetworkVarSync, 1000.0f);
		m_bIsParticleEmtEnabled = new CNetworkVar<bool>(OnNetworkVarSync, false);
	}
	
	// Use this for initialization
	void Start () 
	{
		
	}
	

	public void OnDestroy()
	{
		
	}


	// Update is called once per frame
	void Update () 
	{
		if(CNetwork.IsServer)
		{
			if(m_bIsParticleEmtEnabled.Get())
			{
				m_fEmitterTimer += Time.deltaTime;
				if(m_fEmitterTimer > 5.0f)
				{
					m_fEmitterTimer = 0.0f;
					m_bIsParticleEmtEnabled.Set(false);
				}
			}
		}
	}
	
	void OnTriggerEnter(Collider _Object)
	{
		if (CNetwork.IsServer)
        {
			
			m_fObjectSize.Set(_Object.bounds.size.magnitude * 50.0f);
			
			m_fTotalNanites.Set(m_fTotalNanites.Get() + m_fObjectSize.Get());
			
			Debug.Log("Nanites are " + m_fTotalNanites.Get().ToString());
			
			// Check for player entity
			if(_Object.gameObject.GetComponent<CPlayerHealth>() != null)
			{
				// If the object is a player actor, kill it.
				float fDamage = 1000.0f;
				
				// Kill player
				_Object.gameObject.GetComponent<CPlayerHealth>().ApplyDamage(fDamage, 0.0f);
				
				m_bIsParticleEmtEnabled.Set(true);
			}
			else
			{
				// If the object is not a player, just deleted it.
				CNetwork.Factory.DestoryObject(_Object.gameObject.GetComponent<CNetworkView>().ViewId);
				
				// Display particles for nanite conversion
				m_bIsParticleEmtEnabled.Set(true);

			}
		}
	}
	
	void OnNetworkVarSync(INetworkVar _cVarInstance)
	{
		if(_cVarInstance == m_bIsParticleEmtEnabled)
		{
			if(m_bIsParticleEmtEnabled.Get())
			{
				gameObject.GetComponentInChildren<ParticleSystem>().Play();
				m_fEmitterTimer = 0.0f;
			}
		}
	}
	
	// Member Fields
	CNetworkVar<float> m_fObjectSize;
	CNetworkVar<float> m_fTotalNanites;
	CNetworkVar<bool> m_bIsParticleEmtEnabled;
	float m_fEmitterTimer; 
}
