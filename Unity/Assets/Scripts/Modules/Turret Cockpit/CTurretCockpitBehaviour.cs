//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CTurretCockpitBehaviour.cs
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


[RequireComponent(typeof(CCockpitInterface))]
public class CTurretCockpitBehaviour : CNetworkMonoBehaviour
{

// Member Types


    public enum ENetworkAction
    {
        SyncRotation
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
            ENetworkAction cAction = _cStream.Read<ENetworkAction>();

            CTurretCockpitBehaviour cTurretCockpitBehaviour = _cStream.Read<TNetworkViewId>().GameObject.GetComponent<CTurretCockpitBehaviour>();

            switch (cAction)
            {
                default:
                    Debug.LogError("Unknown network action: " + cAction);
                    break;
            }
        }
    }


	void Start()
	{
        m_cModuleInterface = GetComponent<CModuleInterface>();
        m_cCockpitInterface = GetComponent<CCockpitInterface>();

        // Subscribe to cockpit events - Does not need to Unsubscribe
        m_cCockpitInterface.EventMounted    += OnEventCockpitMounted;
        m_cCockpitInterface.EventDismounted += OnEventCockpitUnmounted;
	}


    void Update()
    {
        UpdateRemoteRotationAlignment();
    }


	void OnDestroy()
	{
        // Empty
	}


    void UpdateRemoteRotationAlignment()
    {
        Vector3 vTargetLocalEuler = Vector3.forward;

        if (m_cCockpitInterface.IsMounted)
        {
            GameObject cMountedPlayerActor = m_cCockpitInterface.MountedPlayerActor;

            CPlayerTurretBehaviour cPlayerTurretBehaviour = cMountedPlayerActor.GetComponent<CPlayerTurretBehaviour>();

            if (cPlayerTurretBehaviour.HasTurretControl)
            {
                CTurretInterface cControllingTurretInterface = cMountedPlayerActor.GetComponent<CPlayerTurretBehaviour>().ControlledTurretInterface;

                if (cControllingTurretInterface.RotationRatioX >= 0.0f)
                {
                    vTargetLocalEuler.x = 360.0f - (m_fRotationMaxX * cControllingTurretInterface.RotationRatioX);
                }
                else
                {
                    vTargetLocalEuler.x = m_fRotationMinX * cControllingTurretInterface.RotationRatioX;
                }

                vTargetLocalEuler.y = 360.0f * cControllingTurretInterface.RotationRatioY;
            }
        }

        Quaternion qTargetRotation = Quaternion.Euler(vTargetLocalEuler.x, vTargetLocalEuler.y, 0.0f);

        if (m_cCockpitInterface.IsMounted &&
            Quaternion.Angle(m_cChairModelTrans.transform.localRotation, qTargetRotation) > 90.0f)
        {
            m_cChairModelTrans.transform.localRotation = qTargetRotation;
        }
        else
        {
            // Rotate slower when returning to default rotation
            float fRotationSpeed = m_cCockpitInterface.IsMounted ? 720.0f : 180.0f;

            m_cChairModelTrans.transform.localRotation = Quaternion.RotateTowards(m_cChairModelTrans.transform.localRotation,
                                                                                  qTargetRotation,
                                                                                  fRotationSpeed * Time.deltaTime);
        }
    }


    void OnEventCockpitMounted(CCockpitInterface _cSender, ulong _ulPlayerId)
	{
        Debug.Log("Player entered cockpit");
	}


    void OnEventCockpitUnmounted(CCockpitInterface _cSender, ulong _ulPlayerId)
	{
		Debug.Log("Player left cockpit");
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
        // Empty
	}


// Member Fields


    public Transform m_cChairModelTrans = null;
    public float m_fRotationMinX = -20.0f;
    public float m_fRotationMaxX =  15.0f;

    
    CModuleInterface m_cModuleInterface = null;
    CCockpitInterface m_cCockpitInterface = null;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
