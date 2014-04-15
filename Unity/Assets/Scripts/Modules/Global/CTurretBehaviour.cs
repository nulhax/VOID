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


	public delegate void UnderControlChangeHandler(CTurretBehaviour _cSender, bool _bUnderControl);
    public event UnderControlChangeHandler EventUnderControlChange;


// Member Properties


	public RenderTexture CameraRenderTexture
	{
		get { return(m_CameraRenderTexture); }
	}


	public GameObject CameraNode
	{
		get { return (m_cShipCamera); }
	}


	public bool IsUnderControl
	{
		get { return (m_bUnderControl.Value); }
	}


	public TRotation Rotation
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
        m_tRotation = _cRegistrar.CreateReliableNetworkVar<TRotation>(OnNetworkVarSync, new TRotation());
		m_bUnderControl = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, false);
	}


	[AServerOnly]
	public void SetUnderControl(bool _bUnderControl)
	{
        m_bUnderControl.Value = _bUnderControl;
	}


	void Start()
	{
		CUserInput.SubscribeClientAxisChange(CUserInput.EAxis.MouseX, OnEventClientAxisControlTurret);
		CUserInput.SubscribeClientAxisChange(CUserInput.EAxis.MouseY, OnEventClientAxisControlTurret);

		// Disable the cameras to begin with
		m_cShipCamera.camera.enabled = false;
		m_cGalaxyCamera.camera.enabled = false;
		m_cBackgroundCamera.camera.enabled = false;

		// Create the rendertexture
		m_CameraRenderTexture = new RenderTexture(2500, 1000, 16);
		m_CameraRenderTexture.Create();

		// Attach the rt to the cameras
		m_cShipCamera.camera.targetTexture = m_CameraRenderTexture;
		m_cGalaxyCamera.camera.targetTexture = m_CameraRenderTexture;
		m_cBackgroundCamera.camera.targetTexture = m_CameraRenderTexture;
	}
	
	
	void OnDestroy()
	{
		// Empty
	}
	
	
	void Update()
	{
		//if (ControllerPlayerId == CNetwork.PlayerId)
		{
			UpdateRotation();
		}
	}


	void LateUpdate()
	{
		// Update the camera position
		CGameShips.ShipGalaxySimulator.TransferFromSimulationToGalaxy(m_cShipCamera.transform.position, m_cShipCamera.transform.rotation, m_cGalaxyCamera.transform);
	}


	[ALocalOnly]
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
        /*
		if(_ulPlayerId == m_bUnderControl.Get())
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
         * */
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_tRotation)
		{
			// Update the rotation of the turret
			transform.localEulerAngles = new Vector3(0.0f, m_tRotation.Value.fY, 0.0f);
			m_cBarrel.transform.localEulerAngles = new Vector3(m_tRotation.Value.fX, 0.0f, 0.0f);
		}
		else if (_cSyncedVar == m_bUnderControl)
		{
            if (EventUnderControlChange != null)
                EventUnderControlChange(this, m_bUnderControl.Value);

			if(ControllerPlayerId != 0)
			{
				// Enable the cameras
				m_cShipCamera.camera.enabled = true;
				m_cGalaxyCamera.camera.enabled = true;
				m_cBackgroundCamera.camera.enabled = true;
			}
			else
			{
				// Disable the cameras
				m_cShipCamera.camera.enabled = false;
				m_cGalaxyCamera.camera.enabled = false;
				m_cBackgroundCamera.camera.enabled = false;
			}
		}
	}


// Member Fields


    CNetworkVar<TRotation> m_tRotation = null;
    CNetworkVar<bool> m_bUnderControl = null;


    public GameObject m_cShipCamera = null;
    public GameObject m_cGalaxyCamera = null;
    public GameObject m_cBackgroundCamera = null;
    public GameObject m_cBarrel = null;


    private RenderTexture m_CameraRenderTexture = null;

    float m_fMinRotationX = 290.0f;
    float m_fMaxRotationX = 359.9f;
    float m_fRotationSpeed = 2.0f;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
