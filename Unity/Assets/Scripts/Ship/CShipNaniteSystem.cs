//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShipNaniteSystem.cs
//  Description :   Keeps track of ship-wide nanites and is responsible for adding and subtracting nanites
//
//  Author  	:  Daniel Langsford
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/* Implementation */

public class CShipNaniteSystem : CNetworkMonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	private List<GameObject> m_NaniteGenerators = new List<GameObject>();
	private List<GameObject> m_NaniteSilos = new List<GameObject>();	

	private int m_ShipNanitesPotential = 0;
	private int m_ShipCurrentNanites = 0;

	// Member Properties
	public float ShipNanitesPotential
	{
		get { return (m_ShipNanitesPotential); } 
	}
	
	public float ShipCurentNanites
	{
		get { return (m_ShipCurrentNanites); } 
	}

	// Member Methods

	// Use this for initialization
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
       
	}
	
	public void OnNetworkVarSync(INetworkVar _VarInstance)
	{
		
	}
	
	public void RegisterNaniteGenerator(GameObject _NaniteGenerator)
	{
		if(!m_NaniteGenerators.Contains(_NaniteGenerator))
		{
			m_NaniteGenerators.Add(_NaniteGenerator);
		}
	}
	
	public void UnregisterNaniteGenerator(GameObject _NaniteGenerator)
	{
		if(m_NaniteGenerators.Contains(_NaniteGenerator))
		{
			m_NaniteGenerators.Remove(_NaniteGenerator);
		}
	}
	
	public void RegisterNaniteSilo(GameObject _NaniteSilo)
	{
		if(!m_NaniteSilos.Contains(_NaniteSilo))
		{
			m_NaniteSilos.Add(_NaniteSilo);
		}
	}
	
	public void UnregisterNaniteSilo(GameObject _NaniteSilo)
	{
		if(m_NaniteSilos.Contains(_NaniteSilo))
		{
			m_NaniteSilos.Remove(_NaniteSilo);
		}
	}

	public void Update()
	{
		// Update the storage variables
		UpdateStorageVariables();
	}

	public bool IsEnoughNanites(int _iNanites)
	{
		return(m_ShipCurrentNanites >= _iNanites);
	}

	private void UpdateStorageVariables()
	{
		m_ShipNanitesPotential = 0;
		m_ShipCurrentNanites = 0;
		foreach(GameObject ns in m_NaniteSilos)
		{
			CNaniteStorageBehaviour nsb = ns.GetComponent<CNaniteStorageBehaviour>();
			
			if(nsb.IsStorageAvailable)
			{
				m_ShipCurrentNanites += nsb.StoredNanites;
				m_ShipNanitesPotential += nsb.NaniteCapacity;
			}
		}
	}
	
	[AServerOnly]
	public void AddNanites(int _iNanites)
	{
		int iNumSilos = m_NaniteSilos.Count;

		int evenDistribution = _iNanites / iNumSilos;

		foreach (GameObject silo in m_NaniteSilos) 
		{
			CNaniteStorageBehaviour siloBehaviour = silo.GetComponent<CNaniteStorageBehaviour>();

			float newNaniteAmount = siloBehaviour.StoredNanites + evenDistribution;

			if(newNaniteAmount > siloBehaviour.NaniteCapacity)
			{
				int chargeAddition = siloBehaviour.AvailableNaniteCapacity;
				siloBehaviour.StoredNanites = siloBehaviour.NaniteCapacity;
				evenDistribution -= chargeAddition;
			}
		}

		foreach(GameObject silo in m_NaniteSilos)
		{
			CNaniteStorageBehaviour siloBehaviour = silo.GetComponent<CNaniteStorageBehaviour>();
			
			if(siloBehaviour.StoredNanites != siloBehaviour.NaniteCapacity)
			{
				siloBehaviour.StoredNanites += evenDistribution;
			}
		}
	}
		
	[AServerOnly]
	public void DeductNanites(int _iNanites)
	{	
		int iOriginalDebt = _iNanites;

		if (_iNanites < m_ShipCurrentNanites) 
		{
			// Take nanites from non-full silos first
			foreach (GameObject silo in m_NaniteSilos) 
			{
				CNaniteStorageBehaviour siloBehaviour = silo.GetComponent<CNaniteStorageBehaviour> ();

				if (siloBehaviour.IsStorageAvailable && siloBehaviour.StoredNanites < siloBehaviour.NaniteCapacity) 
				{
					if (siloBehaviour.StoredNanites > _iNanites) 
					{
						siloBehaviour.DeductNanites (_iNanites);
						_iNanites = 0;
						break;
					}
					else 
					{
						int availableAmount = siloBehaviour.StoredNanites;
						siloBehaviour.DeductNanites (availableAmount);
						_iNanites -= availableAmount;
					}
				}
			}

			//If there is a remaining debt, start removing nanites from full silos
			if (_iNanites > 0) 
			{						
				foreach (GameObject silo in m_NaniteSilos) 
				{
					CNaniteStorageBehaviour siloBehaviour = silo.GetComponent<CNaniteStorageBehaviour> ();
			
					if (siloBehaviour.IsStorageAvailable) 
					{
						if (siloBehaviour.StoredNanites > _iNanites) 
						{
							siloBehaviour.DeductNanites (_iNanites);
							_iNanites = 0;
							break;
						}
						else 
						{
							int availableAmount = siloBehaviour.StoredNanites;
							siloBehaviour.DeductNanites (availableAmount);
							_iNanites -= availableAmount;
						}
					}
				}
			}
		}
		else
		{
			Debug.LogError("Insufficient nanites - always check available nanites before deducting");
		}
	}

	public void OnGUI()
	{
        if (CNetwork.IsConnectedToServer)
        {
            float boxWidth = 150;
            float boxHeight = 40;

            GUI.Box(new Rect(Screen.width - boxWidth - 10, Screen.height - boxHeight - 110, boxWidth, boxHeight),
                             "[Ship Nanites]\n" + m_ShipCurrentNanites.ToString());
        }
	}
}
