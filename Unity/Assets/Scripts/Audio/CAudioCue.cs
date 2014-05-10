
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CAudioCue : MonoBehaviour 
{	
	public string m_strCueName;
	public float m_fPitchMin = 1.0f;
	public float m_fpitchMax = 1.0f;
	public float m_fVolumeMin = 1.0f;
	public float m_fVolumeMax = 1.0f;
	public float[] m_fFadeInTimeList = new float[0];
	public float[] m_fFadeOutTimeList = new float[0];
	public AudioClip[] m_arAudioClipPool = new AudioClip[0];
	public bool[] m_barLoopList = new bool[0]; //Determines which sounds should be looped.
	public bool m_bContinuousEvent = false; // if this is set to true, the audiocue will start a new clip after a specified amount of time.
	public float m_fPlayFrequency; //if an audio cue is a continuous event, this determines how often a new sound will be played. 
	public CAudioSystem.SoundType m_eSoundType;
	private List<AudioSource> m_arAttachedAudioSource = new List<AudioSource>();
	
	void Update()
	{
		
	}

	/// <summary>
	/// Initialises the sound so it can be played later.
	/// </summary>
	/// <param name="pathname">Path and name of the sound.</param>
	/// <param name="fadeInTime">Fade in effect, in seconds.</param>
	/// <param name="fadeOutTime">Face out effect, in seconds.</param>
	/// <param name="loop">Will loop the audio if true.</param>
	/// <returns>Sound index to provide to the Play function.</returns>
	public int AddSound(string pathname, float fadeInTime, float fadeOutTime, bool loop)
	{
		bool glitchInTheMatrix = false;	// Know when to get to the phone.
		int index = m_arAudioClipPool.Length;

		// Audio clip.
		AudioClip[] newAudioClipArray = new AudioClip[index + 1];	// Create new array to contain new AudioClip.
		for (int i = 0; i < index; ++i) newAudioClipArray[i] = m_arAudioClipPool[i];	// Copy over existing audio clips.
		newAudioClipArray[index] = Resources.Load<AudioClip>(pathname);	// Load new AudioClip from resource.
		if (newAudioClipArray[index] == null) { Debug.LogError("AudioClip " + pathname + " does not exist!"); glitchInTheMatrix = true; }	// Error check.
		m_arAudioClipPool = newAudioClipArray;	// Switch to new array.

		// Fade in time.
		float[] newFadeInTimeList = new float[index + 1];	// Create new array to contain new fade in time.
		for (int i = 0; i < index && i < m_fFadeInTimeList.Length; ++i) newFadeInTimeList[i] = m_fFadeInTimeList[i];	// Copy over existing fade in times.
		newFadeInTimeList[index] = fadeInTime;	// Add new fade in time.
		m_fFadeInTimeList = newFadeInTimeList;	// Switch to new fade in time array.

		// Fade out time.
		float[] newFadeOutTimeList = new float[index + 1];	// Create new array to contain new fade out time.
		for (int i = 0; i < index && i < m_fFadeOutTimeList.Length; ++i) newFadeOutTimeList[i] = m_fFadeOutTimeList[i];	// Copy over existing fade out times.
		newFadeOutTimeList[index] = fadeOutTime;	// Add new fade out time.
		m_fFadeOutTimeList = newFadeOutTimeList;	// Switch to new fade out time array.

		// Loop.
		bool[] newLoopList = new bool[index + 1];	// Create new array to contain new loop setting.
		for (int i = 0; i < index && i < m_barLoopList.Length; ++i) newLoopList[i] = m_barLoopList[i];	// Copy over existing loop settings.
		newLoopList[index] = loop;	// Add new loop setting.
		m_barLoopList = newLoopList;	// Switch to new loop setting array.

		return glitchInTheMatrix ? -1 : index;
	}
				
	//As implied, stops all sounds that are currently playing.
	public void StopAllSound()
	{
		//if(m_arAttachedAudioSource != null)	// Jade: Unnecessary as the list is created on construction.
		//{
			foreach(AudioSource audioSource in m_arAttachedAudioSource)
			{
				if(audioSource != null && audioSource.isPlaying)
				{
					CAudioSystem.StopSound(audioSource);
				}
			}		
			
			m_arAttachedAudioSource.Clear();
		//}
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
					CAudioSystem.FadeOut(m_arAttachedAudioSource[i], m_fFadeOutTimeList[i]);
				}
				else if(m_arAttachedAudioSource[i] != null && m_arAttachedAudioSource[i].isPlaying )
				{
					//If the correct fadeout information cannot be found, simply stop the sound.
					CAudioSystem.StopSound(m_arAttachedAudioSource[i]);										
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
		if(m_barLoopList.Length < m_arAudioClipPool.Length)
		{
			m_barLoopList = new bool[m_arAudioClipPool.Length];
			for(int i = 0; i < m_arAudioClipPool.Length; i++)
			{
				m_barLoopList[i] = false;
			}
		}

		for(int i = 0; i < m_arAudioClipPool.Length; i++)
		{
			Play(parent, volumeScale, m_barLoopList[i], i);
		}
	}
	
	//Will play specified sound if one is set. Otherwise, a random clip will be used.
	//A random sound can be specified be setting the index to negative one (-1).
	//Will only work if object already has an audiosource, else use the overloaded method
	public void Play( float volumeScale, bool loop, int index, int randomRangeStart = -1, int randomRangeEnd = -1)
	{
		AudioSource attachedAudioSource = GetComponent<AudioSource>();
		
		//Make sure fade in times are set. If not, assign default values.
		if (m_fFadeInTimeList.Length < m_arAudioClipPool.Length)
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
        if(index == -1 && randomRangeStart != -1       && 
                randomRangeEnd <= m_arAudioClipPool.Length   &&
                randomRangeEnd > 0)
        {
            index = Random.Range(randomRangeStart, randomRangeEnd);
        }

        else  if(index == -1 && randomRangeStart == -1)
        {
            index = Random.Range(0, m_arAudioClipPool.Length);
        }
			
		attachedAudioSource.clip = m_arAudioClipPool[index];
		
		//Allow the AudioSystem to handle the new audio source.
		CAudioSystem.Play(	attachedAudioSource, Random.Range(m_fVolumeMin, m_fVolumeMax) * volumeScale,
										Random.Range(m_fPitchMin, m_fpitchMax), loop,
										m_fFadeInTimeList[index],
										m_eSoundType, true );	
		
		//Add this to the list of attached audio sources.
		m_arAttachedAudioSource.Add(attachedAudioSource);
	}
	
	//Will play specified sound if one is set. Otherwise, a random clip will be used.
	//A random sound can be specified be setting the index to negative one (-1).
	//This function should only be used if the object needs to have an audiosource attached to it.
    public void Play( Transform parent, float volumeScale, bool loop, int index, int randomRangeStart = -1, int randomRangeEnd = -1)
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
        if(index == -1 && randomRangeStart != -1       && 
           randomRangeEnd <= m_arAudioClipPool.Length   &&
           randomRangeEnd > 0)
        {
            index = Random.Range(randomRangeStart, randomRangeEnd);
            if(index == 8)
            {
                Debug.Log("How the fuck?");    
                int i = 0;
            }
        }   		
        else if(index == -1)
        {
            index = Random.Range(0, m_arAudioClipPool.Length);
            if(index == 8)
            {
                Debug.Log("How the fuck?");
                int i = 0;
            }
        }
      
		//Allow the AudioSystem to handle the new audio source.
		newAudioSource = CAudioSystem.Play(	m_arAudioClipPool[index], parent,
														Random.Range(m_fVolumeMin, m_fVolumeMax) * volumeScale,
														Random.Range(m_fPitchMin, m_fpitchMax), loop,
														m_fFadeInTimeList[index],
														m_eSoundType, true);	
		
		//Add this to the list of attached audio sources.
		m_arAttachedAudioSource.Add(newAudioSource);
	}
	
	//Plays a random clip once, then discards it. Useful for sounds that will be played many times, especially if those sounds overlap
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