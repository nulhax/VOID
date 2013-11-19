//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   EngineBehaviour.cs
//  Description :   Core logic for the engine
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
	static SpeexEncoder encoder = new SpeexEncoder(BandMode.Wide);
	static SpeexDecoder decoder = new SpeexDecoder(BandMode.Wide, false);
	static SpeexJitterBuffer jitterBuffer = new SpeexJitterBuffer(decoder);
	public AudioClip testClip;
	private int encodedSize = 0;
		
	// Use this for initialization
	void Start () 
	{
		if(CNetwork.IsServer)
		{
			CNetwork.Server.EventRecievedPlayerMicrophoneAudio += new CNetworkServer.NotifyRecievedMicrophoneAudio(Decode);
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.Tab))
		{
			//Get raw clip data
			float[] clipData = new float[testClip.samples];//testClip.channels * 
			testClip.GetData(clipData, 0);			
			
			int sizeInMegabytes = (clipData.Length * sizeof(float)) / 1000;
			Debug.Log("Raw clip is " + sizeInMegabytes.ToString() + " Kilobytes long.");
			
			//Make sure to only send in complete frames. Discard any additional data which does not fit in a full frame.
			int numSamplesToEncode = testClip.samples;
			numSamplesToEncode -= numSamplesToEncode % encoder.FrameSize;
						
			CNetworkStream decodedData = new CNetworkStream();
			
			encodedSize = 0;
			
			//Break up samples into frames. Iterate through these frames and encode each one.
			for(int i = 0 ; i < numSamplesToEncode; i += encoder.FrameSize)
			{
				//create a buffer equal to the length of the encoder frame size
				float[] clipFrame = new float[encoder.FrameSize];
				//Populate with current frame data
				Buffer.BlockCopy(clipData,i,clipFrame,0,encoder.FrameSize * sizeof(float));
				
				int numRawBytes = encoder.FrameSize * sizeof(float);
								
				byte[] encodedFrameBuffer = EncodeData(clipFrame, numRawBytes);
				
				CNetworkStream newStream = new CNetworkStream();
				newStream.Write(numRawBytes);
				newStream.Write(encodedFrameBuffer);
				
				decodedData.Write(Decode(null, newStream));				
			}	
			
			int decodedSizeInKiloBytes = encodedSize / 1024;
			Debug.Log("Encoded data size was " + decodedSizeInKiloBytes.ToString() + " kilobytes");
			
			//Now take the data saved into the stream and place it in a new audioclip
			byte[] decodedBytes = decodedData.ReadBytes(decodedData.NumUnreadBytes);
						
			//convert to float array
			float[] decodedFloatData = new float[decodedBytes.Length / sizeof(float)];
			Buffer.BlockCopy(decodedBytes,0,decodedFloatData,0,decodedBytes.Length);
			
			AudioClip newClip = AudioClip.Create("Test", testClip.samples, testClip.channels, testClip.frequency, false, false);
			newClip.SetData(decodedFloatData, 0);
			
			AudioSource newSource = gameObject.AddComponent<AudioSource>();
			newSource.clip = newClip;
			newSource.Play();
			
		}		
	}
	
	public byte[] EncodeData(float[] rawData, int bytesRecorded)
	{
		// note: the number of samples per frame must be a multiple of encoder.FrameSize
		bytesRecorded -= bytesRecorded % encoder.FrameSize;
		
		short[] data = new short[bytesRecorded / sizeof(short)];
		Buffer.BlockCopy(rawData, 0, data, 0, bytesRecorded);	
		
		var encodedData = new byte[bytesRecorded];			
		var encodedBytes = encoder.Encode(data, 0, data.Length, encodedData, 0, encodedData.Length);
		encodedSize += encodedBytes;
		
		var returnData = new byte[encodedBytes];
		Buffer.BlockCopy(encodedData, 0, returnData, 0, encodedBytes);			
		
		return(returnData);	
	}
	
	public void SendEncodedData(byte[] encodedData, int bytesRecorded)
	{
		CNetworkStream newStream = new CNetworkStream();
		newStream.Write(bytesRecorded);
		newStream.Write(encodedData);
								
		CNetwork.Connection.TransmitMicrophoneAudio(newStream);	
	}
	
	private byte[] Decode(CNetworkPlayer _cPlayer, CNetworkStream _cAudioDataStream)
	{
		/*int iSamples = _cAudioDataStream.ReadInt();
		iSamples -= iSamples % decoder.FrameSize;
		
		//This will be populated with all decoded chunks.
		short[] decodedData = new short[iSamples / 2];
		
		//Data must be decoded in chunks.
		//Iterate through all samples, decoding each chunk.
		//Chunks should be equal to the decoder framesize.
		
		for(int i = 0; i < iSamples; i += decoder.FrameSize)
		{		
			byte[] encodedData = new byte[decoder.FrameSize];	
			encodedData = _cAudioDataStream.ReadBytes(decoder.FrameSize);
			
			short[] decodedFrame = new short[decoder.FrameSize / 2];
			decoder.Decode(encodedData, 0, decoder.FrameSize, decodedFrame, 0, false);
			
			//Add this new decoded block into the buffer
			Buffer.BlockCopy(encodedData, 0, decodedData, i, encodedData.Length);
		}*/
		
		int BytesRecorded = _cAudioDataStream.ReadInt();
		
		byte[] encodedData = _cAudioDataStream.ReadBytes(_cAudioDataStream.NumUnreadBytes);
		
		short[] decodedFrame = new short[BytesRecorded];
		int numDecoded = decoder.Decode(encodedData, 0, encodedData.Length, decodedFrame, 0, false);
		
		byte[] dedocedBytes = new byte[numDecoded * sizeof(short)];
		Buffer.BlockCopy(decodedFrame,0,dedocedBytes,0,numDecoded * 2); 
		
		return(dedocedBytes);
		// todo: do something with the decoded data
	}
}
