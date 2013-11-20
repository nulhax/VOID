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


public class CFireExtinguisherSpray : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Functions


	public override void InstanceNetworkVars()
	{
		m_bActive = new CNetworkVar<bool>(OnNetworkVarSync, false);
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


	public void Start()
	{
		m_cSprayParticalSystem = transform.FindChild("ParticalSprayer").particleSystem;

		gameObject.GetComponent<CToolInterface>().EventPrimaryActivate += new CToolInterface.NotifyPrimaryActivate(OnUseStart);
		gameObject.GetComponent<CToolInterface>().EventPrimaryDeactivate += new CToolInterface.NotifyPrimaryDeactivate(OnUseEnd);
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
		// Empty
	}


	[AServerMethod]
	public void OnUseStart(GameObject _cInteractableObject)
	{
		m_bActive.Set(true);
	}


	[AServerMethod]
	public void OnUseEnd()
	{
		m_bActive.Set(false);
	}


// Member Fields


	CNetworkVar<bool> m_bActive = null;


	ParticleSystem m_cSprayParticalSystem = null;


};
