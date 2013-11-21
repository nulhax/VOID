//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CodexWrapper.cs
//  Description :   Used for encoding and decoding audio
//
//  Author  	:  Daniel Langsford
//  Mail    	:  folduppugg@hotmail.com
//

// Namespaces
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NSpeex;



/* Implementation */

public class CodexWrapper : MonoBehaviour
{
	// Member Types

	// Member Delegates & Events
		
	// Member Properties
		
	// Member Functions
		
	// Member Fields
	static SpeexEncoder m_cEncoder = new SpeexEncoder(BandMode.Narrow);
	static SpeexDecoder m_eDecoder = new SpeexDecoder(BandMode.Narrow, false);
	static SpeexJitterBuffer jitterBuffer = new SpeexJitterBuffer(m_eDecoder);
	static int m_iFrameSize = m_cEncoder.FrameSize;
		
	AudioClip m_acVoiceInput;
	string[] m_astrDevices;
	float m_fRecordingTimer;
	const int m_kiRecordTime = 1;
	bool m_bRecording = false;
	
	//private int encodedSize = 0;
	private const int m_kiFrequency = 44000;
		
	// Use this for initialization
	void Start () 
	{
		CNetwork.Server.EventRecievedPlayerMicrophoneAudio += new CNetworkServer.NotifyRecievedPlayerMicrophoneAudio(OnRecievedPlayerMicrophoneAudio);
		CNetwork.Connection.EventRecievedMicrophoneAudio += new CNetworkConnection.HandleRecievedMicrophoneAudio(OnRecievedMicrophoneAudio);
		
		m_astrDevices = Microphone.devices;
		foreach(string device in m_astrDevices)
		{
			int iMinFrequency;
			int iMaxFrequency;
			Microphone.GetDeviceCaps(device, out iMinFrequency, out iMaxFrequency);
		}		
	}

	[AServerMethod]
	void OnRecievedPlayerMicrophoneAudio(CNetworkPlayer _cPlayer, CNetworkStream _cAudioDataStream)
	{
		Dictionary<ulong,CNetworkPlayer> players = CNetwork.Server.FindNetworkPlayers();
		
		foreach(ulong playerID in players.Keys)
		{
			CNetwork.Server.TransmitMicrophoneAudio(playerID, _cAudioDataStream); 		
		}
	}

	[AClientMethod]
	void OnRecievedMicrophoneAudio(CNetworkStream _cAudioDataStream)
	{
		// Convert to float
		short[] saDecodedFrames = DecodeAudio(_cAudioDataStream);
		
		_cAudioDataStream.SetReadOffset(0);
		int numSamples = _cAudioDataStream.ReadInt();
		
		float[] faDecodedAudioData = new float[numSamples];

		for (int i = 0; i < faDecodedAudioData.Length; ++i)
		{
			faDecodedAudioData[i] = (float)((float)saDecodedFrames[i] / 32767.0f);
		}

		// Play audio
		AudioClip newClip = AudioClip.Create("Test", faDecodedAudioData.Length, 1, m_kiFrequency, false, false);
		newClip.SetData(faDecodedAudioData, 0);
		Debug.Log("Audio length: " + newClip.length);
		AudioSource newSource = gameObject.GetComponent<AudioSource>();
		newSource.priority = 10;
		newSource.PlayOneShot(newClip);
	}

	void Update()
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
				CNetworkStream testPackage = EncodeAudio(m_acVoiceInput);
				
				CNetwork.Connection.TransmitMicrophoneAudio(testPackage);
			}
		}
	}
	
	CNetworkStream EncodeAudio(AudioClip _rawAudio)
	{
		// Calculate sound size (To the nearest frame size)
		int iAudioDataSize = _rawAudio.samples * _rawAudio.channels * sizeof(float);
		iAudioDataSize -= iAudioDataSize % m_iFrameSize;
		Debug.Log("Audio data size: " + iAudioDataSize);
		
		
		// Extract audio data
		float[] fAudioData = new float[iAudioDataSize / sizeof(float)];
		_rawAudio.GetData(fAudioData, 0);
		Debug.Log("Audio length: " + _rawAudio.length);

		
		// Convert to short
		short[] saAudioData = new short[fAudioData.Length];

		for (int i = 0; i < fAudioData.Length; ++i)
		{
			saAudioData[i] = (short)(fAudioData[i] * 32767.0f);
		}

		// Encode frames
		byte[] baEncodedData = new byte[iAudioDataSize];
		int iTotalNumEncodedBytes = m_cEncoder.Encode(saAudioData, 0, saAudioData.Length, baEncodedData, 0, baEncodedData.Length);
		Debug.Log("Num encoded bytes: " + iTotalNumEncodedBytes);
		
		CNetworkStream returnData = new CNetworkStream();
		returnData.IgnoreBytes(1);
		returnData.Write(iAudioDataSize);
		returnData.Write(iTotalNumEncodedBytes);
		returnData.Write(baEncodedData, iTotalNumEncodedBytes);
		
		return(returnData);
	}
	
	short[] DecodeAudio(CNetworkStream _cAudioDataStream)
	{
		//Pull relevant information out of the network stream.
		int iNumSamples = _cAudioDataStream.ReadInt();
		int iNumEncodedBytes = _cAudioDataStream.ReadInt();		
		byte[] baEncodedData = _cAudioDataStream.ReadBytes(_cAudioDataStream.NumUnreadBytes);
				
		// Decode
		short[] saDecodedFrames = new short[iNumSamples];
		int iNumDecodedBytes = m_eDecoder.Decode(baEncodedData, 0, iNumEncodedBytes, saDecodedFrames, 0, false);
		Debug.Log("Decoded audio data size: " + iNumDecodedBytes + " : " + saDecodedFrames.Length);
		
		return(saDecodedFrames);
	}
}
