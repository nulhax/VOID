//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLifeSupportPlants.cs
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


public class CLifeSupportPlants : MonoBehaviour
{

// Member Types


	const string ksPlantObjectName = "Plant";
	const float ckfMaxO2Production = 100.0f;

	public struct TPlant
	{
		public GameObject cObject;
		public float fHealth;
		public float fAge;
	}


// Member Delegates & Events


// Member Properties


// Member Functions


	public void Start()
	{
		ScanPlantObjects();
	}


	public void OnDestroy()
	{
	}


	public void Update()
	{

	}


	void ScanPlantObjects()
	{
		for (int i = 0; i < transform.childCount; ++i)
		{
			if (transform.GetChild(i).name == CLifeSupportPlants.ksPlantObjectName)
			{
				GameObject cPlantObject = (GameObject)GameObject.Instantiate(Resources.Load(string.Format("Prefabs/Ship/Facilities/Life Support/LifeSupportPlant{0}", Random.Range(1,3)), typeof(GameObject)));
				cPlantObject.transform.position = transform.GetChild(i).gameObject.transform.position;


				TPlant tPlant = new TPlant();
				tPlant.cObject = cPlantObject;
				m_aPlants.Add(tPlant);
			}
		}


		foreach (TPlant tPlant in m_aPlants)
		{
			tPlant.cObject.transform.parent = transform;
		}
	}


// Member Fields


	List<TPlant> m_aPlants = new List<TPlant>();


};
