//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   PulseTurretSmallBehaviour.cs
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


public class CPulseTurretSmallBehaviour : CNetworkMonoBehaviour
{

// Member Types


    public enum ENetworkAction
    {
        FireLaser,
    }


// Member Delegates & Events


// Member Properties


// Member Methods


    public override void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar)
    {
        _cRegistrar.RegisterRpc(this, "RemoteFirePrimary");
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
            CPulseTurretSmallBehaviour cBehaviour = _cStream.Read<TNetworkViewId>().GameObject.GetComponent<CPulseTurretSmallBehaviour>();

            ENetworkAction eAction = _cStream.Read<ENetworkAction>();

            switch (eAction)
            {
                case ENetworkAction.FireLaser:
                    cBehaviour.InvokeRpcAll("RemoteFirePrimary", cBehaviour.m_cTurretBehaviour.GetRandomProjectileNode().transform.position,
                                                                 cBehaviour.m_cTurretBehaviour.GetRandomProjectileNode().transform.eulerAngles);
                    break;

                default:
                    Debug.LogError("Unknown network action: " + eAction);
                    break;
            }
        }
    }


	void Start()
	{
        m_cTurretBehaviour = GetComponent<CTurretBehaviour>();
        m_cTurretBehaviour.EventPrimaryFire += OnEventFirePrimary;
        m_cTurretBehaviour.EventSecondaryFire += OnEventFireSecondary;
	}


	void OnDestroy()
	{
	}


	void Update()
	{
        // Empty
	}


    [ANetworkRpc]
    void RemoteFirePrimary(Vector3 _vSpawnPosition, Vector3 _vDirection)
    {   
        GameObject cProjectile = Resources.Load(CNetwork.Factory.GetRegisteredPrefabFile(CGameRegistrator.ENetworkPrefab.LaserProjectile), typeof(GameObject)) as GameObject;
        cProjectile = GameObject.Instantiate(cProjectile, _vSpawnPosition, Quaternion.Euler(_vDirection)) as GameObject;
    }


    [ALocalOnly]
    void OnEventFirePrimary(CTurretBehaviour _cSender)
    {
        s_cSerializeStream.Write(NetworkViewId);
        s_cSerializeStream.Write(ENetworkAction.FireLaser);
    }


    [ALocalOnly]
    void OnEventFireSecondary(CTurretBehaviour _cSender)
    {
    }


// Member Fields


    CTurretBehaviour m_cTurretBehaviour = null;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
