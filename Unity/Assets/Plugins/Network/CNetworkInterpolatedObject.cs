//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CNetworkInterpolatedObject.cs
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


public class CNetworkInterpolatedObject : CNetworkMonoBehaviour
{

// Member Types


	public struct TPosition
	{
		public TPosition(float _fX, float _fY, float _fZ)
		{
			fX = _fX;
			fY = _fY;
			fZ = _fZ;
		}

		public float fX;
		public float fY;
		public float fZ;
	}


// Member Delegates & Events


// Member Properties


// Member Methods


	public override void InstanceNetworkVars()
	{
		m_tPosition = new CNetworkVar<TPosition>(OnNetworkVarSync);
	}


	public void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		if (_cSyncedNetworkVar == m_tPosition)
		{
			transform.position = new Vector3(m_tPosition.Get().fX, m_tPosition.Get().fY, m_tPosition.Get().fZ);
		}
	}


	public void Start()
	{
		//rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
	}


	public void OnDestroy()
	{
	}


	public void Update()
	{
		m_fInterpolationTimer += Time.deltaTime;


		if (m_fInterpolationTimer >= 1.0f)
		{
			m_fInterpolationTimer -= 1.0f;
		}


		int iHistoryElement = (int)((float)CNetworkConnection.k_uiOutboundRate * m_fInterpolationTimer);
		m_vaPositions[iHistoryElement] = transform.position;
		m_vaRotations[iHistoryElement] = transform.rotation;


		//if (CNetwork.IsServer)
		//{

		//}
	}


	[AServerMethod]
	public void SetCurrentPosition(Vector3 _vPosition)
	{
		Logger.WriteErrorOn(!CNetwork.IsServer, "Only servers can set the interpolated objects current position");

		m_tPosition.Set(new TPosition(_vPosition.x, _vPosition.y, _vPosition.z));
	}


// Member Fields


	CNetworkVar<TPosition> m_tPosition = null;


	float m_fInterpolationTimer = 0.0f;


	Vector3[] m_vaPositions = new Vector3[CNetworkConnection.k_uiOutboundRate];
	Quaternion[] m_vaRotations = new Quaternion[CNetworkConnection.k_uiOutboundRate];


};
