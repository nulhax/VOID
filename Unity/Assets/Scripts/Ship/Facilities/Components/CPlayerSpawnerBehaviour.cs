//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerSpawnerBehaviour.cs
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


public class CPlayerSpawnerBehaviour : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


	public bool IsBlocked
	{
		get { ValidateContainedPlayers(); return (m_bBlocked.Get()); }
	}


// Member Methods


	public override void InstanceNetworkVars()
	{
		m_bBlocked = new CNetworkVar<bool>(OnNetworkVarSync, false);
	}


	void Start()
	{
		// Find owner facility
		//		Signup to power evetns, requires power to operate
		// 		Signup to temperature changes, will over heat
		//		Signup to hull breaches, wont spawn during hull breach
	}


	void OnDestroy()
	{
		// Empty
	}


	void Update()
	{
		// Empty
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		// Empty
	}


	void OnTriggerEnter(Collider _cOtherCollider)
	{
		if (CNetwork.IsServer &&
		    _cOtherCollider.gameObject.GetComponent<CPlayerHealth>() != null)
		{
			m_cContainedPlayers.Add(_cOtherCollider.gameObject);

			m_bBlocked.Set(true);
		}
	}


	void OnTriggerExit(Collider _cOtherCollider)
	{
		if (CNetwork.IsServer &&
		    _cOtherCollider.gameObject.GetComponent<CPlayerHealth>() != null)
		{
			m_cContainedPlayers.Remove(_cOtherCollider.gameObject);

			ValidateContainedPlayers();
		}
	}


	void ValidateContainedPlayers()
	{
		if (CNetwork.IsServer)
		{
			foreach (GameObject cPlayerActor in m_cContainedPlayers.ToArray())
			{
				if (cPlayerActor == null)
				{
					m_cContainedPlayers.Remove(null);
				}
			}

			if (m_cContainedPlayers.Count == 0)
			{
				m_bBlocked.Set(false);
			}
		}
	}


// Member Fields


	public GameObject m_cSpawnPosition = null;


	List<GameObject> m_cContainedPlayers = new List<GameObject>();


	CNetworkVar<bool> m_bBlocked = null;


};
