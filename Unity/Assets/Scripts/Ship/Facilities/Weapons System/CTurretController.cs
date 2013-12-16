//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CTurretController.cs
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


public class CTurretController : CNetworkMonoBehaviour
{

// Member Types


	public enum ENetworkAction
	{
		UpdateRotation,
		FireLasers,
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


// Member Properties


	public GameObject TurretCamera
	{
		get { return (m_cCameraObject); }
	}


	public ulong MountedPlayerId
	{
		get { return (m_ulMountedPlayerId.Get()); }
	}


	public bool IsMounted
	{
		get { return (MountedPlayerId != 0); }
	}


// Member Methods


	public override void InstanceNetworkVars()
	{
		m_tRotation = new CNetworkVar<Vector2>(OnNetworkVarSync);
		m_ulMountedPlayerId = new CNetworkVar<ulong>(OnNetworkVarSync);
	}


	public void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_tRotation)
		{
			transform.eulerAngles = new Vector3(m_tRotation.Get().x, m_tRotation.Get().y, transform.eulerAngles.z);
		}
		else if (_cSyncedVar == m_ulMountedPlayerId)
		{
			Debug.Log(m_ulMountedPlayerId.Get());
			Debug.Log(m_ulMountedPlayerId.GetPrevious());
			if (m_ulMountedPlayerId.Get() == CNetwork.PlayerId)
			{
				// Subscribe to input events
				CGame.UserInput.EventMouseMoveX += new CUserInput.NotifyMouseInput(RotateX);
				CGame.UserInput.EventMouseMoveY += new CUserInput.NotifyMouseInput(RotateY);
				CGame.UserInput.EventPrimary += new CUserInput.NotifyKeyChange(FireLasers);

				// Enabled turret camera
				m_cCameraObject.camera.enabled = true;
			}
			else if (m_ulMountedPlayerId.GetPrevious() == CNetwork.PlayerId)
			{
				// Unsubscriber to input events
				CGame.UserInput.EventMouseMoveX -= new CUserInput.NotifyMouseInput(RotateX);
				CGame.UserInput.EventMouseMoveY -= new CUserInput.NotifyMouseInput(RotateY);
				CGame.UserInput.EventPrimary -= new CUserInput.NotifyKeyChange(FireLasers);

				// Disable turret camera
				m_cCameraObject.camera.enabled = false;
			}
		}
	}


	public void Start()
	{
		// Empty
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
		// Check to send rotation
		if (m_bSendRotation)
		{
			// Write update rotation action
			s_cSerializeStream.Write((byte)ENetworkAction.UpdateRotation);
			s_cSerializeStream.Write(transform.rotation.x);
			s_cSerializeStream.Write(transform.rotation.y);

			m_bSendRotation = true;
		}

		m_fFireTimer += Time.deltaTime;

		if (m_bFireLasers)
		{
			if (m_fFireTimer < m_fFireInterval)
			{
				// Write fire lasers action
				s_cSerializeStream.Write((byte)ENetworkAction.UpdateRotation);
				s_cSerializeStream.Write(transform.rotation.x);
				s_cSerializeStream.Write(transform.rotation.y);

				m_fFireTimer = 0.0f;
			}
		}
	}


	[AServerMethod]
	public void Mount(ulong _ulPlayerId)
	{
		Debug.Log(string.Format("Player ({0}) mounted turret", _ulPlayerId));

		m_ulMountedPlayerId.Set(_ulPlayerId);
	}


	[AServerMethod]
	public void Unmount()
	{
		Debug.Log(string.Format("Player ({0}) unmounted turret", m_ulMountedPlayerId.GetPrevious()));

		m_ulMountedPlayerId.Set(0);
	}


	[AClientMethod]
	public void RotateX(float _fAmount)
	{
		m_vRotation.y += _fAmount;

		// Keep y rotation within 360 range
		m_vRotation.y -= (m_vRotation.y >= 360.0f) ? 360.0f : 0.0f;
		m_vRotation.y += (m_vRotation.y <= -360.0f) ? 360.0f : 0.0f;

		// Clamp rotation
		m_vRotation.y = Mathf.Clamp(m_vRotation.y, m_vMinRotationY.y, m_vMaxRotationY.y);

		transform.localEulerAngles = new Vector3(0.0f, m_vRotation.y, 0.0f);

		m_bSendRotation = true;
	}


	[AClientMethod]
	public void RotateY(float _fAmount)
	{
		// Retrieve new rotations
		m_vRotation.x += _fAmount;

		// Clamp rotation
		m_vRotation.x = Mathf.Clamp(m_vRotation.x, m_vMinRotationX.x, m_vMaxRotationX.x);

		// Apply the pitch to the camera
		transform.FindChild("TurretBarrels").localEulerAngles = new Vector3(m_vRotation.x, 0.0f, 0.0f);

		m_bSendRotation = true;
	}


	[AClientMethod]
	public void FireLasers(bool _bDown)
	{
		m_bFireLasers = _bDown;
	}


	[AClientMethod]
	public static void SerializeOutbound(CNetworkStream _cStream)
    {
		_cStream.Write(s_cSerializeStream);

		s_cSerializeStream.Clear();
    }


	[AServerMethod]
	public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
	}


// Member Fields


	CNetworkVar<Vector2> m_tRotation = null;
	CNetworkVar<ulong> m_ulMountedPlayerId = null;


	public GameObject m_cCameraObject = null;


	Vector3 m_vRotation = Vector3.zero;
	Vector2 m_vMinRotationY = new Vector2(-50.0f, -360.0f);
	Vector2 m_vMaxRotationY = new Vector2(60.0f, 360.0f);
	Vector2 m_vMinRotationX = new Vector2(-80, -60);
	Vector2 m_vMaxRotationX = new Vector2(0, 70);


	float m_fFireTimer	  = 0.0f;
	float m_fFireInterval = 0.1f;


	bool m_bSendRotation = false;
	bool m_bFireLasers   = false;


	static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
