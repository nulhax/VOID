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

	CNetworkVar<int> m_fShipNanitePool = null;

	// Member Properties
	public int ShipNanites
	{
		get{return(m_fShipNanitePool.Get());}
		set{m_fShipNanitePool.Set(value);}
	}
	
	// Member Methods

	// Use this for initialization
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
        m_fShipNanitePool = _cRegistrar.CreateNetworkVar<int>(OnNetworkVarSync, 0);
	}
	
	public void OnNetworkVarSync(INetworkVar _VarInstance)
	{
		
	}
	
	public void Update()
	{
		if(CNetwork.IsServer)
		{

		}
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

	public bool IsEnoughNanites(int _iNanites)
	{
		return(ShipNanites >= _iNanites);
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

			if(newNaniteAmount > siloBehaviour.MaxNaniteCapacity)
			{
				int chargeAddition = siloBehaviour.AvailableNaniteCapacity;
				siloBehaviour.StoredNanites = siloBehaviour.MaxNaniteCapacity;
				evenDistribution -= chargeAddition;
			}
		}

		int totalNanites = 0;
		foreach(GameObject silo in m_NaniteSilos)
		{
			CNaniteStorageBehaviour siloBehaviour = silo.GetComponent<CNaniteStorageBehaviour>();
			
			if(siloBehaviour.StoredNanites != siloBehaviour.MaxNaniteCapacity)
			{
				siloBehaviour.StoredNanites += evenDistribution;
			}
			
			 totalNanites += siloBehaviour.StoredNanites;
		}

		ShipNanites = totalNanites;
	}
		
	[AServerOnly]
	private void DeductNanites(int _iNanites)
	{	
		int iOriginalDebt = _iNanites;

		if (_iNanites < ShipNanites) 
		{
			// Take nanites from non-full silos first
			foreach (GameObject silo in m_NaniteSilos) 
			{
				CNaniteStorageBehaviour siloBehaviour = silo.GetComponent<CNaniteStorageBehaviour> ();

				if (siloBehaviour.HasAvailableNanites && siloBehaviour.StoredNanites < siloBehaviour.MaxNaniteCapacity) 
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
			
					if (siloBehaviour.HasAvailableNanites) 
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

			ShipNanites -= iOriginalDebt;
		}
		else
		{
			Debug.LogError("Insufficient nanites - always check available nanites before deducting");
		}

		//ShipNanites -= _iNanites;
	}

	public void OnGUI()
	{
		float boxWidth = 100;
		float boxHeight = 100;

		string nanites = ShipNanites.ToString();
		GUI.Label(new Rect(Screen.width / 2 + boxWidth * 5, Screen.height / 2 + boxHeight, boxWidth, boxHeight), "Ship Nanites: " +  nanites);
	}
}
