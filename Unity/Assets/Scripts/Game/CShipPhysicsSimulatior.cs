//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CShipSimulator.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CShipPhysicsSimulatior : CNetworkMonoBehaviour 
{
	// Member Types
	
	// Member Fields
	public float m_WorldShipMass = 1000.0f;
	public float m_WorldShipDrag = 0.1f;
	public float m_WorldShipAngularDrag = 1.0f;
	
	private GameObject m_WorldShip = null;
	
	private Dictionary<GameObject, GameObject> m_ActorPairs = new Dictionary<GameObject, GameObject>();
	private Dictionary<GameObject, Dictionary<GameObject, GameObject>> m_ActorChildrenPairs = new Dictionary<GameObject, Dictionary<GameObject, GameObject>>();
	
    protected CNetworkVar<float> m_cPositionX    = null;
    protected CNetworkVar<float> m_cPositionY    = null;
    protected CNetworkVar<float> m_cPositionZ    = null;
	
    protected CNetworkVar<float> m_EulerAngleX    = null;
    protected CNetworkVar<float> m_EulerAngleY    = null;
    protected CNetworkVar<float> m_EulerAngleZ    = null;
	
	// Member Properties
	public GameObject WorldShip
	{
		get { return(m_WorldShip); }
	}
	
	public GameObject PlayerWorldActor
	{
		get { return(GetWorldActor(CGame.PlayerActor)); }
	}

	public Vector3 Position
    {
        set 
		{ 
			m_cPositionX.Set(value.x); m_cPositionY.Set(value.y); m_cPositionZ.Set(value.z); 
		}
        get 
		{ 
			return (new Vector3(m_cPositionX.Get(), m_cPositionY.Get(), m_cPositionZ.Get())); 
		}
    }
	
	public Vector3 EulerAngles
    {
        set 
		{ 
			m_EulerAngleX.Set(value.x); m_EulerAngleY.Set(value.y); m_EulerAngleZ.Set(value.z);
		}
        get 
		{ 
			return (new Vector3(m_EulerAngleX.Get(), m_EulerAngleY.Get(), m_EulerAngleZ.Get())); 
		}
    }
	
	// Member Methods
    public override void InstanceNetworkVars()
    {
		m_cPositionX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cPositionY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cPositionZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		
        m_EulerAngleX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_EulerAngleY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_EulerAngleZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}
	
	public void OnNetworkVarSync(INetworkVar _rSender)
	{
		if(!CNetwork.IsServer)
		{
			// Position
	        if (_rSender == m_cPositionX || _rSender == m_cPositionY || _rSender == m_cPositionZ)
			{
				m_WorldShip.rigidbody.position = Position;
			}
			
			// Rotation
	        else if (_rSender == m_EulerAngleX || _rSender == m_EulerAngleY || _rSender == m_EulerAngleZ)
	        {	
	            m_WorldShip.transform.eulerAngles = EulerAngles;
	        }
		}
	}
	
	public void Awake()
	{
		// Create a world ship to explore the galaxy
		string prefabFile = CNetwork.Factory.GetRegisteredPrefabFile(CGame.ENetworkRegisteredPrefab.WorldShip);
		m_WorldShip = GameObject.Instantiate(Resources.Load("Prefabs/" + prefabFile, typeof(GameObject))) as GameObject;
		
		// Set the layer of the ship to "World"
		m_WorldShip.layer = LayerMask.NameToLayer("World");	
		
		// Placeholder: Move the ship away
		m_WorldShip.transform.position = Vector3.right * 20.0f;
	}
	
	public void AddWorldActor(GameObject _SimulationActor)
	{	
		// Create the local simulation player actor
		GameObject worldActor = new GameObject("World " + _SimulationActor.name);
		
		// Add the children of this actor to the world actor
		m_ActorChildrenPairs.Add(_SimulationActor, new Dictionary<GameObject, GameObject>());
		for(int i = 0; i < _SimulationActor.transform.childCount; ++i)
		{	
			GameObject simulationChild = _SimulationActor.transform.GetChild(i).gameObject;
			
			GameObject worldChild = (GameObject)GameObject.Instantiate(simulationChild);
			worldChild.name = simulationChild.name;
			worldChild.transform.parent = worldActor.transform;
			
			m_ActorChildrenPairs[_SimulationActor].Add(simulationChild, worldChild);
		}
		
		// Set the world ship as this actors parent
		worldActor.transform.parent = m_WorldShip.transform;
		
		// Set the layer as "Simulation" recursively
		CUtility.SetLayerRecursively(worldActor, LayerMask.NameToLayer("World"));
		
		// Add to the actor list of actor pairs
		m_ActorPairs.Add(_SimulationActor, worldActor);
	}
	
	public GameObject GetWorldActor(GameObject _SimulationActor)
	{
		GameObject worldActor = null;
		
		if(!m_ActorPairs.ContainsKey(_SimulationActor))
			Debug.LogError("CShipPhysicsSimulatior GetWorldActor: simulation actor not found: " + _SimulationActor.name);
		else
			worldActor = m_ActorPairs[_SimulationActor];
		
		return(worldActor);
	}
	
	public GameObject GetWorldChildActor(GameObject _SimulationParent, GameObject _SimulationChildActor)
	{
		GameObject worldActor = GetWorldActor(_SimulationParent);
		GameObject childWorldActor = null;

		Dictionary<GameObject, GameObject> childrenPairs = m_ActorChildrenPairs[_SimulationParent];
		
		if(!childrenPairs.ContainsKey(_SimulationChildActor))
			Debug.LogError("CShipPhysicsSimulatior GetWorldChildActor: simulation child actor not found: " + _SimulationChildActor.name);
		else
			childWorldActor = childrenPairs[_SimulationChildActor];
		
		return(childWorldActor);
	}
	
	public void DestroyWorldActor(GameObject _SimulationActor)
	{
		GameObject worldActor = GetWorldActor(_SimulationActor);
		
		Dictionary<GameObject, GameObject> childrenPairs = m_ActorChildrenPairs[_SimulationActor];
		foreach(GameObject childWorldActor in childrenPairs.Values)
		{
			Destroy(childWorldActor);
		}
		
		m_ActorPairs.Remove(_SimulationActor);
		m_ActorChildrenPairs.Remove(_SimulationActor);
		
		Destroy(worldActor);
	}
	
	public void LateUpdate()
	{
		UpdateWorldActorTransforms();
		
		if(CNetwork.IsServer)
			SyncWorldShipTransform();
	}
	
	protected void SyncWorldShipTransform()
	{
		Position = m_WorldShip.rigidbody.position;
		EulerAngles = m_WorldShip.transform.eulerAngles;
	}
	
	private void UpdateWorldActorTransforms()
	{
		foreach(GameObject simulationActor in m_ActorPairs.Keys)
		{
			// Get the simulation actors position relative to the ship
			Vector3 simRelPos = simulationActor.transform.position - transform.position;
			Quaternion simRelRot = Quaternion.Inverse(transform.rotation) * simulationActor.transform.rotation;
			
			// Get the world actor and set the new transforms based on the world ship
			GameObject worldActor = m_ActorPairs[simulationActor];	
			
			if(worldActor.transform.localPosition != simRelPos)
				worldActor.transform.localPosition = simRelPos;
			
			if(worldActor.transform.localRotation != simRelRot)
				worldActor.transform.localRotation = simRelRot;
			
			if(worldActor.transform.localScale != simulationActor.transform.localScale)
				worldActor.transform.localScale = simulationActor.transform.localScale;
			
			// Syncronize the children
			Dictionary<GameObject, GameObject> childrenPairs = m_ActorChildrenPairs[simulationActor];
			foreach(GameObject simulationChild in childrenPairs.Keys)
			{			
				GameObject worldChild = childrenPairs[simulationChild];
				
				if(worldChild.transform.localPosition != simulationChild.transform.localPosition)
					worldChild.transform.localPosition = simulationChild.transform.localPosition;
			
				if(worldChild.transform.localRotation != simulationChild.transform.localRotation)
					worldChild.transform.localRotation = simulationChild.transform.localRotation;
				
				if(worldChild.transform.localScale != simulationChild.transform.localScale)
					worldChild.transform.localScale = simulationChild.transform.localScale;
			}
		}
	}
}
