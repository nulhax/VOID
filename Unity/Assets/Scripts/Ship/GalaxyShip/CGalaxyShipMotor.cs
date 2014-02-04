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


// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        for (int i = 0; i < (int)EThrusters.MAX; ++ i)
        {
            m_baThustersEnabled[i] = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, false);
        }
    }


    public void SetThrusterEnabled(EThrusters _eThusters, float _bPowerRatio)
    {
        m_baThustersEnabled[(int)_eThusters].Set(_bPowerRatio > 0.0f);
        m_faPowerRatios[(int)_eThusters] = _bPowerRatio;
    }


    public bool IsThusterEnabled(EThrusters _eThuster)
    {
        return (m_baThustersEnabled[(int)_eThuster].Get());
    }


    public float GetThustersPowerRatio(EThrusters _eThusters)
    {
        return (m_faPowerRatios[(int)_eThusters]);
    }


    void Start()
    {
    }


    void OnDestroy()
    {
    }


    void Update()
    {
        if (CNetwork.IsServer)
        {
            UpdateDirectionalThusters();
            UpdateAngularThusers();
        }
    }


    void UpdateDirectionalThusters()
    {
        Rigidbody cGalaxyShipRigidbody = CGameShips.GalaxyShip.rigidbody;
        Vector3 vDirectionalAcceleration = new Vector3();
        float fDeltaAcceleration = m_fAcceleration;

        // Forwards
        if ( IsThusterEnabled(EThrusters.Forward) &&
            !IsThusterEnabled(EThrusters.Backward) &&
             cGalaxyShipRigidbody.velocity.z < m_fMaxSpeed)
        {
            vDirectionalAcceleration += cGalaxyShipRigidbody.transform.forward * fDeltaAcceleration * m_faPowerRatios[(int)EThrusters.Forward];
        }
        
        // Backwards
        else if ( IsThusterEnabled(EThrusters.Backward) &&
                 !IsThusterEnabled(EThrusters.Forward) &&
                  cGalaxyShipRigidbody.velocity.z > -m_fMaxSpeed)
        {
            vDirectionalAcceleration -= cGalaxyShipRigidbody.transform.forward * fDeltaAcceleration * m_faPowerRatios[(int)EThrusters.Backward];
        }

        // Right
        if ( IsThusterEnabled(EThrusters.StrafeRight) &&
            !IsThusterEnabled(EThrusters.StrafeLeft) &&
             cGalaxyShipRigidbody.velocity.x < m_fMaxSpeed)
        {
            vDirectionalAcceleration += cGalaxyShipRigidbody.transform.right * fDeltaAcceleration * m_faPowerRatios[(int)EThrusters.StrafeRight];
        }

        // Left
        else if ( IsThusterEnabled(EThrusters.StrafeLeft) &&
                 !IsThusterEnabled(EThrusters.StrafeRight) &&
                  cGalaxyShipRigidbody.velocity.x > -m_fMaxSpeed)
        {
            vDirectionalAcceleration -= cGalaxyShipRigidbody.transform.right * fDeltaAcceleration * m_faPowerRatios[(int)EThrusters.StrafeLeft];
        }

        // Up
        if ( IsThusterEnabled(EThrusters.Up) &&
            !IsThusterEnabled(EThrusters.Down) &&
             cGalaxyShipRigidbody.velocity.y < m_fMaxSpeed)
        {
            vDirectionalAcceleration += cGalaxyShipRigidbody.transform.up * fDeltaAcceleration * m_faPowerRatios[(int)EThrusters.Up];
        }

        // Down
        else if ( IsThusterEnabled(EThrusters.Down) &&
                 !IsThusterEnabled(EThrusters.Up) &&
                  cGalaxyShipRigidbody.velocity.y > -m_fMaxSpeed)
        {
            vDirectionalAcceleration -= cGalaxyShipRigidbody.transform.up * fDeltaAcceleration * m_faPowerRatios[(int)EThrusters.Down];
        }

        cGalaxyShipRigidbody.AddForce(vDirectionalAcceleration, ForceMode.Acceleration);
    }


    void UpdateAngularThusers()
    {
        Rigidbody cGalaxyShipRigidbody = CGameShips.GalaxyShip.rigidbody;
        Vector3 vAngularVelocity = Vector3.zero;
        float fDeltaAcceleration = m_fAngularAcceleration;

        // Roll Left
        if ( IsThusterEnabled(EThrusters.RollLeft) &&
            !IsThusterEnabled(EThrusters.RollRight))
        {
            vAngularVelocity -= cGalaxyShipRigidbody.transform.forward * fDeltaAcceleration * m_faPowerRatios[(int)EThrusters.RollLeft];
        }

        // Roll Right
        else if ( IsThusterEnabled(EThrusters.RollRight) &&
                 !IsThusterEnabled(EThrusters.RollLeft))
        {
            vAngularVelocity += cGalaxyShipRigidbody.transform.forward * fDeltaAcceleration * m_faPowerRatios[(int)EThrusters.RollRight];
        }

        // Yaw Left
        if ( IsThusterEnabled(EThrusters.YawLeft) &&
            !IsThusterEnabled(EThrusters.YawRight))
        {
            vAngularVelocity += cGalaxyShipRigidbody.transform.up * fDeltaAcceleration * m_faPowerRatios[(int)EThrusters.YawLeft] * 2.0f;
        }

        // Yaw Right
        else if ( IsThusterEnabled(EThrusters.YawRight) &&
                 !IsThusterEnabled(EThrusters.YawLeft))
        {
            vAngularVelocity -= cGalaxyShipRigidbody.transform.up * fDeltaAcceleration * m_faPowerRatios[(int)EThrusters.YawRight] * 2.0f;
        }

        // Pitch Up
        if (IsThusterEnabled(EThrusters.PitchDown) &&
            !IsThusterEnabled(EThrusters.PitchUp))
        {
            vAngularVelocity += cGalaxyShipRigidbody.transform.right * fDeltaAcceleration * m_faPowerRatios[(int)EThrusters.PitchDown] * 2.0f;
        }

        // Pitch Down
        else if (IsThusterEnabled(EThrusters.PitchUp) &&
                 !IsThusterEnabled(EThrusters.PitchDown))
        {
            vAngularVelocity -= cGalaxyShipRigidbody.transform.right * fDeltaAcceleration * m_faPowerRatios[(int)EThrusters.PitchUp] * 2.0f;
        }

        cGalaxyShipRigidbody.AddTorque(vAngularVelocity, ForceMode.Acceleration);
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
    }


// Member Fields


    CNetworkVar<bool>[] m_baThustersEnabled = new CNetworkVar<bool>[(int)EThrusters.MAX];


    float[] m_faPowerRatios = new float[(int)EThrusters.MAX];


    float m_fAngularAcceleration = (3.14f / 180.0f) * 25.0f;
    float m_fMaxAngularSpeed     = (3.14f / 180.0f) * 60.0f;


    float m_fMaxSpeed       = 400.0f; // 400 M sec
    float m_fAcceleration   = 50.0f;  // 50 M sec
    float m_fDrag           = 25.0f;


// Server Members Fields




};
