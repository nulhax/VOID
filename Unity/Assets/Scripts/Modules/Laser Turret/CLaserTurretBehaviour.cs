//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLaserTurretBehaviour.cs
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


[RequireComponent(typeof(CTurretBehaviour))]
public class CLaserTurretBehaviour : CNetworkMonoBehaviour
{

// Member Types


	public enum ENetworkAction
	{
		FireLasers,
	}


// Member Delegates & Events


// Member Properties


// Member Methods


	public override void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar)
	{
		_cRegistrar.RegisterRpc(this, "StartMuzzleFlash");
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
			TNetworkViewId cTurretViewId = _cStream.Read<TNetworkViewId>();
			GameObject cTurretObject = CNetwork.Factory.FindGameObject(cTurretViewId);
			
			if (cTurretObject != null)
			{
				CLaserTurretBehaviour cLaserTurretBehaviour = cTurretObject.GetComponent<CLaserTurretBehaviour>();
				ENetworkAction eAction = (ENetworkAction)_cStream.Read<byte>();
				
				switch (eAction)
				{
				case ENetworkAction.FireLasers:
					cLaserTurretBehaviour.FireLasers();
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
		//GetComponent<CTurretBehaviour>().EventControllerChange += OnTurretControllerChange;
	}


	void OnDestroy()
	{
		//GetComponent<CTurretBehaviour>().EventControllerChange -= OnTurretControllerChange;
	}


	void Update()
	{
		if (CNetwork.IsServer)
		{
			m_fServerFireTimer += Time.deltaTime;
		}

		//if (GetComponent<CTurretBehaviour>().ControllerPlayerId == CNetwork.PlayerId)
		{
			//UpdateFiring();
		}
	}


	[ALocalOnly]
	void UpdateFiring()
	{
		// Fire lasers
		m_fClientFireTimer += Time.deltaTime;
		
		if (m_bFireLasers)
		{
			if (m_fClientFireTimer > m_fClientFireInterval)
			{
				// Write fire lasers action
				s_cSerializeStream.Write(NetworkView.ViewId);
				s_cSerializeStream.Write((byte)ENetworkAction.FireLasers);
				
				m_fClientFireTimer = 0.0f;
			}
		}
	}


	[AServerOnly]
	void FireLasers()
	{
        if(transform.FindChild("CalibratorComponent").GetComponent<CActorHealth>().health > 0)
        {
		    if (m_fServerFireTimer > m_fServerFireInterval)
		    {
			    Vector3 projPos = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyPos(m_aLaserNodes[m_iLaserNodeIndex].transform.position);
			    Quaternion projRot = CGameShips.ShipGalaxySimulator.GetSimulationToGalaxyRot(m_aLaserNodes[m_iLaserNodeIndex].transform.rotation);
			
			    GameObject cProjectile = CNetwork.Factory.CreateGameObject(CGameRegistrator.ENetworkPrefab.LaserProjectile);
			    cProjectile.GetComponent<CNetworkView>().SetPosition(projPos);
			    cProjectile.GetComponent<CNetworkView>().SetEuler(projRot.eulerAngles);
			
				InvokeRpcAll("StartMuzzleFlash", m_iLaserNodeIndex);

			    ++m_iLaserNodeIndex;
			    m_iLaserNodeIndex = (m_iLaserNodeIndex >= m_aLaserNodes.Length) ? 0 : m_iLaserNodeIndex;
			
			    m_fServerFireTimer = 0.0f;
		    }
        }
	}


	[ALocalOnly]
	void OnTurretControllerChange(ulong _ulPreviousPlayerId, ulong _ulNewPlayerId)
	{
		if (_ulNewPlayerId == CNetwork.PlayerId)
		{
			// Subscribe to input events
            CUserInput.SubscribeInputChange(CUserInput.EInput.Primary, OnFireLasersCommand);
		}

		if (_ulPreviousPlayerId == CNetwork.PlayerId)
		{
			// Unsubscriber to input events
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Primary, OnFireLasersCommand);
		}
	}


	[ALocalOnly]
    void OnFireLasersCommand(CUserInput.EInput _eInput, bool _bDown)
	{
		m_bFireLasers = _bDown;
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		// Empty
	}

	[ANetworkRpc]
	private void StartMuzzleFlash(int _iLaserIndex)
	{
		// Start the corouteen for muzzle flash
		this.StartCoroutine("MuzzleFlash", _iLaserIndex);
	}

	private IEnumerator MuzzleFlash(int _iLaserIndex)
	{
		float timer = 0.0f;
		float flashTime = m_fClientFireInterval;
		float origIntensity = m_aLaserNodes[_iLaserIndex].light.intensity;

		bool light = true;
		m_aLaserNodes[_iLaserIndex].light.enabled = true;
		while(light)
		{
			timer += Time.deltaTime;
			if(timer > flashTime)
			{
				timer = flashTime;
				light = false;
			}

			m_aLaserNodes[_iLaserIndex].light.intensity = origIntensity * (1.0f - (timer/flashTime));
			yield return null;
		}
		m_aLaserNodes[_iLaserIndex].light.intensity = origIntensity;
		m_aLaserNodes[_iLaserIndex].light.enabled = false;
	}


// Member Fields

	
	public GameObject[] m_aLaserNodes = null;


	float m_fClientFireTimer	= 0.0f;
	float m_fClientFireInterval = 0.1f;
	float m_fServerFireTimer	= 0.0f;
	float m_fServerFireInterval = 0.1f;
	
	
	int m_iLaserNodeIndex = 0;


	bool m_bFireLasers   = false;


	static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
