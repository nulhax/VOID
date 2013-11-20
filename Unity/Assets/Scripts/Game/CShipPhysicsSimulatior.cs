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
		
		// Set the world ship as the parent
		worldActor.transform.parent = m_WorldShip.transform;
		
		// Set the layer as "World" recursively
		CUtility.SetLayerRecursively(worldActor, LayerMask.NameToLayer("World"));
	}
	
	public GameObject GetWorldActor(GameObject _SimulationActor)
	{
		GameObject worldActor = null;
		
		if(!m_SimWorldActorPairs.ContainsKey(_SimulationActor))
			Debug.LogError("CShipPhysicsSimulatior GetWorldActor: simulation actor not found: " + _SimulationActor.name);
		else
			worldActor = m_SimWorldActorPairs[_SimulationActor];
		
		return(worldActor);
	}
	
	public GameObject GetWorldChildActor(GameObject _SimulationRootParent, GameObject _SimulationChildActor)
	{
		GameObject worldActor = GetWorldActor(_SimulationRootParent);
		GameObject childWorldActor = RecursiveFindWorldChild(worldActor, _SimulationChildActor.name);;

		if(childWorldActor == null)
			Debug.LogError("CShipPhysicsSimulatior GetWorldChildActor: simulation child actor not found (" + _SimulationChildActor.name + ") within (" + _SimulationRootParent.name + ")");

		return(childWorldActor);
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
	
	private GameObject RecursiveFindWorldChild(GameObject _Actor, string _childName)
	{
		Transform childTransform = _Actor.transform.FindChild(_childName);
		GameObject worldChild = null;
		
		if(childTransform != null)
		{
			worldChild = childTransform.gameObject;
		}
		else
		{
			for(int i = 0; i < _Actor.transform.childCount; ++i)
			{
				worldChild = RecursiveFindWorldChild(_Actor.transform.GetChild(i).gameObject, _childName);
				if(worldChild != null)
					break;
			}
		}
		
		return(worldChild);
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
			Vector3 simRelPos = WorldShip.transform.position +  simulationActor.transform.position - transform.position;
			Quaternion simRelRot = WorldShip.transform.rotation * Quaternion.Inverse(transform.rotation) * simulationActor.transform.rotation;
			
			// Update the transform
			UpdateWorldActorTransform(worldActor, simRelPos, simRelRot, simulationActor.transform.localScale);
		}
		
		// Delete any of the keys added
		foreach(GameObject key in keysToDelete)
		{
			m_SimWorldActorPairs.Remove(key);
		}
	}
	
	private void UpdateWorldActorTransform(GameObject _WorldActor, Vector3 _Postion, Quaternion _Rotation, Vector3 _LocalScale)
	{
		if(_WorldActor.transform.position != _Postion)
			_WorldActor.transform.position = _Postion;
		
		if(_WorldActor.transform.rotation != _Rotation)
			_WorldActor.transform.rotation = _Rotation;
		
		if(_WorldActor.transform.localScale != _LocalScale)
			_WorldActor.transform.localScale = _LocalScale;
	}
}
