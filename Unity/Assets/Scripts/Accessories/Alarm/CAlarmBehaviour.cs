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


    public enum EType
    {
        INVALID,

        Spinning,
        Flashing,

        MAX
    }


// Member Delegates & Events


// Member Properties


	public bool IsActive
	{
		get { return (m_bActive.Get()); }
	}


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_bActive = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, false);
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
            if (m_eType == EType.Spinning)
            {
                m_cSpinningLight.transform.Rotate(new Vector3(0.0f, m_fRotationSpeed * Time.deltaTime, 0.0f));
            }
            else if (m_eType == EType.Flashing)
            {
                m_fFlashTimer += Time.deltaTime;

                if (m_fFlashTimer > 0.5f)
                {
                    m_cSpinningLight.light.enabled = false;

                    if (m_fFlashTimer > 1.0f)
                    {
                        m_cSpinningLight.light.enabled = true;
                        m_fFlashTimer = 0.0f;
                    }
                }
            }
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

        if (m_eType == EType.Spinning)
        {
            gameObject.GetComponent<AudioSource>().enabled = true;
        }
	}


	void DeactivateAlarm()
	{
		m_cSpinningLight.light.enabled = false;

        if (m_eType == EType.Spinning)
        {
            gameObject.GetComponent<AudioSource>().enabled = false;
        }
	}


// Member Fields


    public EType m_eType = EType.INVALID;
	public GameObject m_cSpinningLight = null;

	CNetworkVar<bool> m_bActive = null;


	float m_fRotationSpeed = 360.0f;
    float m_fFlashTimer = 0.0f;


};
