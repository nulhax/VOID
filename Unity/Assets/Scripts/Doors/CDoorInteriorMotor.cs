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


public class CDoorInteriorMotor : MonoBehaviour
{
	// Member Types


	// Member Delegates & Events


	// Member Fields
	private CDoorInterface m_DoorInterface = null;

	private int m_PlayersInsideTrigger = 0;


	// Member Properties


	// Member Methods
	private void Start()
	{
		m_DoorInterface = gameObject.GetComponent<CDoorInterface>();
		
		m_DoorInterface.EventOpenStart += OnDoorOpenStart;
		m_DoorInterface.EventCloseStart += OnDoorCloseStart;
	}
	
	[AServerOnly]
	public void OnTriggerEnter(Collider _Collider)
	{
		if(!CNetwork.IsServer)
			return;

		bool isPlayer = _Collider.gameObject.GetComponent<CPlayerInterface>();

		if(isPlayer)
			m_PlayersInsideTrigger += 1;
			
		if(m_PlayersInsideTrigger == 1)
			m_DoorInterface.SetDoorState(true);
	}

	[AServerOnly]
	public void OnTriggerExit(Collider _Collider)
	{
		if(!CNetwork.IsServer)
			return;

		bool isPlayer = _Collider.gameObject.GetComponent<CPlayerInterface>();
		
		if(isPlayer)
			m_PlayersInsideTrigger -= 1;

		if(m_PlayersInsideTrigger == 0)
			m_DoorInterface.SetDoorState(false);
	}

	private void OnDoorOpenStart(CDoorInterface _Self)
	{
		animation.CrossFadeQueued("Interior_Door_Open");
		gameObject.GetComponent<CAudioCue>().Play(1.0f, false, 0);
	}

	private void OnDoorCloseStart(CDoorInterface _Self)
	{
		animation.CrossFadeQueued("Interior_Door_Close");
		GetComponent<CAudioCue>().Play(1.0f, false, 1);
	}

	public void OnAnimDoorOrificeOpened()
	{
		m_DoorInterface.DoorOrificeArea = 5.0f;
	}
	
	public void OnAnimDoorOrificeClosed()
	{
		m_DoorInterface.DoorOrificeArea = 0.0f;
	}
};
