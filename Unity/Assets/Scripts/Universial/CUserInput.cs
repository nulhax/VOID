//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CUserInput.cs
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


public class CUserInput : CNetworkMonoBehaviour
{

// Member Types


    public enum EAxis
    {
        INVALID = -1,

        MouseX,
        MouseY,
        LeftAnalogueX,
        LeftAnalogueY,
        RightAnalogueX,
        RightAnalogueY,

        MAX
    }


	public enum EInput
	{
        INVALID = -1,

		Primary,                  // Move Right                                      
		Secondary,                // Move Left
                                 
		Use,                      // F
                                 
		MoveGround_Forward,       // W
		MoveGround_Backwards,     // S
		MoveGround_StrafeLeft,    // A
		MoveGround_StrafeRight,   // D
        MoveGround_Jump,          // Space
        MoveGround_Crouch,        // Control
                                 
		MoveFly_Up,               // Space
		MoveFly_Down,             // Control
		MoveFly_RollLeft,         // E
		MoveFly_RollRight,        // Q
                                
		Move_Turbo,               // Shift
                                
        GalaxyShip_Forward,       // W
        GalaxyShip_Backward,      // S
        GalaxyShip_Up,            // Space
        GalaxyShip_Down,          // Control
        GalaxyShip_StrafeLeft,    // D
        GalaxyShip_StrafeRight,   // A
        GalaxyShip_YawLeft,       // Mouse X
        GalaxyShip_YawRight,      // Mouse X
        GalaxyShip_PitchUp,       // Mouse Y
        GalaxyShip_PitchDown,     // Mouse Y
        GalaxyShip_RollLeft,      // Q
        GalaxyShip_RollRight,     // E
        GalaxyShip_Turbo,         // Shift
                                
        Tool_SelectSlot1,         // 1
        Tool_SelectSlot2,         // 2
        Tool_SelectSlot3,         // 3
        Tool_SelectSlot4,         // 4
		Tool_Reload,              // R
        Tool_Drop,                // G
                                
        ReturnKey,                // Enter

        MAX
	}


    public struct TPlayerStates
    {
        public ulong ulPreviousInput;
        public ulong ulInput;
        public float fMouseX;
        public float fMouseY;
    }


// Member Delegates & Events


    public delegate void NotifyAxisChange(EAxis _eAxis, ulong _ulPlayerId, float _fValue);
    public delegate void NotifyInputChange(EInput _eInput, ulong _ulPlayerId, bool _bDown);


// Member Properties


	public static float SensitivityX { get; set; }
	public static float SensitivityY { get; set; }


    public static float MouseMovementX { get { return (s_cInstance.m_fMouseMovementX); } }
    public static float MouseMovementY { get { return (s_cInstance.m_fMouseMovementY); } }


// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        // Empty
    }


    public static void SetKeyBinding(EInput _eInput, KeyCode _eKeyCode)
    {
        s_cInstance.m_aKeyBindings[(int)_eInput] = _eKeyCode;   
    }


    public static void SubscribeInputChange(EInput _eInput, NotifyInputChange _nCallback)
    {
        if (!s_cInstance.m_mInputCallbacks.ContainsKey(_eInput))
        {
            s_cInstance.m_mInputCallbacks.Add(_eInput, new List<NotifyInputChange>());
        }

        s_cInstance.m_mInputCallbacks[_eInput].Add(_nCallback);
    }


    public static void UnsubscribeInputChange(EInput _eInput, NotifyInputChange _nCallback)
    {
        if (s_cInstance.m_mInputCallbacks.ContainsKey(_eInput))
        {
            s_cInstance.m_mInputCallbacks[_eInput].Remove(_nCallback);
        }
    }


    public static void SubscribeAxisChange(EAxis _eAxis, NotifyAxisChange _nCallback)
    {
        if (!s_cInstance.m_mAxisCallbacks.ContainsKey(_eAxis))
        {
            s_cInstance.m_mAxisCallbacks.Add(_eAxis, new List<NotifyAxisChange>());
        }

        s_cInstance.m_mAxisCallbacks[_eAxis].Add(_nCallback);
    }


    public static void UnsubscribeAxisChange(EAxis _eAxis, NotifyAxisChange _nCallback)
    {
        if (s_cInstance.m_mAxisCallbacks.ContainsKey(_eAxis))
        {
            s_cInstance.m_mAxisCallbacks[_eAxis].Remove(_nCallback);
        }
    }
	
	
	public static void UnsubscribeAll()
	{
        for (int i = 0; i < (int)EInput.MAX; ++i)
        {
            if (s_cInstance.m_mInputCallbacks.ContainsKey((EInput)i))
            {
                s_cInstance.m_mInputCallbacks[(EInput)i].Clear();
            }
        }

        for (int i = 0; i < (int)EAxis.MAX; ++i)
        {
            if (s_cInstance.m_mAxisCallbacks.ContainsKey((EAxis)i))
            {
                s_cInstance.m_mAxisCallbacks[(EAxis)i].Clear();
            }
        }
	}


    public static float GetMouseMovementX()
    {
        return (s_cInstance.m_fMouseMovementX);
    }


    public static float GetMouseMovementY()
    {
        return (s_cInstance.m_fMouseMovementY);
    }


	public static bool IsInputDown(EInput _eInput)
	{
		return (Input.GetKey(s_cInstance.m_aKeyBindings[(int)_eInput]));
	}


    public static bool IsInputDown(ulong _ulPlayerId, EInput _eInput)
    {
        return ((s_cInstance.m_mPlayerStates[_ulPlayerId].ulInput & (ulong)1 << (int)_eInput) > 0);
    }


    public void SerializeOutbound(CNetworkStream _cStream)
    {
        if (!m_InFocus)
            return ;


        _cStream.Write(s_cInstance.m_ulInputStates);
        _cStream.Write(s_cInstance.m_fMouseMovementX * SensitivityX);
        _cStream.Write(s_cInstance.m_fMouseMovementY * SensitivityY * -1.0f);
    }


    public void SerializeInbound(CNetworkPlayer _cPlayer, CNetworkStream _cStream)
    {
        TPlayerStates tPlayerStates = s_cInstance.m_mPlayerStates[_cPlayer.PlayerId];

        tPlayerStates.ulPreviousInput = tPlayerStates.ulInput;
        tPlayerStates.ulInput = _cStream.ReadULong();
        tPlayerStates.fMouseX = _cStream.ReadFloat();
        tPlayerStates.fMouseY = _cStream.ReadFloat();
    }


    void Awake()
    {
		s_cInstance = this;

        SensitivityX = 4.0f;
        SensitivityY = 4.0f;
    }


    void Start()
    {
        CNetwork.Server.EventPlayerConnect += new CNetworkServer.NotifyPlayerConnect(OnEventPlayerDisconnect);
        CNetwork.Server.EventPlayerDisconnect += new CNetworkServer.NotifyPlayerDisconnect(OnEventPlayerConnect);
        CNetwork.Server.EventShutdown += new CNetworkServer.NotifyShutdown(OnEventShutdown);
    }


    void Update()
    {
        UpdateStates();
        ProcessEvents();

        if (CNetwork.IsServer)
        {
            ProcessClientEvents();
        }
    }


    void UpdateStates()
    {
        m_ulPreviousInputStates = m_ulInputStates;
        m_ulInputStates = 0;

        for (int i = 0; i < (int)EInput.MAX; ++i)
        {
            if (Input.GetKey(s_cInstance.m_aKeyBindings[i]))
            {
                m_ulInputStates |= (ulong)1 << i;
            }
        }
    }


    void ProcessEvents()
    {
        for (int i = 0; i < (int)EInput.MAX; ++i)
        {
            if ((m_ulPreviousInputStates & (ulong)1 << i) == 0 &&
                (m_ulInputStates & (ulong)1 << i) > 0)
            {
                InvokeInputEvent((EInput)i, 0, true);
            }
            else if ((m_ulPreviousInputStates & (ulong)1 << i) > 0 &&
                        (m_ulInputStates & (ulong)1 << i) == 0)
            {
                InvokeInputEvent((EInput)i, 0, false);
            }
        }

        m_fMouseMovementX = Input.GetAxis("Mouse X") * SensitivityX;
        m_fMouseMovementY = Input.GetAxis("Mouse Y") * -1.0f * SensitivityY;

        InvokeAxisEvent(EAxis.MouseX, 0, m_fMouseMovementX);
        InvokeAxisEvent(EAxis.MouseY, 0, m_fMouseMovementY);
    }


    [AServerOnly]
    void ProcessClientEvents()
    {
        // Send out input events to subscribers on server
        foreach (KeyValuePair<ulong, TPlayerStates> tEntry in m_mPlayerStates)
        {
            TPlayerStates cPlayerStates = tEntry.Value;

            for (int i = 0; i < (int)EInput.MAX; ++i)
            {
                if ((cPlayerStates.ulPreviousInput & (ulong)1 << i) == 0 &&
                    (cPlayerStates.ulInput         & (ulong)1 << i) >  0)
                {
                    InvokeInputEvent((EInput)i, tEntry.Key, true);
                }
                else if ((cPlayerStates.ulPreviousInput & (ulong)1 << i) >  0 &&
                         (cPlayerStates.ulInput         & (ulong)1 << i) == 0)
                {
                    InvokeInputEvent((EInput)i, tEntry.Key, false);
                }
            }
        }

    }


    void InvokeInputEvent(EInput _eInput, ulong _ulPlayerId, bool _bDown)
    {
        if (m_mInputCallbacks.ContainsKey(_eInput))
        {
            List<NotifyInputChange> aSubscribers = m_mInputCallbacks[_eInput];

            foreach (NotifyInputChange cSubscriber in aSubscribers)
            {
                cSubscriber(_eInput, _ulPlayerId, _bDown);
            }
        }
    }


    void InvokeAxisEvent(EAxis _eAxis, ulong _ulPlayerId, float _fValue)
    {
        if (m_mAxisCallbacks.ContainsKey(_eAxis))
        {
            List<NotifyAxisChange> aSubscribers = m_mAxisCallbacks[_eAxis];

            foreach (NotifyAxisChange cSubscriber in aSubscribers)
            {
                cSubscriber(_eAxis, _ulPlayerId, _fValue);
            }
        }
    }


    void OnEventPlayerDisconnect(CNetworkPlayer _cPlayer)
    {
        m_mPlayerStates.Remove(_cPlayer.PlayerId);
    }


    void OnEventPlayerConnect(CNetworkPlayer _cPlayer)
    {
        m_mPlayerStates.Add(_cPlayer.PlayerId, new TPlayerStates());
    }


    void OnEventShutdown()
    {
        m_mPlayerStates.Clear();
    }


	void OnApplicationFocus(bool _Focus)
	{
		m_InFocus = _Focus;
	}


// Member Fields


    Dictionary<EInput, List<NotifyInputChange>> m_mInputCallbacks = new Dictionary<EInput, List<NotifyInputChange>>();
    Dictionary<EAxis, List<NotifyAxisChange>> m_mAxisCallbacks = new Dictionary<EAxis, List<NotifyAxisChange>>();


    KeyCode[] m_aKeyBindings = new KeyCode[(uint)EInput.MAX];


    ulong m_ulPreviousInputStates = 0;
    ulong m_ulInputStates = 0;


    float m_fMouseMovementX = 0.0f;
    float m_fMouseMovementY = 0.0f;


	bool m_InFocus = true;


	static CUserInput s_cInstance = null;


// Server Member Fields


    Dictionary<ulong, TPlayerStates> m_mPlayerStates = new Dictionary<ulong, TPlayerStates>();


};
