//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CAk47Behaviour.cs
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

[RequireComponent(typeof(CToolInterface))]
public class CAk47Behaviour : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_bAmmo = _cRegistrar.CreateNetworkVar<byte>(OnNetworkVarSync, m_bAmmoCapacity);

        _cRegistrar.RegisterRpc(this, "ExecuteShootEffect");
	}


	public void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{

	}


	public void Start()
	{
		m_cNossle = transform.FindChild("Nossle").gameObject;

		gameObject.GetComponent<CToolInterface>().EventPrimaryActivate += OnUseStart;
		gameObject.GetComponent<CToolInterface>().EventPrimaryDeactivate += OnUseEnd;
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
		if (CNetwork.IsServer)
		{
			if (m_bShoot)
			{
				m_fShootTimer += Time.deltaTime;

				if (m_fShootTimer > m_fShootInterval)
				{
					InvokeRpcAll("ExecuteShootEffect");

					m_fShootTimer -= m_fShootInterval;
				}
			}
		}
	}


	[ANetworkRpc]
	void ExecuteShootEffect()
	{
		GameObject cBullet = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Tools/Ak47/Bullet", typeof(GameObject)), m_cNossle.transform.position, m_cNossle.transform.rotation);


		cBullet.rigidbody.AddForce(cBullet.transform.forward * 40.0f + rigidbody.velocity, ForceMode.VelocityChange);
	}


	[AServerOnly]
	void OnUseStart(GameObject _cInteractableObject)
	{
		m_bShoot = true;
	}


	[AServerOnly]
	void OnUseEnd(GameObject _cInteractableObject)
	{
		m_bShoot = false;
	}


// Member Fields


	GameObject m_cNossle = null;


	CNetworkVar<byte> m_bAmmo = null;


	float m_fShootTimer = 0.0f;
	float m_fShootInterval = 0.1f;


	byte m_bAmmoCapacity = 30;


	bool m_bShoot = false;


};
