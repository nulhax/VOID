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


public class CDoorExteriorMotor : MonoBehaviour
{
	// Member Types


	// Member Delegates & Events


	// Member Fields
	public CDUIConsole m_DoorControlPanelInner = null;
	public CDUIConsole m_DoorControlPanelOuter = null;

	public float m_DistanceToActivateUI = 5.0f;

	private CDUIDoorControl m_DoorControlInner = null;
	private CDUIDoorControl m_DoorControlOuter = null;

	private CDoorInterface m_DoorInterface = null;

	private static string k_OpenAnimation = "Exterior_Door_Open";
	private static string k_CloseAnimation = "Exterior_Door_Close";

	// Member Properties


	// Member Methods
	private void Start()
	{
		m_DoorInterface = gameObject.GetComponent<CDoorInterface>();

		m_DoorInterface.EventOpenStart += OnDoorOpenStart;
		m_DoorInterface.EventCloseStart += OnDoorCloseStart;

		m_DoorControlInner = m_DoorControlPanelInner.DUIRoot.GetComponent<CDUIDoorControl>();

		m_DoorControlInner.EventOpenDoorButtonPressed += OnDoorControlOpenPressed;
		m_DoorControlInner.EventCloseDoorButtonPressed += OnDoorControlClosePressed;

		m_DoorControlOuter = m_DoorControlPanelOuter.DUIRoot.GetComponent<CDUIDoorControl>();
		
		m_DoorControlOuter.EventOpenDoorButtonPressed += OnDoorControlOpenPressed;
		m_DoorControlOuter.EventCloseDoorButtonPressed += OnDoorControlClosePressed;
	}

	private void Update()
	{
		if(CGameCameras.ShipCamera == null)
			return;

		Vector3 playerHeadPosition = CGameCameras.ShipCamera.transform.position;

		float dotForwardPanelInner = Vector3.Dot((playerHeadPosition - m_DoorControlPanelInner.ConsoleScreen.transform.position).normalized, 
		                                         m_DoorControlPanelInner.ConsoleScreen.transform.forward);

		float dotForwardPanelOuter = Vector3.Dot((playerHeadPosition - m_DoorControlPanelOuter.ConsoleScreen.transform.position).normalized,
		                                         m_DoorControlPanelOuter.ConsoleScreen.transform.forward);

		float distanceToDoor = (playerHeadPosition - transform.position).magnitude;

		bool activeInner = dotForwardPanelInner > 0.0f && distanceToDoor < m_DistanceToActivateUI;
		bool activeOuter = dotForwardPanelOuter > 0.0f && distanceToDoor < m_DistanceToActivateUI;

		if(activeInner != m_DoorControlPanelInner.ConsoleScreen.activeSelf)
			m_DoorControlPanelInner.ConsoleScreen.SetActive(activeInner);

		if(activeOuter != m_DoorControlPanelOuter.ConsoleScreen.activeSelf)
			m_DoorControlPanelOuter.ConsoleScreen.SetActive(activeOuter);
	}

	[AServerOnly]
    private void OnDoorControlOpenPressed(CDUIDoorControl _DUI)
    {
		// Open the door
		m_DoorInterface.SetDoorState(true);

		// Set the DUI panel to close door
		m_DoorControlInner.SetPanel(CDUIDoorControl.EPanel.CloseDoor);
		m_DoorControlOuter.SetPanel(CDUIDoorControl.EPanel.CloseDoor);
    }

	[AServerOnly]
	private void OnDoorControlClosePressed(CDUIDoorControl _DUI)
	{
		// Close the door
		m_DoorInterface.SetDoorState(false);
		
		// Set the DUI panel to close door
		m_DoorControlInner.SetPanel(CDUIDoorControl.EPanel.OpenDoor);
		m_DoorControlOuter.SetPanel(CDUIDoorControl.EPanel.OpenDoor);
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
