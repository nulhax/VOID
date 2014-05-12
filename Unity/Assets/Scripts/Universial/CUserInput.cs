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
		MouseScroll,
        LeftAnalogueX,
        LeftAnalogueY,
        RightAnalogueX,
        RightAnalogueY,

       // GalaxyShip_YawLeft,       // Mouse X
        //GalaxyShip_YawRight,      // Mouse X
        //GalaxyShip_PitchUp,       // Mouse Y
       // GalaxyShip_PitchDown,     // Mouse Y

        MAX
    }


	public enum EInput
	{
        INVALID = -1,

		Primary,                 // Move Right                                      
		Secondary,               // Move Left
                                  
		Use,                     // F
		Visor,                   // C
		Push_To_Talk,		     // Alt	
                                  
		Move_Forward,            // W
		Move_Backwards,          // S
		Move_StrafeLeft,         // A
		Move_StrafeRight,        // D
        MoveGround_Jump,         // Space
        Move_Crouch,             // Control
        Move_Run,                // Shift
                                 
		MoveFly_Up,               // Space
		MoveFly_Down,             // Control
		MoveFly_RollLeft,         // E
		MoveFly_RollRight,        // Q
        MoveFly_Stabilize,        // Shift
                                
        GalaxyShip_Forward,       // W
        GalaxyShip_Backward,      // S
        GalaxyShip_Up,            // Space
        GalaxyShip_Down,          // Control
        GalaxyShip_StrafeLeft,    // D
        GalaxyShip_StrafeRight,   // A
		GalaxyShip_RollLeft,
		GalaxyShip_RollRight,
        GalaxyShip_Turbo,         // Shift
                                
        Tool_EquipToolSlot1,      // 1
        Tool_EquipToolSlot2,      // 2
        Tool_EquipToolSlot3,      // 3
        Tool_EquipToolSlot4,      // 4
		Tool_Reload,              // R
        Tool_Drop,                // G

        ModuleMenu_ToggleDisplay, // B
        TurretMenu_ToggleDisplay, // Tab
                                
        ReturnKey,                // Enter
        Escape,                   // P... (LOL) Nah Esc bra

        MAX
	}


    public class TPlayerStates
    {
        public ulong ulPreviousInput;
        public ulong ulInput;
        public float fPreviousMouseX;
        public float fPreviousMouseY;
		public float fPreviousMouseScroll;
        public float fMouseX;
        public float fMouseY;
		public float fMouseScroll;
    }


// Member Delegates & Events


    public delegate void NotifyAxisChange(EAxis _eAxis, float _fValue);
    public delegate void NotifyInputChange(EInput _eInput, bool _bDown);


    public delegate void NotifyClientAxisChange(EAxis _eAxis, ulong _ulPlayerId, float _fValue);
    public delegate void NotifyClientInputChange(EInput _eInput, ulong _ulPlayerId, bool _bDown);


// Member Properties


	public static float SensitivityX { get; set; }
	public static float SensitivityY { get; set; }
	public static float SensitivityScroll { get; set; }


    public static float MouseMovementX { get { return (s_cInstance.m_fMouseMovementX); } }
    public static float MouseMovementY { get { return (s_cInstance.m_fMouseMovementY); } }
	public static float MouseMovementScroll { get { return (s_cInstance.m_fMouseScroll); } }


// Member Methods


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        // Empty
    }


    public static void SetKeyBinding(EInput _eInput, KeyCode _eKeyCode)
    {
        s_cInstance.m_aKeyBindings[(int)_eInput] = _eKeyCode;   
    }


	public static KeyCode GetKeyBinding(EInput _eInput)
	{
		return(s_cInstance.m_aKeyBindings[(int)_eInput]);
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


    public static void SubscribeClientInputChange(EInput _eInput, NotifyClientInputChange _nCallback)
    {
        if (!s_cInstance.m_mClientInputCallbacks.ContainsKey(_eInput))
        {
            s_cInstance.m_mClientInputCallbacks.Add(_eInput, new List<NotifyClientInputChange>());
        }

        s_cInstance.m_mClientInputCallbacks[_eInput].Add(_nCallback);
    }


    public static void UnsubscribeClientInputChange(EInput _eInput, NotifyClientInputChange _nCallback)
    {
        if (s_cInstance.m_mClientInputCallbacks.ContainsKey(_eInput))
        {
            s_cInstance.m_mClientInputCallbacks[_eInput].Remove(_nCallback);
        }
    }


    public static void SubscribeClientAxisChange(EAxis _eClientAxis, NotifyClientAxisChange _nCallback)
    {
        if (!s_cInstance.m_mClientAxisCallbacks.ContainsKey(_eClientAxis))
        {
            s_cInstance.m_mClientAxisCallbacks.Add(_eClientAxis, new List<NotifyClientAxisChange>());
        }

        s_cInstance.m_mClientAxisCallbacks[_eClientAxis].Add(_nCallback);
    }


    public static void UnsubscribeClientAxisChange(EAxis _eClientAxis, NotifyClientAxisChange _nCallback)
    {
        if (s_cInstance.m_mClientAxisCallbacks.ContainsKey(_eClientAxis))
        {
            s_cInstance.m_mClientAxisCallbacks[_eClientAxis].Remove(_nCallback);
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


	public static float GetMouseScroll()
	{
		return (s_cInstance.m_fMouseScroll);
	}


	public static bool IsInputDown(EInput _eInput)
	{
		return (Input.GetKey(s_cInstance.m_aKeyBindings[(int)_eInput]));
	}


    public static bool IsInputDown(ulong _ulPlayerId, EInput _eInput)
    {
        return ((s_cInstance.m_mPlayerStates[_ulPlayerId].ulInput & (ulong)1 << (int)_eInput) > 0);
    }


    public static void SerializeOutbound(CNetworkStream _cStream)
    {
        if (!s_cInstance.m_InFocus)
            return ;

        _cStream.Write(s_cInstance.m_ulInputStates);
        _cStream.Write(s_cInstance.m_fSerializeMouseMovementX);
        _cStream.Write(s_cInstance.m_fSerializeMouseMovementY);
		_cStream.Write(s_cInstance.m_fSerializeMouseScroll);

        s_cInstance.m_fSerializeMouseMovementX = 0.0f;
        s_cInstance.m_fSerializeMouseMovementY = 0.0f;
		s_cInstance.m_fSerializeMouseScroll = 0.0f;
    }


    public static void UnserializeInbound(CNetworkPlayer _cPlayer, CNetworkStream _cStream)
    {
        TPlayerStates tPlayerStates = s_cInstance.m_mPlayerStates[_cPlayer.PlayerId];

        tPlayerStates.ulPreviousInput = tPlayerStates.ulInput;
        tPlayerStates.ulInput = _cStream.Read<ulong>();
        tPlayerStates.fPreviousMouseX = tPlayerStates.fMouseX;
        tPlayerStates.fPreviousMouseY = tPlayerStates.fMouseY;
		tPlayerStates.fPreviousMouseScroll = tPlayerStates.fMouseScroll;
        tPlayerStates.fMouseX = _cStream.Read<float>();
        tPlayerStates.fMouseY = _cStream.Read<float>();
		tPlayerStates.fMouseScroll = _cStream.Read<float>();

        if (tPlayerStates.fPreviousMouseX != tPlayerStates.fMouseX)
        {
            s_cInstance.InvokeClientClientAxisEvent(EAxis.MouseX, _cPlayer.PlayerId, tPlayerStates.fMouseX);
        }

        if (tPlayerStates.fPreviousMouseY != tPlayerStates.fMouseY)
        {
            s_cInstance.InvokeClientClientAxisEvent(EAxis.MouseY, _cPlayer.PlayerId, tPlayerStates.fMouseY);
        }

		if (tPlayerStates.fPreviousMouseScroll != tPlayerStates.fMouseScroll)
		{
			s_cInstance.InvokeClientClientAxisEvent(EAxis.MouseScroll, _cPlayer.PlayerId, tPlayerStates.fMouseScroll);
		}
        
        s_cInstance.ProcessClientEvents(_cPlayer.PlayerId, tPlayerStates);
    }


    void Awake()
    {
		s_cInstance = this;

        SensitivityX = 4.0f;
        SensitivityY = 4.0f;
		SensitivityScroll = 1.0f;
    }


    void Start()
    {
        CNetwork.Server.EventPlayerConnect += new CNetworkServer.NotifyPlayerConnect(OnEventPlayerConnect);
        CNetwork.Server.EventPlayerDisconnect += new CNetworkServer.NotifyPlayerDisconnect(OnEventPlayerDisconnect);
        CNetwork.Server.EventShutdown += new CNetworkServer.NotifyShutdown(OnEventShutdown);
    }


    void Update()
    {
        if ( CNetwork.Connection.IsConnected &&
            !CNetwork.Connection.IsDownloadingInitialGameData)
        {
            UpdateStates();
            ProcessEvents();
        }

        if (CNetwork.IsServer)
        {
            //ProcessClientEvents();
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
                InvokeInputEvent((EInput)i, true);
            }
            else if ((m_ulPreviousInputStates & (ulong)1 << i) > 0 &&
                        (m_ulInputStates & (ulong)1 << i) == 0)
            {
                InvokeInputEvent((EInput)i, false);
            }
        }

        m_fPreviousMouseMovementX = m_fMouseMovementX;
        m_fPreviousMouseMovementY = m_fMouseMovementY;
		m_fPreviousMouseScroll = m_fMouseScroll;
        m_fMouseMovementX = Input.GetAxis("Mouse X") * SensitivityX;
        m_fMouseMovementY = Input.GetAxis("Mouse Y") * -1.0f * SensitivityY;
		m_fMouseScroll = Input.GetAxis("Mouse ScrollWheel") * SensitivityScroll;

        // Increment mouse movement
        m_fSerializeMouseMovementX += m_fMouseMovementX;
        m_fSerializeMouseMovementY += m_fMouseMovementY;

		m_fSerializeMouseScroll += m_fMouseScroll;

        if (m_fMouseMovementX != m_fPreviousMouseMovementX)
            InvokeAxisEvent(EAxis.MouseX, m_fMouseMovementX);

        if (m_fMouseMovementY != m_fPreviousMouseMovementY)
            InvokeAxisEvent(EAxis.MouseY, m_fMouseMovementY);

		if (m_fMouseScroll != m_fPreviousMouseScroll)
			InvokeAxisEvent(EAxis.MouseScroll, m_fMouseScroll);
    }


    [AServerOnly]
    void ProcessClientEvents(ulong _ulPlayerId, TPlayerStates _tPlayers)
    {
        // Send out input events to subscribers on server
        for (int i = 0; i < (int)EInput.MAX; ++i)
        {
            if ((_tPlayers.ulPreviousInput & (ulong)1 << i) == 0 &&
                (_tPlayers.ulInput         & (ulong)1 << i) > 0)
            {
                InvokeClientInputEvent((EInput)i, _ulPlayerId, true);
            }
            else if ((_tPlayers.ulPreviousInput & (ulong)1 << i) > 0 &&
                     (_tPlayers.ulInput         & (ulong)1 << i) == 0)
            {
                InvokeClientInputEvent((EInput)i, _ulPlayerId, false);
            }
        }
    }


    void InvokeInputEvent(EInput _eInput, bool _bDown)
    {
        if (m_mInputCallbacks.ContainsKey(_eInput))
        {
            List<NotifyInputChange> aSubscribers = m_mInputCallbacks[_eInput];

            foreach (NotifyInputChange cSubscriber in aSubscribers)
            {
                cSubscriber(_eInput, _bDown);
            }
        }
    }


    void InvokeAxisEvent(EAxis _eAxis, float _fValue)
    {
        if (m_mAxisCallbacks.ContainsKey(_eAxis))
        {
            List<NotifyAxisChange> aSubscribers = m_mAxisCallbacks[_eAxis];

            foreach (NotifyAxisChange cSubscriber in aSubscribers)
            {
                cSubscriber(_eAxis, _fValue);
            }
        }
    }


    void InvokeClientInputEvent(EInput _eInput, ulong _ulPlayerId, bool _bDown)
    {
        if (m_mClientInputCallbacks.ContainsKey(_eInput))
        {
            List<NotifyClientInputChange> aSubscribers = m_mClientInputCallbacks[_eInput];

            foreach (NotifyClientInputChange cSubscriber in aSubscribers)
            {
                cSubscriber(_eInput, _ulPlayerId, _bDown);
            }
        }
    }


    void InvokeClientClientAxisEvent(EAxis _eClientAxis, ulong _ulPlayerId, float _fValue)
    {
        if (m_mClientAxisCallbacks.ContainsKey(_eClientAxis))
        {
            List<NotifyClientAxisChange> aSubscribers = m_mClientAxisCallbacks[_eClientAxis];

            foreach (NotifyClientAxisChange cSubscriber in aSubscribers)
            {
                cSubscriber(_eClientAxis, _ulPlayerId, _fValue);
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


// Member Fields


    Dictionary<EInput, List<NotifyInputChange>> m_mInputCallbacks = new Dictionary<EInput, List<NotifyInputChange>>();
    Dictionary<EAxis, List<NotifyAxisChange>> m_mAxisCallbacks = new Dictionary<EAxis, List<NotifyAxisChange>>();


    KeyCode[] m_aKeyBindings = new KeyCode[(uint)EInput.MAX];


    ulong m_ulPreviousInputStates = 0;
    ulong m_ulInputStates = 0;


    float m_fMouseMovementX = 0.0f;
    float m_fMouseMovementY = 0.0f;
	float m_fMouseScroll = 0.0f;
    float m_fPreviousMouseMovementX = 0.0f;
    float m_fPreviousMouseMovementY = 0.0f;
	float m_fPreviousMouseScroll = 0.0f;
    float m_fSerializeMouseMovementX = 0.0f;
    float m_fSerializeMouseMovementY = 0.0f;
	float m_fSerializeMouseScroll = 0.0f;


	bool m_InFocus = true;


	static CUserInput s_cInstance = null;


// Server Member Fields


    Dictionary<EInput, List<NotifyClientInputChange>> m_mClientInputCallbacks = new Dictionary<EInput, List<NotifyClientInputChange>>();
    Dictionary<EAxis, List<NotifyClientAxisChange>> m_mClientAxisCallbacks = new Dictionary<EAxis, List<NotifyClientAxisChange>>();
   

    Dictionary<ulong, TPlayerStates> m_mPlayerStates = new Dictionary<ulong, TPlayerStates>();


};
