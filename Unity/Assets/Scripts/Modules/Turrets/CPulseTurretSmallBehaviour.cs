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
            CPulseTurretSmallBehaviour cBehaviour = _cStream.Read<TNetworkViewId>().GameObject.GetComponent<CPulseTurretSmallBehaviour>();

            ENetworkAction eAction = _cStream.Read<ENetworkAction>();

            switch (eAction)
            {
                case ENetworkAction.FireLaser:
                    {
                        Transform cRandomNode = cBehaviour.m_cTurretInterface.GetRandomProjectileNode();

                        GameObject cProjectile = CNetwork.Factory.CreateGameObject(CGameRegistrator.ENetworkPrefab.LaserProjectile);
                        cProjectile.GetComponent<CNetworkView>().SetPosition(cRandomNode.position);
                        cProjectile.GetComponent<CNetworkView>().SetRotation(cRandomNode.rotation);
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
        m_cTurretInterface.EventSecondaryFire += OnEventFireSecondary;
	}


	void OnDestroy()
	{
	}


	void Update()
	{
        if (m_bRotateBarrel)
        {
            m_cBarrel.localEulerAngles = m_cBarrel.localEulerAngles + new Vector3(0.0f, 0.0f, -720.0f * Time.deltaTime);

            m_bRotateBarrel = false;
        }
	}


    [ANetworkRpc]
    void RemoteFirePrimary(Vector3 _vSpawnPosition, Vector3 _vDirection)
    {   
    }


    [ALocalOnly]
    void OnEventFirePrimary(CTurretInterface _cSender)
    {
        s_cSerializeStream.Write(NetworkViewId);
        s_cSerializeStream.Write(ENetworkAction.FireLaser);

        m_bRotateBarrel = true;
    }


    [ALocalOnly]
    void OnEventFireSecondary(CTurretInterface _cSender)
    {
    }


// Member Fields


    public Transform m_cBarrel = null;


    CTurretInterface m_cTurretInterface = null;


    bool m_bRotateBarrel = false;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
