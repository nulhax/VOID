using UnityEngine;
using System.Collections;

public class CLerpShip : MonoBehaviour {

	public bool IsActive
	{
		get { return(m_bIsActive); }
		set { m_bIsActive = value ;}
	}
	
	public float fLerpDuration = 20.0f;
	public float m_fLerpTimer = 0.0f;
	Vector3 shipPos = new Vector3(0.0f, 0.0f, 0.0f);
	Vector3 SpawnPos = new Vector3(0.0f, 0.0f, -500.0f);
	Vector3 EndPos = new Vector3(0.0f, 0.0f, 10000.0f);

	// Use this for initialization
	void Start () 
	{
		ResetShip();
	}
	
	// Update is called once per frame
	void Update () 
	{
		//Lerp
		if(IsActive)
		{		
			m_fLerpTimer += Time.deltaTime;
			
			float fLerpFactor = m_fLerpTimer / fLerpDuration;
			transform.position = Vector3.Lerp(SpawnPos, EndPos, fLerpFactor);
			
			if(m_fLerpTimer > fLerpDuration)
			{
				m_fLerpTimer = 0.0f;

				IsActive = false;
				
				ResetShip();
			}
		}
	}

	public void BeginShipMove()
	{
		IsActive = true;
		transform.position = SpawnPos;

		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		foreach(Renderer renderer in renderers)
		{
			renderer.enabled = true;
		}
		
	}
	
	public void ResetShip()
	{
		transform.position = SpawnPos;
		IsActive = false;

		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		foreach(Renderer renderer in renderers)
		{
			renderer.enabled = false;
		}
	}
	private bool m_bIsActive = false;
}
