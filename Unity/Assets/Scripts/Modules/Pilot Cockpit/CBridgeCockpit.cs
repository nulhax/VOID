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


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
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
        m_cCockpitInterface = GetComponent<CCockpitInterface>();

		m_cCockpitInterface.EventMounted += OnCockpitMount;
       	m_cCockpitInterface.EventDismounted += OnCockpitUnmount;

        CUserInput.SubscribeClientAxisChange(CUserInput.EAxis.MouseX, OnEventAxisControlShip);
        CUserInput.SubscribeClientAxisChange(CUserInput.EAxis.MouseY, OnEventAxisControlShip);

        CUserInput.SubscribeClientInputChange(CUserInput.EInput.GalaxyShip_Forward, OnEventInputControlShip);
        CUserInput.SubscribeClientInputChange(CUserInput.EInput.GalaxyShip_Backward, OnEventInputControlShip);
        CUserInput.SubscribeClientInputChange(CUserInput.EInput.GalaxyShip_StrafeLeft, OnEventInputControlShip);
        CUserInput.SubscribeClientInputChange(CUserInput.EInput.GalaxyShip_StrafeRight, OnEventInputControlShip);
        CUserInput.SubscribeClientInputChange(CUserInput.EInput.GalaxyShip_Up, OnEventInputControlShip);
        CUserInput.SubscribeClientInputChange(CUserInput.EInput.GalaxyShip_Down, OnEventInputControlShip);
        CUserInput.SubscribeClientInputChange(CUserInput.EInput.GalaxyShip_RollLeft, OnEventInputControlShip);
        CUserInput.SubscribeClientInputChange(CUserInput.EInput.GalaxyShip_RollRight, OnEventInputControlShip);

        CUserInput.SubscribeClientInputChange(CUserInput.EInput.Use, OnEventInputControlShip);
	}               


	void OnDestroy()
	{
	}


	[ALocalOnly]
	void OnEventAxisControlCamera(CUserInput.EAxis _eAxis, float _fValue)
	{
		CGalaxyShipCamera galaxyShipCamera = CGameShips.GalaxyShip.GetComponent<CGalaxyShipCamera>();

		switch (_eAxis)
		{
		case CUserInput.EAxis.MouseScroll:
			galaxyShipCamera.AdjustZoom(_fValue);
			break;
		}
	}

		
	[AServerOnly]
	void OnEventAxisControlShip(CUserInput.EAxis _eAxis, ulong _ulPlayerId, float _fValue)
	{
		if (_ulPlayerId != 0 &&
		    _ulPlayerId == m_cCockpitInterface.MountedPlayerId)
		{
			CGalaxyShipMotor cGalaxyShipMotor = CGameShips.GalaxyShip.GetComponent<CGalaxyShipMotor>();
			
			switch (_eAxis)
			{
			case CUserInput.EAxis.MouseX:
				if (_fValue == 0.0f)
				{
	                cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.YawLeft, 0.0f);
	                cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.YawRight, 0.0f);
	            }
	            else
	            {
	                if (_fValue > 0.0f)
	                {
	                    // / Screen.width
						cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.YawLeft, Mathf.Clamp(_fValue / 15, 0.0f, 1.0f));
	                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.YawRight, 0.0f);

	                }
	                else
	                {
						cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.YawLeft, 0.0f);
	                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.YawRight, Mathf.Clamp(_fValue / 15 * -1.0f, 0.0f, 1.0f));
	                }
	            }
	            break;

	        case CUserInput.EAxis.MouseY:
	            if (_fValue == 0.0f)
	            {
	                cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.PitchUp, 0.0f);
	                cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.PitchDown, 0.0f);
	            }
	            else
	            {
	                if (_fValue > 0.0f)
	                {
	                    // / Screen.width
	                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.PitchUp, Mathf.Clamp(_fValue / 20, 0.0f, 1.0f));
	                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.PitchDown, 0.0f);

	                }
	                else
	                {
	                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.PitchUp, 0.0f);
	                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.PitchDown, Mathf.Clamp(_fValue / 20 * -1.0f, 0.0f, 1.0f));
	                }
	            }
	            break;

	        default:
	            Debug.LogError("Unknown input");
	            break;
            }
        }
    }


    [AServerOnly]
    void OnEventInputControlShip(CUserInput.EInput _eInput, ulong _ulPlayerId, bool _bDown)
    {
        if (_ulPlayerId != 0 &&
            _ulPlayerId == m_cCockpitInterface.MountedPlayerId)
        {
            CGalaxyShipMotor cGalaxyShipMotor = CGameShips.GalaxyShip.GetComponent<CGalaxyShipMotor>();
            float fPower = _bDown ? 1.0f : 0.0f;

            switch (_eInput)
            {
                case CUserInput.EInput.GalaxyShip_Forward:
                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.Forward, fPower);
                    break;

                case CUserInput.EInput.GalaxyShip_Backward:
                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.Backward, fPower);
                    break;

                case CUserInput.EInput.GalaxyShip_StrafeLeft:
                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.StrafeLeft, fPower);
                    break;

                case CUserInput.EInput.GalaxyShip_StrafeRight:
                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.StrafeRight, fPower);
                    break;

                case CUserInput.EInput.GalaxyShip_Up:
                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.Up, fPower);
                    break;

                case CUserInput.EInput.GalaxyShip_Down:
                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.Down, fPower);
                    break;

                case CUserInput.EInput.GalaxyShip_RollLeft:
                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.RollLeft, fPower);
                    break;

                case CUserInput.EInput.GalaxyShip_RollRight:
                    cGalaxyShipMotor.SetThrusterEnabled(CGalaxyShipMotor.EThrusters.RollRight, fPower);
                    break;

                case CUserInput.EInput.Use:
                    {
                        if (_bDown)
                        {
                            m_cCockpitInterface.EjectPlayer();
                        }
                    }
                    break;

                default:
                    Debug.LogError("Unknown input");
                    break;
            }
        }
    }


	void OnCockpitMount(CCockpitInterface _cSender, ulong _ulPlayerId)
	{
		if(CNetwork.PlayerId == _ulPlayerId)
		{
			CGameCameras.SetObserverSpace(false);
			CGameCameras.SetObserverPerspective(CGameShips.GalaxyShip.GetComponent<CGalaxyShipCamera>().PilotCameraPosition);

			CGameHUD.SetHUDState(false);

			CUserInput.SubscribeAxisChange(CUserInput.EAxis.MouseX, OnEventAxisControlCamera);
			CUserInput.SubscribeAxisChange(CUserInput.EAxis.MouseY, OnEventAxisControlCamera);
			CUserInput.SubscribeAxisChange(CUserInput.EAxis.MouseScroll, OnEventAxisControlCamera);
		}
	}

    void OnCockpitUnmount(CCockpitInterface _cSender, ulong _ulPlayerId)
    {
		if(CNetwork.IsServer)
		{
			// Disable the thrusters
	        CGalaxyShipMotor cGalaxyShipMotor = CGameShips.GalaxyShip.GetComponent<CGalaxyShipMotor>();
	        for (CGalaxyShipMotor.EThrusters i = 0; i < CGalaxyShipMotor.EThrusters.MAX; ++i)
	        {
	            cGalaxyShipMotor.SetThrusterEnabled(i, 0.0f);
	        }
		}
		if(CNetwork.PlayerId == _ulPlayerId)
		{
			CGameCameras.SetObserverSpace(true);
			CGameCameras.SetObserverPerspective(CGamePlayers.SelfActorHead);

			CGameHUD.SetHUDState(true);

			CUserInput.UnsubscribeAxisChange(CUserInput.EAxis.MouseX, OnEventAxisControlCamera);
			CUserInput.UnsubscribeAxisChange(CUserInput.EAxis.MouseY, OnEventAxisControlCamera);
			CUserInput.UnsubscribeAxisChange(CUserInput.EAxis.MouseScroll, OnEventAxisControlCamera);
		}
    }
	

    void OnGUI()
    {
        if (m_cCockpitInterface.MountedPlayerId == CNetwork.PlayerId)
        {
            float shipSpeed = CGameShips.GalaxyShip.rigidbody.velocity.magnitude;
            Vector3 absShipPos = CGalaxy.instance.RelativePointToAbsolutePoint(CGameShips.GalaxyShip.transform.position);

            string shipOutput = string.Format("\tShip Speed: [{0}] \nShip Position [{1}] ",
                                        shipSpeed.ToString("F2"),
                                        absShipPos.ToString("F2"));

            float boxWidth = 320;
            float boxHeight = 54;

            GUI.Box(new Rect(Screen.width / 2 - boxWidth / 2,
                             Screen.height - boxHeight - 200,
                             boxWidth, boxHeight),
                      "Ship Speed\n" + shipOutput);
        }
    }


// Member Fields


    CCockpitInterface m_cCockpitInterface = null;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
