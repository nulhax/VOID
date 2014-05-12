//Controls sound effects for objects like tools, props, etc.

using UnityEngine;
using System.Collections;

public class CImpactSFX : MonoBehaviour {

	void OnCollisionEnter(Collision _cCollision)
	{
		CAudioCue[] audioCues = GetComponents<CAudioCue>();
		foreach(CAudioCue cue in audioCues)
		{
			if(cue.m_strCueName == "ImpactSFX")
			{
				cue.Play(1,false,-1);
			}
		}
	}
}
