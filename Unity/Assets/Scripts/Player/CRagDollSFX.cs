using UnityEngine;
using System.Collections;

public class CRagDollSFX : MonoBehaviour 
{

	CAudioCue audioCue = null;

	// Use this for initialization
	void Start () 
	{
		audioCue = gameObject.AddComponent<CAudioCue>();
		audioCue.AddSound("Audio/Ragdoll Impact/Ragdoll Impact 1", 0, 0, false);
		audioCue.AddSound("Audio/Ragdoll Impact/Ragdoll Impact 2", 0, 0, false);
		audioCue.AddSound("Audio/Ragdoll Impact/Ragdoll Impact 3", 0, 0, false);
	}
	
	void OnCollisionEnter(Collision _cCollision)
	{
		if(_cCollision.relativeVelocity.magnitude > 3)
		{
			audioCue.Play(transform, 1.0f, false, -1);
		}
	}
}
