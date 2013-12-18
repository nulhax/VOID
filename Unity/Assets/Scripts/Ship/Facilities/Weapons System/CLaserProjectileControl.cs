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
public class CLaserProjectileControl : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


	static CLaserProjectileControl()
	{
		s_iAstroidLayer = LayerMask.NameToLayer("Galaxy");
		s_iEnemyLayer   = 0;
	}


	public void Start()
	{
		// Precalculate velocity
		m_vVelocty = transform.forward * k_fSpeed;

		GetComponent<CDynamicActor>().TransferActorToGalaxySpace();

		GetComponent<CDynamicActor>().BoardingState = CDynamicActor.EBoardingState.Disembarking;
	}


	public void OnDestroy()
	{
		// Empty
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
				CNetwork.Factory.DestoryObject(GetComponent<CNetworkView>().ViewId);
			}
		}
	}


	void OnCollisionEnter(Collision _cCollision) 
	{
		if (!m_bDestroyed)
		{
			if ((_cCollision.gameObject.layer & s_iAstroidLayer) > 0)
			{

			}

			// Create hit particles
			GameObject cHitParticles = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Ship/Facilities/Weapons System/LaserHitParticles"));
			
			cHitParticles.transform.position = _cCollision.contacts[0].point;
			cHitParticles.transform.rotation = Quaternion.LookRotation(transform.position - _cCollision.transform.position);
			
			// Destroy particles are 1 second
			GameObject.Destroy(cHitParticles, 1.0f);

			// Destroy self
			CNetwork.Factory.DestoryObject(GetComponent<CNetworkView>().ViewId);
			
			m_bDestroyed = true;
		}
	}


// Member Fields


	const float k_fSpeed = 40.0f;


	Vector3 m_vVelocty;


	float m_fLifeTimer = 5.0f;


	bool m_bDestroyed = false;


	static int s_iAstroidLayer = 0;
	static int s_iEnemyLayer = 0;


};
