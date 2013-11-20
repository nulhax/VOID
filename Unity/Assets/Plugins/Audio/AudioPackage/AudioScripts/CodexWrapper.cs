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
using System.Collections;
using System.Collections.Generic;
using NSpeex;
using System;


/* Implementation */

public class CodexWrapper : MonoBehaviour
{
	// Member Types

	// Member Delegates & Events
		
	// Member Properties
		
	// Member Functions
		
	// Member Fields
	static SpeexEncoder m_cEncoder = new SpeexEncoder(BandMode.UltraWide);
	static SpeexDecoder m_eDecoder = new SpeexDecoder(BandMode.UltraWide, false);
	static SpeexJitterBuffer jitterBuffer = new SpeexJitterBuffer(m_eDecoder);
	static int m_iFrameSize = m_cEncoder.FrameSize;
	
	private int encodedSize = 0;
		
	// Use this for initialization
	void Start () 
	{
		CNetwork.Server.EventRecievedPlayerMicrophoneAudio += new CNetworkServer.NotifyRecievedPlayerMicrophoneAudio(OnRecievedPlayerMicrophoneAudio);
		CNetwork.Connection.EventRecievedMicrophoneAudio += new CNetworkConnection.HandleRecievedMicrophoneAudio(OnRecievedMicrophoneAudio);
	}

	[AServerMethod]
	void OnRecievedPlayerMicrophoneAudio(CNetworkPlayer _cPlayer, CNetworkStream _cAudioDataStream)
	{
	}

	[AClientMethod]
	void OnRecievedMicrophoneAudio(CNetworkStream _cAudioDataStream)
	{
		
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			/*
			// Convert to float
			float[] faDecodedAudioData = new float[fAudioData.Length];

			for (int i = 0; i < faDecodedAudioData.Length; ++i)
			{
				faDecodedAudioData[i] = (float)((float)saDecodedFrames[i] / 32767.0f);
			}

			// Play audio
			AudioClip newClip = AudioClip.Create("Test", faDecodedAudioData.Length, m_cTestSoundClip.channels, m_cTestSoundClip.frequency, false, false);
			newClip.SetData(faDecodedAudioData, 0);
			Debug.Log("Audio length: " + newClip.length);
			AudioSource newSource = gameObject.GetComponent<AudioSource>();
			newSource.priority = 10;
			newSource.PlayOneShot(newClip);*/
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
		returnData.Write(iAudioDataSize);
		returnData.Write(iTotalNumEncodedBytes);
		returnData.Write(baEncodedData);
		
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
