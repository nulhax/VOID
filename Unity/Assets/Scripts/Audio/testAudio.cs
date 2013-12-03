using UnityEngine;
using System.Collections;

public class testAudio : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
	{
		AudioSource test = GetComponent<AudioSource>();
		
		AudioSystem.Play(test, 1.0f, 1.0f, true, 0.0f, AudioSystem.SoundType.SOUND_EFFECTS, true);
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
