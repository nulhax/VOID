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


    public CCockpit OwnerCockpitBehaviour
    {
        get
        {
            if (OwnerTurretCockpit == null)
                return (null);

            return (OwnerTurretCockpit.GetComponent<CCockpit>());
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


	public bool IsUnderControl
	{
        get { return (m_cOwnerCockpitViewId.Value != null); }
	}


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
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
        m_cBase.transform.Rotate(_fX, 0.0f, 0.0f);
    }


    [ALocalOnly]
    public void RotateY(float _fY)
    {
        m_cBase.transform.Rotate(0.0f, _fY, 0.0f);
    }


    [ALocalOnly]
    public static void SerializeOutbound(CNetworkStream _cStream)
    {
        if (s_cLocalOwnedTurretBehaviour != null)
        {
            _cStream.Write(ENetworkAction.UpdateRotation);
            _cStream.Write(s_cLocalOwnedTurretBehaviour.NetworkViewId);
            _cStream.Write(s_cLocalOwnedTurretBehaviour.m_cBarrel.transform.localEulerAngles.x);
            _cStream.Write(s_cLocalOwnedTurretBehaviour.m_cBase.transform.localEulerAngles.y);
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
        // Empty
	}


	void LateUpdate()
	{
		// Update the camera position
		//CGameShips.ShipGalaxySimulator.TransferFromSimulationToGalaxy(m_cShipCamera.transform.position, m_cShipCamera.transform.rotation, m_cGalaxyCamera.transform);
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_cOwnerCockpitViewId)
		{
            // Check I own this turret locally
            if (OwnerCockpitBehaviour.MountedPlayerId == CNetwork.PlayerId)
            {
                s_cLocalOwnedTurretBehaviour = this;

                // Enable the cameras
                m_cShipCamera.camera.enabled = true;
                m_cGalaxyCamera.camera.enabled = true;

                m_cOwnerCockpitViewId.Value.GameObject.GetComponent<CTurretCockpitBehaviour>().Screen.renderer.material.mainTexture = m_cCameraRenderTexture;
            }

            // Check I do not own this turret locally anymore
            if (s_cLocalOwnedTurretBehaviour == this &&
                OwnerCockpitBehaviour.MountedPlayerId != CNetwork.PlayerId)
            {
                s_cLocalOwnedTurretBehaviour = null;

                // Disable the cameras
                m_cShipCamera.camera.enabled = false;
                m_cGalaxyCamera.camera.enabled = false;

                m_cOwnerCockpitViewId.Value.GameObject.GetComponent<CTurretCockpitBehaviour>().Screen.renderer.material.mainTexture = null;
            }

            if (EventCockpitOwnerChange != null)
                EventCockpitOwnerChange(this, m_cOwnerCockpitViewId.Value);
		}
	}


// Member Fields


    public Camera m_cShipCamera = null;
    public Camera m_cGalaxyCamera = null;
    public GameObject m_cBase = null;
    public GameObject m_cBarrel = null;
    public float m_fRotationSpeed = 2.0f;
    public float m_fEulerMinX = -15.0f;
    public float m_fEulerMaxX =  80.0f;


    CNetworkVar<TNetworkViewId> m_cOwnerCockpitViewId = null;
    CNetworkVar<float> m_fRemoteRotationX = null;
    CNetworkVar<float> m_fRemoteRotationY = null;

    RenderTexture m_cCameraRenderTexture = null;

    static CTurretBehaviour s_cLocalOwnedTurretBehaviour = null;


};
