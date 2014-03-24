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


    public enum ENetworkAction
    {
        INVALID,

        SprayStart,
        SprayEnd,

        MAX
    }


// Member Delegates & Events


// Member Properties


// Member Functions


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_bActive = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, false);
    }


    [ALocalOnly]
    public static void SerializeOutbound(CNetworkStream _cStream)
    {
        _cStream.Write(s_cSerializeStream);
        s_cSerializeStream.Clear();
    }


    [AServerOnly]
    public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        ENetworkAction eAction = (ENetworkAction)_cStream.Read<byte>();
        CNetworkViewId cMedicalSpayViewId = _cStream.Read<CNetworkViewId>();

        switch (eAction)
        {
            case ENetworkAction.SprayStart:
                cMedicalSpayViewId.GameObject.GetComponent<CMedicalSpray>().m_bActive.Set(true);
                break;

            case ENetworkAction.SprayEnd:
                cMedicalSpayViewId.GameObject.GetComponent<CMedicalSpray>().m_bActive.Set(false);
                break;

            default:
                Debug.LogError("Unknown network action");
                break;
        }
    }


    void Awake()
    {
        m_cSprayParticalSystem = transform.FindChild("ParticalSprayer").particleSystem;
    }


    void Start()
    {
        GetComponent<CToolInterface>().EventPrimaryActiveChange += OnEventPrimaryActiveChange;
    }


    void OnDestroy()
    {
        GetComponent<CToolInterface>().EventPrimaryActiveChange -= OnEventPrimaryActiveChange;
    }



    void Update()
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


    [ALocalOnly]
    void OnEventPrimaryActiveChange(bool _bActive)
    {
        //m_bActive.Set(true);
        if (_bActive)
        {
            s_cSerializeStream.Write((byte)ENetworkAction.SprayStart);
        }
        else
        {
            s_cSerializeStream.Write((byte)ENetworkAction.SprayEnd);
        }

        s_cSerializeStream.Write(SelfNetworkView.ViewId);
    }


    [AServerOnly]
    public void OnUseStart(GameObject _cInteractableObject)
    {
        m_bActive.Set(true);
        gameObject.GetComponent<CAudioCue>().Play(0.8f, true, 0);
        Debug.Log("OnUseStart");
    }

    [AServerOnly]
    public void OnUseEnd(GameObject _cInteractableObject)
    {
        m_bActive.Set(false);
        gameObject.GetComponent<CAudioCue>().StopAllSound();
    }

    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        m_bActive.Set(false);
        gameObject.GetComponent<CAudioCue>().StopAllSound();
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


// Member Fields


    CNetworkVar<bool> m_bActive = null;


    ParticleSystem m_cSprayParticalSystem = null;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
