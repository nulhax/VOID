//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CGalaxyShipShield.cs
//  Description :   --------------------------
//
//  Author      :  
//  Mail        :  
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CShipShieldSystem : CNetworkMonoBehaviour 
{

// Member Types


	public enum EShieldState
	{
		INVALID,
		
		MAX
	}


// Member Delegates & Events


// Member Properties


    public float GenerationRateAvailableRatio
    {
        get 
        { 
            if (GenerationRateMax <= 0.0f) return (0.0f); 
            
            return (GenerationRateCurrent / GenerationRateMax); 
        }
    }


    public float GenerationRateCurrent
    {
        get { return (m_fGenerationRateCurrent.Value); }
    }


    public float GenerationRateMax
    {
        get { return (m_fGenerationRateMax.Value); }
    }


    public float CapacityAvailableRatio
    {
        get 
        { 
            if (CapacityMax <= 0.0f) return (0.0f); 
            
            return (CapacityCurrent / CapacityMax); 
        }
    }


    public float CapacityCurrent
    {
        get { return (m_fCapacityCurrent.Value); }
    }


    public float CapacityMax
    {
        get { return (m_fCapacityMax.Value); }
    }


    public float ChargedRatio
    {
        get
        {
            if (CapacityCurrent <= 0.0f) return (0.0f);

            return (ChargeCurrent / CapacityCurrent);
        }
    }


    public float ChargeCurrent
    {
        get { return (m_fCurrentCharge.Value); }
    }


    public bool IsGenerating
    {
        get { return (GenerationRateCurrent > 0.0f); }
    }


    public bool IsFullyCharged
    {
        get { return (ChargeCurrent == CapacityCurrent); }
    }


    public bool HasCapacity
    {
        get { return (CapacityCurrent > 0.0f); }
    }
	

// Member Methods


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        m_fGenerationRateMax = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fGenerationRateCurrent = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fCapacityMax = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fCapacityCurrent = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_fCurrentCharge = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);

        _cRegistrar.RegisterRpc(this, "RemoteShowShieldHit");
        _cRegistrar.RegisterRpc(this, "RemoteShowHitNoShield");
    }


    public void ChangeGenerationRateMax(float _fValue)
    {
        m_fGenerationRateMax.Value += _fValue;

        Debug.Log(string.Format("Ship shield generation max change({0}) max generation({1})", _fValue, m_fGenerationRateMax.Value));
    }


    public void ChangeGenerationRateCurrent(float _fValue)
    {
        m_fGenerationRateCurrent.Value += _fValue;

        Debug.Log(string.Format("Ship shield generation total change({0}) total generation({1})", _fValue, m_fGenerationRateCurrent.Value));
    }


    public void ChangeCapacityMax(float _fValue)
    {
        m_fCapacityMax.Value += _fValue;

        Debug.Log(string.Format("Ship shield capacity max change({0}) max capacity({1})", _fValue, m_fCapacityMax.Value));
    }


    public void ChangeCapacityCurrent(float _fValue)
    {
        m_fCapacityCurrent.Value += _fValue;

        Debug.Log(string.Format("Ship shield capacity total change({0}) total capacity({1})", _fValue, m_fCapacityCurrent.Value));
    }


    [AServerOnly]
    public bool ProjectileHit(float _fDamage, Vector3 _vPosition, Vector3 _vEurler)
    {
        bool bReduced = false;

        if (ChargeCurrent > _fDamage)
        {
            m_fCurrentCharge.Value -= _fDamage;

            InvokeRpcAll("RemoteShowShieldHit", _vPosition, _vEurler);

            bReduced = true;
        }

        return (bReduced);
    }


    [AServerOnly]
    public void ProjectileHitNoShield(Vector3 _vPosition, Vector3 _vEurler)
    {
        InvokeRpcAll("RemoteShowHitNoShield", _vPosition, _vEurler);
    }


	void Start()
	{

	}


    void Update()
    {
        if (CNetwork.IsServer)
        {
            UpdateCharge();
        }
    }


    void UpdateCharge()
    {
        // Check already fully charged
        if (IsFullyCharged)
            return;

        // Check no generation
        if (!IsGenerating)
            return;

        // Check no capacity to store charge
        if (!HasCapacity)
            return;

        // Calculate new charge
        float fNewCharge = ChargeCurrent + GenerationRateCurrent * Time.deltaTime;

        // Set new capacity if not bigger to available capacity
        if (fNewCharge < CapacityCurrent)
        {
            m_fCurrentCharge.Value = fNewCharge;
        }

        // Set to full available capacity
        else
        {
            m_fCurrentCharge.Value = CapacityCurrent;
        }
    }


	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
        // Empty
	}


    [ANetworkRpc]
    void RemoteShowShieldHit(Vector3 _vPosition, Vector3 _vEulerRotation)
    {
        GameObject cShipHitEffect = Resources.Load("Prefabs/Ship/Shield Hit", typeof(GameObject)) as GameObject;
        cShipHitEffect = GameObject.Instantiate(cShipHitEffect) as GameObject;

        cShipHitEffect.transform.position = CGameShips.ShipGalaxySimulator.GetGalaxyToSimulationPos(_vPosition);
        cShipHitEffect.transform.rotation = CGameShips.ShipGalaxySimulator.GetGalaxyToSimulationRot(Quaternion.Euler(_vEulerRotation));
    }


    [ANetworkRpc]
    void RemoteShowHitNoShield(Vector3 _vPosition, Vector3 _vEulerRotation)
    {
        GameObject cShipHitEffect = Resources.Load("Prefabs/Galaxy/Enemy Hit Particles", typeof(GameObject)) as GameObject;
        cShipHitEffect = GameObject.Instantiate(cShipHitEffect) as GameObject;

        cShipHitEffect.transform.position = CGameShips.ShipGalaxySimulator.GetGalaxyToSimulationPos(_vPosition);
        cShipHitEffect.transform.rotation = CGameShips.ShipGalaxySimulator.GetGalaxyToSimulationRot(Quaternion.Euler(_vEulerRotation));
    }
	
	
// Member Fields


    CNetworkVar<float> m_fGenerationRateMax = null;
    CNetworkVar<float> m_fGenerationRateCurrent = null;
    CNetworkVar<float> m_fCapacityMax = null;
    CNetworkVar<float> m_fCapacityCurrent = null;
    CNetworkVar<float> m_fCurrentCharge = null;


}
