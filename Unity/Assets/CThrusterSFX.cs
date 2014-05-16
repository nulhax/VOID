using UnityEngine;
using System.Collections;

public class CThrusterSFX : MonoBehaviour 
{
	
	public CAudioCue thrusterActive;
	public CAudioCue thrusterIdle;
	
	// Use this for initialization
	void Start () 
	{
		CGameShips.GalaxyShip.GetComponent<CGalaxyShipMotor>().EventShipThrustChanged += OnThrustStateChange;
		AudioSource source = thrusterIdle.Play(transform, 1.0f, true, -1);
		source.maxDistance = 5000;
	}
	
	void OnThrustStateChange(bool _bThrustEngaged)
	{
		if (thrusterActive == null || thrusterIdle == null) 
		{
			return;
		}
		if (_bThrustEngaged)
		{
			AudioSource source = thrusterActive.Play(transform, 1.0f, true, -1);
			source.maxDistance = 5000;
			thrusterIdle.StopAllSound();
		} 
		else
		{
			AudioSource source = thrusterIdle.Play(transform, 1.0f, true, -1);
			source.maxDistance = 5000;
			thrusterActive.StopAllSound();
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
}
