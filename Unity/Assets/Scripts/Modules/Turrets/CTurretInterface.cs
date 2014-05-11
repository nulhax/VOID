//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CTurretBehaviour.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;


/* Implementation */


public class CTurretInterface : CNetworkMonoBehaviour
{

// Member Types


	public enum ENetworkAction
	{
		UpdateRotation
	}


// Member Delegates & Events


	public delegate void ControlTakenHandler(CTurretInterface _cSender, ulong _ulPlayerId);
    public event ControlTakenHandler EventControlTaken;


    public delegate void ControlReleasedHandler(CTurretInterface _cSender, ulong _ulPlayerId);
    public event ControlReleasedHandler EventControlReleased;


    public delegate void PrimaryFireHandler(CTurretInterface _cSender);
    public event PrimaryFireHandler EventPrimaryFire;


    public delegate void SecondaryFireHandler(CTurretInterface _cSender);
    public event SecondaryFireHandler EventSecondaryFire;


// Member Properties


    public ulong ControllerPlayerId
    {
        get 
        {
            return (m_ulControllerPlayerId.Value); 
        }
    }


    public Transform[] ProjectileNodes
    {
        get
        {
            return (m_caProjectileNodes);
        }
    }


    public float RotationRatioX
    {
        get
        {
            Vector3 vBarrelLocalEurer = m_cBarrelTrans.transform.localEulerAngles;

            if (vBarrelLocalEurer.x > 180.0f)
            {
                vBarrelLocalEurer.x -= 360.0f;
            }

            if (vBarrelLocalEurer.x < 0.0f)
            {
                return (Mathf.Abs(vBarrelLocalEurer.x / m_fBarrelEulerMaxX));
            }
            else
            {
                return (vBarrelLocalEurer.x / m_fBarrelEulerMinX);
            }
        }
    }


    public float RotationRatioY
    {
        get
        {
            return (m_cBaseTrans.transform.localEulerAngles.y / 360.0f);
        }
    }


	public bool IsUnderControl
	{
        get { return (m_ulControllerPlayerId.Value != 0); }
	}


// Member Methods


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
        m_ulControllerPlayerId = _cRegistrar.CreateReliableNetworkVar<ulong>(OnNetworkVarSync, 0);
        m_fRemoteRotationX = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fRemoteRotationY = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}


	[AServerOnly]
	public bool TakeControl(ulong _ulPlayerId)
	{
        bool bControlTaken = false;

        if (m_ulControllerPlayerId.Value == 0)
        {
            m_ulControllerPlayerId.Value = _ulPlayerId;
            bControlTaken = true;
        }

        return (bControlTaken);
	}


    [AServerOnly]
    public void ReleaseControl()
    {
        if (m_ulControllerPlayerId.Value == null)
            Debug.LogError("Cannot release control of turret if there is no owner");

        m_ulControllerPlayerId.Value = 0;
    }


    [ALocalOnly]
    public void RotateX(float _fX)
    {
        m_cBaseTrans.transform.Rotate(_fX, 0.0f, 0.0f);
    }


    [ALocalOnly]
    public void RotateY(float _fY)
    {
        m_cBaseTrans.transform.Rotate(0.0f, _fY, 0.0f);
    }


    public int GetNextProjectileNodeIndex()
    {
        m_iProjectileNodeIndex++;

        if (m_iProjectileNodeIndex >= m_caProjectileNodes.Length)
        {
            m_iProjectileNodeIndex = 0;
        }

        return (m_iProjectileNodeIndex);
    }


    
    public int GetRandomProjectileNodeIndex()
    {
        return (UnityEngine.Random.Range(0, m_caProjectileNodes.Length));
    }


    public Transform GetRandomProjectileNode()
    {
        return (m_caProjectileNodes[UnityEngine.Random.Range(0, m_caProjectileNodes.Length)]);
    }

   
    [ALocalOnly]
    public RaycastHit[] ScanTargets(float _fRayRange)
    {
        Ray cRay = new Ray(CGameCameras.GalaxyCamera.transform.position, CGameCameras.GalaxyCamera.transform.forward);
        RaycastHit[] taRaycastHits = Physics.RaycastAll(cRay, _fRayRange, 1 << LayerMask.NameToLayer("Galaxy"));
        taRaycastHits.OrderBy((_tRaycastHit) => _tRaycastHit.distance);

        return (taRaycastHits);
    }


    [ALocalOnly]
    public static void SerializeOutbound(CNetworkStream _cStream)
    {
        if (CGamePlayers.SelfActor == null)
            return;

        CPlayerTurretBehaviour cPlayerTurretBehaviour = CGamePlayers.SelfActor.GetComponent<CPlayerTurretBehaviour>();
        
        if (cPlayerTurretBehaviour.HasTurretControl)
        {
            CTurretInterface cControlledTurretInterface = cPlayerTurretBehaviour.ControlledTurretInterface;

            _cStream.Write(ENetworkAction.UpdateRotation);
            _cStream.Write(cControlledTurretInterface.NetworkViewId);
            _cStream.Write(cControlledTurretInterface.m_cBarrelTrans.transform.localEulerAngles.x);
            _cStream.Write(cControlledTurretInterface.m_cBaseTrans.transform.localEulerAngles.y);
        }
    }


    [AServerOnly]
    public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        while (_cStream.HasUnreadData)
        {
            // Extract network action
            ENetworkAction eNetworkAction = _cStream.Read<ENetworkAction>();

            // Extract turret
            GameObject cTurret = _cStream.Read<TNetworkViewId>().GameObject;

            // Process action
            switch (eNetworkAction)
            {
                case ENetworkAction.UpdateRotation:
                    cTurret.GetComponent<CTurretInterface>().m_fRemoteRotationX.Value = _cStream.Read<float>();
                    cTurret.GetComponent<CTurretInterface>().m_fRemoteRotationY.Value = _cStream.Read<float>();
                    break;

                default:
                    Debug.LogError("Unknown network action: " + eNetworkAction);
                    break;
            }
        }
    }


	void Start()
	{
        m_fPrimaryFireTimer = m_fPrimaryFireInterval;
	}
	
	
	void OnDestroy()
	{
		// Empty
	}
	
	
	void Update()
	{
        if (IsUnderControl &&
            m_ulControllerPlayerId.Value == CNetwork.PlayerId)
        {
            m_fPrimaryFireTimer += Time.deltaTime;
            m_fSecondaryFireTimer = Time.deltaTime;

            if (m_fPrimaryFireTimer > m_fPrimaryFireInterval &&
                CUserInput.IsInputDown(CUserInput.EInput.Primary))
            {
                if (EventPrimaryFire != null)
                    EventPrimaryFire(this);

                m_fPrimaryFireTimer = 0.0f;
                
            }

            if (m_fSecondaryFireTimer > m_fSecondaryFireInterval && 
                CUserInput.IsInputDown(CUserInput.EInput.Secondary))
            {
                if (EventSecondaryFire != null)
                    EventSecondaryFire(this);

                m_fSecondaryFireTimer = 0.0f;
            }
        }
	}


    [ALocalOnly]
    void OnEventInputAxisChange(CUserInput.EAxis _eAxis, float _fValue)
    {
        if (!CCursorControl.IsCursorLocked)
            return;

        Vector3 vBaseLocalEuler = m_cBaseTrans.transform.localEulerAngles;
        Vector3 vBarrelLocalEuler = m_cBarrelTrans.transform.localEulerAngles;

        switch (_eAxis)
        {
            case CUserInput.EAxis.MouseX:
                vBaseLocalEuler.y += _fValue;
                break;

            case CUserInput.EAxis.MouseY:
                {
                    // Make rotations relevant to 180
                    if (vBarrelLocalEuler.x > 180.0f)
                        vBarrelLocalEuler.x -= 360.0f;

                    // Bound rotations to their limits
                    vBarrelLocalEuler.x = Mathf.Clamp(vBarrelLocalEuler.x + _fValue, m_fBarrelEulerMaxX * -1, m_fBarrelEulerMinX * -1);
                }
                break;

            default:
                Debug.LogError("Unknown user input axis: " + _eAxis);
                break;
        }

        m_cBaseTrans.transform.localEulerAngles = vBaseLocalEuler;
        m_cBarrelTrans.transform.localEulerAngles = vBarrelLocalEuler;
    }


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_ulControllerPlayerId)
		{
            HandleVarSyncControllingPlayerId();
		}
        else if (_cSyncedVar == m_fRemoteRotationX ||
                 _cSyncedVar == m_fRemoteRotationY)
        {
            HandleRemoteRotationChange();
        }
	}


    void HandleVarSyncControllingPlayerId()
    {
        // Check I own this turret locally
        if (m_ulControllerPlayerId.Value != 0 &&
            m_ulControllerPlayerId.Value == CNetwork.PlayerId)
        {
            CGameCameras.SetObserverSpace(true);
            CGameCameras.SetObserverPerspective(m_cCameraNode);
            CGameHUD.SetHUDState(true);

            CUserInput.SubscribeAxisChange(CUserInput.EAxis.MouseX, OnEventInputAxisChange);
            CUserInput.SubscribeAxisChange(CUserInput.EAxis.MouseY, OnEventInputAxisChange);
        }

        // Check I do not own this turret locally anymore
        if (m_ulControllerPlayerId.PreviousValue == CNetwork.PlayerId)
        {
            CGameCameras.SetObserverSpace(true);
            CGameCameras.SetObserverPerspective(CGamePlayers.SelfActorHead);
            CGameHUD.SetHUDState(true);

            CUserInput.UnsubscribeAxisChange(CUserInput.EAxis.MouseX, OnEventInputAxisChange);
            CUserInput.UnsubscribeAxisChange(CUserInput.EAxis.MouseY, OnEventInputAxisChange);
        }

        if (m_ulControllerPlayerId.Value == 0)
        {
            if (EventControlReleased != null)
                EventControlReleased(this, m_ulControllerPlayerId.PreviousValue);
        }
        else
        {
            if (EventControlTaken != null)
                EventControlTaken(this, m_ulControllerPlayerId.Value);
        }
    }


    void HandleRemoteRotationChange()
    {
        if (m_ulControllerPlayerId.Value != CNetwork.PlayerId)
        {
            Vector3 vBaseLocalEuler = m_cBaseTrans.transform.localEulerAngles;
            Vector3 vBarrelLocalEuler = m_cBarrelTrans.transform.localEulerAngles;

            vBaseLocalEuler.y = m_fRemoteRotationY.Value;
            vBarrelLocalEuler.x = m_fRemoteRotationX.Value;

            m_cBaseTrans.transform.localEulerAngles = vBaseLocalEuler;
            m_cBarrelTrans.transform.localEulerAngles = vBarrelLocalEuler;
        }
    }


// Member Fields


    public GameObject m_cCameraNode = null;
    public Transform m_cBaseTrans = null;
    public Transform m_cBarrelTrans = null;
    public Transform[] m_caProjectileNodes = null;
    public float m_fRotationSpeed = 2.0f;
    public float m_fBarrelEulerMinX = -15.0f;
    public float m_fBarrelEulerMaxX =  80.0f;
    public float m_fPrimaryFireInterval = 0.1f;
    public float m_fSecondaryFireInterval = 0.1f;


    CNetworkVar<ulong> m_ulControllerPlayerId = null;
    CNetworkVar<float> m_fRemoteRotationX = null;
    CNetworkVar<float> m_fRemoteRotationY = null;

    float m_fPrimaryFireTimer = 0.0f;
    float m_fSecondaryFireTimer = 0.0f;

    int m_iProjectileNodeIndex = 0;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
