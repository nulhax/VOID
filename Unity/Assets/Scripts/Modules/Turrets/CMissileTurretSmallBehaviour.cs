//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   MissileTurretSmallBehaviour.cs
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


public class CMissileTurretSmallBehaviour : CNetworkMonoBehaviour
{

// Member Types


    [ABitSize(4)]
    public enum ENetworkAction
    {
        FireMissileTarget,
        FireMissileNoTarget,
    }


// Member Delegates & Events


// Member Properties


// Member Methods


    public override void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar)
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
            CMissileTurretSmallBehaviour cBehaviour = _cStream.Read<TNetworkViewId>().GameObject.GetComponent<CMissileTurretSmallBehaviour>();

            ENetworkAction eAction = _cStream.Read<ENetworkAction>();

            Transform cRandomProjectileNode = cBehaviour.m_cTurretBehaviour.GetRandomProjectileNode();

            switch (eAction)
            {
                case ENetworkAction.FireMissileTarget:
                    {
                        GameObject cProjectile = CNetwork.Factory.CreateGameObject(CGameRegistrator.ENetworkPrefab.MissileProjectile);
                        cProjectile.GetComponent<CMissileProjectileBehaviour>().InvokeRpcAll("RemoteInitWithTarget", cRandomProjectileNode.transform.position,
                                                                                                                     cRandomProjectileNode.transform.eulerAngles,
                                                                                                                     _cStream.Read<TNetworkViewId>(),
                                                                                                                     _cStream.Read<Vector3>());
                    }
                    break;

                case ENetworkAction.FireMissileNoTarget:
                    {
                        GameObject cProjectile = CNetwork.Factory.CreateGameObject(CGameRegistrator.ENetworkPrefab.MissileProjectile);
                        cProjectile.GetComponent<CMissileProjectileBehaviour>().InvokeRpcAll("RemoteInitNoTarget", cRandomProjectileNode.transform.position,
                                                                                                                   cRandomProjectileNode.transform.eulerAngles);
                    }
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
	}


	void OnDestroy()
	{
	}


	void Update()
	{
	}


    [ALocalOnly]
    void OnEventFirePrimary(CTurretBehaviour _cSender)
    {
        RaycastHit[] taRaycastHits = m_cTurretBehaviour.ScanTargets(5000.0f);
        bool bTargetFound = false;

        foreach (RaycastHit cRaycastHit in taRaycastHits)
        {
            if (cRaycastHit.transform.GetComponent<CEnemyShip>() != null)
            {
                s_cSerializeStream.Write(NetworkViewId);
                s_cSerializeStream.Write(ENetworkAction.FireMissileTarget);
                s_cSerializeStream.Write(cRaycastHit.transform.GetComponent<CNetworkView>().ViewId);
                s_cSerializeStream.Write(cRaycastHit.point - cRaycastHit.transform.position);
                bTargetFound = true;
                break;
            }
        }

        if (!bTargetFound)
        {
            s_cSerializeStream.Write(NetworkViewId);
            s_cSerializeStream.Write(ENetworkAction.FireMissileNoTarget);
        }
    }


// Member Fields


    CTurretBehaviour m_cTurretBehaviour = null;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
