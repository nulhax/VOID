//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CTorchLight.cs
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
public class CTorchLight : CNetworkMonoBehaviour
{

// Member Types


    public enum ENetworkAction
    {
        INVALID,

        TurnOnLight,
        TurnOffLight,
        ToggleColour,

        MAX
    }


// Member Delegates & Events


// Member Properties


// Member Functions


    public override void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar)
    {
        m_bTorchLit = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, true);
		m_bTorchColour = _cRegistrar.CreateReliableNetworkVar<byte>(OnNetworkVarSync, 0);
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
        while (_cStream.HasUnreadData)
        {
            ENetworkAction eAction = (ENetworkAction)_cStream.Read<byte>();
            TNetworkViewId cModuleGunViewId = _cStream.Read<TNetworkViewId>();

            GameObject cModuleGunObject = cModuleGunViewId.GameObject;
            CToolInterface cToolInterface = cModuleGunObject.GetComponent<CToolInterface>();
            CTorchLight cTorchLight = cModuleGunObject.GetComponent<CTorchLight>();

            switch (eAction)
            {
                case ENetworkAction.TurnOnLight:
                    cTorchLight.SetActive(true);
                    break;

                case ENetworkAction.TurnOffLight:
                    cTorchLight.SetActive(false);
                    break;

                case ENetworkAction.ToggleColour:
                    cTorchLight.ToggleColour();
                    break;


                default:
                    Debug.LogError("Unknown network action: " + eAction);
                    break;
            }
        }
    }


	void Start()
	{
        GetComponent<CToolInterface>().EventPrimaryActiveChange += (_bDown) =>
        {
            if (_bDown)
            {
                if (m_bTorchLit.Get())
                {
                    s_cSerializeStream.Write((byte)ENetworkAction.TurnOffLight);
                    s_cSerializeStream.Write(NetworkView.ViewId);
                }
                else
                {
                    s_cSerializeStream.Write((byte)ENetworkAction.TurnOnLight);
                    s_cSerializeStream.Write(NetworkView.ViewId);
                }
            }
        };

        GetComponent<CToolInterface>().EventSecondaryActiveChange += (_bDown) =>
        {
            if (_bDown)
            {
                s_cSerializeStream.Write((byte)ENetworkAction.ToggleColour);
                s_cSerializeStream.Write(NetworkView.ViewId);
            }
        };

		if (CNetwork.IsServer)
		{
			m_bTorchLit.Set(false);
		}
		
		//Get audio cues
		CAudioCue[] cues = gameObject.GetComponents<CAudioCue>();
	}


	void OnDestroy()
	{
	}


	void Update()
	{
	}


    [AServerOnly]
	void SetActive(bool _bActive)
    {
        m_bTorchLit.Set(_bActive);		
    }


    [AServerOnly]
	void ToggleColour()
    {
		byte bNextNumber = (byte)(m_bTorchColour.Get() + 1);

		if (bNextNumber > 3)
		{
			bNextNumber = 0;
		}

		m_bTorchColour.Set(bNextNumber);		
	}


    void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        if (_cVarInstance == m_bTorchLit)
        {
            if (!m_bTorchLit.Get())
            {
                light.intensity = 0;
            }
            else
            {
                light.intensity = 1.2f;
            }

        }
        else if (_cVarInstance == m_bTorchColour)
        {
            switch (m_bTorchColour.Get())
            {
                case 0:
                    light.color = new Color(174.0f / 255.0f, 208.0f / 255.0f, 1.0f);
                    break;
                case 1:
                    light.color = new Color(174.0f / 255.0f, 208.0f / 255.0f, 1.0f);
                    break;
                case 2:
                    light.color = new Color(174.0f / 255.0f, 208.0f / 255.0f, 1.0f);
                    break;
                case 3:
                    light.color = new Color(174.0f / 255.0f, 208.0f / 255.0f, 1.0f);
                    break;
            }
        }
    }


// Member Fields


    CNetworkVar<bool> m_bTorchLit = null;
	CNetworkVar<byte> m_bTorchColour = null;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};