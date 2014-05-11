//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   PulseTurretMediumBehaviour.cs
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


public class CPulseTurretMediumBehaviour : CNetworkMonoBehaviour
{

// Member Types


    public enum ENetworkAction
    {
        FireLaser,
    }


// Member Delegates & Events


// Member Properties


// Member Methods


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        //_cRegistrar.RegisterRpc(this, "RemoteFirePrimary");
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
            CPulseTurretMediumBehaviour cBehaviour = _cStream.Read<TNetworkViewId>().GameObject.GetComponent<CPulseTurretMediumBehaviour>();

            ENetworkAction eAction = _cStream.Read<ENetworkAction>();

            switch (eAction)
            {
                case ENetworkAction.FireLaser:
                    {
                        int iIndex = cBehaviour.m_cTurretInterface.GetNextProjectileNodeIndex();

                        GameObject cProjectile = CNetwork.Factory.CreateGameObject(CGameRegistrator.ENetworkPrefab.LaserProjectile);
                        cProjectile.GetComponent<CNetworkView>().SetPosition(CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyPos(cBehaviour.m_cTurretInterface.m_caProjectileNodes[iIndex].position));
                        cProjectile.GetComponent<CNetworkView>().SetRotation(CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyRot(cBehaviour.m_cTurretInterface.m_caProjectileNodes[iIndex].rotation));
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
        if (m_bRotateBarrels)
        {
            foreach (Transform cTrans in m_caBarrels)
            {
                cTrans.localEulerAngles = cTrans.localEulerAngles + new Vector3(0.0f, 0.0f, 45.0f);
            }

            m_bRotateBarrels = false;
        }
	}


    [ALocalOnly]
    void OnEventFirePrimary(CTurretInterface _cSender)
    {
        s_cSerializeStream.Write(NetworkViewId);
        s_cSerializeStream.Write(ENetworkAction.FireLaser);

        m_bRotateBarrels = true;
    }


    [ALocalOnly]
    void OnEventFireSecondary(CTurretInterface _cSender)
    {
    }


// Member Fields


    public Transform[] m_caBarrels = null;


    CTurretInterface m_cTurretInterface = null;


    bool m_bRotateBarrels = false;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
