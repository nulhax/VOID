//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLASSNAME.cs
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


public class CDoorInterface : CNetworkMonoBehaviour 
{
	// Member Types
	public enum EEventType
	{
		OpenStart,
		Opened,
		CloseStart,
		Closed
	}


	// Member Delegates & Events
	public delegate void HandleDoorEvent(CDoorInterface _Sender);
	
	public event HandleDoorEvent EventOpenStart;
	public event HandleDoorEvent EventOpenFinished;
	public event HandleDoorEvent EventCloseStart;
	public event HandleDoorEvent EventCloseFinish;
	
	
	// Member Fields
	public GameObject m_FirstConnectedFacility = null;
	public GameObject m_SecondConnectedFacility = null;

	public AnimationClip m_OpenAnimation = null;
	public AnimationClip m_CloseAnimation = null;

	private CNetworkVar<bool> m_Opened = null;
	private float m_OpenTimer = 0.0f;
	private float m_OrificeArea = 0.0f;

	// Member Properties
	public bool IsOpened
	{
		get { return(m_Opened.Value); }
	}

	public float OpenTimer
	{
		get { return(m_OpenTimer); }
	}

	public float DoorOrificeArea
	{
		get { return(m_OrificeArea); }
		set { m_OrificeArea = value; }
	}
	
	
	// Member Methods
	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
		m_Opened = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, false);
	}
	
	private void OnNetworkVarSync(INetworkVar _SyncedVar)
	{
		if(m_Opened == _SyncedVar)
		{
			if(IsOpened)
			{
				if(EventOpenStart != null)
					EventOpenStart(this);
			}
			else
			{
				if(EventCloseStart != null) 
					EventCloseStart(this);
			}
		}
	}

	private void Update()
	{
		if(!CNetwork.IsServer)
			return;

		if(IsOpened)
			m_OpenTimer += Time.deltaTime;
	}

	public void OnAnimDoorOpenFinished()
	{
		if (EventOpenFinished != null) 
			EventOpenFinished(this);
	}
	
	public void OnAnimDoorCloseFinished()
	{
		if (EventOpenFinished != null) 
			EventOpenFinished(this);
	}

	[AServerOnly]
	public void SetDoorState(bool _OpenState)
	{
		m_Opened.Value = _OpenState;

		if(_OpenState)
			m_OpenTimer = 0.0f;
	}
}
