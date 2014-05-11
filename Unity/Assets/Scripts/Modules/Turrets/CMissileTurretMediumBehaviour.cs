//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   MissileTurretMediumBehaviour.cs
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


public class CMissileTurretMediumBehaviour : CNetworkMonoBehaviour
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


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
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
            CMissileTurretMediumBehaviour cBehaviour = _cStream.Read<TNetworkViewId>().GameObject.GetComponent<CMissileTurretMediumBehaviour>();

            ENetworkAction eAction = _cStream.Read<ENetworkAction>();

            Transform cRandomProjectileNode = cBehaviour.m_cTurretInterface.ProjectileNodes[cBehaviour.m_cTurretInterface.GetNextProjectileNodeIndex()];

            switch (eAction)
            {
                case ENetworkAction.FireMissileTarget:
                    {
                        GameObject cProjectile = CNetwork.Factory.CreateGameObject(CGameRegistrator.ENetworkPrefab.MissileProjectile);
                        cProjectile.GetComponent<CMissileProjectileBehaviour>().InvokeRpcAll("RemoteInitWithTarget", CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyPos(cRandomProjectileNode.transform.position),
                                                                                                                     CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyRot(cRandomProjectileNode.transform.rotation).eulerAngles,
                                                                                                                     _cStream.Read<TNetworkViewId>(),
                                                                                                                     _cStream.Read<Vector3>());
                    }
                    break;

                case ENetworkAction.FireMissileNoTarget:
                    {
                        GameObject cProjectile = CNetwork.Factory.CreateGameObject(CGameRegistrator.ENetworkPrefab.MissileProjectile);
                        cProjectile.GetComponent<CMissileProjectileBehaviour>().InvokeRpcAll("RemoteInitNoTarget", CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyPos(cRandomProjectileNode.transform.position),
                                                                                                                   CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyRot(cRandomProjectileNode.transform.rotation).eulerAngles);
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
        m_cTurretInterface = GetComponent<CTurretInterface>();

        m_cTurretInterface.EventPrimaryFire += OnEventFirePrimary;
        m_cTurretInterface.EventPrimaryFire += OnEventFireSecondary;
	}


	void OnDestroy()
	{
	}


	void Update()
	{
	}


    void OnEventFirePrimary(CTurretInterface _cSender)
    {
        RaycastHit[] taRaycastHits = m_cTurretInterface.ScanTargets(5000.0f);
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


    void OnEventFireSecondary(CTurretInterface _cSender)
    {
    }


// Member Fields


    CTurretInterface m_cTurretInterface = null;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
