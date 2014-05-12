using UnityEngine;
using System.Collections;

public class CAudioAmbience : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
    {
        GetComponent<CAudioCue>().Play(1.0f, true, -1);
	}	
}
