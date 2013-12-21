//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLaserProjectileControl.cs
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


[RequireComponent(typeof(CNetworkView))]
public class CLaserProjectileControl : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


	static CLaserProjectileControl()
	{
		s_iEnemyLayer   = 0;
	}


	public override void InstanceNetworkVars()
	{

	}
	
	public void OnNetworkVarSync(INetworkVar _rSender)
	{

   	}

	public void Start()
	{
		// Precalculate velocity
		m_vVelocty = transform.forward * k_fSpeed;
	}


	public void Update()
	{
		if (!m_bDestroyed)
		{
			// Move
			transform.position += m_vVelocty * Time.deltaTime;

			// Life timer
			m_fLifeTimer -= Time.deltaTime;

			if (m_fLifeTimer < 0.0f)
			{
				m_bDestroyed = true;
			}
		}
		else if(CNetwork.IsServer)
		{
			CNetwork.Factory.DestoryObject(GetComponent<CNetworkView>().ViewId);
		}
	}

	[AServerMethod]
	void OnCollisionEnter(Collision _cCollision) 
	{
		if (!m_bDestroyed && CNetwork.IsServer)
		{
			m_bDestroyed = true;

			InvokeRpc(0, "CreateHitParticles", _cCollision.contacts[0].point, Quaternion.LookRotation(transform.position - _cCollision.transform.position));
		}
	}

	[ANetworkRpc]
	void CreateHitParticles(Vector3 _HitPos, Quaternion _HitRot)
	{
		// Create hit particles
		GameObject cHitParticles = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Ship/Facilities/Weapons System/LaserHitParticles"));
		
		cHitParticles.transform.position = _HitPos;
		cHitParticles.transform.rotation = _HitRot;

		// Destroy particles are 1 second
		GameObject.Destroy(cHitParticles, 1.0f);
	}


// Member Fields


	const float k_fSpeed = 40.0f;


	Vector3 m_vVelocty;


	float m_fLifeTimer = 5.0f;


	bool m_bDestroyed = false;

	
	static int s_iEnemyLayer = 0;
};
