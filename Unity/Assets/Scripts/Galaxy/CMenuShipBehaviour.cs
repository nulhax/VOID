using UnityEngine;
using System.Collections;

public class CMenuShipBehaviour : MonoBehaviour {


	public float SpawnFreq = 15.0f;
	public float fSpawnTimer = 0.0f;



	// Use this for initialization
	void Start () 
	{
		// Load ships
		m_ShipArray[0] = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/EnemyShips/EnemyShip2"));
		m_ShipArray[1] = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/EnemyShips/EnemyShip3"));
		m_ShipArray[2] = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/EnemyShips/EnemyShip8"));


		// Set ship render to false and remove  unwanted scripts
		foreach( GameObject Ship in  m_ShipArray)
		{
			Destroy(Ship.GetComponent<GalaxyShiftable>());
			Destroy(Ship.GetComponent<CEnemyShip>());
			Destroy(Ship.GetComponent<NetworkedEntity>());
			Destroy(Ship.GetComponent<CActorHealth>());
			Destroy(Ship.GetComponent<CNetworkView>());

			MonoBehaviour[] behaviours = Ship.GetComponentsInChildren<MonoBehaviour>();
			
			foreach(MonoBehaviour behaviour in behaviours)
			{
				Destroy(behaviour);
			}

			Ship.collider.isTrigger = true;

			Ship.AddComponent<CLerpShip>();
		}

		// Lerp variables
		isLerpActive = false;
		m_fLerpTimer = 0.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		fSpawnTimer += Time.deltaTime;

		if(fSpawnTimer > SpawnFreq)
		{
			m_iCurrentShipIndex = Random.Range(0, 3);

			TriggerDummyMovement();
			fSpawnTimer = 0.0f;
		}
	}

	void TriggerDummyMovement()
	{
		m_ShipArray[m_iCurrentShipIndex].GetComponent<CLerpShip>().BeginShipMove();
		Debug.Log("Dummy " + m_iCurrentShipIndex.ToString() + " now active");
	}

	private GameObject[] m_ShipArray = new GameObject[3];
	private int m_iCurrentShipIndex = 0;
}
