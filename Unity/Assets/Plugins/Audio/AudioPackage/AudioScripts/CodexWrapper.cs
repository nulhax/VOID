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
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using NSpeex;



/* Implementation */

public class CodexWrapper : MonoBehaviour
{
	// Member Types
	struct AudioData
	{
		public int iFrequency;
		public int iAudioDataSize;
		public short[] saData;
	};	
	
	// Member Delegates & Events
		
	// Member Properties
		
	// Member Functions
		
	// Member Fields
	static SpeexEncoder m_cEncoder = new SpeexEncoder(BandMode.Narrow);
	static SpeexDecoder m_eDecoder = new SpeexDecoder(BandMode.Narrow, false);
	static SpeexJitterBuffer jitterBuffer = new SpeexJitterBuffer(m_eDecoder);
	static int m_iFrameSize = m_cEncoder.FrameSize;
		
	public AudioClip m_acVoiceInput;
	float m_fRecordingTimer;
	const int m_kiRecordTime = 1;
	bool m_bRecording = false;
	
	static CNetworkStream s_AudioPacket = new CNetworkStream();
	Thread m_AudioThread;
	bool m_bEncoding = false;
	
	// Use this for initialization
	void Start () 
	{
		CNetwork.Server.EventRecievedPlayerMicrophoneAudio += new CNetworkServer.NotifyRecievedPlayerMicrophoneAudio(OnRecievedPlayerMicrophoneAudio);
		CNetwork.Connection.EventRecievedMicrophoneAudio += new CNetworkConnection.HandleRecievedMicrophoneAudio(OnRecievedMicrophoneAudio);
		
		m_cEncoder.Quality = 5;
		
		foreach(string device in Microphone.devices)
		{
			int iMinFrequency;
			int iMaxFrequency;
			Microphone.GetDeviceCaps(device, out iMinFrequency, out iMaxFrequency);
		}	
		
		m_AudioThread = new Thread(new ParameterizedThreadStart(EncodeAudio));
	}

	[AServerMethod]
	void OnRecievedPlayerMicrophoneAudio(CNetworkPlayer _cPlayer, CNetworkStream _cAudioDataStream)
	{
		Dictionary<ulong,CNetworkPlayer> players = CNetwork.Server.FindNetworkPlayers();
		
		
		foreach(ulong playerID in players.Keys)
		{
			//if(_cPlayer.PlayerId != playerID)
			{
				CNetwork.Server.TransmitMicrophoneAudio(playerID, _cAudioDataStream); 		
			}
		}
	}

	[AClientMethod]
	void OnRecievedMicrophoneAudio(CNetworkStream _cAudioDataStream)
	{
		// Convert to float
		short[] saDecodedFrames = DecodeAudio(_cAudioDataStream);
		
		_cAudioDataStream.SetReadOffset(1);
		int frequency = _cAudioDataStream.ReadInt();
		int numSamples = _cAudioDataStream.ReadInt();
		
		float[] faDecodedAudioData = new float[numSamples];

		for (int i = 0; i < faDecodedAudioData.Length; ++i)
		{
			faDecodedAudioData[i] = (float)((float)saDecodedFrames[i] / 32767.0f);
		}

		// Play audio
		AudioClip newClip = AudioClip.Create("Test", faDecodedAudioData.Length, 1, frequency, false, false);
		newClip.SetData(faDecodedAudioData, 0);
		Debug.Log("Audio length: " + newClip.length);
		AudioSource newSource = gameObject.GetComponent<AudioSource>();
		newSource.priority = 10;
		newSource.PlayOneShot(newClip);
	}

	void Update()
	{
		if(Microphone.devices.Length != 0 && false)
		{
			if(m_bRecording == false)
			{
				m_acVoiceInput = Microphone.Start(Microphone.devices[0], true, m_kiRecordTime, 44000);
				
				//start timer
				m_fRecordingTimer = 0.0f; 
				m_bRecording = true;
			}			
			else
			{
				m_fRecordingTimer += Time.deltaTime;				
				if(m_fRecordingTimer >= (float)m_kiRecordTime)
				{
					Microphone.End(Microphone.devices[0]);
					
					// Calculate sound size (To the nearest frame size)
					int iAudioDataSize = m_acVoiceInput.samples * m_acVoiceInput.channels * sizeof(float);
					iAudioDataSize -= iAudioDataSize % m_iFrameSize;
									
					// Extract audio data through the bum
					float[] fAudioData = new float[iAudioDataSize / sizeof(float)];
					m_acVoiceInput.GetData(fAudioData, 0);
					
					// Convert to short
					short[] saAudioData = new short[fAudioData.Length];
			
					for (int i = 0; i < fAudioData.Length; ++i)
					{
						saAudioData[i] = (short)(fAudioData[i] * 32767.0f);
					}					
								
					AudioData voiceData = new AudioData();
					voiceData.iAudioDataSize = iAudioDataSize;
					voiceData.iFrequency = m_acVoiceInput.frequency;
					voiceData.saData = saAudioData;
					
				
					
					m_bEncoding = true;
					
					m_AudioThread = new Thread(new ParameterizedThreadStart(EncodeAudio));
					m_AudioThread.Start((object)voiceData);
					
					m_bRecording = false;
				}
			}
		}
		
		if(!m_AudioThread.IsAlive && m_bEncoding)
		{
			m_bEncoding = false;
			CNetwork.Connection.TransmitMicrophoneAudio(s_AudioPacket);
			
			s_AudioPacket.Clear();
		}
	}
	
	void EncodeAudio(object _rawData)
	{		
		//Extract data from incoming data
		AudioData voiceData = (AudioData)_rawData;
		int iAudioDataSize = voiceData.iAudioDataSize;
		int iFrequency = voiceData.iFrequency;
		short[] saAudioData = voiceData.saData;		

		// Encode frames
		byte[] baEncodedData = new byte[iAudioDataSize];
		int iTotalNumEncodedBytes = m_cEncoder.Encode(saAudioData, 0, saAudioData.Length, baEncodedData, 0, baEncodedData.Length);
				
	
		s_AudioPacket.Write(iFrequency);
		s_AudioPacket.Write(iAudioDataSize);
		s_AudioPacket.Write(iTotalNumEncodedBytes);
		s_AudioPacket.Write(baEncodedData, iTotalNumEncodedBytes);
		
									
		//return(returnData);
	}
	
	short[] DecodeAudio(CNetworkStream _cAudioDataStream)
	{
		//Pull relevant information out of the network stream.
		_cAudioDataStream.IgnoreBytes(4);
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