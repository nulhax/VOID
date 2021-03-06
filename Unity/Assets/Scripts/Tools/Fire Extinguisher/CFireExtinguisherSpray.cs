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


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
		m_bActive = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, false);
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
        TNetworkViewId cFireExtinguisherViewId = _cStream.Read<TNetworkViewId>();

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
			if (m_bActive.Value)
			{
				Collider[] colliders = Physics.OverlapSphere(m_cSprayParticalSystem.transform.position + m_cSprayParticalSystem.transform.forward * 2.0f, 2.0f);

				foreach(Collider collider in colliders)
				{
					CFireHazard fire = CUtility.FindInParents<CFireHazard>(collider.gameObject);
					if (fire != null)
					{
						fire.health.health += 20 * Time.deltaTime;
						break;
					}
				}
			}
		}
	}


	[AServerOnly]
	public void OnUseStart(GameObject _cInteractableObject)
	{
		m_bActive.Set(true);
        Debug.Log("OnUseStart");
	}


    [ALocalOnly]
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

        s_cSerializeStream.Write(NetworkView.ViewId);
    }


	[AServerOnly]
	public void OnUseEnd(GameObject _cInteractableObject)
	{
		m_bActive.Set(false);
	}


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_bActive)
        {
            if (m_bActive.Get())
            {
                m_cSprayParticalSystem.Play();

				CAudioCue[] audioCues = GetComponents<CAudioCue>();
				foreach(CAudioCue cue in audioCues)
				{
					if(cue.m_strCueName == "FireExtinguisherSFX")
					{

						cue.PlayAll(transform, 1.0f);
					}
				}
            }
            else
            {
                m_cSprayParticalSystem.Stop();
                gameObject.GetComponent<CAudioCue>().FadeOut();
            }
        }
    }


// Member Fields


	CNetworkVar<bool> m_bActive = null;


	ParticleSystem m_cSprayParticalSystem = null;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
