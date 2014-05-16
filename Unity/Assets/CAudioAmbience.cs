using UnityEngine;
using System.Collections;

public class CAudioAmbience : MonoBehaviour 
{
	
	// Use this for initialization
	void Start () 
	{
		CAudioCue[] cues = GetComponents<CAudioCue>();
		foreach (CAudioCue cue in cues)
		{
			if(cue.m_strCueName == "Ambience")
			{
				cue.Play(1.0f, true, -1);
			}
		}
	}	
}
