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
		Rotation,
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


// Member Methods


	public override void InstanceNetworkVars()
	{
		m_tRotation = new CNetworkVar<Vector2>(OnNetworkVarSync);
	}


	public void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_tRotation)
		{
			transform.eulerAngles = new Vector3(m_tRotation.Get().x, m_tRotation.Get().y, transform.eulerAngles.z);
		}
	}


	public void Start()
	{
		
	}


	public void OnDestroy()
	{
	}


	public void Update()
	{
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


	public GameObject m_cCameraObject = null;


	Vector3 m_vRotation = Vector3.zero;
	Vector2 m_vMinRotationY = new Vector2(-50.0f, -360.0f);
	Vector2 m_vMaxRotationY = new Vector2(60.0f, 360.0f);
	Vector2 m_vMinRotationX = new Vector2(-80, -60);
	Vector2 m_vMaxRotationX = new Vector2(0, 70);


	bool m_bSendRotation = true;


	static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
