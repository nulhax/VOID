//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPilotHologramDisplay.cs
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


public class CLocalHolographicDisplay : MonoBehaviour
{
	
	// Member Types
	
	
	// Member Delegates & Events


	// Member Fields
	public Shader m_HolographicShader = Shader.Find("VOID/Holographic");
	public float m_Radius = 5.0f;
	public float m_ScalingFactor = 0.01f;

	private Material m_HolographicMaterial = null;
	private CGalaxyShipMotor m_CachedShipMotor = null;

	private GameObject m_ShipRepresentation = null;
	private GameObject m_GalaxyRepresentation = null;

	private Dictionary<CNetworkViewId, GameObject> m_ProximityGalaxyObjects = new Dictionary<CNetworkViewId, GameObject>();


	// Member Properties
	
	
	// Member Methods
	private void Start()
	{
		m_CachedShipMotor = CGameShips.GalaxyShip.GetComponent<CGalaxyShipMotor>();
		m_HolographicMaterial = new Material(m_HolographicShader);

		m_ShipRepresentation = new GameObject("Ship");
		m_ShipRepresentation.transform.parent = transform;
		m_ShipRepresentation.transform.localPosition = Vector3.zero;
		m_ShipRepresentation.transform.localRotation = Quaternion.identity;

		m_GalaxyRepresentation = new GameObject("Galaxy");
		m_GalaxyRepresentation.transform.parent = transform;
		m_GalaxyRepresentation.transform.localScale = Vector3.one * m_ScalingFactor;

		UpdateShipPresentation();

		CGameShips.Ship.GetComponent<CShipFacilities>().EventOnFaciltiyCreate += OnFacilitiesChanged;
		CGameShips.Ship.GetComponent<CShipFacilities>().EventOnFaciltiyDestroy += OnFacilitiesChanged;
	}

	private void Update()
	{
		UpdateShipPosition();
		UpdateProximityGalaxy();
	}

	private void UpdateShipPosition()
	{
//		// Get the current velocity of the ship
//		Vector3 shipVelocity = Quaternion.Inverse(m_CachedShipMotor.rigidbody.rotation) * m_CachedShipMotor.rigidbody.velocity;
//		float maxVelocity = m_CachedShipMotor.DirectionalMaxSpeed;
//		
//		// Forward/Backward amount
//		float fbAmount = -shipVelocity.z / maxVelocity;
//		
//		// Left/Right amount
//		float lrAmount = shipVelocity.x / maxVelocity;
//		
//		// Up/Down amount
//		float udAmount = -shipVelocity.y / maxVelocity;
//		
//		// Calculate the destination postion to be at
//		float speedRatio = shipVelocity.magnitude / maxVelocity;
//		Vector3 destN = new Vector3(lrAmount, udAmount, fbAmount).normalized;
//		Vector3 destPos = destN * (shipVelocity.magnitude / maxVelocity) * m_Radius;
//		
//		m_ShipRepresentation.transform.localPosition = destPos;
		
		// Get the current angular velocity of the ship
		Vector3 shipAVelocity = Quaternion.Inverse(m_CachedShipMotor.rigidbody.rotation) * m_CachedShipMotor.rigidbody.angularVelocity;
		float maxAVelocity = m_CachedShipMotor.AngularMaxSpeed;
		
		// Pitch amount
		float pitchAmount = shipAVelocity.x / maxAVelocity;
		
		// Yaw amount
		float yawAmount = shipAVelocity.y / maxAVelocity;
		
		// Roll amount
		float rollAmount = shipAVelocity.z / maxAVelocity;
		
		// Calculate the destination rotation
		Quaternion destRot = Quaternion.Euler(new Vector3(pitchAmount * 30.0f, yawAmount * 30.0f, rollAmount * 30.0f));
		
		m_ShipRepresentation.transform.localRotation = destRot;
		
		//		float aSpeedRatio = shipVelocity.magnitude / maxVelocity;
		//		Vector3 destN = new Vector3(lrAmount, udAmount, fbAmount).normalized;
		//		Vector3 destPos = destN * (shipVelocity.magnitude / maxVelocity) * m_YRadius;
		//		
		//		m_ShipRepresentation.transform.localPosition = destPos;
		
		// Calculate the destination postion to be at
		//		float maxXZ = Mathf.Abs(lrAmount) > Mathf.Abs(fbAmount) ? Mathf.Abs(lrAmount) : Mathf.Abs(fbAmount);
		//		Vector2 xz = new Vector2(lrAmount, fbAmount).normalized * maxXZ * m_XZRadius;
		//		Vector3 destPos = new Vector3(xz.x, udAmount * m_YRadius, xz.y);
		
		
		//		//Vector3 currentDir = localPos.normalized;
		//		//float maxDistance = Mathf.Sqrt(1.0f / (Mathf.Pow(currentDir.x/m_XZRadius, 2.0f) + Mathf.Pow(currentDir.y/m_YRadius, 2.0f) + Mathf.Pow(currentDir.z/m_XZRadius, 2.0f)));
		
		//		// Calculate the distance to the destination
		//		Vector3 acceleration = (destPos - localPos) * 100.0f;
		//
		//		// v = d/t, a = v/t - d = vt, v = at - d = at^2
		//		Vector3 movement = acceleration * Mathf.Pow(Time.fixedDeltaTime, 2);

	}

	private void UpdateProximityGalaxy()
	{
		// Position the galaxy represetnation to the ship
		m_GalaxyRepresentation.transform.position = m_ShipRepresentation.transform.position;
		m_GalaxyRepresentation.transform.rotation = m_ShipRepresentation.transform.rotation;

		// List proximity objects to remove, copy from the current key list to remove elements that fail the overlap test
		List<CNetworkViewId> toRemoveKeys = new List<CNetworkViewId>(m_ProximityGalaxyObjects.Keys);

		// Calculate the radius
		float radius = 1.0f / m_ScalingFactor * m_Radius;

		// Find the objects within a radius of the ship
		Collider[] colliders = Physics.OverlapSphere(m_CachedShipMotor.transform.position, radius);
		foreach(Collider collider in colliders)
		{
			GalaxyShiftable shiftable = collider.gameObject.GetComponent<GalaxyShiftable>();

			if(shiftable == null)
				shiftable = CUtility.FindInParents<GalaxyShiftable>(collider.gameObject);

			if(shiftable != null)
			{
				GameObject galaxyObject = shiftable.gameObject;

				// Skip own ship and objects who dont have network view id's
				CNetworkView nv = galaxyObject.GetComponent<CNetworkView>();
				if(galaxyObject == m_CachedShipMotor.gameObject || nv == null)
					continue;

				// Add new proximity object if it doesnt already exisit
				if(!m_ProximityGalaxyObjects.ContainsKey(nv.ViewId))
				{
					Mesh combineMesh = CUtility.CreateCombinedMesh(galaxyObject);

					GameObject proximityObject = new GameObject(galaxyObject.name + "_Projection");
					proximityObject.transform.parent = m_GalaxyRepresentation.transform;

					// Add the combined mesh and renderer
					proximityObject.AddComponent<MeshFilter>().mesh = combineMesh;
					MeshRenderer mr = proximityObject.AddComponent<MeshRenderer>();
					mr.castShadows = false;
					mr.receiveShadows = false;
					mr.material = m_HolographicMaterial;

					// Apply the scale
					proximityObject.transform.localScale = Vector3.one;

					// Add the new objects to the dictionary
					m_ProximityGalaxyObjects.Add(nv.ViewId, proximityObject);
				}
				else
				{
					// Remove the object from the remove list
					toRemoveKeys.Remove(nv.ViewId);
				}
			}
		}

		// Position the objects relative to the ship, check distance
		foreach(KeyValuePair<CNetworkViewId, GameObject> pair in m_ProximityGalaxyObjects)
		{
			// Check object hasnt been deleted
			if(pair.Key.GameObject == null)
			{
				// If the original object deleted, remove it and its proxmity object
				toRemoveKeys.Add(pair.Key);
				continue;
			}
		
			// Get the relative position to the galaxy ship
			Vector3 relativePos = Quaternion.Inverse(m_CachedShipMotor.transform.rotation) * (pair.Key.GameObject.transform.position - m_CachedShipMotor.transform.position);
			Quaternion relativeRot = Quaternion.Inverse(m_CachedShipMotor.transform.rotation) * pair.Key.GameObject.transform.rotation;
			
			// Apply the position and rotation
			pair.Value.transform.localPosition = relativePos;
			pair.Value.transform.localRotation = relativeRot;
		}

		// Remove listed objects objects
		foreach(CNetworkViewId key in toRemoveKeys)
		{
			Destroy(m_ProximityGalaxyObjects[key]);
			m_ProximityGalaxyObjects.Remove(key);
		}
	}

	private void OnFacilitiesChanged(GameObject _Facility)
	{
		UpdateShipPresentation();
	}

	private void UpdateShipPresentation()
	{
		GameObject tempShip = new GameObject(m_ShipRepresentation.name);
		tempShip.transform.position = Vector3.zero;
		tempShip.transform.rotation = Quaternion.identity;

		// Get all facilities and copy their combined mesh
		CShipFacilities sf = CGameShips.Ship.GetComponent<CShipFacilities>();
		foreach(GameObject facility in sf.Facilities)
		{
			CFacilityInterface fi = facility.GetComponent<CFacilityInterface>();
			GameObject child = new GameObject(facility.name);
			child.transform.position = facility.transform.position;
			child.transform.rotation = facility.transform.rotation;

			// Add the combined mesh and renderer
			child.AddComponent<MeshFilter>().mesh = fi.m_CombinedMesh;
			MeshRenderer mr = child.AddComponent<MeshRenderer>();
			mr.castShadows = false;
			mr.receiveShadows = false;
			mr.material = m_HolographicMaterial;

			child.transform.parent = tempShip.transform;
		}

		// Scale down the objects
		tempShip.transform.localScale = Vector3.one * m_ScalingFactor;

		// Reposition new representation
		tempShip.transform.position = m_ShipRepresentation.transform.position;
		tempShip.transform.rotation = m_ShipRepresentation.transform.rotation;
		tempShip.transform.parent = transform;

		// Destroy old representation
		DestroyImmediate(m_ShipRepresentation);
		m_ShipRepresentation = tempShip;
	}
};
