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


[RequireComponent(typeof(CCockpit))]
public class CTurretCockpitBehaviour : CNetworkMonoBehaviour
{

// Member Types


    public enum ENetworkAction
    {
        SyncRotation
    }


// Member Delegates & Events


    public GameObject Screen
    {
        get { return (m_cScreen); }
    }


// Member Properties


	public TNetworkViewId ActiveTurretViewId
	{
		get 
		{ 
			return (m_cActiveTurretViewId.Value); 
		}
	}


    public CTurretBehaviour ActiveTurretBehaviour
    {
        get
        {
            if (ActiveTurretViewId == null)
            {
                return (null);
            }

            return (m_cActiveTurretViewId.Value.GameObject.GetComponent<CTurretBehaviour>());
        }
    }


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
		m_cActiveTurretViewId = _cRegistrar.CreateReliableNetworkVar<TNetworkViewId>(OnNetworkVarSync, null);
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
        m_cCockpit = GetComponent<CCockpit>();

        // Subscribe to cockpit events - Does not need to Unsubscribe
        m_cCockpit.EventMounted    += OnEventCockpitMounted;
        m_cCockpit.EventDismounted += OnEventCockpitUnmounted;
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

        if (ActiveTurretViewId != null)
        {
            if (ActiveTurretBehaviour.RotationRatioX >= 0.0f)
            {
                vTargetLocalEuler.x = 360.0f - (m_fRotationMaxX * ActiveTurretBehaviour.RotationRatioX);
            }
            else
            {
                vTargetLocalEuler.x = m_fRotationMinX * ActiveTurretBehaviour.RotationRatioX;
            }

            vTargetLocalEuler.y = 360.0f * ActiveTurretBehaviour.RotationRatioY;
        }

        Quaternion qTargetRotation = Quaternion.Euler(vTargetLocalEuler.x, vTargetLocalEuler.y, 0.0f);

        if (m_cCockpit.IsMounted &&
            Quaternion.Angle(m_cChairModelTrans.transform.localRotation, qTargetRotation) > 90.0f)
        {
            m_cChairModelTrans.transform.localRotation = qTargetRotation;
        }
        else
        {
            // Rotate slower when returning to default rotation
            float fRotationSpeed = m_cCockpit.IsMounted ? 720.0f : 180.0f;

            m_cChairModelTrans.transform.localRotation = Quaternion.RotateTowards(m_cChairModelTrans.transform.localRotation,
                                                                                  qTargetRotation,
                                                                                  fRotationSpeed * Time.deltaTime);
        }
    }


	void OnEventCockpitMounted(ulong _ulPlayerId)
	{
        if (_ulPlayerId == CNetwork.PlayerId)
        {
            Camera cam = CGameCameras.MainCamera.camera;
            float pos = (cam.nearClipPlane + 0.01f);
            m_cScreen.transform.position = cam.transform.position + cam.transform.forward * pos;
            float h = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f) * pos * 2f;
            m_cScreen.transform.localScale = new Vector3(h * cam.aspect, h, 0f);

            s_cLocalOwnedTurretCockpitBehaviour = this;
        }

        if (CNetwork.IsServer)
        {
            List<GameObject> acTurrets = CModuleInterface.FindModulesByCategory(CModuleInterface.ECategory.Turrets);

            if (acTurrets != null &&
                acTurrets.Count > 0)
            {
                foreach (GameObject cTurretObject in acTurrets)
                {
                    if (!cTurretObject.GetComponent<CTurretBehaviour>().IsUnderControl)
                    {
                        m_cActiveTurretViewId.Set(cTurretObject.GetComponent<CNetworkView>().ViewId);
                        break;
                    }
                }
            }
        }
	}


	void OnEventCockpitUnmounted(ulong _ulPlayerId)
	{
        if (_ulPlayerId == CNetwork.PlayerId)
        {
            s_cLocalOwnedTurretCockpitBehaviour = null;
        }

        if (CNetwork.IsServer)
        {
            if (ActiveTurretViewId != null)
            {
                m_cActiveTurretViewId.Set(null);
            }
        }

		//Debug.Log("Player left cockpit");
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_cActiveTurretViewId)
		{
            HandleActiveTurretChange();
		}
	}


    void HandleActiveTurretChange()
    {
        if (CNetwork.IsServer)
        {
            if (m_cActiveTurretViewId.PreviousValue != null)
            {
                // Release control of previous turret
                m_cActiveTurretViewId.PreviousValue.GameObject.GetComponent<CTurretBehaviour>().ReleaseControl();
            }

            if (m_cActiveTurretViewId.Value != null)
            {
                // Take control of turret
                ActiveTurretViewId.GameObject.GetComponent<CTurretBehaviour>().TakeControl(NetworkViewId);
            }
        }
    }


// Member Fields


    public GameObject m_cScreen = null;
    public Transform m_cChairModelTrans = null;
    public float m_fRotationMinX = -20.0f;
    public float m_fRotationMaxX =  15.0f;


    CNetworkVar<TNetworkViewId> m_cActiveTurretViewId = null;

    CModuleInterface m_cModuleInterface = null;
    CCockpit m_cCockpit = null;


    static CTurretCockpitBehaviour s_cLocalOwnedTurretCockpitBehaviour = null;
    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
