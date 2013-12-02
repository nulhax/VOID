
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioCue : MonoBehaviour 
{	
	public string m_strCueName;
	public float m_fPitchMin = 1.0f;
	public float m_fpitchMax = 1.0f;
	public float m_fVolumeMin = 1.0f;
	public float m_fVolumeMax = 1.0f;
	public float[] m_fFadeInTimeList;
	public float[] m_fFadeOutTimeList;
	public AudioClip[] m_arAudioClipPool;
	public bool[] m_barLoopList; //Determines which sounds should be looped.
	public bool m_bContinuousEvent = false; // if this is set to true, the audiocue will start a new clip after a specified amount of time.
	public float m_fPlayFrequency; //if an audio cue is a continuous event, this determines how often a new sound will be played. 
	public AudioSystem.SoundType m_eSoundType;
	private List<AudioSource> m_arAttachedAudioSource = new List<AudioSource>();
	
	void Update()
	{
		
	}
				
	//As implied, stops all sounds that are currently playing.
	public void StopAllSound()
	{
		if(m_arAttachedAudioSource != null)
		{
			foreach(AudioSource audioSource in m_arAttachedAudioSource)
			{
				if(audioSource != null && audioSource.isPlaying)
				{
					AudioSystem.GetInstance.StopSound(audioSource);
				}
			}		
			
			m_arAttachedAudioSource.Clear();
		}
	}
	
	//If there is relevant information set, this function will fade out sounds over time, then stop them.
	public void FadeOut()
	{
		if( m_fFadeOutTimeList.Length > 0 && m_arAttachedAudioSource != null)
		{		
			for(int i = 0; i < m_arAttachedAudioSource.Count; i++)
			{
				if(m_arAttachedAudioSource[i] != null && m_arAttachedAudioSource[i].isPlaying && i < m_fFadeOutTimeList.Length )
				{
					AudioSystem.GetInstance.FadeOut(m_arAttachedAudioSource[i], m_fFadeOutTimeList[i]);
				}
				else if(m_arAttachedAudioSource[i] != null && m_arAttachedAudioSource[i].isPlaying )
				{
					//If the correct fadeout information cannot be found, simply stop the sound.
					AudioSystem.GetInstance.StopSound(m_arAttachedAudioSource[i]);										
				}
			}
			
			m_arAttachedAudioSource.Clear();
		}
		else
		{
			StopAllSound();
		}		
	}
	
	//This function will play all attached sounds, and will loop clips as determined by loopFlags.
	public void PlayAll( Transform parent, float volumeScale)
	{
		for(int i = 0; i < m_arAudioClipPool.Length; i++)
		{
			Play(parent, volumeScale, m_barLoopList[i], i);
		}
	}
	
	//Will play specified sound if one is set. Otherwise, a random clip will be used.
	//A random sound can be specified be setting the index to negative one (-1).
	//Will only work if object already has an audiosource, else use the overloaded method
	public void Play( float volumeScale, bool loop, int index)
	{
		AudioSource newAudioSource = GetComponent<AudioSource>();
		
		//Make sure fade in times are set. If not, assign default values.
		if(m_fFadeInTimeList.Length < m_arAudioClipPool.Length)
		{
			m_fFadeInTimeList = new float[m_arAudioClipPool.Length];
			for(int i = 0; i < m_arAudioClipPool.Length; i++)
			{
				m_fFadeInTimeList[i] = 0.0f;
			}
		}
		
		//Make sure fade out times are set. If not, assign default values.
		if(m_fFadeOutTimeList.Length < m_arAudioClipPool.Length)
		{
			m_fFadeOutTimeList = new float[m_arAudioClipPool.Length];
			for(int i = 0; i < m_arAudioClipPool.Length; i++)
			{
				m_fFadeOutTimeList[i] = 0.0f;
			}
		}
		
		//Assign a random index if one is not set.
		if(index == -1)
		{
			index = Random.Range(0, m_arAudioClipPool.Length);
		}
			
		newAudioSource.clip = m_arAudioClipPool[index];
		
		//Allow the AudioSystem to handle the new audio source.
		AudioSystem.GetInstance.Play( 	newAudioSource, Random.Range(m_fVolumeMin, m_fVolumeMax) * volumeScale,
								   		Random.Range(m_fPitchMin, m_fpitchMax), loop,
										m_fFadeInTimeList[index],
										m_eSoundType, true );	
		
		//Add this to the list of attached audio sources.
		m_arAttachedAudioSource.Add(newAudioSource);
	}
	
	//Will play specified sound if one is set. Otherwise, a random clip will be used.
	//A random sound can be specified be setting the index to negative one (-1).
	//This function should only be used if the object needs to have an audiosource attached to it.
	
	public void Play( Transform parent, float volumeScale, bool loop, int index)
	{
		AudioSource newAudioSource;
		
		//Make sure fade in times are set. If not, assign default values.
		if(m_fFadeInTimeList.Length < m_arAudioClipPool.Length)
		{
			m_fFadeInTimeList = new float[m_arAudioClipPool.Length];
			for(int i = 0; i < m_arAudioClipPool.Length; i++)
			{
				m_fFadeInTimeList[i] = 0.0f;
			}
		}
		
		//Make sure fade out times are set. If not, assign default values.
		if(m_fFadeOutTimeList.Length < m_arAudioClipPool.Length)
		{
			m_fFadeOutTimeList = new float[m_arAudioClipPool.Length];
			for(int i = 0; i < m_arAudioClipPool.Length; i++)
			{
				m_fFadeOutTimeList[i] = 0.0f;
			}
		}
		
		//Assign a random index if one is not set.
		if(index == -1)
		{
			index = Random.Range(0, m_arAudioClipPool.Length);
		}
			
		//Allow the AudioSystem to handle the new audio source.
		newAudioSource = AudioSystem.GetInstance.Play(	m_arAudioClipPool[index], parent,
											  		 	Random.Range(m_fVolumeMin, m_fVolumeMax) * volumeScale,
											   			Random.Range(m_fPitchMin, m_fpitchMax), loop,
														m_fFadeInTimeList[index],
														m_eSoundType, true);	
		
		//Add this to the list of attached audio sources.
		m_arAttachedAudioSource.Add(newAudioSource);
	}
	
	//Plays a random clip once, then discards it.
	public void PlayOneShot( float volumeScale, AudioSource audioSource)
	{
		if( audioSource == null )
		{
			audioSource = GetComponent<AudioSource>();
		}
		audioSource.pitch = UnityEngine.Random.Range( m_fPitchMin, m_fpitchMax );
		var volume = UnityEngine.Random.Range ( m_fVolumeMin, m_fVolumeMax ) * volumeScale;
		
		audioSource.PlayOneShot(m_arAudioClipPool[ UnityEngine.Random.Range( 0, m_arAudioClipPool.Length) ], volume );	
	}	
	
	//Used to check if any sounds are playing.
	public bool IsPlaying()
	{
		foreach(AudioSource audioSource in m_arAttachedAudioSource)	
		{
			if(audioSource != null && audioSource.isPlaying)
			{
				return(true);
			}
		}
		
		return(false);
	}		
}


