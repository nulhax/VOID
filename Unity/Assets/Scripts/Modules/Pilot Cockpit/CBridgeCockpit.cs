//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CBridgeCockpit.cs
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


public class CBridgeCockpit : CNetworkMonoBehaviour
{

// Member Types


    public enum ENetworkAction
    {
        MoveForward,
        MoveBackward,
        MoveUp,
        MoveDown,
        StrafeLeft,
        StrafeRight,
        RotateLeft,
        RotateRight,
        PitchUp,
        PitchDown
    }


// Member Delegates & Events


// Member Properties


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        // Empty
    }


    public static void SerializeCockpitInteractions(CNetworkStream _cStream)
    {
        _cStream.Write(s_cSerializeStream);
        s_cSerializeStream.Clear();
    }


    public static void UnserializeCockpitInteractions(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
    }


	void Start()
	{
        m_cCockpitBehaviour = GetComponent<CCockpit>();


        CUserInput.SubscribeClientAxisChange(CUserInput.EAxis.MouseX, OnEventAxisControlShip);
        CUserInput.SubscribeClientAxisChange(CUserInput.EAxis.MouseY, OnEventAxisControlShip);


        CUserInput.SubscribeClientInputChange(CUserInput.EInput.GalaxyShip_Forward, OnEventInputControlShip);
        CUserInput.SubscribeClientInputChange(CUserInput.EInput.GalaxyShip_Backward, OnEventInputControlShip);
        CUserInput.SubscribeClientInputChange(CUserInput.EInput.GalaxyShip_StrafeLeft, OnEventInputControlShip);
        CUserInput.SubscribeClientInputChange(CUserInput.EInput.GalaxyShip_StrafeRight, OnEventInputControlShip);
        CUserInput.SubscribeClientInputChange(CUserInput.EInput.GalaxyShip_Up, OnEventInputControlShip);
        CUserInput.SubscribeClientInputChange(CUserInput.EInput.GalaxyShip_Down, OnEventInputControlShip);
	}               


	void OnDestroy()
	{
	}


	void Update()
	{
        if (CNetwork.IsServer &&
            m_cCockpitBehaviour.IsMounted)
        {
            UpdateInput();
        }
	}


    [AServerOnly]
    void UpdateInput()
    {
        
    }


    void OnEventAxisControlShip(CUserInput.EAxis _eAxis, ulong _ulPlayerId, float _fValue)
    {

    }


    void OnEventInputControlShip(CUserInput.EInput _eInput, ulong _ulPlayerId, bool _bDown)
    {
        if (_ulPlayerId != 0 &&
            _ulPlayerId == m_cCockpitBehaviour.MountedPlayerId)
        {
            CGalaxyShipMotor cGalaxyShipMotor = CGameShips.GalaxyShip.GetComponent<CGalaxyShipMotor>();
            bool bEnable = (_bDown ? true : false);

            switch (_eInput)
            {
                case CUserInput.EInput.GalaxyShip_Forward:
                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.Forward, bEnable);
                    break;

                case CUserInput.EInput.GalaxyShip_Backward:
                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.Backward, bEnable);
                    break;

                case CUserInput.EInput.GalaxyShip_StrafeLeft:
                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.StrafeLeft, bEnable);
                    break;

                case CUserInput.EInput.GalaxyShip_StrafeRight:
                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.StrafeRight, bEnable);
                    break;

                case CUserInput.EInput.GalaxyShip_Up:
                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.Up, bEnable);
                    break;

                case CUserInput.EInput.GalaxyShip_Down:
                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.Down, bEnable);
                    break;

                default:
                    Debug.LogError("Unknown input");
                    break;
            }
        }
    }


// Member Fields


    CCockpit m_cCockpitBehaviour = null;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
