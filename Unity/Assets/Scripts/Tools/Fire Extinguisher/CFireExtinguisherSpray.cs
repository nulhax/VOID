//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFireExtinguisherSpray.cs
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
public class CFireExtinguisherSpray : CNetworkMonoBehaviour
{

	// Member Types


	// Member Delegates & Events


	// Member Properties


	// Member Functions


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_bActive = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, false);
	}


	public void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_bActive)
		{
			if (m_bActive.Get())
			{
				m_cSprayParticalSystem.Play();
			}
			else
			{
				m_cSprayParticalSystem.Stop();
			}
		}
	}


	public void Awake()
	{
		m_cSprayParticalSystem = transform.FindChild("ParticalSprayer").particleSystem;
	}


	public void Start()
	{
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
			if (m_bActive.Get())
			{
				RaycastHit _rh;
				Ray ray = new Ray(m_cSprayParticalSystem.gameObject.transform.position, m_cSprayParticalSystem.gameObject.transform.forward);

				if (Physics.Raycast(ray, out _rh, 2.0f))
				{
					CFireHazard_Old oldFireType = _rh.collider.gameObject.GetComponent<CFireHazard_Old>();
					if (oldFireType != null)
						oldFireType.Health -= 80.0f * Time.deltaTime;

					CFireHazard fire = _rh.collider.gameObject.GetComponent<CFireHazard>();
					if (fire != null)
						fire.GetComponent<CActorHealth>().health += 5 * Time.deltaTime;
				}
			}
		}
	}


	[AServerOnly]
	public void OnUseStart(GameObject _cInteractableObject)
	{
		m_bActive.Set(true);
	}


	[AServerOnly]
	public void OnUseEnd(GameObject _cInteractableObject)
	{
		m_bActive.Set(false);
	}


	// Member Fields


	CNetworkVar<bool> m_bActive = null;


	ParticleSystem m_cSprayParticalSystem = null;


};
