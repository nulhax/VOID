using UnityEngine;
using System.Collections;

public class CPlayerSoundEffects : MonoBehaviour 
{

    CAudioCue m_FootStepCue = null;

	// Use this for initialization
	void Start () 
	{
		CAudioCue[] audioCues = GetComponents<CAudioCue>();

        foreach(CAudioCue cue in audioCues)
        {
            if(cue.m_strCueName == "FootSteps")
            {
                m_FootStepCue = cue;
            }
        }
	}

	void PlayFootStep()
	{
		m_FootStepCue.Play(gameObject.transform, 1.0f, false, -1, 0, 3);
	}

    void PlayLandingAudio()
    {
        m_FootStepCue.Play(gameObject.transform, 1.0f, false, 5);
        Debug.Log("Playing landing sound");
    }

    void PlaySlideAudio()
    {
        m_FootStepCue.Play(gameObject.transform, 1.0f, false, 6);
        Debug.Log("Playing landing sound");
    }
}
