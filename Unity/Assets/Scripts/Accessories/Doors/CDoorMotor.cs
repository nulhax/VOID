//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDoorMotor.cs
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


public class CDoorMotor : CNetworkMonoBehaviour
{
	
// Member Types
	public enum EDoorState
    {
		INVALID,
        Opened,
        Opening,
        Closed,
        Closing
    }
	
	
// Member Delegates & Events
	public delegate void DoorStateHandler(GameObject _Sender);
   
	public event DoorStateHandler EventDoorStateOpened;
	public event DoorStateHandler EventDoorStateOpening;
	public event DoorStateHandler EventDoorStateClosed;
	public event DoorStateHandler EventDoorStateClosing;
	
// Member Fields
	public float m_DoorOpenTime = 1.0f;
	public float m_CloseTime = 1.0f;

	private CNetworkVar<EDoorState> m_DoorState = null;

	private Vector3 m_OpenedPosition = Vector3.zero;
	private Vector3 m_ClosedPosition = Vector3.zero;
	private float m_StateChangeTimer = 0.0f;

// Member Properties
	
	public EDoorState DoorState 
	{ 
		get { return((EDoorState)m_DoorState.Get()); }

		[AServerOnly]
		set { m_DoorState.Set(value); }
	}


// Member Methods

	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
		m_DoorState = _cRegistrar.CreateReliableNetworkVar<EDoorState>(OnNetworkVarSync, EDoorState.INVALID);
	}
	
	public void OnNetworkVarSync(INetworkVar _rSender)
    {
		// Door State
		if(_rSender == m_DoorState)
		{
			if(DoorState == EDoorState.Opened)
			{
				if(EventDoorStateOpened != null)
					EventDoorStateOpened(gameObject);			
			}
			else if(DoorState == EDoorState.Closed)
			{
				if(EventDoorStateClosed != null)
					EventDoorStateClosed(gameObject);
			}
		}
    }
	
	public void Awake()
	{

	}

	public void Start()
	{
		m_ClosedPosition = transform.position;
		m_OpenedPosition = m_ClosedPosition + new Vector3(0.0f, collider.bounds.size.y);

		// Debug: Open the door
		if(CNetwork.IsServer)
			OpenDoor();
	}
	
	public void Update()
	{
		if(CNetwork.IsServer)
		{
			UpdateDoorStateTransitions();
		}
	}

	public void UpdateDoorStateTransitions()
	{
		if(DoorState == EDoorState.Opening)
		{
			m_StateChangeTimer += Time.deltaTime;
			
			if(m_StateChangeTimer > m_DoorOpenTime)
			{
				m_StateChangeTimer = m_DoorOpenTime;
				DoorState = EDoorState.Opened;
			}
			
			transform.position = Vector3.Lerp(m_ClosedPosition, m_OpenedPosition, m_StateChangeTimer/m_DoorOpenTime);
		}
		else if(DoorState == EDoorState.Closing)
		{
			m_StateChangeTimer += Time.deltaTime;
			
			if(m_StateChangeTimer > m_CloseTime)
			{
				m_StateChangeTimer = m_CloseTime;
				DoorState = EDoorState.Closed;
			}
			
			transform.position = Vector3.Lerp(m_OpenedPosition, m_ClosedPosition, m_StateChangeTimer/m_CloseTime);
		}
	}

	[AServerOnly]
    public void OpenDoor()
    {
		if(DoorState == EDoorState.Closed)
			m_StateChangeTimer = 0.0f;

		if(DoorState != EDoorState.Opened)
		{

			DoorState = EDoorState.Opening;
		}
    }

	[AServerOnly]
    public void CloseDoor()
    {
		if(DoorState == EDoorState.Opened)
			m_StateChangeTimer = 0.0f;

		if(DoorState != EDoorState.Closed)
		{
			DoorState = EDoorState.Opening;
		}
    }
}
