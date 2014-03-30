//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CMineralDeposits.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CMineralDeposits : CNetworkMonoBehaviour
{
	
// Member Types
	
	
// Member Delegates & Events
	
	
// Member Properties

	
// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		// Empty
	}


	void Start()
    {
		if (CNetwork.IsServer)
		{
			GenerateMinerals();

			gameObject.GetComponent<CNetworkView>().EventPreDestory += () =>
			{
				foreach (GameObject cMineralObject in m_aDeposits)
				{
					CNetwork.Factory.DestoryObject(cMineralObject);
				}
			};
		}
	}


	void OnDestroy()
	{
		// Empty
	}


	void GenerateMinerals()
	{
		float minimumResourceRequiredToSpawnMineral = 20.0f;
		float fRayLength = 500.0f;
		float totalResourceAmount = CGalaxy.instance.ResourceAmount(CGalaxy.instance.RelativePointToAbsoluteCell(transform.position));

		while(totalResourceAmount >= minimumResourceRequiredToSpawnMineral)
		{
			float mineralResourceAmount = Random.Range(0.0f, totalResourceAmount * 1.5f);	// Get random amount of resource for the mineral, where ⅓ of the time it may consume all of the resource.
			if (mineralResourceAmount >= totalResourceAmount)	// Prevent the mineral from holding more than the maximum.
				mineralResourceAmount = totalResourceAmount;
			totalResourceAmount -= mineralResourceAmount;	// Subtract from the total.

			// Minerals below a minimum resource amount do not spawn.
			if (mineralResourceAmount < minimumResourceRequiredToSpawnMineral)
				continue;

			// Mineral has enough resource to spawn.
			// Spawn the mineral:
			Vector3 vRayDirection = Random.onUnitSphere;
			RaycastHit cRaycastHit;
			Ray cRay = new Ray(transform.position + vRayDirection * fRayLength, -vRayDirection);

			if (gameObject.collider.Raycast(cRay, out cRaycastHit, fRayLength))
			{
				// Todo: Create local mineral and test for intersection. If none, create networked mineral. Delete the local copy either way.

				GameObject cCrystal = CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.Crystal);
				cCrystal.GetComponent<CNetworkView>().SetParent(gameObject.GetComponent<CNetworkView>().ViewId);
				cCrystal.GetComponent<CNetworkView>().SetPosition(cRaycastHit.point);
				cCrystal.GetComponent<CNetworkView>().SetScale(Vector3.one * Mathf.Pow(mineralResourceAmount, 1.0f / 3.0f));
				cCrystal.GetComponent<CNetworkView>().SetRotation(Quaternion.LookRotation(vRayDirection) * Quaternion.AngleAxis(90.0f, Vector3.right));

				cCrystal.GetComponent<CMineralsBehaviour>().Quantity = mineralResourceAmount;

				m_aDeposits.Add(cCrystal);
			}
		}

        /*
        RaycastHit[] cRaycastHits = Physics.RaycastAll(transform.position, Random.insideUnitSphere, 300);

        foreach (RaycastHit cRaycastHit in cRaycastHits)
        {
            int iRandom = Random.Range(0, cRaycastHits.Length);
            //Debug.Log(string.Format("Start({0}) End({1}) Result({2})", 0, cRaycastHits.Length, iRandom));
            GameObject cCrystal = CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.Crystal);
            cCrystal.GetComponent<CNetworkView>().SetPosition(cRaycastHits[iRandom].point);
            cCrystal.GetComponent<CNetworkView>().SetParent(gameObject.GetComponent<CNetworkView>().ViewId);


            break;
        }
        */
	}


// Member Fields


	List<GameObject> m_aDeposits = new List<GameObject>();


};
