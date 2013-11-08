
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioCue : MonoBehaviour 
{	
	public float pitchMin = 1.0f;
	public float pitchMax = 1.0f;
	public float volumeMin = 1.0f;
	public float volumeMax = 1.0f;
	public float[] fadeInTimeList;
	public float[] fadeOutTimeList;
	public AudioClip[] audioClipPool;
	public bool[] loopList; //Determines which sounds should be looped.
	public AudioSystem.SoundType soundType;
	private List<AudioSource> attachedAudioSource = new List<AudioSource>();
				
	//As implied, stops all sounds that are currently playing.
	public void StopAllSound()
	{
		if(attachedAudioSource != null)
		{
			foreach(AudioSource audioSource in attachedAudioSource)
			{
				if(audioSource != null && audioSource.isPlaying)
				{
					AudioSystem.GetInstance.StopSound(audioSource);
				}
			}		
			
			attachedAudioSource.Clear();
		}
	}
	
	//If there is relevant information set, this function will fade out sounds over time, then stop them.
	public void FadeOut()
	{
		if( fadeOutTimeList.Length > 0 && attachedAudioSource != null)
		{		
			for(int i = 0; i < attachedAudioSource.Count; i++)
			{
				if(attachedAudioSource[i] != null && attachedAudioSource[i].isPlaying && i < fadeOutTimeList.Length )
				{
					AudioSystem.GetInstance.FadeOut(attachedAudioSource[i], fadeOutTimeList[i]);
				}
				else if(attachedAudioSource[i] != null && attachedAudioSource[i].isPlaying )
				{
					//If the correct fadeout information cannot be found, simply stop the sound.
					AudioSystem.GetInstance.StopSound(attachedAudioSource[i]);										
				}
			}
			
			attachedAudioSource.Clear();
		}
		else
		{
			StopAllSound();
		}		
	}
	
	//This function will play all attached sounds, and will loop clips as determined by loopFlags.
	public void PlayAll( Transform parent, float volumeScale)
	{
		for(int i = 0; i < audioClipPool.Length; i++)
		{
			Play(parent, volumeScale, loopList[i], i);
		}
	}
	
	//Will play specified sound if one is set. Otherwise, a random clip will be used.
	//A random sound can be specified be setting the index to negative one (-1).
	public void Play( Transform parent, float volumeScale, bool loop, int index)
	{
		AudioSource newAudioSource;
		
		//Make sure fade in times are set. If not, assign default values.
		if(fadeInTimeList.Length < audioClipPool.Length)
		{
			fadeInTimeList = new float[audioClipPool.Length];
			for(int i = 0; i < audioClipPool.Length; i++)
			{
				fadeInTimeList[i] = 0.0f;
			}
		}
		
		//Make sure fade out times are set. If not, assign default values.
		if(fadeOutTimeList.Length < audioClipPool.Length)
		{
			fadeOutTimeList = new float[audioClipPool.Length];
			for(int i = 0; i < audioClipPool.Length; i++)
			{
				fadeOutTimeList[i] = 0.0f;
			}
		}
		
		//Assign a random index if one is not set.
		if(index == -1)
		{
			index = Random.Range(0, audioClipPool.Length);
		}
			
		//Allow the AudioSystem to handle the new audio source.
		newAudioSource = AudioSystem.GetInstance.Play(	audioClipPool[index], parent,
												  		 	Random.Range(volumeMin, volumeMax) * volumeScale,
												   			Random.Range(pitchMin, pitchMax), loop,
															fadeInTimeList[index],
															soundType);	
		
		//Add this to the list of attached audio sources.
		attachedAudioSource.Add(newAudioSource);
	}
	
	//Plays a random clip once, then discards it.
	public void PlayOneShot( float volumeScale, AudioSource audioSource)
	{
		if( audioSource == null )
			audioSource = GetComponent<AudioSource>();
		audioSource.pitch = UnityEngine.Random.Range( pitchMin, pitchMax );  // not at all sure this works w/ one shot. wishlist: test.
		var volume = UnityEngine.Random.Range ( volumeMin, volumeMax ) * volumeScale;
		
		audioSource.PlayOneShot(audioClipPool[ UnityEngine.Random.Range( 0, audioClipPool.Length) ], volume );	
	}	
	
	//Used to check if any sounds are playing.
	public bool IsPlaying()
	{
		foreach(AudioSource audioSource in attachedAudioSource)	
		{
			if(audioSource != null && audioSource.isPlaying)
			{
				return(true);
			}
		}
		
		return(false);
	}		
}


