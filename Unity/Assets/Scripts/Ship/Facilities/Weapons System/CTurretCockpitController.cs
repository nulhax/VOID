//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CTurretCockpitController.cs
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


[RequireComponent(typeof(CNetworkView))]
[RequireComponent(typeof(CCockpit))]
public class CTurretCockpitController : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


	public GameObject ActiveTurretObject
	{
		get { return (CNetwork.Factory.FindObject(m_cTurretViewId.Get())); }
	}


// Member Methods
	

	public override void InstanceNetworkVars()
    {
		m_cTurretViewId = new CNetworkVar<ushort>(OnNetworkVarSync, 0);
    }

	
	public void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_cTurretViewId)
		{
			// Check is connected to turret
			if (m_cTurretViewId.Get() != 0)
			{
				// Check player is in cockpit
				if (m_cCockpit.MountedPlayerId != 0)
				{
					if (CNetwork.IsServer)
					{
						// Notify turret that it has been mounted by player
						ActiveTurretObject.GetComponent<CTurretController>().Mount(m_cCockpit.MountedPlayerId);
					}
				}

				// There is no player in the cockpit
				else
				{
					if (CNetwork.IsServer)
					{
						// Notify turret that it has been mounted by player
						ActiveTurretObject.GetComponent<CTurretController>().Unmount();
					}
				}
			}
		}
	}


	public void Start()
	{
		m_cCockpit = gameObject.GetComponent<CCockpit>();

		// Subscribe to cockpit events
		m_cCockpit.EventPlayerEnter += new CCockpit.HandlePlayerEnter(OnPlayerEnterCockpit);
		m_cCockpit.EventPlayerLeave += new CCockpit.HandlePlayerLeave(OnPlayerLeaveCockpit);
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
		CCockpit cCockpit = gameObject.GetComponent<CCockpit>();

		if (cCockpit.IsMounted &&
			cCockpit.MountedPlayerId == CGame.PlayerActorViewId)
		{
			//if (Input.GetKeyDown(KeyCode.Space))
		}
	}


	[AServerMethod]
	[AClientMethod]
	void OnPlayerEnterCockpit(ulong _ulPlayerId)
	{
		// Debug - Set default turret
		if (CNetwork.IsServer)
		{	
			m_cTurretViewId.Set(GameObject.Find("Turret").GetComponent<CNetworkView>().ViewId);
		}
	}


	[AServerMethod]
	[AClientMethod]
	void OnPlayerLeaveCockpit(ulong _ulPlayerId)
	{
		// Debug - Set default turret
		if (CNetwork.IsServer)
		{
			ActiveTurretObject.GetComponent<CTurretController>().Unmount();
			m_cTurretViewId.Set(0);
		}
	}


// Member Fields


	CNetworkVar<ushort> m_cTurretViewId = null;


	CCockpit m_cCockpit = null;


	float m_fFireTimer		= 0.0f;
	float m_fFireInterval	= 0.2f;


	bool m_bUpdateRotation = false;



};
