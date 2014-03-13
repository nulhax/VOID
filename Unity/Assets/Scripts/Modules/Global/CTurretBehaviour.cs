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


	public struct TRotation
	{
		public TRotation(float _fX, float _fY)
		{
			fX = _fX;
			fY = _fY;
		}

		public float fX;
		public float fY;
	}


// Member Delegates & Events


	public delegate void NotifyControlState();

	public event NotifyControlState EventTakenControl;
	public event NotifyControlState EventReleasedControl;

	public delegate void NotifyTurretRotation(Vector2 _Rotations, Vector2 _MinMaxEulerX);

	public event NotifyTurretRotation EventTurretRotated;

	public delegate void NotifyControllerChange(ulong _ulOldPlayerId, ulong _ulNewPlayerId);

	public event NotifyControllerChange EventControllerChange;


// Member Fields
	
	
	CNetworkVar<Vector2> m_tRotation = null;
	CNetworkVar<ulong> m_ulControllerPlayerId = null;
	
	
	public GameObject m_ShipCamera = null;
	public GameObject m_GalaxyCamera = null;
	public GameObject m_BackgroundCamera = null;
	public GameObject m_cBarrel = null;


	private RenderTexture m_CameraRenderTexture = null;
	
	float m_fMinRotationX		= 290.0f;
	float m_fMaxRotationX		= 359.9f;
	float m_fRotationSpeed		= 2.0f;
	
	
	static CNetworkStream s_cSerializeStream = new CNetworkStream();


// Member Properties


	public RenderTexture CameraRenderTexture
	{
		get { return(m_CameraRenderTexture); }
	}

	public GameObject TurretCameraNode
	{
		get { return (m_ShipCamera); }
	}


	public ulong ControllerPlayerId
	{
		get { return (m_ulControllerPlayerId.Get()); }
	}


	public bool IsUnderControl
	{
		get { return (ControllerPlayerId != 0); }
	}


	public Vector2 TurretRotations
	{
		get { return (m_tRotation.Get()); }
	}

	public Vector2 MinMaxRotationX
	{
		get { return (new Vector2(m_fMinRotationX, m_fMaxRotationX)); }
	}


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_tRotation = _cRegistrar.CreateNetworkVar<Vector2>(OnNetworkVarSync, Vector2.zero);
		m_ulControllerPlayerId = _cRegistrar.CreateNetworkVar<ulong>(OnNetworkVarSync, 0);
	}


	[AServerOnly]
	public void TakeControl(ulong _ulPlayerId)
	{
		//Debug.Log(string.Format("Player ({0}) mounted turret", _ulPlayerId));

		m_ulControllerPlayerId.Set(_ulPlayerId);

		if (EventTakenControl != null) 
			EventTakenControl();
	}


	[AServerOnly]
	public void ReleaseControl()
	{
		//Debug.Log(string.Format("Player ({0}) unmounted turret", m_ulMountedPlayerId.Get()));

		m_ulControllerPlayerId.Set(0);

		if (EventReleasedControl != null) 
			EventReleasedControl();
	}


	void Start()
	{
		CUserInput.SubscribeClientAxisChange(CUserInput.EAxis.MouseX, OnEventClientAxisControlTurret);
		CUserInput.SubscribeClientAxisChange(CUserInput.EAxis.MouseY, OnEventClientAxisControlTurret);

		// Disable the cameras to begin with
		m_ShipCamera.camera.enabled = false;
		m_GalaxyCamera.camera.enabled = false;
		m_BackgroundCamera.camera.enabled = false;

		// Create the rendertexture
		m_CameraRenderTexture = new RenderTexture(2500, 1000, 16);
		m_CameraRenderTexture.Create();

		// Attach the rt to the cameras
		m_ShipCamera.camera.targetTexture = m_CameraRenderTexture;
		m_GalaxyCamera.camera.targetTexture = m_CameraRenderTexture;
		m_BackgroundCamera.camera.targetTexture = m_CameraRenderTexture;
	}
	
	
	void OnDestroy()
	{
		// Empty
	}
	
	
	void Update()
	{
		if (ControllerPlayerId == CNetwork.PlayerId)
		{
			UpdateRotation();
		}
	}


	void LateUpdate()
	{
		// Update the camera position
		CGameShips.ShipGalaxySimulator.TransferFromSimulationToGalaxy(m_ShipCamera.transform.position, m_ShipCamera.transform.rotation, m_GalaxyCamera.transform);
	}


	[AClientOnly]
	void UpdateRotation()
	{
//        if (transform.FindChild("RatchetComponent").GetComponent<CActorHealth>().health > 0)
//        {
//            Vector2 vRotation = new Vector2(m_cBarrel.transform.eulerAngles.x, transform.rotation.eulerAngles.y);
//
//            // Update rotations
//            vRotation.x += CUserInput.MouseMovementY;
//            vRotation.y += CUserInput.MouseMovementX;
//
//            // Clamp rotation
//            vRotation.x = Mathf.Clamp(vRotation.x, m_fMinRotationX, m_fMaxRotationX);
//
//            // Apply rotations to objects
//            m_cBarrel.transform.localEulerAngles = new Vector3(vRotation.x, 0.0f, 0.0f);
//            transform.localEulerAngles = new Vector3(0.0f, vRotation.y, 0.0f);
//
//            // Write update rotation action
//            s_cSerializeStream.Write(ThisNetworkView.ViewId);
//            s_cSerializeStream.Write((byte)ENetworkAction.UpdateRotation);
//            s_cSerializeStream.Write(vRotation.x);
//            s_cSerializeStream.Write(vRotation.y);
//        }
	}

	[AServerOnly]
	private void OnEventClientAxisControlTurret(CUserInput.EAxis _eAxis, ulong _ulPlayerId, float _fValue)
	{
		if(_ulPlayerId == m_ulControllerPlayerId.Get())
		{
            if (transform.FindChild("MechanicalComponent").GetComponent<CActorHealth>().health > 0)
            {
                switch (_eAxis)
                {
                    case CUserInput.EAxis.MouseX:
                        {
                            Vector2 vRotation = new Vector2(m_cBarrel.transform.eulerAngles.x, transform.rotation.eulerAngles.y);

                            // Update rotation
                            vRotation.y += _fValue;

                            // Apply rotations to turret
                            transform.localEulerAngles = new Vector3(0.0f, vRotation.y, 0.0f);
                            m_cBarrel.transform.localEulerAngles = new Vector3(vRotation.x, 0.0f, 0.0f);

                            // Server updates the rotation for other clients
                            m_tRotation.Set(new Vector2(vRotation.x, vRotation.y));
                            break;
                        }

                    case CUserInput.EAxis.MouseY:
                        {
                            Vector2 vRotation = new Vector2(m_cBarrel.transform.eulerAngles.x, transform.rotation.eulerAngles.y);

                            // Update rotation
                            vRotation.x += _fValue;
                            vRotation.x = Mathf.Clamp(vRotation.x, m_fMinRotationX, m_fMaxRotationX);

                            // Apply rotations to turret
                            transform.localEulerAngles = new Vector3(0.0f, vRotation.y, 0.0f);
                            m_cBarrel.transform.localEulerAngles = new Vector3(vRotation.x, 0.0f, 0.0f);

                            // Server updates the rotation for other clients
                            m_tRotation.Set(new Vector2(vRotation.x, vRotation.y));
                            break;
                        }

                    default:
                        Debug.LogError("Unknown input");
                        break;
                }
            }
		}
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_tRotation)
		{
			// Update the rotation of the turret
			transform.localEulerAngles = new Vector3(0.0f, m_tRotation.Get().y, 0.0f);
			m_cBarrel.transform.localEulerAngles = new Vector3(m_tRotation.Get().x, 0.0f, 0.0f);

			if(EventTurretRotated != null)
				EventTurretRotated(m_tRotation.Get(), MinMaxRotationX);
		}
		else if (_cSyncedVar == m_ulControllerPlayerId)
		{
			if(ControllerPlayerId != 0)
			{
				// Enable the cameras
				m_ShipCamera.camera.enabled = true;
				m_GalaxyCamera.camera.enabled = true;
				m_BackgroundCamera.camera.enabled = true;
			}
			else
			{
				// Disable the cameras
				m_ShipCamera.camera.enabled = false;
				m_GalaxyCamera.camera.enabled = false;
				m_BackgroundCamera.camera.enabled = false;
			}

			if (EventControllerChange != null)
				EventControllerChange(m_ulControllerPlayerId.GetPrevious(), m_ulControllerPlayerId.Get());
		}
	}


};
