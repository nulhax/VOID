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
    }

	// Member Delegates & Events
		
	// Member Properties
		
	// Member Fields	
	List<ClipInfo> m_activeAudio;
	private float musicVolume;
	private float effectsVolume;
	
	// Member Functions
	
	void Awake() 
	{
        Debug.Log("AudioManager Initialising");
       		
		m_activeAudio = new List<ClipInfo>();
    }
	
	void Update() 
	{
		ProcessActiveAudio();
	}
	
	void ProcessActiveAudio()
	{ 
	    var toRemove = new List<ClipInfo>();
	    try 
		{
	        foreach(ClipInfo audioClip in m_activeAudio) 
			{
	            if(!audioClip.audioSource) 
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
	
	public AudioSource Play(AudioClip _clip, Vector3 _soundOrigin, float _volume, float _pitch, bool _loop, float _fadeInTime, SoundType _soundType) 
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
		m_activeAudio.Add(new ClipInfo { fadeInTime = _fadeInTime, fadeInTimer = 0, audioSource = audioSource,
										defaultVolume = _volume, soundLocoation = soundLoc, soundType = _soundType});
		return(audioSource);
	}
	
	public AudioSource Play(AudioClip _clip, Transform _emitter, float _volume, float _pitch, bool _loop, float _fadeInTime, SoundType _soundType) 
	{
		
		//Create the source
		AudioSource audioSource = Play(_clip, _emitter.position, _volume, _pitch, _loop, _fadeInTime, _soundType);
		audioSource.transform.parent = _emitter;
				
		return(audioSource);
	}
	
	public AudioSource PlaySource(AudioSource _source, float _volume, float _pitch, bool _loop, float _fadeInTime, SoundType _soundType)
	{
		if(_fadeInTime > 0)
		{
			_source.volume = 0;
		}
		else
		{
			_source.volume = _volume;
		}
				
		GameObject soundLoc = new GameObject("Audio: " + _source.clip.name);
		soundLoc.transform.position = _source.transform.position;
		
		m_activeAudio.Add(new ClipInfo { fadeInTime = _fadeInTime, fadeInTimer = 0, audioSource = _source,
										defaultVolume = _volume, soundLocoation = soundLoc, soundType = _soundType});
		
		_source.Play();
		
		return(_source);
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
