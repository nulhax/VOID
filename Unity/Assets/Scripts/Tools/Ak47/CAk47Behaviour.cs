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


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
		m_bAmmo = _cRegistrar.CreateReliableNetworkVar<byte>(OnNetworkVarSync, m_bAmmoCapacity);

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
            TNetworkViewId cAk47ViewId = _cStream.Read<TNetworkViewId>();

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
		
	}

	void Start()
	{
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
		GameObject cBullet = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Tools/Projectiles/Bullet", typeof(GameObject)), m_cNossle.transform.position, m_cNossle.transform.rotation);
		cBullet.rigidbody.velocity = cBullet.transform.forward * 40.0f;

		if(m_particleEmitter != null)
		{
			m_particleEmitter.Play();
		}

		CAudioCue[] audioCues = GetComponents<CAudioCue>();

		foreach(CAudioCue cue in audioCues)
		{
			if(cue.m_strCueName == "GunFire")
			{
				cue.Play(transform, 1.0f, false, -1);
			}
		}
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

        s_cSerializeStream.Write(NetworkView.ViewId);
	}


// Member Fields


	public GameObject m_cNossle = null;
	public  ParticleSystem m_particleEmitter = null;

	CNetworkVar<byte> m_bAmmo = null;


	float m_fShootTimer = 0.0f;
	float m_fShootInterval = 0.1f;


	byte m_bAmmoCapacity = 30;


	bool m_bShoot = false;

    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
