
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

public class CActorLocator : CNetworkMonoBehaviour 
{

// Member Types
	


// Member Delegates & Events


	public delegate void FacilityChangeHandler(GameObject _cPreviousFacility, GameObject _cNewFacility);
	public event FacilityChangeHandler EventFacilityChangeHandler;


    public delegate void EnterShipHandler(GameObject _cActor);
    public event EnterShipHandler EventEnterShip;


    public delegate void LeaveShipHandler(GameObject _cActor);
    public event LeaveShipHandler EventLeaveShip;


// Member Properties	


    [AServerOnly]
    public List<GameObject> ContainingFacilities
    {
        get { return (m_aContainingFacilities); }
    }


	public GameObject CurrentFacility
	{
		get 
        {
            if (m_tCurrentFacilityViewId == null)
                Debug.LogError(gameObject.name + " is missing a network view? The network variables were not instanced");

            if (m_tCurrentFacilityViewId.Value == null)
            {
                return (null);
            }

            return (m_tCurrentFacilityViewId.Value.GameObject); 
        }
	}


    public bool IsInShip
    {
        get { return (m_tCurrentFacilityViewId.Value != null); }
    }


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
        m_tCurrentFacilityViewId = _cRegistrar.CreateReliableNetworkVar<TNetworkViewId>(OnNetworkVarSync, null);
	}


    [AServerOnly]
	public void NotifyEnteredFacility(GameObject _cFacility)
	{
        // Add containing facility to list
		m_aContainingFacilities.Add(_cFacility);

        // Remember current facility
        m_tCurrentFacilityViewId.Value = _cFacility.GetComponent<CNetworkView>().ViewId;
	}


    [AServerOnly]
	public void NotifyExitedFacility(GameObject _cFacility)
	{
        // Remove containing facility from list
		m_aContainingFacilities.Remove(_cFacility);

        // Check still in the ship
        if (m_aContainingFacilities.Count == 0)
        {
            m_tCurrentFacilityViewId.Value = null;

			// Disembark the player from the ship
			gameObject.GetComponent<CActorBoardable>().DisembarkActor();
        }
	}


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_tCurrentFacilityViewId)
        {
            // Check not in a facility
            if (m_tCurrentFacilityViewId.Value == null)
            {
                // Notify observers that actor left ship
                if (EventLeaveShip != null) EventLeaveShip(gameObject);

                // Check we were in a facility
                if (m_tCurrentFacilityViewId.PreviousValue != null)
                {
                    // Notify observers about leaving the facility
                    if (EventFacilityChangeHandler != null) EventFacilityChangeHandler(m_tCurrentFacilityViewId.PreviousValue.GameObject, null);
                }
            }
            else
            {
                // Check actor came from space
                if (m_tCurrentFacilityViewId.PreviousValue == null)
                {
                    // Notify observers about entering the ship
                    if (EventEnterShip != null) EventEnterShip(gameObject);

                    // Notify observers about entering a facility
                    if (EventFacilityChangeHandler != null) EventFacilityChangeHandler(null, m_tCurrentFacilityViewId.Value.GameObject);
                }
                else
                {
                    // Notify observers about entering a facility
                    if (EventFacilityChangeHandler != null) EventFacilityChangeHandler(m_tCurrentFacilityViewId.PreviousValue.GameObject, m_tCurrentFacilityViewId.Value.GameObject);
                }
            }
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
            }

            sFacilityText += "Gravity: " + (GetComponent<CActorGravity>().IsUnderGravityInfluence ? "True" : "False") + "\n";
            sFacilityText += "In Ship: " + (IsInShip ? "True" : "False");


            GUI.Box(new Rect(10, Screen.height - 240, 240, 140),
                    "[Facility]\n" +
                    sFacilityText);
        }
    }


// Member Fields


    List<GameObject> m_aContainingFacilities = new List<GameObject>();


    CNetworkVar<TNetworkViewId> m_tCurrentFacilityViewId = null;


}
