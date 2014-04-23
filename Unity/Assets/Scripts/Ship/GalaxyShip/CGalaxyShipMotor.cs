//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CGalaxyShipMotor.cs
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


public class CGalaxyShipMotor : CNetworkMonoBehaviour
{

// Member Types


    public enum EThrusters
    {
        INVALID = -1,

        Forward,
        Backward,
        StrafeLeft,
        StrafeRight,
        Up,
        Down,
        RollLeft,
        RollRight,
        PitchUp,
        PitchDown,
        YawLeft,
        YawRight,

        MAX
    }


// Member Delegates & Events


// Member Properties
	
	public float AngularAcceleration
	{
		get { return(m_fAngularAcceleration); }
	}

	public float AngularMaxSpeed
	{
		get { return(m_fAngularMaxSpeed); }
	}

	public float AngularVelocityDamp
	{
		get { return(m_fAngularVelocityDamp); }
	}

	public float AngularHandbreakpower
	{
		get { return(m_fAngularHandbreakpower); }
	}

	public float DirectionalMaxSpeed
	{
		get { return(m_fDirectionalMaxSpeed); }
	}

	public float DirectionalAcceleration
	{
		get { return(m_fDirectionalAcceleration); }
	}

	public float DirectionalHandbreakPower
	{
		get { return(m_fDirectionalHandbreakPower); }
	}


// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        for (int i = 0; i < (int)EThrusters.MAX; ++ i)
        {
            m_baThustersEnabled[i] = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, false);
        }
    }


    public void SetThrusterEnabled(EThrusters _eThusters, float _bPowerRatio)
    {
        m_baThustersEnabled[(int)_eThusters].Set(_bPowerRatio > 0.0f);
        m_faThruserPowerRatios[(int)_eThusters] = _bPowerRatio;
    }


    public bool IsThusterEnabled(EThrusters _eThuster)
    {
        return (m_baThustersEnabled[(int)_eThuster].Get());
    }


    public float GetThustersPowerRatio(EThrusters _eThusters)
    {
        return (m_faThruserPowerRatios[(int)_eThusters]);
    }


    void Start()
    {
		m_CachedShipPropulsionSystem = CGameShips.Ship.GetComponent<CShipPropulsionSystem>();
    }


    void OnDestroy()
    {
    }


    void Update()
    {
		UpdateVariables();
    }


    void FixedUpdate()
    {
        if (CNetwork.IsServer)
        {
            UpdateDirectionalThusters();
            UpdateAngularThusers();
        }
    }

	void UpdateVariables()
	{
        float currentPropulsion = m_CachedShipPropulsionSystem.TotalPropulsion;

		// Debug: Set the galaxy ship stuff based on the propulsion

		m_fAngularAcceleration   = Mathf.Deg2Rad * currentPropulsion * 0.33f;
		m_fAngularMaxSpeed       = Mathf.Deg2Rad * 60.0f;
		m_fAngularVelocityDamp   = Mathf.Deg2Rad * 10.0f;
		m_fAngularHandbreakpower = 0.5f;

		m_fDirectionalMaxSpeed       = 400.0f;
		m_fDirectionalAcceleration   = currentPropulsion;
		m_fDirectionalHandbreakPower = 1.0f;
	}


    void UpdateDirectionalThusters()
    {
        Rigidbody cGalaxyShipRigidbody = CGameShips.GalaxyShip.rigidbody;
        Vector3 vDirectionalAcceleration = Vector3.zero;
        Vector3 vCurrentDirectionalVelocity = Quaternion.Inverse(cGalaxyShipRigidbody.transform.rotation) * cGalaxyShipRigidbody.velocity;
        float fDeltaAcceleration = m_fDirectionalAcceleration * Time.fixedDeltaTime;

        ComputeDirectionalSpeed(EThrusters.Forward    , EThrusters.Backward  , fDeltaAcceleration, ref vCurrentDirectionalVelocity.z, ref vDirectionalAcceleration.z);
        ComputeDirectionalSpeed(EThrusters.StrafeRight, EThrusters.StrafeLeft, fDeltaAcceleration, ref vCurrentDirectionalVelocity.x, ref vDirectionalAcceleration.x);
        ComputeDirectionalSpeed(EThrusters.Up         , EThrusters.Down      , fDeltaAcceleration, ref vCurrentDirectionalVelocity.y, ref vDirectionalAcceleration.y, 2.0f);

        cGalaxyShipRigidbody.velocity = cGalaxyShipRigidbody.transform.rotation * vCurrentDirectionalVelocity;
        cGalaxyShipRigidbody.AddRelativeForce(vDirectionalAcceleration, ForceMode.Acceleration);
    }


    void ComputeDirectionalSpeed(EThrusters _eThusters, EThrusters _eOppositeThusers, float _fDeltaAcceleration, ref float _rfAxisLocalSpeed, ref float _rfAxisLocalAcceleration, float _fPowerMultiplier = 1.0f)
    {
        if (IsThusterEnabled(_eThusters) &&
            !IsThusterEnabled(_eOppositeThusers) &&
             _rfAxisLocalSpeed < m_fDirectionalMaxSpeed)
        {
            // Acceleration
            _rfAxisLocalAcceleration += m_fDirectionalAcceleration * m_faThruserPowerRatios[(int)_eThusters] * _fPowerMultiplier;

            // Handbreaking
            if (_rfAxisLocalSpeed < 0.0f)
            {
                _rfAxisLocalSpeed = ComputeDamping(_rfAxisLocalSpeed, _fDeltaAcceleration * m_fDirectionalHandbreakPower);
            }
        }
        else if (IsThusterEnabled(_eOppositeThusers) &&
                 !IsThusterEnabled(_eThusters) &&
                  _rfAxisLocalSpeed > -m_fDirectionalMaxSpeed)
        {
            // Acceleration
            _rfAxisLocalAcceleration -= m_fDirectionalAcceleration * m_faThruserPowerRatios[(int)_eOppositeThusers] * _fPowerMultiplier;

            // Handbreaking
            if (_rfAxisLocalSpeed > 0.0f)
            {
                _rfAxisLocalSpeed = ComputeDamping(_rfAxisLocalSpeed, _fDeltaAcceleration * m_fDirectionalHandbreakPower);
            }
        }
        else if (_rfAxisLocalSpeed != 0.0f)
        {
            // Damping
            _rfAxisLocalSpeed = ComputeDamping(_rfAxisLocalSpeed, _fDeltaAcceleration);
        }
    }


    void UpdateAngularThusers()
    {
        Rigidbody cGalaxyShipRigidbody = CGameShips.GalaxyShip.rigidbody;
        Vector3 vAngularAcceleration = Vector3.zero;
        Vector3 vCurrentAngularVelocity = Quaternion.Inverse(cGalaxyShipRigidbody.transform.rotation) * cGalaxyShipRigidbody.angularVelocity;
        float fDeltaAcceleration = m_fAngularAcceleration * Time.fixedDeltaTime;

        ComputeAngularSpeed(EThrusters.PitchDown , EThrusters.PitchUp, fDeltaAcceleration, ref vCurrentAngularVelocity.x, ref vAngularAcceleration.x, 1.5f);
        ComputeAngularSpeed(EThrusters.YawRight, EThrusters.YawLeft , fDeltaAcceleration, ref vCurrentAngularVelocity.y, ref vAngularAcceleration.y);
        ComputeAngularSpeed(EThrusters.RollRight, EThrusters.RollLeft, fDeltaAcceleration, ref vCurrentAngularVelocity.z, ref vAngularAcceleration.z, 1.5f);

        cGalaxyShipRigidbody.angularVelocity = cGalaxyShipRigidbody.transform.rotation * vCurrentAngularVelocity;
        cGalaxyShipRigidbody.AddRelativeTorque(vAngularAcceleration, ForceMode.Acceleration);
    }


    void ComputeAngularSpeed(EThrusters _eThusters, EThrusters _eOppositeThusers, float _fDeltaAcceleration, ref float _rfAxisLocalSpeed, ref float _rfAxisLocalAcceleration, float _fPowerMultiplier = 1.0f)
    {
        if ( IsThusterEnabled(_eThusters) &&
            !IsThusterEnabled(_eOppositeThusers) &&
             _rfAxisLocalSpeed > -m_fAngularMaxSpeed)
        {
            _rfAxisLocalAcceleration -= m_fAngularAcceleration * m_faThruserPowerRatios[(int)_eThusters] * _fPowerMultiplier;

            // Handbreaking
            if (_rfAxisLocalSpeed > 0.0f)
            {
                _rfAxisLocalSpeed = ComputeDamping(_rfAxisLocalSpeed, _fDeltaAcceleration * m_fAngularHandbreakpower);
            }
        }
        else if (IsThusterEnabled(_eOppositeThusers) &&
                 !IsThusterEnabled(_eThusters) &&
                  _rfAxisLocalSpeed < m_fAngularMaxSpeed)
        {
            _rfAxisLocalAcceleration += m_fAngularAcceleration * m_faThruserPowerRatios[(int)_eOppositeThusers] * _fPowerMultiplier;

            // Handbreaking
            if (_rfAxisLocalSpeed < 0.0f)
            {
                _rfAxisLocalSpeed = ComputeDamping(_rfAxisLocalSpeed, _fDeltaAcceleration * m_fAngularHandbreakpower);
            }
        }
        else if (_rfAxisLocalSpeed != 0.0f)
        {
            _rfAxisLocalSpeed = ComputeDamping(_rfAxisLocalSpeed, _fDeltaAcceleration);
        }
    }


    float ComputeDamping(float _fCurrentValue, float _fDeltaDamp)
    {
        if (_fCurrentValue > 0.0f)
        {
            _fCurrentValue = Mathf.Clamp(_fCurrentValue - _fDeltaDamp, 0.0f, float.PositiveInfinity);
        }
        else
        {
            _fCurrentValue = Mathf.Clamp(_fCurrentValue + _fDeltaDamp, float.NegativeInfinity, 0.0f);
        }

        return (_fCurrentValue);
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
    }


// Member Fields


    CNetworkVar<bool>[] m_baThustersEnabled = new CNetworkVar<bool>[(int)EThrusters.MAX];


    float[] m_faThruserPowerRatios = new float[(int)EThrusters.MAX];


	private CShipPropulsionSystem m_CachedShipPropulsionSystem = null;


	float m_fAngularAcceleration   = 0.0f;
	float m_fAngularMaxSpeed       = 0.0f;
	float m_fAngularVelocityDamp   = 0.0f;
	float m_fAngularHandbreakpower = 0.0f;


    float m_fDirectionalMaxSpeed       = 0.0f;
    float m_fDirectionalAcceleration   = 0.0f;
    float m_fDirectionalHandbreakPower = 0.0f;


// Server Members Fields



};
