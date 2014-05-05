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


public class CTurretBehaviour : CNetworkMonoBehaviour
{

// Member Types


	public enum ENetworkAction
	{
		UpdateRotation
	}


// Member Delegates & Events


	public delegate void CockpitOwnerChangeHandler(CTurretBehaviour _cSender, TNetworkViewId _tOwnerCockputViewId);
    public event CockpitOwnerChangeHandler EventCockpitOwnerChange;


    public delegate void PrimaryFireHandler(CTurretBehaviour _cSender);
    public event PrimaryFireHandler EventPrimaryFire;


    public delegate void SecondaryFireHandler(CTurretBehaviour _cSender);
    public event SecondaryFireHandler EventSecondaryFire;


// Member Properties


	public RenderTexture CameraRenderTexture
	{
		get { return(m_cCameraRenderTexture); }
	}


    public Camera CameraShip
	{
		get { return (m_cShipCamera); }
	}


    public GameObject OwnerTurretCockpit
    {
        get 
        {
            if (m_cOwnerCockpitViewId.Value == null)
                return null;

            return (m_cOwnerCockpitViewId.Value.GameObject); 
        }
    }


    public CCockpitBehaviour OwnerCockpitBehaviour
    {
        get
        {
            if (OwnerTurretCockpit == null)
                return (null);

            return (OwnerTurretCockpit.GetComponent<CCockpitBehaviour>());
        }
    }


    public CTurretCockpitBehaviour OwnerTurretCockpitBehaviour
    {
        get
        {
            if (OwnerTurretCockpit == null)
                return (null);

            return (OwnerTurretCockpit.GetComponent<CTurretCockpitBehaviour>());
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
        get { return (m_cOwnerCockpitViewId.Value != null); }
	}


// Member Methods


	public override void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar)
	{
        m_cOwnerCockpitViewId = _cRegistrar.CreateReliableNetworkVar<TNetworkViewId>(OnNetworkVarSync, null);
        m_fRemoteRotationX = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fRemoteRotationY = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}


	[AServerOnly]
	public void TakeControl(TNetworkViewId _cOwnerCockpitViewId)
	{
        if (m_cOwnerCockpitViewId.Value != null)
            Debug.LogError("Cannot take control of turret until previous control has been released");

        m_cOwnerCockpitViewId.Value = _cOwnerCockpitViewId;
	}


    [AServerOnly]
    public void ReleaseControl()
    {
        if (m_cOwnerCockpitViewId.Value == null)
            Debug.LogError("Cannot release control of turret if there is no owner");

        m_cOwnerCockpitViewId.Value = null;
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


    public Transform GetRandomProjectileNode()
    {
        return (m_caProjectileNodes[UnityEngine.Random.Range(0, m_caProjectileNodes.Length)]);
    }

    
    [ALocalOnly]
    public RaycastHit[] ScanTargets(float _fRayRange)
    {
        Ray cRay = new Ray(m_cGalaxyCamera.transform.position, m_cGalaxyCamera.transform.forward);
        RaycastHit[] taRaycastHits = Physics.RaycastAll(cRay, _fRayRange, 1 << LayerMask.NameToLayer("Galaxy"));
        taRaycastHits.OrderBy((_tRaycastHit) => _tRaycastHit.distance);

        return (taRaycastHits);
    }


    [ALocalOnly]
    public static void SerializeOutbound(CNetworkStream _cStream)
    {
        if (s_cLocalOwnedTurretBehaviour != null)
        {
            _cStream.Write(ENetworkAction.UpdateRotation);
            _cStream.Write(s_cLocalOwnedTurretBehaviour.NetworkViewId);
            _cStream.Write(s_cLocalOwnedTurretBehaviour.m_cBarrelTrans.transform.localEulerAngles.x);
            _cStream.Write(s_cLocalOwnedTurretBehaviour.m_cBaseTrans.transform.localEulerAngles.y);
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
                    cTurret.GetComponent<CTurretBehaviour>().m_fRemoteRotationX.Value = _cStream.Read<float>();
                    cTurret.GetComponent<CTurretBehaviour>().m_fRemoteRotationY.Value = _cStream.Read<float>();
                    break;

                default:
                    Debug.LogError("Unknown network action: " + eNetworkAction);
                    break;
            }
        }
    }


	void Start()
	{
		// Disable the cameras to begin with
		m_cShipCamera.camera.enabled = false;
		m_cGalaxyCamera.camera.enabled = false;

		// Create the render texture
		m_cCameraRenderTexture = new RenderTexture(Screen.width, Screen.height, 24);
		m_cCameraRenderTexture.Create();

		// Attach the rt to the cameras
		m_cShipCamera.camera.targetTexture = m_cCameraRenderTexture;
		m_cGalaxyCamera.camera.targetTexture = m_cCameraRenderTexture;
	}
	
	
	void OnDestroy()
	{
		// Empty
	}
	
	
	void Update()
	{
        if (s_cLocalOwnedTurretBehaviour == this)
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


	void LateUpdate()
	{
		// Update the camera position
		CGameShips.ShipGalaxySimulator.TransferFromSimulationToGalaxy(m_cShipCamera.transform.position, m_cShipCamera.transform.rotation, m_cGalaxyCamera.transform);
	}


    [ALocalOnly]
    void OnEventInputAxisChange(CUserInput.EAxis _eAxis, float _fValue)
    {
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
		if (_cSyncedVar == m_cOwnerCockpitViewId)
		{
            HandleOwnerCockpitChange();
		}
        else if (_cSyncedVar == m_fRemoteRotationX ||
                 _cSyncedVar == m_fRemoteRotationY)
        {
            HandleRemoteRotationChange();
        }
	}


    void HandleOwnerCockpitChange()
    {
        if (m_cOwnerCockpitViewId.Value != null)
        {
            // Check I own this turret locally
            if (OwnerCockpitBehaviour.MountedPlayerId == CNetwork.PlayerId)
            {
                s_cLocalOwnedTurretBehaviour = this;

                // Enable the cameras
                m_cShipCamera.camera.enabled = true;
                m_cGalaxyCamera.camera.enabled = true;

                m_cOwnerCockpitViewId.Value.GameObject.GetComponent<CTurretCockpitBehaviour>().Screen.renderer.material.SetTexture("_UI", m_cCameraRenderTexture);

                CUserInput.SubscribeAxisChange(CUserInput.EAxis.MouseX, OnEventInputAxisChange);
                CUserInput.SubscribeAxisChange(CUserInput.EAxis.MouseY, OnEventInputAxisChange);
            }
        }

        // Check I do not own this turret locally anymore
        if (s_cLocalOwnedTurretBehaviour == this &&
            (OwnerCockpitBehaviour == null ||
             OwnerCockpitBehaviour.MountedPlayerId != CNetwork.PlayerId))
        {
            s_cLocalOwnedTurretBehaviour = null;

            // Disable the cameras
            m_cShipCamera.camera.enabled = false;
            m_cGalaxyCamera.camera.enabled = false;

            m_cOwnerCockpitViewId.PreviousValue.GameObject.GetComponent<CTurretCockpitBehaviour>().Screen.renderer.material.SetTexture("_UI", null);

            CUserInput.UnsubscribeAxisChange(CUserInput.EAxis.MouseX, OnEventInputAxisChange);
            CUserInput.UnsubscribeAxisChange(CUserInput.EAxis.MouseY, OnEventInputAxisChange);
        }

        if (EventCockpitOwnerChange != null)
            EventCockpitOwnerChange(this, m_cOwnerCockpitViewId.Value);
    }


    void HandleRemoteRotationChange()
    {
        
    }


// Member Fields


    public Camera m_cShipCamera = null;
    public Camera m_cGalaxyCamera = null;
    public Transform m_cBaseTrans = null;
    public Transform m_cBarrelTrans = null;
    public Transform[] m_caProjectileNodes = null;
    public float m_fRotationSpeed = 2.0f;
    public float m_fBarrelEulerMinX = -15.0f;
    public float m_fBarrelEulerMaxX =  80.0f;
    public float m_fPrimaryFireInterval = 0.1f;
    public float m_fSecondaryFireInterval = 0.1f;


    CNetworkVar<TNetworkViewId> m_cOwnerCockpitViewId = null;
    CNetworkVar<float> m_fRemoteRotationX = null;
    CNetworkVar<float> m_fRemoteRotationY = null;

    RenderTexture m_cCameraRenderTexture = null;

    float m_fPrimaryFireTimer = 0.0f;
    float m_fSecondaryFireTimer = 0.0f;

    static CTurretBehaviour s_cLocalOwnedTurretBehaviour = null;


};
