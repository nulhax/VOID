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
	
	private Dictionary<GameObject, GameObject> m_SimWorldActorPairs = new Dictionary<GameObject, GameObject>();
	private Dictionary<GameObject, Dictionary<GameObject, GameObject>> m_WorldChildrenSimWorldPairs = new Dictionary<GameObject, Dictionary<GameObject, GameObject>>();
	
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
		// Check if the object wasn't already created
		if(m_SimWorldActorPairs.ContainsKey(_SimulationActor))
			return;
		
		// Disable the simulation actor
		_SimulationActor.SetActive(false);
		
		// Clone the simulation actors
		GameObject worldActor = (GameObject)GameObject.Instantiate(_SimulationActor);
		
		// Add to the actor list of actor pairs
		m_SimWorldActorPairs.Add(_SimulationActor, worldActor);
		
		// Strip the unnecessary components
		StripUnnecessaryComponents(worldActor);
		
		// Add all of the children to the list of simulation children pairs
		RecursiveAddChildren(_SimulationActor, worldActor);
		
		// Reactivate the actors
		worldActor.SetActive(true);
		_SimulationActor.SetActive(true);
		
		// Set the layer as "World" recursively
		CUtility.SetLayerRecursively(worldActor, LayerMask.NameToLayer("World"));
	}
	
	public GameObject GetWorldActor(GameObject _SimulationActor)
	{
		GameObject worldActor = null;
		
		if(_SimulationActor.transform == transform)
			return(m_WorldShip);
		
		if(!m_SimWorldActorPairs.ContainsKey(_SimulationActor))
			Debug.LogError("CShipPhysicsSimulatior GetWorldActor: simulation actor not found: " + _SimulationActor.name);
		else
			worldActor = m_SimWorldActorPairs[_SimulationActor];
		
		return(worldActor);
	}
	
	public void DestroyWorldActor(GameObject _SimulationActor)
	{
		GameObject worldActor = GetWorldActor(_SimulationActor);
		
		m_SimWorldActorPairs.Remove(_SimulationActor);
		
		Destroy(worldActor);
	}
	
	public void LateUpdate()
	{
		UpdateWorldActorTransforms();
		
		if(CNetwork.IsServer)
			SyncWorldShipTransform();
	}
	
	private void StripUnnecessaryComponents(GameObject _Actor)
	{
		foreach(Component component in _Actor.GetComponents<Component>())
		{
			if(	component.GetType() != typeof(Transform) && 
				component.GetType() != typeof(MeshRenderer) &&
				component.GetType() != typeof(MeshFilter) &&
				component.GetType() != typeof(Animator))
			{
				DestroyImmediate(component);
			}
		}
	}
	
	private void RecursiveAddChildren(GameObject _SimulationActorParent, GameObject _WorldActorParent)
	{
		for(int i = 0; i < _SimulationActorParent.transform.childCount; ++i)
		{
			GameObject simChild = _SimulationActorParent.transform.GetChild(i).gameObject;
			GameObject worldChild = _WorldActorParent.transform.GetChild(i).gameObject;
			
			m_SimWorldActorPairs.Add(simChild, worldChild);
			
			StripUnnecessaryComponents(worldChild);
			RecursiveAddChildren(simChild, worldChild);
		}
	}
	
	private void RecursiveStrip(GameObject _Actor)
	{
		StripUnnecessaryComponents(_Actor);
		
		for(int i = 0; i < _Actor.transform.childCount; ++i)
		{
			StripUnnecessaryComponents(_Actor.transform.GetChild(i).gameObject);
		}
	}
	
	private void SyncWorldShipTransform()
	{
		Position = m_WorldShip.rigidbody.position;
		EulerAngles = m_WorldShip.transform.eulerAngles;
	}
	
	private void UpdateWorldActorTransforms()
	{	
		List<GameObject> keysToDelete = new List<GameObject>();
		
		foreach(var pair in m_SimWorldActorPairs)
		{	
			GameObject simulationActor = pair.Key;
			GameObject worldActor = pair.Value;
			
			// Check that the simulation actor still exists
			if(simulationActor == null)
			{
				keysToDelete.Add(simulationActor);
				Destroy(worldActor);
				continue;
			}
			
			// Get the simulation actors position relative to the ship
			Vector3 simRelPos = simulationActor.transform.localPosition;
			Quaternion simRelRot = simulationActor.transform.localRotation;
			GameObject worldParent = null;
			
			// If the sim parent is nothing, make it the ship and 
			if(simulationActor.transform.parent == null)
			{
				worldParent = WorldShip;
				simRelPos -= transform.position;
				simRelRot *= Quaternion.Inverse(transform.rotation);
			}
			else
			{
				worldParent = GetWorldActor(simulationActor.transform.parent.gameObject);
			}
			
			// Update the transform
			SyncWorldActorTransform(worldActor, worldParent, simRelPos, simRelRot, simulationActor.transform.localScale);
		}
		
		// Delete any of the keys added
		foreach(GameObject key in keysToDelete)
		{
			m_SimWorldActorPairs.Remove(key);
		}
	}
	
	private void SyncWorldActorTransform(GameObject _WorldActor, GameObject _WorldActorParent, Vector3 _LocalPostion, Quaternion _LocalRotation, Vector3 _LocalScale)
	{
		if(_WorldActor.transform.parent != _WorldActorParent.transform)
			_WorldActor.transform.parent = _WorldActorParent.transform;
		
		if(_WorldActor.transform.localPosition != _LocalPostion)
			_WorldActor.transform.localPosition = _LocalPostion;
		
		if(_WorldActor.transform.localRotation != _LocalRotation)
			_WorldActor.transform.localRotation = _LocalRotation;
		
		if(_WorldActor.transform.localScale != _LocalScale)
			_WorldActor.transform.localScale = _LocalScale;
	}
}
