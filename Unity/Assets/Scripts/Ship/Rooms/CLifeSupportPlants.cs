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


	public struct TPlant
	{
		public GameObject cObject;
		public float fHealth;
		public float fLight;
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
				TPlant tPlant = new TPlant();
				tPlant.cObject = transform.GetChild(i).gameObject;

				m_aPlants.Add(tPlant);
			}
		}
	}


// Member Fields


	List<TPlant> m_aPlants;


};
