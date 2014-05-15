using UnityEngine;
using System.Collections;

public class CPlayerSoundEffects : MonoBehaviour 
{

    CAudioCue m_FootStepCue = null;
	public Transform m_soundLocation = null;

	// Use this for initialization
	void Start () 
	{
		CAudioCue[] audioCues = GetComponents<CAudioCue>();

        foreach(CAudioCue cue in audioCues)
        {
            if(cue.m_strCueName == "PlayerSFX")
            {
                m_FootStepCue = cue;
            }
        }
	}

	void Update()
	{
		GameObject currentFacility = gameObject.GetComponent<CActorLocator>().CurrentFacility;
		float density = 0;

		if (currentFacility != null)
		{
			density = currentFacility.GetComponent<CFacilityAtmosphere>().Density;			
		}

		Mathf.Clamp(density, 0.1f, 1);
		CAudioSystem.Instance.SoundMediumDensity = density;
	}
		
	void PlayFootStep()
	{
			m_FootStepCue.Play(m_soundLocation, 1.0f, false, -1, 0, 3);
	}

    void PlayLandingAudio()
    {
		m_FootStepCue.Play(m_soundLocation, 1.0f, false, 5);       
    }

    void PlaySlideAudio()
    {
		m_FootStepCue.Play(m_soundLocation, 1.0f, false, 6);       
    }
}
