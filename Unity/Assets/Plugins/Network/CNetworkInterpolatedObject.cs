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


	public const float m_fInterp = 0.1f;


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
			InsertNewPosition();
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
		return;
		//if (CGame.PlayerActor == gameObject)
		{
			//return;
		}


		m_fInterpolationTimer += Time.deltaTime;


		if (m_fInterpolationTimer >= 1.0f)
		{
			m_fInterpolationTimer -= 1.0f;
		}


		float fCurrentTick = CNetwork.Connection.Tick;
		fCurrentTick -= m_fInterp;

		if (fCurrentTick < 0.0f)
		{
			fCurrentTick = CNetworkServer.k_fSendRate + fCurrentTick; // Set new tick rate
		}

		//rigidbody.position = new Vector3(m_vaPositions[(int)fCurrentTick].x, m_vaPositions[(int)fCurrentTick].y, m_vaPositions[(int)fCurrentTick].z);
		//Debug.LogError((int)fCurrentTick);
		//int iHistoryElement = (int)((float)CNetworkConnection.k_uiOutboundRate * m_fInterpolationTimer);
		//m_vaPositions[iHistoryElement] = transform.position;
		//m_vaRotations[iHistoryElement] = transform.rotation;


		//if (CNetwork.IsServer)
		//{

		//}
	}


	[AServerMethod]
	public void SetCurrentPosition(Vector3 _vPosition)
	{
		return;
		Logger.WriteErrorOn(!CNetwork.IsServer, "Only servers can set the interpolated objects current position");

		m_tPosition.Set(new TPosition(_vPosition.x, _vPosition.y, _vPosition.z));
	}


	void InsertNewPosition()
	{
		return;
		float fSyncedTime = m_tPosition.GetLastSyncedTick();

		// Calcuate which tick to put the position
		uint uiSyncedTick = (uint)((fSyncedTime - Mathf.Floor(fSyncedTime)) * CNetworkServer.k_fSendRate);


		m_vaPositions[uiSyncedTick] = new Vector3(m_tPosition.Get().fX, m_tPosition.Get().fY, m_tPosition.Get().fZ);
	}


	static uint CalculateTickAtTime()
	{
		uint uiTick = 0;


		return (uiTick);
	}


// Member Fields


	CNetworkVar<TPosition> m_tPosition = null;


	float m_fInterpolationTimer = 0.0f;


	Vector3[] m_vaPositions = new Vector3[(int)CNetworkServer.k_fSendRate];
	Quaternion[] m_vaRotations = new Quaternion[(int)CNetworkServer.k_fSendRate];


};
