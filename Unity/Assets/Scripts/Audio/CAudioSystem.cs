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

public class CAudioSystem : MonoBehaviour
{ 
	// Member Types
	public enum SoundType	
	{
		SOUND_EFFECTS,
		SOUND_MUSIC,
		SOUND_AMBIENCE,
		SOUND_VOICE,
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
		public float 		defaultVolume 	= 1;
		public GameObject  	soundLocoation	{ get; set; }
		public SoundType	soundType;
		public bool 		useOcclusion	{ get; set; }	
    }

	// Member Delegates & Events
		
	// Member Properties
	public static CAudioSystem Instance
	{
		get { return (s_cInstance); }
	}	
	
	// Member Fields	
	static List<ClipInfo> s_activeAudio;
	
	static private float m_fMusicVolume = 0.75f;
	static private float m_fEffectsVolume = 0.75f;
	static private float m_fVoiceVolume = 1;
	static private float m_fAmbienceVolume = 0.75f;
	static private AudioListener m_listener;
	static private OcclusionState occludeState;

	bool m_bOccludeAll = false;
	
	static CAudioSystem s_cInstance = null;
	
	// Member Functions
	
	void Awake() 
	{
		s_cInstance = this;
		
        //Debug.Log("AudioManager Initialising");
       		
		s_activeAudio = new List<ClipInfo>();
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

		//Debug grossness!
		if(Input.GetKeyDown(KeyCode.Keypad1))
		{
			BalanceVolumes(-1, m_fEffectsVolume - 0.1f, -1, -1);
		}
		if(Input.GetKeyDown(KeyCode.Keypad2))
		{
			BalanceVolumes(-1, -1, m_fAmbienceVolume - 0.1f, -1);
		}
		if(Input.GetKeyDown(KeyCode.Keypad3))
		{
			BalanceVolumes(-1, -1, -1, m_fVoiceVolume - 0.1f);
		}
		if(Input.GetKeyDown(KeyCode.Keypad4))
		{
			BalanceVolumes(-1, m_fEffectsVolume + 0.1f, -1, -1);
		}
		if(Input.GetKeyDown(KeyCode.Keypad5))
		{
			BalanceVolumes(-1, -1, m_fAmbienceVolume + 0.1f, -1);
		}
		if(Input.GetKeyDown(KeyCode.Keypad6))
		{
			BalanceVolumes(-1, -1, -1, m_fVoiceVolume + 0.1f);
		}

	}
	
	void ProcessActiveAudio()
	{ 
	    var toRemove = new List<ClipInfo>();
	    //try 
		{
	        foreach(ClipInfo audioClip in s_activeAudio) 
			{
	            if(!audioClip.audioSource || audioClip.audioSource.isPlaying == false) 
				{
	                toRemove.Add(audioClip);
	            } 
				else
				{
					//process current volume scaling
					ScaleVolume(audioClip);

					//Process audio occlusion
					if(audioClip.soundType != SoundType.SOUND_AMBIENCE && audioClip.useOcclusion)			
					{
						ProcessAudioOcclusion(audioClip);					
					}

					//process fade in
					if(audioClip.fadeInTimer < audioClip.fadeInTime)
					{												
						audioClip.fadeInTimer += Time.deltaTime;
						float timeScale = audioClip.fadeInTimer / audioClip.fadeInTime;						
						audioClip.audioSource.volume =  audioClip.defaultVolume * timeScale;
						
						if(audioClip.audioSource.volume >= audioClip.defaultVolume)
						{

						}
					}
					
					//process fade out
					if(audioClip.fadeOutTimer < audioClip.fadeOutTime)
					{
																		
						audioClip.fadeOutTimer += Time.deltaTime;
						float timeScale = 1 - audioClip.fadeOutTimer / audioClip.fadeOutTime;						
						audioClip.audioSource.volume = audioClip.defaultVolume * timeScale;
						
						//Remove the sound once it has faded out
						if(audioClip.fadeOutTimer >= audioClip.fadeOutTime || audioClip.audioSource.volume == 0)
						{
							toRemove.Add(audioClip);
						}																	
					}					
				}
			}
	    } 
			
		    
		// Cleanup
	    foreach(var audioClip in toRemove) 
		{
	        s_activeAudio.Remove(audioClip);
			Destroy(audioClip.soundLocoation);
		}
    }

	void ScaleVolume(ClipInfo _currentClip)
	{
		switch(_currentClip.soundType)
		{
			case SoundType.SOUND_MUSIC:
			{
				//scale all music sounds by the new music volume 
				_currentClip.audioSource.volume = _currentClip.defaultVolume * m_fMusicVolume;
				break;
			}
				
			case SoundType.SOUND_EFFECTS:
			{
				//scale all sound effects by the SFX volume 
				_currentClip.audioSource.volume = _currentClip.defaultVolume * m_fEffectsVolume;
				break;
			}

			case SoundType.SOUND_VOICE:
			{
				//scale all sound effects by the SFX volume 
				_currentClip.audioSource.volume = _currentClip.defaultVolume * m_fVoiceVolume;
				break;
			}
				
			case SoundType.SOUND_AMBIENCE:
			{
				_currentClip.audioSource.volume = _currentClip.defaultVolume * m_fAmbienceVolume;	
				break;						
			}	
		}
	}
	
	void ProcessAudioOcclusion(ClipInfo _audioClip)
	{
		//Get the audioListener in the scene
		Vector3 listenerPos = m_listener.transform.position;
		GameObject soundOwner = _audioClip.audioSource.gameObject;

		Vector3 sourcePos = soundOwner.transform.position;

		//Use collider position!
		if(soundOwner.GetComponent<SphereCollider>() != null)
		{
			sourcePos += soundOwner.GetComponent<SphereCollider>().center;
		}
		if(soundOwner.GetComponent<BoxCollider>() != null)
		{
			sourcePos += soundOwner.GetComponent<BoxCollider>().center;
		}
		if(soundOwner.GetComponent<CapsuleCollider>() != null)
		{
			sourcePos += soundOwner.GetComponent<CapsuleCollider>().center;
		}

		Vector3 direction = listenerPos - sourcePos;
		bool bOccluded = false;

		RaycastHit[] classicHits;
		classicHits = Physics.RaycastAll(listenerPos, direction, direction.magnitude);
		{
			//Debug.DrawLine(sourcePos, listenerPos, Color.cyan, 1.0f);
			Debug.DrawLine(listenerPos, sourcePos, Color.yellow, 1.0f);
			foreach(RaycastHit hit in classicHits)
			{	
	           	if(hit.collider.gameObject != _audioClip.audioSource.gameObject && 
				   hit.collider.isTrigger == false && bOccluded == false)
				{
					//Occlusion
					//Debug.Log(_audioClip.audioSource.clip.name + " occluded by " + hit.collider.gameObject.name);
					bOccluded = true;
					break;
				}			
				else
				{	
					//No occlusion
					bOccluded = false;
				}
			}					 
		}

		if(bOccluded || m_bOccludeAll)
		{
			//Add Occlusion
			AudioLowPassFilter audioFilter = _audioClip.audioSource.gameObject.GetComponent<AudioLowPassFilter>(); 
			
			if(audioFilter == null)
			{
				//Debug.Log("added filter");
				AudioLowPassFilter filter =_audioClip.audioSource.gameObject.AddComponent<AudioLowPassFilter>();
				filter.cutoffFrequency = 1500; 
				_audioClip.defaultVolume /= 2;
			}
		}
		else
		{
			//Remove Occlusion
			if(_audioClip.audioSource.gameObject.GetComponent<AudioLowPassFilter>() != null)
			{
				//Debug.Log("Removed filter");
				Destroy(_audioClip.audioSource.gameObject.GetComponent<AudioLowPassFilter>());
				_audioClip.defaultVolume *= 2;
				bOccluded = true;
			}
		}
	}

	public void SetOccludeAll(bool _bOccludeAll)
	{
		m_bOccludeAll = _bOccludeAll;
	}
	
	public static AudioSource Play(AudioClip _clip, Vector3 _soundOrigin, float _volume, float _pitch, bool _loop,
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
		s_activeAudio.Add(new ClipInfo { fadeInTime = _fadeInTime, fadeInTimer = 0, audioSource = audioSource, defaultVolume = _volume,
										 soundLocoation = soundLoc, soundType = _soundType, useOcclusion = _useOcclusion});
		return(audioSource);
	}
	
	public static AudioSource Play(AudioClip _clip, Transform _emitter, float _volume, float _pitch, bool _loop,
							float _fadeInTime, SoundType _soundType, bool _useOcclusion) 
	{
		
		//Create the source
		AudioSource audioSource = Play(_clip, _emitter.position, _volume, _pitch, _loop, _fadeInTime, _soundType, _useOcclusion);
		audioSource.transform.parent = _emitter;
				
		return(audioSource);
	}
	
	public static AudioSource Play(AudioSource _source, float _volume, float _pitch, bool _loop, float _fadeInTime, SoundType _soundType,  bool _useOcclusion)
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
		
		s_activeAudio.Add(new ClipInfo { fadeInTime = _fadeInTime, fadeInTimer = 0, fadeOutTime = 0, audioSource = _source, defaultVolume = _volume,
										 soundType = _soundType, useOcclusion = _useOcclusion});
		
		_source.Play();
		
		return(_source);
	}

	public static AudioSource Play(AudioSource _source, float _volume, SoundType _soundType,  bool _useOcclusion)
	{

		_source.volume = _volume;
		
		_source.loop = false;
		
		s_activeAudio.Add(new ClipInfo { fadeInTime = 0, fadeInTimer = 0, fadeOutTime = 0, audioSource = _source, defaultVolume = _volume,
			soundType = _soundType, useOcclusion = _useOcclusion});
		
		_source.Play();
		
		return(_source);
	}
	
	private static void SetAudioSource(ref AudioSource _source, AudioClip _clip, float _volume) 
	{
		_source.rolloffMode = AudioRolloffMode.Linear;
		_source.dopplerLevel = 0.01f;
		_source.minDistance = 1.0f;
		_source.maxDistance = 5.0f;
		_source.clip = _clip;
		_source.volume = _volume;
	}
	
	public static void StopSound(AudioSource _toStop) 
	{
		try 
		{
			ClipInfo clip = s_activeAudio.Find(s => s.audioSource == _toStop);
            s_activeAudio.Remove(clip);
            _toStop.Stop();
		} 
		catch 
		{
			Debug.Log("Error trying to stop audio source " + _toStop);
		}
	}
	
	public static void FadeOut(AudioSource _toStop, float _fadeOutTime) 
	{
		if(_fadeOutTime == 0.0f)
		{
			_fadeOutTime = 0.1f;
		}
		
		s_activeAudio.Find(s => s.audioSource == _toStop).fadeOutTime = _fadeOutTime;	
	}

	//Pass in -1 if you don't wish to change a volume
	//For all other values, pass in a float between 0 and 1
	public static void BalanceVolumes(float _musicVolume, float _effectsVolume, float _ambienceVolume, float _voiceVolume)
	{
		if(_musicVolume > -1 && _musicVolume < 1)		  
		{
			m_fMusicVolume = _musicVolume;
		}
		if( _effectsVolume > -1 && _effectsVolume < 1)
		{
			m_fEffectsVolume = _effectsVolume;
		}
		if(_ambienceVolume > -1 && _ambienceVolume < 1)		  
		{
			m_fAmbienceVolume = _ambienceVolume;
		}
		if( _voiceVolume > -1 && _voiceVolume < 1)
		{
			m_fVoiceVolume = _voiceVolume;
		}
	}
	
	//Returns data from an AudioClip as a byte array.
	public static byte[] GetClipData(AudioClip _clip)
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
