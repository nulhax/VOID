//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDUIShipPlayersRoot.cs
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


[RequireComponent(typeof(CNetworkView))]
public class CDUIShipPlayersRoot : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public UIGrid m_PlayersGridList = null;
	public GameObject m_ListItemTemplate = null;

	private Dictionary<ulong, GameObject> m_PlayersList = new Dictionary<ulong, GameObject>();


	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		CGamePlayers.Instance.EventPlayerJoin += HandlePlayerJoin;
		CGamePlayers.Instance.EventPlayerLeave += HandlePlayerLeave;

		// Create the initial list from the list of players
		foreach(ulong player in CGamePlayers.Players)
		{
			AddPlayer(player);
		}
	}

	public void Update()
	{
		UpdateListInformation();
	}

	private void OnDestroy()
	{
		CGamePlayers.Instance.EventPlayerJoin -= HandlePlayerJoin;
		CGamePlayers.Instance.EventPlayerLeave -= HandlePlayerLeave;
	}

	private void HandlePlayerJoin(ulong _PlayerId)
	{
		AddPlayer(_PlayerId);
	}

	private void HandlePlayerLeave(ulong _PlayerId)
	{
		RemovePlayer(_PlayerId);
	}

	private void AddPlayer(ulong _PlayerId)
	{
		// Add this player if they dont exist yet
		GameObject listItem = null;
		if(!m_PlayersList.ContainsKey(_PlayerId))
		{
			listItem = (GameObject)GameObject.Instantiate(m_ListItemTemplate);
			listItem.SetActive(true);
			listItem.name += m_PlayersList.Count;
			listItem.transform.parent = m_PlayersGridList.transform;
			listItem.transform.localPosition = Vector3.one;
			listItem.transform.localEulerAngles = Vector3.zero;
			listItem.transform.localScale = Vector3.one;

			m_PlayersList.Add(_PlayerId, listItem);
		}
		else
		{
			listItem = m_PlayersList[_PlayerId];
		}

		// Update the player name
		string playerName = CGamePlayers.GetPlayerName(_PlayerId);
		UILabel nameLabel = listItem.GetComponentInChildren<UILabel>();
		nameLabel.text = playerName;

		// Reorganize the list
		m_PlayersGridList.Reposition();
	}

	private void RemovePlayer(ulong _PlayerId)
	{
		// Add this player if they dont exist yet
		if(_PlayerId != 0 && m_PlayersList.ContainsKey(_PlayerId))
		{
			Destroy(m_PlayersList[_PlayerId]);
			m_PlayersList.Remove(_PlayerId);
		}

		// Reorganize the list
		m_PlayersGridList.Reposition();
	}

	private void UpdateListInformation()
	{
		foreach(KeyValuePair<ulong, GameObject> listItem in m_PlayersList)
		{
			// Set the alive status
			CPlayerHealth playerActorHealth = CGamePlayers.GetPlayerActor(listItem.Key).GetComponent<CPlayerHealth>();
			if(playerActorHealth.CurrentHealthState == CPlayerHealth.HealthState.ALIVE)
			{
				listItem.Value.transform.FindChild("StatusAlive").gameObject.SetActive(true);
				listItem.Value.transform.FindChild("StatusDead").gameObject.SetActive(false);
			}
			else
			{
				listItem.Value.transform.FindChild("StatusAlive").gameObject.SetActive(false);
				listItem.Value.transform.FindChild("StatusDead").gameObject.SetActive(true);
			}
		}
	}
}