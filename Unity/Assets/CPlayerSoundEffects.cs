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

		GetComponent<CPlayerMotor>().EventStateChange += OnMovementStateChange;
	}

	void OnMovementStateChange(CPlayerMotor.EState _ePrevious, CPlayerMotor.EState _eNew)
	{
		if(_eNew == CPlayerMotor.EState.AirThustersInSpace)
		{
			CAudioSystem.Instance.SetOccludeAll(true);
		}
		else
		{
			CAudioSystem.Instance.SetOccludeAll(false);
		}
	}

	void PlayFootStep()
	{
		m_FootStepCue.Play(gameObject.transform, 1.0f, false, -1, 0, 3);
	}

    void PlayLandingAudio()
    {
        m_FootStepCue.Play(gameObject.transform, 1.0f, false, 5);       
    }

    void PlaySlideAudio()
    {
        m_FootStepCue.Play(gameObject.transform, 1.0f, false, 6);       
    }
}
