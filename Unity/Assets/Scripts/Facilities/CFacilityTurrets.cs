//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFacilityInfo.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


/* Implementation */


public class CFacilityTurrets : CNetworkMonoBehaviour
{
	
	// Member Types
	
	
	// Member Delegates & Events


	// Member Fields
	private Dictionary<uint, GameObject> m_TurretNodes = new Dictionary<uint, GameObject>();


	// Member Properties

	
	// Member Methods
	
	
	public override void InstanceNetworkVars()
	{

	}
	
	public void OnNetworkVarSync(INetworkVar _cVarInstance)
	{
		
	}

	public void Awake()
	{
		SearchTurretNodes();
	}

	[AServerOnly]
	public void CreateTurret(uint _TurretNodeId, CTurretInterface.ETurretType _TurretType)
	{
		// Get the turret node
		GameObject turretNode = m_TurretNodes[_TurretNodeId];

		// Retrieve the turret prefab
		CGameRegistrator.ENetworkPrefab eRegisteredPrefab = CTurretInterface.GetTurretPrefab(_TurretType);
		
		// Create the turret
		GameObject newTurretObject = CNetwork.Factory.CreateObject(eRegisteredPrefab);

		// Set turrets parent
		newTurretObject.GetComponent<CNetworkView>().SetParent(GetComponent<CNetworkView>().ViewId);

		// Set position & rotation
		newTurretObject.GetComponent<CNetworkView>().SetPosition(turretNode.transform.position);
		newTurretObject.GetComponent<CNetworkView>().SetRotation(turretNode.transform.eulerAngles);

		// Set turret properties
		CTurretInterface turretInterface = newTurretObject.GetComponent<CTurretInterface>();
		turretInterface.TurretType = _TurretType;
		turretInterface.TurretId = _TurretNodeId;
	}

	public GameObject GetTurretNode(uint _iTurretNode)
	{
		if(!m_TurretNodes.ContainsKey(_iTurretNode))
			Debug.LogError("GetTurretNode the turret node does not exist! " + _iTurretNode.ToString());

		return(m_TurretNodes[_iTurretNode]);
	}

	public List<GameObject> GetAllUnmountedTurrets()
	{
		var unmountedTurrets =
			from tn in m_TurretNodes.Values
			where tn.GetComponent<CTurretNodeInterface>().IsTurretInstalled && !tn.GetComponent<CTurretNodeInterface>().AttachedTurret.GetComponent<CTurretBehaviour>().IsMounted
			select tn.GetComponent<CTurretNodeInterface>().AttachedTurret;

		return(new List<GameObject>(unmountedTurrets));
	}

	public List<GameObject> GetAllFreeTurretNodes()
	{
		var freeTurretNodes =
			from tn in m_TurretNodes.Values
				where !tn.GetComponent<CTurretNodeInterface>().IsTurretInstalled
				select tn;
		
		return(new List<GameObject>(freeTurretNodes));
	}

	private void SearchTurretNodes()
	{
		uint counter = 0;
		foreach(CTurretNodeInterface node in gameObject.GetComponentsInChildren<CTurretNodeInterface>())
		{
			m_TurretNodes.Add(counter, node.gameObject);
			node.TurretNodeId = counter++;
		}
	}
};
