//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLaserTurretBehaviour.cs
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


[RequireComponent(typeof(CTurretBehaviour))]
public class CLaserTurretBehaviour : CNetworkMonoBehaviour
{

// Member Types


	public enum ENetworkAction
	{
		FireLasers,
	}


// Member Delegates & Events


// Member Properties


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		// Empty
	}


	[AClientOnly]
	public static void SerializeOutbound(CNetworkStream _cStream)
	{
		_cStream.Write(s_cSerializeStream);
		s_cSerializeStream.Clear();
	}
	
	
	[AServerOnly]
	public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{;
		while (_cStream.HasUnreadData)
		{
			CNetworkViewId cTurretViewId = _cStream.ReadNetworkViewId();
			GameObject cTurretObject = CNetwork.Factory.FindObject(cTurretViewId);
			
			if (cTurretObject != null)
			{
				CLaserTurretBehaviour cLaserTurretBehaviour = cTurretObject.GetComponent<CLaserTurretBehaviour>();
				ENetworkAction eAction = (ENetworkAction)_cStream.ReadByte();
				
				switch (eAction)
				{
				case ENetworkAction.FireLasers:
					cLaserTurretBehaviour.FireLasers();
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
		GetComponent<CTurretBehaviour>().EventControllerChange += OnTurretControllerChange;
	}


	void OnDestroy()
	{
		GetComponent<CTurretBehaviour>().EventControllerChange -= OnTurretControllerChange;
	}


	void Update()
	{
		if (CNetwork.IsServer)
		{
			m_fServerFireTimer += Time.deltaTime;
		}

		if (GetComponent<CTurretBehaviour>().ControllerPlayerId == CNetwork.PlayerId)
		{
			UpdateFiring();
		}
	}


	[AClientOnly]
	void UpdateFiring()
	{
		// Fire lasers
		m_fClientFireTimer += Time.deltaTime;
		
		if (m_bFireLasers)
		{
			if (m_fClientFireTimer > m_fClientFireInterval)
			{
				// Write fire lasers action
				s_cSerializeStream.Write(ThisNetworkView.ViewId);
				s_cSerializeStream.Write((byte)ENetworkAction.FireLasers);
				
				m_fClientFireTimer = 0.0f;
			}
		}
	}


	[AServerOnly]
	void FireLasers()
	{
		if (m_fServerFireTimer > m_fServerFireInterval)
		{
			Vector3 projPos = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyPos(m_aLasterNodes[m_iLaserNodeIndex].transform.position);
			Quaternion projRot = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyRot(m_aLasterNodes[m_iLaserNodeIndex].transform.rotation);
			
			GameObject cProjectile = CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.LaserTurretProjectile);
			cProjectile.GetComponent<CNetworkView>().SetPosition(projPos);
			cProjectile.GetComponent<CNetworkView>().SetRotation(projRot.eulerAngles);
			
			++ m_iLaserNodeIndex;
			m_iLaserNodeIndex = (m_iLaserNodeIndex >= m_aLasterNodes.Length) ? 0 : m_iLaserNodeIndex;
			
			m_fServerFireTimer = 0.0f;
		}
	}


	[AClientOnly]
	void OnTurretControllerChange(ulong _ulPreviousPlayerId, ulong _ulNewPlayerId)
	{
		if (_ulNewPlayerId == CNetwork.PlayerId)
		{
			// Subscribe to input events
            CUserInput.SubscribeInputChange(CUserInput.EInput.Primary, OnFireLasersCommand);
		}

		if (_ulPreviousPlayerId == CNetwork.PlayerId)
		{
			// Unsubscriber to input events
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Primary, OnFireLasersCommand);
		}
	}


	[AClientOnly]
    void OnFireLasersCommand(CUserInput.EInput _eInput, bool _bDown)
	{
		m_bFireLasers = _bDown;
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		// Empty
	}


// Member Fields


	public GameObject[] m_aLasterNodes = null;


	float m_fClientFireTimer	= 0.0f;
	float m_fClientFireInterval = 0.1f;
	float m_fServerFireTimer	= 0.0f;
	float m_fServerFireInterval = 0.1f;
	
	
	int m_iLaserNodeIndex = 0;


	bool m_bFireLasers   = false;


	static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
