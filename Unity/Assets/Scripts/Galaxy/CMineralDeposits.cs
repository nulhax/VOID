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
			GenerateMinerals();
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
		float fRayLength = 500.0f;
		Vector3 vRayDirection = Random.onUnitSphere;

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


        /*

		Ray cRay = new Ray(transform.position + vRayDirection * fRayLength, -vRayDirection);
		
		if (Physics.Raycast(cRay, out cRaycastHit, fRayLength))
		{
			GameObject cCrystal = CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.Crystal);
			cCrystal.GetComponent<CNetworkView>().SetPosition(cRaycastHit.point);
			cCrystal.GetComponent<CNetworkView>().SetParent(gameObject.GetComponent<CNetworkView>().ViewId);

			m_aDeposits.Add(cCrystal);
		}




		
		 RaycastHit{
			var length : float = 100.0;
			var direction : Vector3 = ;

			var hit : RaycastHit;
			collider.Raycast (ray, hit, length*2);
			return hit;
		}
*/
	}


// Member Fields


	List<GameObject> m_aDeposits = new List<GameObject>();


};
