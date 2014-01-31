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


	[AClientOnly]
	public delegate void HandleTakenControl();
	public event HandleTakenControl EventTakenControl;


	[AServerOnly]
	public delegate void HandleReleasedControl();
	public event HandleTakenControl EventReleasedControl;


	[AClientOnly]
	public delegate void HandleControllerChange(ulong _ulOldPlayerId, ulong _ulNewPlayerId);
	public event HandleControllerChange EventControllerChange;


// Member Properties


	public GameObject TurretCameraNode
	{
		get { return (m_cCameraObject); }
	}


	public ulong ControllerPlayerId
	{
		get { return (m_ulControllerPlayerId.Get()); }
	}


	public bool IsUnderControl
	{
		get { return (ControllerPlayerId != 0); }
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

		if (EventTakenControl != null) EventTakenControl();
	}


	[AServerOnly]
	public void ReleaseControl()
	{
		//Debug.Log(string.Format("Player ({0}) unmounted turret", m_ulMountedPlayerId.Get()));

		m_ulControllerPlayerId.Set(0);

		if (EventReleasedControl != null) EventReleasedControl();
	}


	[AClientOnly]
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
			CNetworkViewId cTurretViewId = _cStream.ReadNetworkViewId();
			GameObject cTurretObject = CNetwork.Factory.FindObject(cTurretViewId);

			if (cTurretObject != null)
			{
				CTurretBehaviour cTurretBehaviour = cTurretObject.GetComponent<CTurretBehaviour>();
				ENetworkAction eAction = (ENetworkAction)_cStream.ReadByte();

				switch (eAction)
				{
				case ENetworkAction.UpdateRotation:
					float fRotationX = _cStream.ReadFloat();
					float fRotationY = _cStream.ReadFloat();
					cTurretBehaviour.m_tRotation.Set(new Vector2(fRotationX, fRotationY));
					break;

				default:
					Debug.LogError(string.Format("Unknown network action ({0})", eAction));
					break;
				}
			}
		}
	}


	void Start()
	{
		// Empty
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


	[AClientOnly]
	void UpdateRotation()
	{
		Vector2 vRotation = new Vector2(m_cBarrel.transform.eulerAngles.x, transform.rotation.eulerAngles.y);
		
		// Update rotations
		vRotation.x += CUserInput.MouseMovementY * m_fRotationSpeed;
		vRotation.y += CUserInput.MouseMovementX * m_fRotationSpeed;
		
		// Clamp rotation
		vRotation.x = Mathf.Clamp(vRotation.x, m_fMinRotationX, m_fMaxRotationX);
		
		// Apply rotations to objects
		m_cBarrel.transform.localEulerAngles = new Vector3(vRotation.x, 0.0f, 0.0f);
		transform.localEulerAngles = new Vector3(0.0f, vRotation.y, 0.0f);
		
		// Write update rotation action
		s_cSerializeStream.Write(ThisNetworkView.ViewId);
		s_cSerializeStream.Write((byte)ENetworkAction.UpdateRotation);
		s_cSerializeStream.Write(vRotation.x);
		s_cSerializeStream.Write(vRotation.y);
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_tRotation)
		{
			if (ControllerPlayerId != CNetwork.PlayerId)
			{
				//Debug.Log("Hello");
				transform.eulerAngles = new Vector3(transform.eulerAngles.x, m_tRotation.Get().y, transform.eulerAngles.z);
				m_cBarrel.transform.eulerAngles = new Vector3(m_tRotation.Get().x, m_cBarrel.transform.eulerAngles.y, m_cBarrel.transform.eulerAngles.z);
			}
		}
		else if (_cSyncedVar == m_ulControllerPlayerId)
		{
			if (ControllerPlayerId == CNetwork.PlayerId)
			{
				// Debug: Move camera to turret camera position
				CGame.CompositeCameraSystem.SetPlayersViewPerspectiveToShip(m_cCameraObject.transform);
			}
			else if (m_ulControllerPlayerId.GetPrevious() == CNetwork.PlayerId)
			{
				// Debug: Move camera to player head position
				CGame.CompositeCameraSystem.SetDefaultViewPerspective();
			}

			if (EventControllerChange != null) EventControllerChange(m_ulControllerPlayerId.GetPrevious(), m_ulControllerPlayerId.Get());
		}
	}


// Member Fields


	CNetworkVar<Vector2> m_tRotation = null;
	CNetworkVar<ulong> m_ulControllerPlayerId = null;


	public GameObject m_cCameraObject = null;
	public GameObject m_cBarrel = null;


	float m_fMinRotationX		= 290.0f;
	float m_fMaxRotationX		= 359.9f;
	float m_fRotationSpeed		= 2.0f;


	static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
