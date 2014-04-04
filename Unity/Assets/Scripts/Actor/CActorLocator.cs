
//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CActorLocator.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[RequireComponent(typeof(Rigidbody))]
public class CActorLocator : MonoBehaviour 
{
	// Member Types
	


	// Member Delegates and Events


	public delegate void NotifyFacilityChange(GameObject _Facility);
	
	public event NotifyFacilityChange EventEnteredFacility;
	public event NotifyFacilityChange EventExitedFacility;


    public delegate void EnterShipHandler(GameObject _cActor);
    public event EnterShipHandler EventEnterShip;


    public delegate void LeaveShipHandler(GameObject _cActor);
    public event LeaveShipHandler EventLeaveShip;


	// Member Fields


	private List<GameObject> m_ContainingFacilities = new List<GameObject>();
	private GameObject m_CurrentFacility = null;


	// Member Properties	


	public GameObject CurrentFacility
	{
		get { return(m_CurrentFacility); }
	}


	public List<GameObject> ContainingFacilities
	{
		get { return(m_ContainingFacilities); }
	}


    public bool IsInShip
    {
        get { return (m_ContainingFacilities.Count > 0); }
    }


	// Member Methods



	public void ActorEnteredFacility(GameObject _Facility)
	{
        // Add containing facility to list
		m_ContainingFacilities.Add(_Facility);
		
        // Remember current facility
		m_CurrentFacility = _Facility;

        // Notify observers about entering a facility
		if(EventEnteredFacility != null) EventEnteredFacility(_Facility);

        // Check actor just entered a facility when not previously
        // being contained by facility
        if (m_ContainingFacilities.Count == 1)
        {
            // Notify observers about entering the ship
            if (EventEnterShip != null) EventEnterShip(gameObject);
        }
	}
	


	public void ActorExitedFacility(GameObject _Facility)
	{
        // Remove containing facility from list
		m_ContainingFacilities.Remove(_Facility);

        if (m_ContainingFacilities.Count == 0)
        {
            m_CurrentFacility = null;
        }

        // Notify observers about exiting a facility
		if(EventExitedFacility != null) EventExitedFacility(_Facility);

        // Check actor is not contained by any facility
        if (m_ContainingFacilities.Count == 0)
        {
            // Notify observers about leaving the ship
            if (EventLeaveShip != null) EventLeaveShip(gameObject);
        }
	}


    void OnGUI()
    {
        if (gameObject == CGamePlayers.SelfActor)
        {
            string sFacilityText = "";
            GameObject cCurrentFacilityObject = GetComponent<CActorLocator>().CurrentFacility;

            if (cCurrentFacilityObject != null)
            {
                sFacilityText += "Atmosphere: " + Math.Round(cCurrentFacilityObject.GetComponent<CFacilityAtmosphere>().Quantity, 0);
                sFacilityText += "/" + Math.Round(cCurrentFacilityObject.GetComponent<CFacilityAtmosphere>().Volume, 0) + "\n";
                sFacilityText += "Atmosphere: " + Math.Round(cCurrentFacilityObject.GetComponent<CFacilityAtmosphere>().QuantityPercent, 1) + "%\n";

                sFacilityText += "Refilling: " + (cCurrentFacilityObject.GetComponent<CFacilityAtmosphere>().IsRefillingEnabled ? "True" : "False") + "\n";
                sFacilityText += "Controlled Decompressing: " + (cCurrentFacilityObject.GetComponent<CFacilityAtmosphere>().IsDepressurizing ? "True" : "False") + "\n";
                sFacilityText += "Explosive Decompressing: " + (cCurrentFacilityObject.GetComponent<CFacilityAtmosphere>().IsExplosiveDepressurizing ? "True" : "False") + "\n";

                sFacilityText += "Power Active: " + (cCurrentFacilityObject.GetComponent<CFacilityPower>().IsPowerActive ? "True" : "False") + "\n";
                sFacilityText += "In Ship: " + (IsInShip ? "True" : "False");
            }
            else
            {
                sFacilityText += "In Ship: " + (IsInShip ? "True" : "False");
            }


            GUI.Box(new Rect(10, Screen.height - 230, 240, 130),
                    "[Facility]\n" +
                    sFacilityText);
        }
    }

}
