//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLaserProjeCTile.cs
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
public class CLaserProjectileBehaviour : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
        _cRegistrar.RegisterRpc(this, "RemoteExplode");
	}
	

	void Start()
	{
		m_vInitialPosition = transform.position;
	}


    void OnDestroy()
    {

    }


	public void Update()
	{
        if (m_fLifeTimer < 1.0f)
        {
            rigidbody.velocity = CGameShips.ShipGalaxySimulator.GetGalaxyVelocityRelativeToShip(transform.position) + (transform.forward * m_InitialProjectileSpeed * m_fLifeTimer);
        }

		if (!m_bDestroyed)
		{
			// Life timer
            m_fLifeTimer += Time.deltaTime;

            if (m_fLifeTimer > m_fLifeDuration)
			{
                CNetwork.Factory.DestoryGameObject(gameObject);

				m_bDestroyed = true;
			}
		}
	}


	[AServerOnly]
	void OnTriggerEnter(Collider _cCollider) 
	{
		if (!m_bDestroyed && 
            CNetwork.IsServer)
		{
            if (_cCollider.gameObject.GetComponent<CEnemyShip>() != null)
            {
                InvokeRpcAll("RemoteExplode", gameObject.transform.position, Quaternion.LookRotation(transform.position - _cCollider.transform.position));

                _cCollider.gameObject.GetComponent<CActorHealth>().health -= 10.0f;
            }
		}
	}


	[ANetworkRpc]
	void RemoteExplode(Vector3 _vHitPos, Quaternion _qHitRot)
	{
        // Create hit particles
        string sHitPartilesFile = CNetwork.Factory.GetRegisteredPrefabFile(CGameRegistrator.ENetworkPrefab.LaserHitParticles);
        GameObject cHitParticles = GameObject.Instantiate(Resources.Load(sHitPartilesFile)) as GameObject;

        cHitParticles.transform.position = _vHitPos;
        cHitParticles.transform.rotation = _qHitRot;

        // Destroy particles are 1 second
        GameObject.Destroy(cHitParticles, cHitParticles.particleSystem.duration);

        m_bDestroyed = true;
        CNetwork.Factory.DestoryGameObject(gameObject);
	}


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        // Empty
    }


// Member Fields


	public float m_InitialProjectileSpeed = 500.0f;
    public float m_fLifeDuration = 5.0f;


	Vector3 m_vInitialPosition;

	float m_fLifeTimer = 0.0f;

	bool m_bDestroyed = false;


};
