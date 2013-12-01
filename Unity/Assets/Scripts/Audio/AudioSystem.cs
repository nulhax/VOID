//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   AudioSystem.cs
//  Description :   Audio system implementation
//
//  Author  	:  Daniel Langsford
//  Mail    	:  folduppugg@hotmail.com
//

// Namespaces
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AudioSystem : Singleton<AudioSystem> 
{ 
	// Member Types
	public enum SoundType	
	{
		SOUND_EFFECTS,
		SOUND_MUSIC,
		SOUND_AMBIENCE,
		SOUND_OTHER,
	};
	
	public enum OcclusionState
	{
		OCCLUSION_FALSE,
		OCCLUSION_PARTIAL,
		OCCLUSION_FULL		
	};
	
	class ClipInfo
    {
	   	public float 		fadeInTime 		{ get; set; }
		public float 		fadeInTimer 	= 0;
		public float 		fadeOutTime 	{ get; set; }
		public float 		fadeOutTimer 	= 0;
		public AudioSource 	audioSource 	{ get; set; }
       	public float 		defaultVolume 	{ get; set; }
		public GameObject  	soundLocoation	{ get; set; }
		public SoundType	soundType;
		public bool 		useOcclusion	{ get; set; }	
    }

	// Member Delegates & Events
		
	// Member Properties
		
	// Member Fields	
	List<ClipInfo> m_activeAudio;
	private float musicVolume;
	private float effectsVolume;
	private AudioListener m_listener;
	private OcclusionState occludeState;
	
	// Member Functions
	
	void Awake() 
	{
        Debug.Log("AudioManager Initialising");
       		
		m_activeAudio = new List<ClipInfo>();
		m_listener = (AudioListener) FindObjectOfType(typeof(AudioListener));
		
		occludeState = OcclusionState.OCCLUSION_FALSE;
    }
	
	void Update() 
	{
		if(m_listener == null)
		{
			m_listener = (AudioListener) FindObjectOfType(typeof(AudioListener));
		}
		else
		{
			ProcessActiveAudio();
		}
	}
	
	void ProcessActiveAudio()
	{ 
	    var toRemove = new List<ClipInfo>();
	    try 
		{
	        foreach(ClipInfo audioClip in m_activeAudio) 
			{
	            if(!audioClip.audioSource || audioClip.audioSource.isPlaying == false) 
				{
	                toRemove.Add(audioClip);
	            } 
				else
				{
					//process fade in
					if(audioClip.fadeInTimer < audioClip.fadeInTime)
					{
												
						audioClip.fadeInTimer += Time.deltaTime;
						float timeScale = audioClip.fadeInTimer / audioClip.fadeInTime;						
						audioClip.audioSource.volume =  audioClip.defaultVolume * timeScale;
						
						if(audioClip.audioSource.volume >= audioClip.defaultVolume)
						{
							//Debug.Log("Clip faded in after " + audioClip.fadeInTimer.ToString() + " seconds");								
							//Debug.Log("Fade in time was set to: " + audioClip.fadeInTime.ToString() + " seconds");
						}
					}
					
					//process fade out
					if(audioClip.fadeOutTimer < audioClip.fadeOutTime)
					{
																		
						audioClip.fadeOutTimer += Time.deltaTime;
						float timeScale = audioClip.fadeOutTimer / audioClip.fadeOutTime;						
						audioClip.audioSource.volume = audioClip.defaultVolume * timeScale;
						
						//Remove the sound once it has faded out
						if(audioClip.fadeOutTimer >= audioClip.fadeOutTime || audioClip.audioSource.volume == 0)
						{
							toRemove.Add(audioClip);
							//Debug.Log("Clip faded out after " + audioClip.fadeOutTimer.ToString() + " seconds");								
							//Debug.Log("Fade out time was set to: " + audioClip.fadeOutTime.ToString() + " seconds" );
						}																	
					}					
				}
				
				//Process audio occlusion
				if(audioClip.useOcclusion)			
				{
					ProcessAudioOcclusion(audioClip);					
				}
			}
	    } 
		catch 
		{
	        Debug.Log("Error updating active audio clips");
	        return;
	    }		
		    
		// Cleanup
	    foreach(var audioClip in toRemove) 
		{
	        m_activeAudio.Remove(audioClip);
			Destroy(audioClip.soundLocoation);
		}
    }
	
	void ProcessAudioOcclusion(ClipInfo _audioClip)
	{
		//Get the audioListener in the scene
		Vector3 listenerPos = m_listener.transform.position;
		Vector3 sourcePos = _audioClip.audioSource.transform.position;
	
		
		int ignoreMask = 3 << 10;		
		ignoreMask = ~ignoreMask;
		
		RaycastHit hit;
        if(Physics.Linecast(sourcePos, listenerPos, out hit, ignoreMask))
		{
			Debug.DrawLine(	sourcePos, listenerPos);
			
           	if(hit.collider.tag != "Listener")
			{			
				//TODO:
				//For now, get every conduit in existence
				GameObject[] conduits = GameObject.FindGameObjectsWithTag("AudioConduit");
				bool occlude = true;
				
				//Before occluding, raycast from audio source to all nearby audio conduits.
				foreach(GameObject conduit in conduits)
				{
					RaycastHit sourceToConduit;
					if(Physics.Linecast(sourcePos, conduit.transform.position, out sourceToConduit))
					{					
						if(sourceToConduit.collider.tag == "AudioConduit")
						{						
							//If there is a conduit within sight of the audio source, check whether the listener has line of sight with the same conduit.				
							RaycastHit LinstenerToConduit;
							if(Physics.Linecast(listenerPos, conduit.transform.position, out LinstenerToConduit))
							{							
								if(LinstenerToConduit.collider.tag == "AudioConduit")
								{
									Debug.DrawLine(	sourcePos, conduit.transform.position, Color.red);
									Debug.DrawLine(	conduit.transform.position, listenerPos, Color.blue);
									
									occlude = false;
									_audioClip.audioSource.volume = _audioClip.defaultVolume / 2;
									
									if(occludeState != OcclusionState.OCCLUSION_PARTIAL)
									{
										occludeState = OcclusionState.OCCLUSION_PARTIAL;
										Debug.Log("Partial Occlusion");
									}									
								}
							}
						}
					}
				}				
				
				AudioLowPassFilter audioFilter = _audioClip.audioSource.gameObject.GetComponent<AudioLowPassFilter>(); 
				
				if(occlude)
				{
					if(audioFilter == null)
					{
						AudioLowPassFilter filter =_audioClip.audioSource.gameObject.AddComponent<AudioLowPassFilter>();
						filter.cutoffFrequency = 2000; 
					}
					
					_audioClip.audioSource.volume = _audioClip.defaultVolume / 10;
					
					if(occludeState != OcclusionState.OCCLUSION_FULL)
					{
						occludeState = OcclusionState.OCCLUSION_FULL;
						Debug.Log("Full Occlusion");
					}					
				}							
			}	
		
			else
			{	
				if(_audioClip.audioSource.gameObject.GetComponent<AudioLowPassFilter>() != null)
				{
					Destroy(_audioClip.audioSource.gameObject.GetComponent<AudioLowPassFilter>());
					_audioClip.audioSource.volume = _audioClip.defaultVolume;
				}
				
				if(occludeState != OcclusionState.OCCLUSION_FALSE)
				{
					occludeState = OcclusionState.OCCLUSION_FALSE;
					Debug.Log("No Occlusion");
				}	
			}
					 
		}
	}
	
	public AudioSource Play(AudioClip _clip, Vector3 _soundOrigin, float _volume, float _pitch, bool _loop,
							float _fadeInTime, SoundType _soundType, bool _useOcclusion) 
	{
		//Create an empty game object
		GameObject soundLoc = new GameObject("Audio: " + _clip.name);
		soundLoc.transform.position = _soundOrigin;
		
		//Create the source
		AudioSource audioSource = soundLoc.AddComponent<AudioSource>();
		
		if(_fadeInTime > 0)
		{
			SetAudioSource(ref audioSource, _clip, 0);
		}
		else
		{
			SetAudioSource(ref audioSource, _clip, _volume);
		}
		audioSource.Play();
		
		// Set the audio to loop
		if(_loop) 
		{
			audioSource.loop = true;
		}
		else
		{
			Destroy(soundLoc, _clip.length);
		}
		
		//Set the source as active
		m_activeAudio.Add(new ClipInfo { fadeInTime = _fadeInTime, fadeInTimer = 0, audioSource = audioSource, defaultVolume = _volume,
										 soundLocoation = soundLoc, soundType = _soundType, useOcclusion = _useOcclusion});
		return(audioSource);
	}
	
	public AudioSource Play(AudioClip _clip, Transform _emitter, float _volume, float _pitch, bool _loop,
							float _fadeInTime, SoundType _soundType, bool _useOcclusion) 
	{
		
		//Create the source
		AudioSource audioSource = Play(_clip, _emitter.position, _volume, _pitch, _loop, _fadeInTime, _soundType, _useOcclusion);
		audioSource.transform.parent = _emitter;
				
		return(audioSource);
	}
	
	public void Play(AudioSource _source, float _volume, float _pitch, bool _loop, float _fadeInTime, SoundType _soundType,  bool _useOcclusion)
	{
		if(_fadeInTime > 0)
		{
			_source.volume = 0;
		}
		else
		{
			_source.volume = _volume;
		}
		
		_source.loop = _loop;
		
		m_activeAudio.Add(new ClipInfo { fadeInTime = _fadeInTime, fadeInTimer = 0, fadeOutTime = 0, audioSource = _source, defaultVolume = _volume,
										 soundType = _soundType, useOcclusion = _useOcclusion});
		
		_source.Play();				
	}
	
	private void SetAudioSource(ref AudioSource _source, AudioClip _clip, float _volume) 
	{
		_source.rolloffMode = AudioRolloffMode.Logarithmic;
		_source.dopplerLevel = 0.01f;
		_source.minDistance = 1.0f;
		_source.maxDistance = 5.0f;
		_source.clip = _clip;
		_source.volume = _volume;
	}
	
	public void StopSound(AudioSource _toStop) 
	{
		try 
		{
			Destroy(m_activeAudio.Find(s => s.audioSource == _toStop).audioSource.gameObject);
		} 
		catch 
		{
			Debug.Log("Error trying to stop audio source " + _toStop);
		}
	}
	
	public void FadeOut(AudioSource _toStop, float _fadeOutTime) 
	{
		if(_fadeOutTime == 0.0f)
		{
			_fadeOutTime = 0.1f;
		}
		
		m_activeAudio.Find(s => s.audioSource == _toStop).fadeOutTime = _fadeOutTime;	
		m_activeAudio.Find(s => s.audioSource == _toStop).defaultVolume = 0.0f;
	}
	
	public void BalanceVolumes(float _musicVolume, float _effectsVolume)
	{
		if(_musicVolume > 0 && _musicVolume < 1 &&
		   _effectsVolume > 0 && _effectsVolume < 1)
		{
			musicVolume = _musicVolume;
			effectsVolume = _effectsVolume;
			
			foreach(var audioClip in m_activeAudio) 
			{
				switch(audioClip.soundType)
				{
					case SoundType.SOUND_MUSIC:
					{
						//scale all music sounds by the new music volume 
						audioClip.audioSource.volume = audioClip.defaultVolume * _musicVolume;
						break;
					}
					
					case SoundType.SOUND_EFFECTS:
					{
						//scale all sound effects by the SFX volume 
						audioClip.audioSource.volume = audioClip.defaultVolume * _effectsVolume;
						break;
					}
					
					case SoundType.SOUND_AMBIENCE:
					{
						//Ambient sounds will only play when there is no music volume
						if(_musicVolume == 0)
						{
							audioClip.audioSource.volume = audioClip.defaultVolume * _effectsVolume;
						}
						else
						{
							audioClip.audioSource.volume = 0.0f;
						}
						
						break;						
					}
				}
			}
		}
	}
	
	//Returns data from an AudioClip as a byte array.
	public byte[] GetClipData(AudioClip _clip)
	{
		//Get data
		float[] floatData = new float[_clip.samples * _clip.channels];
		_clip.GetData(floatData,0);			
		
		//convert to byte array
		byte[] byteData = new byte[floatData.Length * 4];
		Buffer.BlockCopy(floatData, 0, byteData, 0, byteData.Length);
		
		return(byteData);
	}	
};
