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

public class CVoiceTransmissionBehaviour : MonoBehaviour
{
	// Member Types
	struct AudioData
	{
		public int iFrequency;
		public int iAudioDataSize;
		public short[] saData;
	};	
	
	struct DecodeInformation
	{
		public short[] saDecodedData;
		public CNetworkViewId cSenderViewID;
		public int iNumSamples;
		public int iFrequency;
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
	const int m_kiNumDecodeThreads = 3;
	bool m_bRecording = false;
	
	static CNetworkStream s_AudioPacket = new CNetworkStream();
	static Queue<DecodeInformation> s_decodedFrames = new Queue<DecodeInformation>();
	static Queue<CNetworkStream> s_framesToDecode = new Queue<CNetworkStream>();
	
	Thread m_EncodeThread;
	Thread[] m_DecodeThreads;
	bool m_bEncoding = false;
	
	public bool m_bReturnToSender = true;
	
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
		
		m_EncodeThread = new Thread(new ParameterizedThreadStart(EncodeAudio));
		
		m_DecodeThreads = new Thread[m_kiNumDecodeThreads];
			
		for(int i = 0; i < m_kiNumDecodeThreads; i++)
		{
			m_DecodeThreads[i] = new Thread(new ParameterizedThreadStart(DecodeAudio));
		}
		
		//m_DecodeThread = new Thread(new ParameterizedThreadStart(DecodeAudio));		
	}

	[AServerOnly]
	void OnRecievedPlayerMicrophoneAudio(CNetworkPlayer _cPlayer, CNetworkStream _cAudioDataStream)
	{		
		GameObject playerActor = CGamePlayers.GetPlayerActor(_cPlayer.PlayerId);
		CNetworkViewId playerViewID = playerActor.GetComponent<CNetworkView>().ViewId;
			
		_cAudioDataStream.SetReadOffset(0);		
		byte[] streamData =  _cAudioDataStream.ReadBytes(_cAudioDataStream.NumUnreadBytes);
		
		CNetworkStream dataStream = new CNetworkStream();
		dataStream.Write(streamData);
		dataStream.Write(playerViewID);		
				
		Dictionary<ulong,CNetworkPlayer> players = CNetwork.Server.GetNetworkPlayers();
					
		foreach(ulong playerID in players.Keys)
		{
			if(m_bReturnToSender)
			{
				CNetwork.Server.TransmitMicrophoneAudio(playerID, dataStream); 	
			}
			else
			{
				if(_cPlayer.PlayerId != playerID)
				{
					CNetwork.Server.TransmitMicrophoneAudio(playerID, dataStream); 		
				}
			}
		}
	}

	[ALocalOnly]
	void OnRecievedMicrophoneAudio(CNetworkStream _cAudioDataStream)
	{
		s_framesToDecode.Enqueue(_cAudioDataStream);
	}

	void Update()
	{	
		//EncodingUpdate();	
		//DecodingUpdate();
	}
	
	void EncodingUpdate()
	{		
		if(Microphone.devices.Length != 0)
		{
			if(m_bRecording == false)
			{
				//if(Input.GetKeyDown(KeyCode.LeftAlt))
				{
					m_acVoiceInput = Microphone.Start(Microphone.devices[0], true, m_kiRecordTime, 44000);
					
					//start timer
					m_fRecordingTimer = 0.0f; 
					m_bRecording = true;
				}
			}			
			else if(m_bRecording)
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
					
					m_EncodeThread = new Thread(new ParameterizedThreadStart(EncodeAudio));
					m_EncodeThread.Start((object)voiceData);
					
					m_bRecording = false;
				}
			}
		}		
		
		if(!m_EncodeThread.IsAlive && m_bEncoding)
		{
			m_bEncoding = false;
			CNetwork.Connection.TransmitMicrophoneAudio(s_AudioPacket);
			
			s_AudioPacket.Clear();
		}		
	}
	
	void DecodingUpdate()
	{
		if(s_decodedFrames.Count > 0)
		{
			//Take data from the queue to add to the audioclip.
			DecodeInformation decodedFrame = s_decodedFrames.Dequeue();
			short[] saDecodedFrames = decodedFrame.saDecodedData;
			int numSamples = decodedFrame.iNumSamples;
			int frequency = decodedFrame.iFrequency;
			CNetworkViewId senderViewID = decodedFrame.cSenderViewID;
			
			float[] faDecodedAudioData = new float[numSamples];
	
			for (int i = 0; i < faDecodedAudioData.Length; ++i)
			{
				faDecodedAudioData[i] = (float)((float)saDecodedFrames[i] / 32767.0f);
			}
	
			// Play audio at location of sender
			GameObject senderNetworkView = CNetworkView.FindUsingViewId(senderViewID).gameObject;
			AudioSource senderAudioSource = senderNetworkView.gameObject.GetComponent<AudioSource>();
			
			AudioClip newClip = AudioClip.Create("Test", faDecodedAudioData.Length, 1, frequency, true, false);
			newClip.SetData(faDecodedAudioData, 0);
						
			senderAudioSource.clip = newClip;
			senderAudioSource.volume = 1.0f;
			
			//AudioSystem.GetInstance.Play(newClip, senderNetworkView.gameObject.transform.position, 1.0f, 1.0f, false, 0.0f, AudioSystem.SoundType.SOUND_EFFECTS, true);
			CAudioSystem.Play(senderAudioSource, 1.0f, 1.0f, false, 0.0f, CAudioSystem.SoundType.SOUND_EFFECTS, true);								
		}
		
		if(s_framesToDecode.Count > 0)
		{			
			for(int i = 0; i < m_kiNumDecodeThreads; i++)
			{
				if(m_DecodeThreads[i].IsAlive == false)
				{
					m_DecodeThreads[i] = new Thread(new ParameterizedThreadStart(DecodeAudio));
					m_DecodeThreads[i].Start((object)s_framesToDecode.Dequeue());
					break;
				}
			}
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
	}
	
	void DecodeAudio(object _rawData)
	{
		//Convert incoming data to a CNetworkStream.
		CNetworkStream _cAudioDataStream = (CNetworkStream)_rawData; 
		
		//Pull relevant information out of the network stream.		
		int iFrequency = _cAudioDataStream.Read<int>();
		int iNumSamples = _cAudioDataStream.Read<int>();
		int iNumEncodedBytes = _cAudioDataStream.Read<int>();
		byte[] baEncodedData = _cAudioDataStream.ReadBytes(iNumEncodedBytes);
		CNetworkViewId cSenderViewID = 	_cAudioDataStream.Read<CNetworkViewId>();	
		
		// Decode
		short[] saDecodedFrames = new short[iNumSamples];
		int iNumDecodedBytes = m_eDecoder.Decode(baEncodedData, 0, iNumEncodedBytes, saDecodedFrames, 0, false);
		//Debug.Log("Decoded audio data size: " + iNumDecodedBytes + " : " + saDecodedFrames.Length);
				
		//Populate a new struct which can be accessed later.
		DecodeInformation decodedFrameInfo;		
		decodedFrameInfo.saDecodedData = saDecodedFrames;
		decodedFrameInfo.iFrequency = iFrequency;
		decodedFrameInfo.cSenderViewID = cSenderViewID;
		decodedFrameInfo.iNumSamples = iNumSamples;
		
		s_decodedFrames.Enqueue(decodedFrameInfo);	
	}
}
