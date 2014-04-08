//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLASSNAME.cs
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


public class CMiningTurretBehaviour : CNetworkMonoBehaviour
{

// Member Types


	public enum ENetworkAction
	{
		StartFractureLaser,
		StopFractureLaser,
		StartExtractorBeam,
		StopExtractorBeam,
	}


// Member Delegates & Events


// Member Properties


	public bool IsLaserVisible
	{
		get { return (m_bFractureLaserVisible.Get()); }
	}


    public bool IsExtractorBeamVisible
    {
        get { return (m_bExtractorBeamVisible.Get()); }
    }


	public bool IsTrackerBeamVisible
	{
		get { return (m_bExtractorBeamVisible.Get()); }
	}


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_cTargetAsteroidViewId = _cRegistrar.CreateReliableNetworkVar<CNetworkViewId>(OnNetworkVarSync, null);
		m_bFractureLaserVisible = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, false);
		m_bExtractorBeamVisible = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, false);
	}


	[ALocalOnly]
	public static void SerializeOutbound(CNetworkStream _cStream)
	{
		_cStream.Write(s_cSerializeStream);
		s_cSerializeStream.Clear();
	}


	[AServerOnly]
	public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		while (_cStream.HasUnreadData)
		{
			CNetworkViewId cTurretViewId = _cStream.Read<CNetworkViewId>();
			GameObject cTurretObject = CNetwork.Factory.FindObject(cTurretViewId);

			if (cTurretObject != null)
			{
				CMiningTurretBehaviour cMiningTurretBehaviour = cTurretObject.GetComponent<CMiningTurretBehaviour>();		
				ENetworkAction eAction = (ENetworkAction)_cStream.Read<byte>();

				switch (eAction)
				{
				case ENetworkAction.StartFractureLaser:
                    cMiningTurretBehaviour.m_bFractureLaserButtonDown = true;
					break;

				case ENetworkAction.StopFractureLaser:
                    cMiningTurretBehaviour.m_bFractureLaserButtonDown = false;
					break;

				case ENetworkAction.StartExtractorBeam:
                    cMiningTurretBehaviour.m_bExtracterBeamButtonDown = true;
					break;

				case ENetworkAction.StopExtractorBeam:
                    cMiningTurretBehaviour.m_bExtracterBeamButtonDown = false;
					break;

				default:
					Debug.LogError(string.Format("Unknown network action ({0})", eAction));
					break;
				}
			}
		}
	}


	void Start()
	{
		GetComponent<CTurretBehaviour>().EventControllerChange += OnTurretControllerChange;

		if(m_MiningLaserPrefab == null)
			Debug.LogError("MiningLaser prefab has not been assigned!");

		m_cFractureLaserObject = GameObject.Instantiate(m_MiningLaserPrefab) as GameObject;
		m_cExtractorBeamObject = GameObject.Instantiate(m_MiningLaserPrefab) as GameObject;
        m_cFractureLaserObject.GetComponentInChildren<Renderer>().material.color = Color.red;
        m_cExtractorBeamObject.GetComponentInChildren<Renderer>().material.color = Color.blue;
		m_cFractureLaserObject.GetComponentInChildren<Light>().color = new Color(1.0f, 0.5f, 0.5f, 1.0f);
		m_cExtractorBeamObject.GetComponentInChildren<Light>().color = new Color(0.5f, 0.5f, 1.0f, 1.0f);
		m_cFractureLaserObject.SetActive(false);
		m_cExtractorBeamObject.SetActive(false);

		m_cBarrelObject = GetComponent<CTurretBehaviour>().m_cBarrel;
	}
	
	
	void OnDestroy()
	{
		GetComponent<CTurretBehaviour>().EventControllerChange -= OnTurretControllerChange;
	}


	void Update()
	{
		if (CNetwork.IsServer)
		{
            if (transform.FindChild("CalibratorComponent").GetComponent<CActorHealth>().health > 0)
            {
                UpdateFractureLaser();
            }
		}

		UpdateFractureLaserProjectile();
        UpdateExtractorBeamProjectile();
	}


	void UpdateFractureLaser()
	{
        CNetworkViewId cTargetAsteroidChunkViewId = null;
        bool bLaserVisible = false;
        bool bExtractorBeamVisible = false;

		if (GetComponent<CTurretBehaviour>().ControllerPlayerId != 0)
		{
            Vector3 vGalaxyBarrelPosition = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyPos(m_cBarrelObject.transform.position);
            Vector3 vGalaxyBarrelRotation = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyRot(m_cBarrelObject.transform.rotation) * Vector3.forward;

			RaycastHit cRaycast;
            Ray cRay = new Ray(vGalaxyBarrelPosition, vGalaxyBarrelRotation);
			
			// Raycast against object
			if (Physics.Raycast(cRay, out cRaycast, 300.0f, 1 << LayerMask.NameToLayer("Galaxy")))
			{
				GameObject cHitObject = cRaycast.collider.gameObject;
                CAsteroidChunkBehaviour cAsteroidChunkBehaviour = cHitObject.GetComponent<CAsteroidChunkBehaviour>();
				
				// Check hit object is asteroid
				if (cAsteroidChunkBehaviour != null)
				{
                    // Save asteroid target
                    cTargetAsteroidChunkViewId = cHitObject.GetComponent<CNetworkView>().ViewId;

                    if (m_bFractureLaserButtonDown)
                    {
                        // Decrement asteroid health
                        cAsteroidChunkBehaviour.DecrementHealth(m_fLaserDamageSec * Time.deltaTime);

                        // Check asteroid is still alive
                        if (cAsteroidChunkBehaviour.IsAlive)
                        {
                            // Show laser
                            bLaserVisible = true;
                        }
                        else
                        {
                            cTargetAsteroidChunkViewId = null;
                        }
                    }
				}
                else
                {
                    CMineralsBehaviour cMinerals = cHitObject.GetComponent<CMineralsBehaviour>();

                    if (cMinerals != null &&
                        m_bExtracterBeamButtonDown)
                    {
                        cMinerals.DecrementQuanity(100.0f * Time.deltaTime);

                        if (!cMinerals.IsDepleted)
                        {
                            bExtractorBeamVisible = true;
                        }
                    }
                }
			}
		}

        m_cTargetAsteroidViewId.Set(cTargetAsteroidChunkViewId);
        m_bFractureLaserVisible.Set(bLaserVisible);
        m_bExtractorBeamVisible.Set(bExtractorBeamVisible);
	}


    [ALocalOnly]
	void UpdateFractureLaserProjectile()
	{
        if (IsLaserVisible)
        {
            Vector3 vGalaxyBarrelPosition = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyPos(m_cBarrelObject.transform.position);
            Vector3 vGalaxyBarrelRotation = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyRot(m_cBarrelObject.transform.rotation) * Vector3.forward;

            RaycastHit cRaycastHit;
            Ray cRay = new Ray(vGalaxyBarrelPosition, vGalaxyBarrelRotation);

            // Raycast against object
            if (m_cTargetAsteroidViewId.Get().GameObject.collider.Raycast(cRay, out cRaycastHit, 300.0f))
            {
                Vector3 cDirection = cRaycastHit.point - vGalaxyBarrelPosition;
                m_cFractureLaserObject.transform.rotation = Quaternion.LookRotation(cDirection.normalized);
                m_cFractureLaserObject.transform.position = vGalaxyBarrelPosition + cDirection.normalized * cDirection.magnitude / 2;
                m_cFractureLaserObject.transform.localScale = new Vector3(1, 1, cDirection.magnitude / 2);
            }
        }
	}


    [ALocalOnly]
    void UpdateExtractorBeamProjectile()
    {
        if (IsExtractorBeamVisible)
        {
            Vector3 vGalaxyBarrelPosition = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyPos(m_cBarrelObject.transform.position);
            Vector3 vGalaxyBarrelRotation = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyRot(m_cBarrelObject.transform.rotation) * Vector3.forward;

            RaycastHit cRaycastHit;
            Ray cRay = new Ray(vGalaxyBarrelPosition, vGalaxyBarrelRotation);

            // Raycast against object
            if (Physics.Raycast(cRay, out cRaycastHit, 300.0f, 1 << LayerMask.NameToLayer("Galaxy")))
            {
                Vector3 cDirection = cRaycastHit.point - vGalaxyBarrelPosition;
                m_cExtractorBeamObject.transform.rotation = Quaternion.LookRotation(cDirection.normalized);
                m_cExtractorBeamObject.transform.position = vGalaxyBarrelPosition + cDirection.normalized * cDirection.magnitude / 2;
                m_cExtractorBeamObject.transform.localScale = new Vector3(1, 1, cDirection.magnitude / 2);
            }
        }
    }


	[ALocalOnly]
	void OnTurretControllerChange(ulong _ulPreviousPlayerId, ulong _ulNewPlayerId)
	{
		if (_ulNewPlayerId == CNetwork.PlayerId)
		{
			// Subscribe to input events
            CUserInput.SubscribeInputChange(CUserInput.EInput.Primary, OnLaserCommand);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Secondary, OnExtracterBeamCommand);
		}

		if (_ulPreviousPlayerId == CNetwork.PlayerId)
		{
			// Unsubscriber to input events
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Primary, OnLaserCommand);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Secondary, OnExtracterBeamCommand);
		}
	}


	[ALocalOnly]
    void OnLaserCommand(CUserInput.EInput _eInput, bool _bDown)
	{
        s_cSerializeStream.Write(NetworkView.ViewId);

        if (_bDown)
        {
            s_cSerializeStream.Write((byte)ENetworkAction.StartFractureLaser);
        }
        else
        {
            s_cSerializeStream.Write((byte)ENetworkAction.StopFractureLaser);
        }
	}


	[ALocalOnly]
    void OnExtracterBeamCommand(CUserInput.EInput _eInput, bool _bDown)
	{
        s_cSerializeStream.Write(NetworkView.ViewId);

        if (_bDown)
        {
            s_cSerializeStream.Write((byte)ENetworkAction.StartExtractorBeam);
        }
        else
        {
            s_cSerializeStream.Write((byte)ENetworkAction.StopExtractorBeam);
        }
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_cTargetAsteroidViewId)
		{
            // Un-highlight previous
			if (m_cTargetAsteroidViewId.GetPrevious() != null &&
                m_cTargetAsteroidViewId.GetPrevious().GameObject != null)
			{
				m_cTargetAsteroidViewId.GetPrevious().GameObject.GetComponent<CAsteroidChunkBehaviour>().SetHighlighted(false);
			}

            // Highlight new target
			if (m_cTargetAsteroidViewId.Get() != null)
			{
				m_cTargetAsteroidViewId.Get().GameObject.GetComponent<CAsteroidChunkBehaviour>().SetHighlighted(true);
			}
		}
		else if (_cSyncedVar == m_bFractureLaserVisible)
		{
            if (m_bFractureLaserVisible.Get())
            {
                // Show laser
                m_cFractureLaserObject.SetActive(true);
            }
            else
            {
                // Hide laser
				m_cFractureLaserObject.SetActive(false);
            }
		}
		else if (_cSyncedVar == m_bExtractorBeamVisible)
		{
            if (m_bExtractorBeamVisible.Get())
            {
                // Show laser
				m_cExtractorBeamObject.SetActive(true);
            }
            else
            {
                // Hide laser
				m_cExtractorBeamObject.SetActive(false);
            }
		}
	}


// Member Fields


	public GameObject m_MiningLaserPrefab = null;


	CNetworkVar<CNetworkViewId> m_cTargetAsteroidViewId = null;
	CNetworkVar<bool> m_bFractureLaserVisible = null;
	CNetworkVar<bool> m_bExtractorBeamVisible = null;

	GameObject m_cBarrelObject		= null;
	GameObject m_cFractureLaserObject = null;
	GameObject m_cExtractorBeamObject = null;

	float m_fLaserDamageSec = 50.0f;

	bool m_bFractureLaserButtonDown = false;
	bool m_bExtracterBeamButtonDown = false;

	static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
