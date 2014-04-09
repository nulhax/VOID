using UnityEngine;
using System.Collections;

public class CGalaxyStorm : MonoBehaviour 
{

	public delegate void NotifyStormDamage();
	public event NotifyStormDamage EventDamageComponent;

	public delegate void NotifyStormStart();
	public event NotifyStormStart EventStartStorm;
	

	// Use this for initialization
	void Start () 
	{
		m_fdensity = 0.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(CGalaxy.instance != null && CGameCameras.MainCamera != null)
			m_fdensity = CGalaxy.instance.DebrisDensity(CGalaxy.instance.RelativePointToAbsoluteCell(CGameCameras.MainCamera.transform.position));

		if(m_fdensity > 0.8f)
		{
			if(EventDamageComponent != null)
				EventDamageComponent();

			if(EventStartStorm != null)
				EventStartStorm();
		}
	}

	private float m_fdensity;
}
