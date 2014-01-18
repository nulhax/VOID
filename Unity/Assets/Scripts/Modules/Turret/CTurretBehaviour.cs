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


	public GameObject TurretCameraNode
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
		m_tRotation = new CNetworkVar<Vector2>(OnNetworkVarSync, Vector2.zero);
		m_ulMountedPlayerId = new CNetworkVar<ulong>(OnNetworkVarSync, 0);
	}


	public void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_tRotation)
		{
			if (m_ulMountedPlayerId.Get() != CNetwork.PlayerId)
			{
				transform.eulerAngles = new Vector3(transform.eulerAngles.x, m_tRotation.Get().y, transform.eulerAngles.z);
				m_cBarrle.transform.eulerAngles = new Vector3(m_tRotation.Get().x, m_cBarrle.transform.eulerAngles.y, m_cBarrle.transform.eulerAngles.z);
			}
		}
		else if (_cSyncedVar == m_ulMountedPlayerId)
		{
			if (m_ulMountedPlayerId.Get() == CNetwork.PlayerId)
			{
				// Subscribe to input events
				CUserInput.EventMouseMoveX += new CUserInput.NotifyMouseInput(RotateX);
				CUserInput.EventMouseMoveY += new CUserInput.NotifyMouseInput(RotateY);
				CUserInput.EventPrimary += new CUserInput.NotifyKeyChange(OnFireLasersCommand);

				// Debug: Move camera to turret camera position
				CGame.CompositeCameraSystem.SetShipViewPerspective(m_cCameraObject.transform);
			}
			else if (m_ulMountedPlayerId.GetPrevious() == CNetwork.PlayerId)
			{
				// Unsubscriber to input events
				CUserInput.EventMouseMoveX -= new CUserInput.NotifyMouseInput(RotateX);
				CUserInput.EventMouseMoveY -= new CUserInput.NotifyMouseInput(RotateY);
				CUserInput.EventPrimary -= new CUserInput.NotifyKeyChange(OnFireLasersCommand);

				// Debug: Move camera to player head position
				CGame.CompositeCameraSystem.SetDefaultViewPerspective();
			}
		}
	}


	public void Start()
	{
		for (int i = 0; i < m_cBarrle.transform.childCount; ++ i)
		{
			if (m_cBarrle.transform.GetChild(i).gameObject.name == "LaserNode")
				m_cLaserNodes.Add(m_cBarrle.transform.GetChild(i).gameObject);
		}
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
		if (CNetwork.IsServer)
		{
			m_fServerFireTimer += Time.deltaTime;
		}
		
		// Sync rotation
		if (m_bSendRotation)
		{
			// Write update rotation action
			s_cSerializeStream.Write(NetworkView.ViewId);
			s_cSerializeStream.Write((byte)ENetworkAction.UpdateRotation);
			s_cSerializeStream.Write(m_cBarrle.transform.eulerAngles.x);
			s_cSerializeStream.Write(transform.eulerAngles.y);

			m_bSendRotation = true;
		}

		// Fire lasers
		m_fClientFireTimer += Time.deltaTime;

		if (m_bFireLasers)
		{
			if (m_fClientFireTimer > m_fClientFireInterval)
			{
				// Write fire lasers action
				s_cSerializeStream.Write(NetworkView.ViewId);
				s_cSerializeStream.Write((byte)ENetworkAction.FireLasers);

				m_fClientFireTimer = 0.0f;
			}
		}
	}


	[AServerOnly]
	public void Mount(ulong _ulPlayerId)
	{
		//Debug.Log(string.Format("Player ({0}) mounted turret", _ulPlayerId));

		m_ulMountedPlayerId.Set(_ulPlayerId);
	}


	[AServerOnly]
	public void Unmount()
	{
		//Debug.Log(string.Format("Player ({0}) unmounted turret", m_ulMountedPlayerId.Get()));

		m_ulMountedPlayerId.Set(0);
	}


	[AClientOnly]
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


	[AClientOnly]
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


	[AClientOnly]
	public void OnFireLasersCommand(bool _bDown)
	{
		m_bFireLasers = _bDown;
	}


	[AServerOnly]
	public void FireLasers()
	{
		if (m_fServerFireTimer > m_fServerFireInterval)
		{
			Vector3 projPos = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyPos(m_cLaserNodes[m_iLaserNodeIndex].transform.position);
			Quaternion projRot = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyRot(m_cLaserNodes[m_iLaserNodeIndex].transform.rotation);

			GameObject cProjectile = CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.LaserTurretProjectile);
			cProjectile.GetComponent<CNetworkView>().SetPosition(projPos);
			cProjectile.GetComponent<CNetworkView>().SetRotation(projRot.eulerAngles);

			++ m_iLaserNodeIndex;
			m_iLaserNodeIndex = (m_iLaserNodeIndex >= m_cLaserNodes.Count) ? 0 : m_iLaserNodeIndex;

			m_fServerFireTimer = 0.0f;
		}
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
			CTurretBehaviour cTurretController = CNetwork.Factory.FindObject(cTurretViewId).GetComponent<CTurretBehaviour>();
			ENetworkAction eAction = (ENetworkAction)_cStream.ReadByte();

			switch (eAction)
			{
			case ENetworkAction.UpdateRotation:
				float fRotationX = _cStream.ReadFloat();
				float fRotationY = _cStream.ReadFloat();
				cTurretController.m_tRotation.Set(new Vector2(fRotationX, fRotationY));
				break;

			case ENetworkAction.FireLasers:
				cTurretController.FireLasers();
				break;

			default:
				Debug.LogError(string.Format("Unknown network action ({0})", eAction));
				break;
			}
		}
	}


// Member Fields


	CNetworkVar<Vector2> m_tRotation = null;
	CNetworkVar<ulong> m_ulMountedPlayerId = null;


	List<GameObject> m_cLaserNodes = new List<GameObject>();


	public GameObject m_cCameraObject = null;
	public GameObject m_cBarrle = null;


	Vector3 m_vRotation = Vector3.zero;
	Vector2 m_vMinRotationY = new Vector2(-50.0f, -360.0f);
	Vector2 m_vMaxRotationY = new Vector2(60.0f, 360.0f);
	Vector2 m_vMinRotationX = new Vector2(-80, -60);
	Vector2 m_vMaxRotationX = new Vector2(0, 70);


	float m_fClientFireTimer	= 0.0f;
	float m_fClientFireInterval = 0.1f;
	float m_fServerFireTimer	= 0.0f;
	float m_fServerFireInterval = 0.1f;


	int m_iLaserNodeIndex = 0;


	bool m_bSendRotation = false;
	bool m_bFireLasers   = false;


	static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
