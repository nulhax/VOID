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


    public enum ENetworkAction
    {
        INVALID,

        ExtinguishFireStart,
        ExtinguishFireEnd,

        MAX
    }


// Member Delegates & Events


// Member Properties


// Member Functions


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_bActive = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, false);
	}


    [AClientOnly]
    public static void SerializeOutbound(CNetworkStream _cStream)
    {
        _cStream.Write(s_cSerializeStream);
        s_cSerializeStream.Clear();
    }


    [AServerOnly]
    public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        ENetworkAction eAction = (ENetworkAction)_cStream.ReadByte();
        CNetworkViewId cFireExtinguisherViewId = _cStream.ReadNetworkViewId();

        switch (eAction)
        {
            case ENetworkAction.ExtinguishFireStart:
                cFireExtinguisherViewId.GameObject.GetComponent<CFireExtinguisherSpray>().m_bActive.Set(true);
                break;

            case ENetworkAction.ExtinguishFireEnd:
                cFireExtinguisherViewId.GameObject.GetComponent<CFireExtinguisherSpray>().m_bActive.Set(false);
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
					CFireHazard_Old oldFireType = _rh.collider.gameObject.GetComponent<CFireHazard_Old>();
					if (oldFireType != null)
						oldFireType.Health -= 80.0f * Time.deltaTime;

					CFireHazard fire = _rh.collider.gameObject.GetComponent<CFireHazard>();
					if (fire != null)
						fire.GetComponent<CActorHealth>().health += 20 * Time.deltaTime;
				}
			}
		}
	}


    [AClientOnly]
    void OnEventPrimaryActiveChange(bool _bActive)
    {
        if (_bActive)
        {
            s_cSerializeStream.Write((byte)ENetworkAction.ExtinguishFireStart);
        }
        else
        {
            s_cSerializeStream.Write((byte)ENetworkAction.ExtinguishFireEnd);
        }

        s_cSerializeStream.Write(ThisNetworkView.ViewId);
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
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


// Member Fields


	CNetworkVar<bool> m_bActive = null;


	ParticleSystem m_cSprayParticalSystem = null;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
