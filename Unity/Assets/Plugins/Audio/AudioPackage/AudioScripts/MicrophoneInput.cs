//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   MicrophoneInput.cs
//  Description :   Used for taking input from user microphone
//
//  Author  	:  Daniel Langsford
//  Mail    	:  folduppugg@hotmail.com
//

// Namespaces
using UnityEngine;
using System.Collections;

public class MicrophoneInput : MonoBehaviour 
{
	// Member Types
		
	// Member Delegates & Events
		
	// Member Properties
		
	// Member Fields
	AudioClip m_acVoiceInput;
	string[] m_astrDevices;
	float m_fRecordingTimer;
	const int m_kiRecordTime = 1;
	bool m_bRecording = false;
	
	// Member Functions

	// Use this for initialization
	void Start () 
	{	
		m_astrDevices = Microphone.devices;
		foreach(string device in m_astrDevices)
		{
			int iMinFrequency;
			int iMaxFrequency;
			Microphone.GetDeviceCaps(device, out iMinFrequency, out iMaxFrequency);
		}		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.LeftAlt) && m_astrDevices[0] != null && !Microphone.IsRecording(m_astrDevices[0]))
		{
			m_acVoiceInput = Microphone.Start(m_astrDevices[0], true, m_kiRecordTime, 44000);
			
			//start timer
			m_fRecordingTimer = 0.0f; 
			m_bRecording = true;
		}
		
		if(m_bRecording)
		{
			m_fRecordingTimer += Time.deltaTime;
			
			if(m_fRecordingTimer >= m_kiRecordTime)
			{
				m_bRecording = false;
			}
		}
	}
}
