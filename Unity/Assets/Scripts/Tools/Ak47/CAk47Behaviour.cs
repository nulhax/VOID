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


    public enum ENetworkAction
    {
        INVALID,

        ShootStart,
        ShootEnd,

        MAX
    }


// Member Delegates & Events


// Member Properties

	private int m_BulletFireSoundIndex = -1;


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_bAmmo = _cRegistrar.CreateNetworkVar<byte>(OnNetworkVarSync, m_bAmmoCapacity);

        _cRegistrar.RegisterRpc(this, "ExecuteShootEffect");
	}


	public void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
        // Empty
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
            CNetworkViewId cAk47ViewId = _cStream.Read<CNetworkViewId>();

            switch (eAction)
            {
                case ENetworkAction.ShootStart:
                    cAk47ViewId.GameObject.GetComponent<CAk47Behaviour>().m_bShoot = true;
                    break;

                case ENetworkAction.ShootEnd:
                    cAk47ViewId.GameObject.GetComponent<CAk47Behaviour>().m_bShoot = false;
                    break;

                default:
                    Debug.LogError("Unknown network action");
                    break;
            }
        }
    }


	void Awake()
	{
		CAudioCue audioCue = GetComponent<CAudioCue>();
		if (audioCue == null)
			audioCue = gameObject.AddComponent<CAudioCue>();
		m_BulletFireSoundIndex = audioCue.AddSound("Audio/BulletFire", 0.0f, 0.0f, false);
	}

	void Start()
	{
		m_cNossle = transform.FindChild("Nossle").gameObject;

        gameObject.GetComponent<CToolInterface>().EventPrimaryActiveChange += OnEventPrimaryActiveChange;
	}


	void OnDestroy()
	{
        gameObject.GetComponent<CToolInterface>().EventPrimaryActiveChange -= OnEventPrimaryActiveChange;
	}


	void Update()
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
		cBullet.rigidbody.velocity = cBullet.transform.forward * 40.0f;

		GetComponent<CAudioCue>().Play(transform, 1.0f, false, m_BulletFireSoundIndex);
	}


	[ALocalOnly]
	void OnEventPrimaryActiveChange(bool _bActive)
	{
        if (_bActive)
        {
            s_cSerializeStream.Write((byte)ENetworkAction.ShootStart);
        }
        else
        {  
            s_cSerializeStream.Write((byte)ENetworkAction.ShootEnd);
        }

        s_cSerializeStream.Write(SelfNetworkView.ViewId);
	}


// Member Fields


	GameObject m_cNossle = null;


	CNetworkVar<byte> m_bAmmo = null;


	float m_fShootTimer = 0.0f;
	float m_fShootInterval = 0.1f;


	byte m_bAmmoCapacity = 30;


	bool m_bShoot = false;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
