//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRoomAtmosphere.cs
//  Description :   Atmosphere information for rooms
//
//  Author  	: 
//  Mail    	: 
//

// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/* Implementation */


public class CFacilityAtmosphere : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


    public delegate void HandleDecompression(bool _bDecompressing, bool _bExplosive);
    public event HandleDecompression EventDecompression;


// Member Properties
	

    public float Quantity
    {
        get { return (m_fQuantity.Get()); }
    }


    public float QuantityPercent
    {
		get { return (Quantity / m_fVolume * 100.0f); }
    }


    public float QuantityRatio
    {
        get { return (Quantity / m_fVolume); }
    }


	public float Volume
	{
		get { return (m_fVolume); } 
	}


	public bool IsRefillingRequired
	{
        get
        {
            return (IsRefillingEnabled &&
                    QuantityRatio != 1.0f); 
        } 
	}


    public bool IsRefillingEnabled
    {
        get
        { 
            return (!IsDepressurizing &&
                    !IsExplosiveDepressurizing &&
                     m_bRefillingEnabled);
        }
    }


    [AServerOnly]
    public float ConsumptionRate
    {
        get { return (m_fConsumptionRate); }
    }


    [AServerOnly]
    public bool IsDepressurizing
    {
        get { return (m_bControlledDepressurizing); }
    }


    [AServerOnly]
    public bool IsExplosiveDepressurizing
    {
        get { return (m_bExplosiveDepressurizing); }
    }


// Member Methods
	

    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_fQuantity = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, Volume / 2);
    }


    [AServerOnly]
    public void SetQuanity(float _fAmount)
    {
        if (_fAmount < 0.0f)
        {
            _fAmount = 0.0f;
        }
        else if (_fAmount > m_fVolume)
        {
            _fAmount = m_fVolume;
        }

        m_fQuantity.Set(_fAmount);
    }


    [AServerOnly]
    public float ChangeQuantityByAmount(float _fAmount)
    {
        float fNewQuanity = m_fQuantity.Get() + _fAmount;
        float fUnusedQuanity = 0.0f;

        if (fNewQuanity < 0.0f)
        {
            fUnusedQuanity = -fNewQuanity;
            fNewQuanity = 0.0f;
        }
        else if (fNewQuanity > m_fVolume)
        {
            fUnusedQuanity = fNewQuanity - m_fVolume;
            fNewQuanity = m_fVolume;
        }

        m_fQuantity.Set(fNewQuanity);

        return (fUnusedQuanity);
    }


    [AServerOnly]
    public void SetRefillingEnabled(bool _bEnabled)
    {
        m_bRefillingEnabled = _bEnabled;
    }


    [AServerOnly]
    public void SetDepressurizingEnabled(bool _bEnabled)
    {
        m_bControlledDepressurizing = _bEnabled;

		if (EventDecompression != null) EventDecompression(m_bControlledDepressurizing, false);
    }


    [AServerOnly]
    public void SetExplosiveDepressurizingEnabled(bool _bEnabled)
    {
        m_bExplosiveDepressurizing = _bEnabled;

        if (EventDecompression != null) EventDecompression(m_bExplosiveDepressurizing, true);
    }


    [AServerOnly]
    public void RegisterAtmosphericConsumer(GameObject _cConsumer)
    {
        if (!m_aConsumers.Contains(_cConsumer))
        {
            m_aConsumers.Add(_cConsumer);
        }
    }


    [AServerOnly]
    public void UnregisterAtmosphericConsumer(GameObject _cConsumer)
    {
        if (m_aConsumers.Contains(_cConsumer))
        {
            m_aConsumers.Remove(_cConsumer);
        }
    }


    void Start()
    {
        if (CNetwork.IsServer)
        {
            // Subscribe to ship atmosphere pre-update
            CGameShips.Ship.GetComponent<CShipAtmosphere>().EventAtmospherePreUpdate += ProcessControlledDecompression;
            CGameShips.Ship.GetComponent<CShipAtmosphere>().EventAtmospherePreUpdate += ProcessExplosiveDecompression;
            CGameShips.Ship.GetComponent<CShipAtmosphere>().EventAtmospherePreUpdate += ProcessConsumption;
            CGameShips.Ship.GetComponent<CShipAtmosphere>().EventAtmospherePreUpdate += ProcessNeighbourTransfer;

            // Subscribe to hull events
            GetComponent<CFacilityHull>().EventBreached += OnHullEvent;
            GetComponent<CFacilityHull>().EventBreachFixed += OnHullEvent;

            // Subscriber to expansion ports door events
            foreach (GameObject cExpansionPort in GetComponent<CFacilityExpansion>().ExpansionPorts)
            {
                CExpansionPortBehaviour cExpansionPortBehaviour = cExpansionPort.GetComponent<CExpansionPortBehaviour>();

                if (cExpansionPortBehaviour.Door != null)
                {
                    cExpansionPortBehaviour.DoorBehaviour.EventOpenStart += OnDoorEvent;
                    cExpansionPortBehaviour.DoorBehaviour.EventClosed += OnDoorEvent;
                }
            }
        }
    }

	
	void Update()
	{
		if(CNetwork.IsServer)
		{
			// Remove consumers that are now null
            m_aConsumers.RemoveAll((_cConsumer) => _cConsumer == null);
		}
	}


    [AServerOnly]
    void ProcessControlledDecompression()
    {
        if (IsDepressurizing)
        {
            if (m_fQuantity.Get() != 0.0f)
            {
                ChangeQuantityByAmount(-m_fControlledDecompressionRate * Time.deltaTime);
            }
        }
    }


    [AServerOnly]
    void ProcessExplosiveDecompression()
    {
        if (IsExplosiveDepressurizing)
        {
            if (m_fQuantity.Get() != 0.0f)
            {
                ChangeQuantityByAmount(-m_fVolume * k_fExplosiveDecompressionRatio * Time.deltaTime);
            }
        }
    }


    [AServerOnly]
    void ProcessConsumption()
    {
        // Calulate the combined consumption rate within the facility
        float fConsumptionRate = 0.0f;

        m_aConsumers.ForEach((GameObject _cConsumer) =>
        {
            CActorAtmosphericConsumer cActorAtmosphereConsumer = _cConsumer.GetComponent<CActorAtmosphericConsumer>();

            if (cActorAtmosphereConsumer.IsConsumingAtmosphere)
            {
                fConsumptionRate += cActorAtmosphereConsumer.AtmosphericConsumptionRate;
            }
        });

        ChangeQuantityByAmount(m_fConsumptionRate * Time.deltaTime);

        // Save consumption rate
        m_fConsumptionRate = fConsumptionRate;
    }


    [AServerOnly]
    void ProcessNeighbourTransfer()
    {
        foreach (GameObject cExpansionPort in GetComponent<CFacilityExpansion>().ExpansionPorts)
        {
            CExpansionPortBehaviour cExpansionPortBehaviour = cExpansionPort.GetComponent<CExpansionPortBehaviour>();

            // Check door is open on this expansion port
            if (cExpansionPortBehaviour.Door.GetComponent<CDoorBehaviour>().IsOpened)
            {
                GameObject cNeighbourFacilityObject = cExpansionPortBehaviour.AttachedFacility;

                if (cNeighbourFacilityObject != null)
                {
                    CFacilityAtmosphere cNeighbourFacilityAtmosphere = cNeighbourFacilityObject.GetComponent<CFacilityAtmosphere>();

                    // Check this facility atmosphere pressure is higher then nighbour facility
                    if (cNeighbourFacilityAtmosphere.QuantityRatio < QuantityRatio)
                    {
                        // Transfer atmosphere to neighbour facility
                        float fRatioDifference = QuantityRatio - cNeighbourFacilityAtmosphere.QuantityRatio;
                        float fDeltaTransfer = 500.0f * fRatioDifference * Time.deltaTime;

                        if (fRatioDifference > 0.01f)
                        {
                            //Debug.LogError(fRatioDifference);
                            cNeighbourFacilityAtmosphere.ChangeQuantityByAmount(fDeltaTransfer);
                            ChangeQuantityByAmount(-fDeltaTransfer);
                        }
                        else
                        {
                            // Just set it to my percent
                            //cNeighbourFacilityAtmosphere.SetQuanity(cNeighbourFacilityAtmosphere.Volume * QuantityRatio);
                        }
                    }
                }
            }
        }
    }


    [AServerOnly]
    void CheckExplosiveDecompression()
    {
        bool bExposiveDecompressing = false;

        if (GetComponent<CFacilityHull>().IsBreached)
        {
            bExposiveDecompressing = true;
        }
        else
        {
            foreach (GameObject cExpansionPort in GetComponent<CFacilityExpansion>().ExpansionPorts)
            {
                CExpansionPortBehaviour cExpansionPortBehaviour = cExpansionPort.GetComponent<CExpansionPortBehaviour>();

                // Check door is open on this expansion port
                if (cExpansionPortBehaviour.Door.GetComponent<CDoorBehaviour>().IsOpened)
                {
                    GameObject cAttachedFacilityObject = cExpansionPortBehaviour.AttachedFacility;

                    // Check there is no a neighbouring facility and the door is open
                    if (cAttachedFacilityObject == null)
                    {
                        bExposiveDecompressing = true;
                    }
                }
            }
        }

        if (bExposiveDecompressing &&
            !IsExplosiveDepressurizing)
        {
            Debug.Log(gameObject.name + " explosive depressurizing enabled");
        }

        SetExplosiveDepressurizingEnabled(bExposiveDecompressing);
    }


    [AServerOnly]
    void OnDoorEvent(CDoorBehaviour _cDoorBehaviour, CDoorBehaviour.EEventType _eEventType)
    {
        switch (_eEventType)
        {
            case CDoorBehaviour.EEventType.OpenStart:
                {
                    CheckExplosiveDecompression();
                }
                break;

            case CDoorBehaviour.EEventType.Closed:
                {
                    CheckExplosiveDecompression();
                }
                break;

            default:
                Debug.LogError("Unknown door event. " + _eEventType);
                break;
        }
    }


    void OnHullEvent(CFacilityHull.EEventType _eEventType)
    {
        switch (_eEventType)
        {
            case CFacilityHull.EEventType.Breached:
                CheckExplosiveDecompression();
                break;

            case CFacilityHull.EEventType.BreachFixed:
                CheckExplosiveDecompression();
                break;

            default:
                Debug.LogError("Unknown facility hull event. " + _eEventType);
                break;
        }
    }


    void OnNetworkVarSync(INetworkVar _cSynedVar)
    {
        // Empty
    }


// Member Fields


    public float m_fVolume = 1000.0f;

    List<GameObject> m_aConsumers = new List<GameObject>();

    CNetworkVar<float> m_fQuantity = null;


    const float m_fControlledDecompressionRate = 50;
    const float k_fExplosiveDecompressionRatio = 0.30f;


// Server Member Fields


    float m_fConsumptionRate = 0.0f;

    bool m_bRefillingEnabled = true;
    bool m_bControlledDepressurizing = false;
    bool m_bExplosiveDepressurizing = false;


};
