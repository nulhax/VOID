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


// Member Properties


	public TNetworkViewId ActiveTurretViewId
	{
		get 
		{ 
			return (m_cActiveTurretViewId.Value); 
		}
	}


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
		m_cActiveTurretViewId = _cRegistrar.CreateReliableNetworkVar<TNetworkViewId>(OnNetworkVarSync, null);
        m_vRotation = _cRegistrar.CreateUnreliableNetworkVar<Vector2>(OnNetworkVarSync, 1.0f / CNetworkConnection.k_fOutboundRate, Vector2.zero);

        // Dont need to sync when not mounted
        if (CNetwork.IsServer)
        {
            m_vRotation.SetSyncEnabled(false);
        }
    }


    [ALocalOnly]
    public static void SerializeOutbound(CNetworkStream _cStream)
    {
        if (s_cLocalOwnedTurretCockpitBehaviour != null)
        {
            _cStream.Write(ENetworkAction.SyncRotation);
            _cStream.Write(s_cLocalOwnedTurretCockpitBehaviour.NetworkViewId);
            _cStream.Write(s_cLocalOwnedTurretCockpitBehaviour.m_cChairModelTrans.transform.localEulerAngles.x);
            _cStream.Write(s_cLocalOwnedTurretCockpitBehaviour.m_cChairModelTrans.transform.localEulerAngles.y);
        }

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
                case ENetworkAction.SyncRotation:
                    cTurretCockpitBehaviour.m_vRotation.Value = new Vector2(_cStream.Read<float>(), _cStream.Read<float>());
                    break;

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

        // Subscribe to cockpit events - Does not need to unsubscribe
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
        if (m_cCockpit.MountedPlayerId == CNetwork.PlayerId)
            return;

        Quaternion qRemoteRotation = Quaternion.Euler(m_vRotation.Value.x, m_vRotation.Value.y, 0.0f);

        if (Quaternion.Angle(m_cChairModelTrans.transform.localRotation, qRemoteRotation) > 90.0f)
        {
            m_cChairModelTrans.transform.localRotation = qRemoteRotation;
        }
        else
        {
            m_cChairModelTrans.transform.localRotation = Quaternion.RotateTowards(m_cChairModelTrans.transform.localRotation,
                                                                                  qRemoteRotation, 
                                                                                  720.0f * Time.deltaTime);
        }
    }


	void OnEventCockpitMounted(ulong _ulPlayerId)
	{
        if (_ulPlayerId == CNetwork.PlayerId)
        {
            CUserInput.SubscribeAxisChange(CUserInput.EAxis.MouseX, OnEventInputAxisChange);
            CUserInput.SubscribeAxisChange(CUserInput.EAxis.MouseY, OnEventInputAxisChange);

            s_cLocalOwnedTurretCockpitBehaviour = this;
        }

        if (CNetwork.IsServer)
        {
            List<GameObject> acTurrets = CModuleInterface.FindModulesByType(CModuleInterface.EType.LaserTurret);

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

            m_vRotation.SetSyncEnabled(true);
        }
	}


	void OnEventCockpitUnmounted(ulong _ulPlayerId)
	{
        if (_ulPlayerId == CNetwork.PlayerId)
        {
            CUserInput.UnsubscribeAxisChange(CUserInput.EAxis.MouseX, OnEventInputAxisChange);
            CUserInput.UnsubscribeAxisChange(CUserInput.EAxis.MouseY, OnEventInputAxisChange);

            s_cLocalOwnedTurretCockpitBehaviour = null;
        }

        if (CNetwork.IsServer)
        {
            if (ActiveTurretViewId != null)
            {
                m_cActiveTurretViewId.Set(null);
            }

            m_vRotation.SetSyncEnabled(false);
        }

		//Debug.Log("Player left cockpit");
	}


    [ALocalOnly]
    void OnEventInputAxisChange(CUserInput.EAxis _eAxis, float _fValue)
    {
        switch (_eAxis)
        {
            case CUserInput.EAxis.MouseX:
                m_cChairModelTrans.transform.Rotate(0.0f, _fValue, 0.0f);
                break;

            case CUserInput.EAxis.MouseY:
                break;

            default:
                Debug.LogError("Unknown user input axis: " + _eAxis);
                break;
        }
    }


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_cActiveTurretViewId)
		{
            if (m_cActiveTurretViewId.PreviousValue != null)
            {
                // Release control of previous turret
                ActiveTurretViewId.GameObject.GetComponent<CTurretBehaviour>().ReleaseControl();
            }

			if (m_cActiveTurretViewId.Value != null)
			{
                // Take control of turret
                ActiveTurretViewId.GameObject.GetComponent<CTurretBehaviour>().TakeControl(NetworkViewId);


				CTurretBehaviour tb = m_cActiveTurretViewId.Get().GameObject.GetComponent<CTurretBehaviour>();

				// Register the handling cockpit rotations
				//tb.EventTurretRotated += HandleCockpitRotations;

				// Set initial states
				//HandleCockpitRotations(tb.Rotation, tb.MinMaxRotationX);

				// Set the render texture from the turret
				//m_CockpitScreen.renderer.material.SetTexture("_MainTex", tb.CameraRenderTexture);
				//m_CockpitScreen2.renderer.material.SetTexture("_MainTex", tb.CameraRenderTexture);
			}
		}
	}


// Member Fields


    public Transform m_cChairModelTrans = null;
    public Vector2 m_MinMaxEulerX = new Vector2(340.0f, 370.0f);


    CNetworkVar<TNetworkViewId> m_cActiveTurretViewId = null;
    CNetworkVar<Vector2> m_vRotation = null;

    CModuleInterface m_cModuleInterface = null;
    CCockpit m_cCockpit = null;


    static CTurretCockpitBehaviour s_cLocalOwnedTurretCockpitBehaviour = null;
    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
