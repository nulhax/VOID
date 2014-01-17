//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CAlarmBehaviour.cs
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


public class CAlarmBehaviour : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


	public bool IsActive
	{
		get { return (m_bActive.Get()); }
	}


// Member Methods


	public override void InstanceNetworkVars()
	{
		m_bActive = new CNetworkVar<bool>(OnNetworkVarSync, false);
	}


	[AServerOnly]
	public void SetAlarmActive(bool _bActive)
	{
		m_bActive.Set(_bActive);
	}


	void Start()
	{
		// Empty
	}


	void OnDestroy()
	{
		// Empty
	}


	void Update()
	{
		if (IsActive)
		{
			m_cSpinningLight.transform.Rotate(new Vector3(0.0f, m_fRotationSpeed * Time.deltaTime, 0.0f));
		}
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_bActive)
		{
			if (m_bActive.Get())
			{
				ActivateAlarm();
			}
			else
			{
				DeactivateAlarm();
			}
		}
	}


	void ActivateAlarm()
	{
		m_cSpinningLight.light.enabled = true;
		gameObject.GetComponent<AudioSource>().enabled = true;
	}


	void DeactivateAlarm()
	{
		m_cSpinningLight.light.enabled = false;
		gameObject.GetComponent<AudioSource>().enabled = false;
	}


// Member Fields


	public GameObject m_cSpinningLight = null;


	CNetworkVar<bool> m_bActive = null;


	float m_fRotationSpeed = 360.0f;


};
