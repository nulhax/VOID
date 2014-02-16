//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CMedicalSpray.cs
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
public class CMedicalSpray : CNetworkMonoBehaviour
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
                    if (_rh.collider.gameObject.GetComponent<CPlayerHealth>() != null)
                    {
                        _rh.collider.gameObject.GetComponent<CPlayerHealth>().ApplyHeal(0.1f);// Health -= 80.0f * Time.deltaTime;
                    }
                }
            }
        }
    }


    [AServerOnly]
    public void OnUseStart(GameObject _cInteractableObject)
    {
        m_bActive.Set(true);
        gameObject.GetComponent<CAudioCue>().Play(0.8f, true, 0);
    }


    [AServerOnly]
	public void OnUseEnd(GameObject _cInteractableObject)
    {
        m_bActive.Set(false);
        gameObject.GetComponent<CAudioCue>().StopAllSound();
    }


    // Member Fields


    CNetworkVar<bool> m_bActive = null;


    ParticleSystem m_cSprayParticalSystem = null;


};
