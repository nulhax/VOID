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
	public CDUIConsole m_DoorControlPanelFirst = null;
	public CDUIConsole m_DoorControlPanelSecond = null;

	public float m_DistanceToActivateUI = 5.0f;

	private CDUIDoorControl m_DoorControlFirst = null;
	private CDUIDoorControl m_DoorControlSecond = null;

	private CDoorInterface m_DoorInterface = null;

	private int m_PlayersInsideTrigger = 0;

	private static string k_OpenAnimation = "Interior_Door_Open";
	private static string k_CloseAnimation = "Interior_Door_Close";


	// Member Properties


	// Member Methods
	private void Start()
	{
		m_DoorInterface = gameObject.GetComponent<CDoorInterface>();
		
		m_DoorInterface.EventOpenStart += OnDoorOpenStart;
		m_DoorInterface.EventCloseStart += OnDoorCloseStart;

		m_DoorControlFirst = m_DoorControlPanelFirst.DUIRoot.GetComponent<CDUIDoorControl>();
		
		m_DoorControlFirst.EventOpenDoorButtonPressed += OnDoorControlOpenPressed;
		m_DoorControlFirst.EventCloseDoorButtonPressed += OnDoorControlClosePressed;
		
		m_DoorControlSecond = m_DoorControlPanelSecond.DUIRoot.GetComponent<CDUIDoorControl>();
		
		m_DoorControlSecond.EventOpenDoorButtonPressed += OnDoorControlOpenPressed;
		m_DoorControlSecond.EventCloseDoorButtonPressed += OnDoorControlClosePressed;
	}

	private void Update()
	{
		if(CGameCameras.ShipCamera == null)
			return;
		
		Vector3 playerHeadPosition = CGameCameras.ShipCamera.transform.position;
		
		float dotForwardPanelFirst = Vector3.Dot((playerHeadPosition - m_DoorControlPanelFirst.ConsoleScreen.transform.position).normalized, 
		                                         m_DoorControlPanelFirst.ConsoleScreen.transform.forward);
		
		float dotForwardPanelSecond = Vector3.Dot((playerHeadPosition - m_DoorControlPanelSecond.ConsoleScreen.transform.position).normalized,
		                                         m_DoorControlPanelSecond.ConsoleScreen.transform.forward);
		
		float distanceToDoor = (playerHeadPosition - transform.position).magnitude;
		
		bool activeInner = dotForwardPanelFirst > 0.0f && distanceToDoor < m_DistanceToActivateUI;
		bool activeOuter = dotForwardPanelSecond > 0.0f && distanceToDoor < m_DistanceToActivateUI;
		
		if(activeInner != m_DoorControlPanelFirst.ConsoleScreen.activeSelf)
			m_DoorControlPanelFirst.ConsoleScreen.SetActive(activeInner);
		
		if(activeOuter != m_DoorControlPanelSecond.ConsoleScreen.activeSelf)
			m_DoorControlPanelSecond.ConsoleScreen.SetActive(activeOuter);
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

		// Set the DUI panel to close door
		m_DoorControlFirst.SetPanel(CDUIDoorControl.EPanel.CloseDoor);
		m_DoorControlSecond.SetPanel(CDUIDoorControl.EPanel.CloseDoor);
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

		// Set the DUI panel to close door
		m_DoorControlFirst.SetPanel(CDUIDoorControl.EPanel.OpenDoor);
		m_DoorControlSecond.SetPanel(CDUIDoorControl.EPanel.OpenDoor);
	}

	[AServerOnly]
	private void OnDoorControlOpenPressed(CDUIDoorControl _DUI)
	{
		// Open the door
		m_DoorInterface.SetDoorState(true);
		
		// Set the DUI panel to close door
		m_DoorControlFirst.SetPanel(CDUIDoorControl.EPanel.CloseDoor);
		m_DoorControlSecond.SetPanel(CDUIDoorControl.EPanel.CloseDoor);
	}
	
	[AServerOnly]
	private void OnDoorControlClosePressed(CDUIDoorControl _DUI)
	{
		// Close the door
		m_DoorInterface.SetDoorState(false);
		
		// Set the DUI panel to close door
		m_DoorControlFirst.SetPanel(CDUIDoorControl.EPanel.OpenDoor);
		m_DoorControlSecond.SetPanel(CDUIDoorControl.EPanel.OpenDoor);
	}

	private void OnDoorOpenStart(CDoorInterface _Self)
	{
		if(animation.IsPlaying(k_OpenAnimation))
			return;
		
		float startTime = animation.IsPlaying(k_CloseAnimation) ? 
			animation[k_OpenAnimation].length - animation[k_CloseAnimation].time : 0.0f;
		
		animation.Play(k_OpenAnimation, PlayMode.StopSameLayer);
		animation[k_OpenAnimation].time = startTime;
		
		gameObject.GetComponent<CAudioCue>().Play(1.0f, false, 0);
	}

	private void OnDoorCloseStart(CDoorInterface _Self)
	{
		if(animation.IsPlaying(k_CloseAnimation))
			return;
		
		float startTime = animation.IsPlaying(k_OpenAnimation) ? 
			animation[k_CloseAnimation].length - animation[k_OpenAnimation].time : 0.0f;
		
		animation.Play(k_CloseAnimation, PlayMode.StopSameLayer);
		animation[k_CloseAnimation].time = startTime;
		
		gameObject.GetComponent<CAudioCue>().Play(1.0f, false, 1);
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
