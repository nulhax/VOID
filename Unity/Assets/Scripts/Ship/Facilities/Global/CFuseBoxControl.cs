//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFuseBoxControl.cs
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


[RequireComponent(typeof(CPanelInterface))]
public class CFuseBoxControl : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


	public delegate void HandleOpenStart();
	public event HandleOpenStart EventOpenStart;


	public delegate void HandleCloseStart();
	public event HandleCloseStart EventCloseStart;


	public delegate void HandleOpened();
	public event HandleOpened EventOpened;


	public delegate void HandleClosed();
	public event HandleClosed EventClosed;


// Member Properties


	public bool IsOpened
	{
		get { return (m_bOpened.Get()); }
	}


	public bool IsBroken
	{
		get { return (m_bWiresBroken.Get()); }
	}


// Member Methods


	public override void InstanceNetworkVars()
	{
		m_bOpened = new CNetworkVar<bool>(OnNetworkVarSync, false);
		m_bWiresBroken = new CNetworkVar<bool>(OnNetworkVarSync, true);
	}
	
	
	public void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_bOpened)
		{
			m_fFrontPlateRotateTimer = 0.0f;
		}
		else if (_cSyncedVar == m_bWiresBroken)
		{
			if (IsBroken)
			{
				Debug.Log("Fuse box broke");
			}
			else
			{
				Debug.Log("Fuse box now fixed");
			}
		}


		if (IsBroken &&
			IsOpened)
		{
			transform.GetComponentInChildren<ParticleSystem>().Play();
		}
		else
		{
			transform.GetComponentInChildren<ParticleSystem>().Stop();
		}
	}


	public void Awake()
	{
		m_fFrontPlateRotateTimer = m_fFrontPlateRotateDuration;
	}


	public void Start()
	{
		// Debug - Break
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
		if (m_fFrontPlateRotateTimer < m_fFrontPlateRotateDuration)
		{
			if (m_bOpened.Get())
			{
				m_cFrontPlate.transform.eulerAngles = Vector3.Lerp(new Vector3(0.0f, 0.0f, 0.0f),
				                                                   new Vector3(0.0f, 90.0f, 0.0f), 
				                                                   m_fFrontPlateRotateTimer);
			}
			else
			{
				m_cFrontPlate.transform.eulerAngles = Vector3.Lerp(new Vector3(0.0f, 90.0f, 0.0f),
				                                                   new Vector3(0.0f, 0.0f, 0.0f), 
				                                                   m_fFrontPlateRotateTimer);	
			}

			m_fFrontPlateRotateTimer += Time.deltaTime;
		}


		// Debug - Break
		if (CNetwork.IsServer)
		{
			if (Input.GetKeyDown(KeyCode.B))
			{
				m_bWiresBroken.Set(true);
			}
		}
	}


	public void OpenFrontPlate()
	{
		m_bOpened.Set(true);
	}


	public void CloseFrontPlate()
	{
		m_bOpened.Set(false);
	}


	public void Fix()
	{
		if (IsOpened)
		{
			m_bWiresBroken.Set(false);
		}
	}



// Member Fields


	public GameObject m_cFrontPlate = null;


	CNetworkVar<bool> m_bOpened = null;
	CNetworkVar<bool> m_bWiresBroken = null;


	float m_fFrontPlateRotateDuration = 1.0f;
	float m_fFrontPlateRotateTimer = 0.0f;


};
