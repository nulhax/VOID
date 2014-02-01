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


public class CUserInput : MonoBehaviour
{

// Member Types


	public enum EInput
	{
		PrimaryDown,
		PrimaryUp,
		SecondaryDown,
		SecondaryUp,
        ReturnKey,
		Use,
		ReloadTool,
        DropTool,
		MoveForward,
		MoveBackwards,
		MoveLeft,
		MoveRight,
		FlyUp,
		FlyDown,
		FlyRollLeft,
		FlyRollRight,
		Jump,
		Sprint,
		Crouch,
        ToolSlot1,
        ToolSlot2,
        ToolSlot3,
        ToolSlot4,
	}


// Member Delegates & Events


	public delegate void NotifyMouseInput(float _fAmount);

	public static event NotifyMouseInput EventMouseMoveX;
	public static event NotifyMouseInput EventMouseMoveY;


	public delegate void NotifyKeyChange(bool _bDown);

	public static event NotifyKeyChange EventPrimary;
	public static event NotifyKeyChange EventSecondary;
    public static event NotifyKeyChange EventReturnKey;
	public static event NotifyKeyChange EventUse;
	public static event NotifyKeyChange EventReloadTool;
	public static event NotifyKeyChange EventDropTool;
	public static event NotifyKeyChange EventMoveForward;
	public static event NotifyKeyChange EventMoveBackward;
	public static event NotifyKeyChange EventMoveLeft;
	public static event NotifyKeyChange EventMoveRight;
	public static event NotifyKeyChange EventMoveJump;
	public static event NotifyKeyChange EventMoveSprint;
	public static event NotifyKeyChange EventCrouch;
	public static event NotifyKeyChange EventFlyUp;
	public static event NotifyKeyChange EventFlyDown;
	public static event NotifyKeyChange EventFlyRollLeft;
	public static event NotifyKeyChange EventFlyRollRight;


	public delegate void NotifyKeyHold();
	
	public static event NotifyKeyHold EventPrimaryHold;
	public static event NotifyKeyHold EventSecondaryHold;
	public static event NotifyKeyHold EventReturnKeyHold;
	public static event NotifyKeyHold EventUseHold;
	public static event NotifyKeyHold EventReloadToolHold;
	public static event NotifyKeyHold EventDropToolHold;
	public static event NotifyKeyHold EventMoveForwardHold;
	public static event NotifyKeyHold EventMoveBackwardHold;
	public static event NotifyKeyHold EventMoveLeftHold;
	public static event NotifyKeyHold EventMoveRightHold;
	public static event NotifyKeyHold EventMoveJumpHold;
	public static event NotifyKeyHold EventMoveSprintHold;
	public static event NotifyKeyHold EventCrouchHold;
	public static event NotifyKeyHold EventFlyUpHold;
	public static event NotifyKeyHold EventFlyDownHold;
	public static event NotifyKeyHold EventFlyRollLeftHold;
	public static event NotifyKeyHold EventFlyRollRightHold;


    public delegate void NotifyChangeToolSlot(byte _bSlot, bool _bDown);
	public static event NotifyChangeToolSlot EventChangeToolSlot;


// Member Properties


	public static float SensitivityX { get; set; }
	public static float SensitivityY { get; set; }


	public static float MouseMovementX { get; set; }
	public static float MouseMovementY { get; set; }


// Member Methods
	
	
	public static void UnregisterAllEvents()
	{
		EventPrimary = null;
		EventSecondary = null;
        EventReturnKey = null;
		EventUse = null;
		EventMoveForward = null;
		EventMoveBackward = null;
		EventMoveLeft = null;
		EventMoveRight = null;
		EventMoveJump = null;
		EventMoveSprint = null;
		EventMouseMoveX = null;
		EventMouseMoveY = null;
	}


	public static bool IsInputDown(EInput _eInput)
	{
		KeyCode eKeyCode = KeyCode.A;

		switch (_eInput)
		{
			case EInput.PrimaryDown:
			case EInput.PrimaryUp: eKeyCode = s_ePrimaryKey; break;
			case EInput.SecondaryDown:
			case EInput.SecondaryUp: eKeyCode = s_eSecondaryKey; break;
            case EInput.ReturnKey: eKeyCode = s_eReturnKey; break;
			case EInput.Use: eKeyCode = s_eUseKey; break;
			case EInput.ReloadTool: eKeyCode = s_eReloadToolKey; break;
            case EInput.DropTool: eKeyCode = s_eDropTool; break;
			case EInput.MoveForward: eKeyCode = s_eMoveForwardKey; break;
			case EInput.MoveBackwards: eKeyCode = s_eMoveBackwardsKey; break;
			case EInput.MoveLeft: eKeyCode = s_eMoveLeftKey; break;
			case EInput.MoveRight: eKeyCode = s_eMoveRightKey; break;
			case EInput.Jump: eKeyCode = s_eJumpKey; break;
			case EInput.Sprint: eKeyCode = s_eSprintKey; break;
			case EInput.Crouch: eKeyCode = m_eCrouchKey; break;
			case EInput.FlyUp: eKeyCode = m_eFlyUp; break;
			case EInput.FlyDown: eKeyCode = m_eFlyDown; break;
			case EInput.FlyRollLeft: eKeyCode = m_eFlyRollLeft; break;
			case EInput.FlyRollRight: eKeyCode = m_eFlyRollRight; break;
            case EInput.ToolSlot1: eKeyCode = s_eToolSlot1; break;
            case EInput.ToolSlot2: eKeyCode = s_eToolSlot2; break;
            case EInput.ToolSlot3: eKeyCode = s_eToolSlot3; break;
            case EInput.ToolSlot4: eKeyCode = s_eToolSlot4; break;

			default: Debug.LogError(string.Format("Unknown input type ({0})", _eInput)); break;
		}

		return (Input.GetKey(eKeyCode));
	}


    void Awake()
    {
		s_cInstance = this;

        SensitivityX = 6.0f;
        SensitivityY = 6.0f;
    }


    void Start()
    {
        // Empty
    }


    void Update()
    {
        UpdateMouseMove();
        UpdatePrimary();
        UpdateSecondary();
        UpdateReturn();
        UpdateUse();
        UpdateMovement();
        UpdateMovementSpecial();
		UpdateAirMovement();
        UpdateTools();
    }


	[AClientOnly]
	void UpdateMouseMove()
	{
		MouseMovementX = Input.GetAxis("Mouse X") * SensitivityX;
		MouseMovementY = Input.GetAxis("Mouse Y") * SensitivityY * -1.0f;

		//if (MouseMovementX != 0.0f)
		{
			if (EventMouseMoveX != null) EventMouseMoveX(MouseMovementX);
		}

		//if (MouseMovementY != 0.0f)
		{
			if (EventMouseMoveY != null) EventMouseMoveY(MouseMovementY);
		}
	}


	[AClientOnly]
	void UpdatePrimary()
	{
		if (Input.GetKeyDown(s_ePrimaryKey))
        {
			if (EventPrimary != null) EventPrimary(true);
		}
		else if (Input.GetKeyUp(s_ePrimaryKey))
		{
			if (EventPrimary != null) EventPrimary(false);
		}
		if(Input.GetKey(s_ePrimaryKey))
		{
			if (EventPrimaryHold != null) EventPrimaryHold();
		}
	}


	[AClientOnly]
	void UpdateSecondary()
	{
		if (Input.GetKeyDown(s_eSecondaryKey))
		{
			if (EventSecondary != null) EventSecondary(true);
		}
		else if (Input.GetKeyUp(s_eSecondaryKey))
		{
			if (EventSecondary != null) EventSecondary(false);
		}
		if(Input.GetKey(s_eSecondaryKey))
		{
			if (EventSecondaryHold != null) EventSecondaryHold();
		}
	}


    [AClientOnly]
    void UpdateReturn()
    {
        if (Input.GetKeyDown(s_eReturnKey))
        {
            if (EventReturnKey != null) EventReturnKey(true);
        }
        else if (Input.GetKeyUp(s_eReturnKey))
        {
            if (EventReturnKey != null) EventReturnKey(false);
        }
		if(Input.GetKey(s_eReturnKey))
		{
			if (EventReturnKeyHold != null) EventReturnKeyHold();
		}
    }


	[AClientOnly]
	void UpdateUse()
	{
		if (Input.GetKeyDown(s_eUseKey))
		{
			if (EventUse != null) EventUse(true);
		}
		else if (Input.GetKeyUp(s_eUseKey))
		{
			if (EventUse != null) EventUse(false);
		}
		if(Input.GetKey(s_eUseKey))
		{
			if (EventUseHold != null) EventUseHold();
		}
	}


	[AClientOnly]
	void UpdateMovement()
	{
		// Forward
		if (Input.GetKeyDown(s_eMoveForwardKey))
		{
			if (EventMoveForward != null) EventMoveForward(true);
		}
		else if (Input.GetKeyUp(s_eMoveForwardKey))
		{
			if (EventMoveForward != null) EventMoveForward(false);
		}
		if(Input.GetKey(s_eMoveForwardKey))
		{
			if (EventMoveForwardHold != null) EventMoveForwardHold();
		}

		// Backwards
		if (Input.GetKeyDown(s_eMoveBackwardsKey))
		{
			if (EventMoveBackward != null) EventMoveBackward(true);
		}
		else if (Input.GetKeyUp(s_eMoveBackwardsKey))
		{
			if (EventMoveBackward != null) EventMoveBackward(false);
		}
		if(Input.GetKey(s_eMoveBackwardsKey))
		{
			if (EventMoveBackwardHold != null) EventMoveBackwardHold();
		}

		// Left
		if (Input.GetKeyDown(s_eMoveLeftKey))
		{
			if (EventMoveLeft != null) EventMoveLeft(true);
		}
		else if (Input.GetKeyUp(s_eMoveLeftKey))
		{
			if (EventMoveLeft != null) EventMoveLeft(false);
		}
		if(Input.GetKey(s_eMoveLeftKey))
		{
			if (EventMoveLeftHold != null) EventMoveLeftHold();
		}

		// Right
		if (Input.GetKeyDown(s_eMoveRightKey))
		{
			if (EventMoveRight != null) EventMoveRight(true);
		}
		else if (Input.GetKeyUp(s_eMoveRightKey))
		{
			if (EventMoveRight != null) EventMoveRight(false);
		}
		if(Input.GetKey(s_eMoveRightKey))
		{
			if (EventMoveRightHold != null) EventMoveRightHold();
		}
	}


	[AClientOnly]
	void UpdateMovementSpecial()
	{
		// Jump
		if (Input.GetKeyDown(s_eJumpKey))
		{
			if (EventMoveJump != null) EventMoveJump(true);
		}
		else if (Input.GetKeyUp(s_eJumpKey))
		{
			if (EventMoveJump != null) EventMoveJump(false);
		}
		if(Input.GetKey(s_eJumpKey))
		{
			if (EventMoveJumpHold != null) EventMoveJumpHold();
		}

		// Sprint
		if (Input.GetKeyDown(s_eSprintKey))
		{
			if (EventMoveSprint != null) EventMoveSprint(true);
		}
		else if (Input.GetKeyUp(s_eSprintKey))
		{
			if (EventMoveSprint != null) EventMoveSprint(false);
		}
		if(Input.GetKey(s_eSprintKey))
		{
			if (EventMoveSprintHold != null) EventMoveSprintHold();
		}

		// Crouch
		if (Input.GetKeyDown(m_eCrouchKey))
		{
			if (EventCrouch != null) EventCrouch(true);
		}
		else if (Input.GetKeyUp(m_eCrouchKey))
		{
			if (EventCrouch != null) EventCrouch(false);
		}
		if(Input.GetKey(m_eCrouchKey))
		{
			if (EventCrouchHold != null) EventCrouchHold();
		}
	}
	
	[AClientOnly]
	void UpdateAirMovement()
	{
		// Fly up
		if (Input.GetKeyDown(m_eFlyUp))
		{
			if (EventFlyUp != null) EventFlyUp(true);
		}
		else if (Input.GetKeyUp(m_eFlyUp))
		{
			if (EventFlyUp != null) EventFlyUp(false);
		}
		if(Input.GetKey(m_eFlyUp))
		{
			if (EventFlyUpHold != null) EventFlyUpHold();
		}

		// Fly down
		if (Input.GetKeyDown(m_eFlyDown))
		{
			if (EventFlyDown != null) EventFlyDown(true);
		}
		else if (Input.GetKeyUp(m_eFlyDown))
		{
			if (EventFlyDown != null) EventFlyDown(false);
		}
		if(Input.GetKey(m_eFlyDown))
		{
			if (EventFlyDownHold != null) EventFlyDownHold();
		}

		// Fly roll left
		if (Input.GetKeyDown(m_eFlyRollLeft))
		{
			if (EventFlyRollLeft != null) EventFlyRollLeft(true);
		}
		else if (Input.GetKeyUp(m_eFlyRollLeft))
		{
			if (EventFlyRollLeft != null) EventFlyRollLeft(false);
		}
		if(Input.GetKey(m_eFlyRollLeft))
		{
			if (EventFlyRollLeftHold != null) EventFlyRollLeftHold();
		}
		
		// Fly roll right
		if (Input.GetKeyDown(m_eFlyRollRight))
		{
			if (EventFlyRollRight != null) EventFlyRollRight(true);
		}
		else if (Input.GetKeyUp(m_eFlyRollRight))
		{
			if (EventFlyRollRight != null) EventFlyRollRight(false);
		}
		if(Input.GetKey(m_eFlyRollRight))
		{
			if (EventFlyRollRightHold != null) EventFlyRollRightHold();
		}
	}


    void UpdateTools()
    {
        // Drop
        if (Input.GetKeyDown(s_eDropTool))
        {
            if (EventDropTool != null) EventDropTool(true);
        }
        else if (Input.GetKeyUp(s_eDropTool))
        {
            if (EventDropTool != null) EventDropTool(false);
        }
		if(Input.GetKey(s_eDropTool))
		{
			if (EventDropToolHold != null) EventDropToolHold();
		}

        // Reload
        if (Input.GetKeyDown(s_eReloadTool))
        {
            if (EventReloadTool != null) EventReloadTool(true);
        }
        else if (Input.GetKeyUp(s_eReloadTool))
        {
            if (EventReloadTool != null) EventReloadTool(false);
        }
		if(Input.GetKey(s_eReloadTool))
		{
			if (EventReloadToolHold != null) EventReloadToolHold();
		}

        // Slot 1
        if (Input.GetKeyDown(s_eToolSlot1))
        {
            if (EventChangeToolSlot != null) EventChangeToolSlot(1, true);
        }
        else if (Input.GetKeyUp(s_eToolSlot1))
        {
            if (EventChangeToolSlot != null) EventChangeToolSlot(1, false);
        }

        // Slot 2
        if (Input.GetKeyDown(s_eToolSlot2))
        {
            if (EventChangeToolSlot != null) EventChangeToolSlot(2, true);
        }
        else if (Input.GetKeyUp(s_eToolSlot2))
        {
            if (EventChangeToolSlot != null) EventChangeToolSlot(2, false);
        }

        // Slot 3
        if (Input.GetKeyDown(s_eToolSlot3))
        {
            if (EventChangeToolSlot != null) EventChangeToolSlot(3, true);
        }
        else if (Input.GetKeyUp(s_eToolSlot3))
        {
            if (EventChangeToolSlot != null) EventChangeToolSlot(3, false);
        }

        // Slot 4
        if (Input.GetKeyDown(s_eToolSlot4))
        {
            if (EventChangeToolSlot != null) EventChangeToolSlot(4, true);
        }
        else if (Input.GetKeyUp(s_eToolSlot4))
        {
            if (EventChangeToolSlot != null) EventChangeToolSlot(4, false);
        }
    }

// Member Fields

    // Tools
    static KeyCode s_eToolSlot1         = KeyCode.Alpha1;
    static KeyCode s_eToolSlot2         = KeyCode.Alpha2;
    static KeyCode s_eToolSlot3         = KeyCode.Alpha3;
    static KeyCode s_eToolSlot4         = KeyCode.Alpha4;
    static KeyCode s_eDropTool          = KeyCode.G;
    static KeyCode s_eReloadTool        = KeyCode.R;


	// Actions
	static KeyCode s_ePrimaryKey		= KeyCode.Mouse0;
	static KeyCode s_eSecondaryKey		= KeyCode.Mouse1;
	static KeyCode s_eUseKey			= KeyCode.E;
	static KeyCode s_eAction2Key		= KeyCode.F;


	// Movement
	static KeyCode s_eMoveForwardKey	= KeyCode.W;
	static KeyCode s_eMoveBackwardsKey	= KeyCode.S;
	static KeyCode s_eMoveLeftKey		= KeyCode.A;
	static KeyCode s_eMoveRightKey		= KeyCode.D;


	// Air Movement
	static KeyCode m_eFlyUp				= KeyCode.Space;
	static KeyCode m_eFlyDown			= KeyCode.LeftControl;
	static KeyCode m_eFlyRollLeft		= KeyCode.Z;
	static KeyCode m_eFlyRollRight		= KeyCode.X;


	// Movement Special
	static KeyCode s_eJumpKey			= KeyCode.Space;
	static KeyCode s_eSprintKey			= KeyCode.LeftShift;
	static KeyCode m_eCrouchKey			= KeyCode.LeftControl;


	// Tools
	static KeyCode s_eReloadToolKey		= KeyCode.R;
	static KeyCode s_eDropToolKey		= KeyCode.G;


	// Cockpits
	static KeyCode s_eStrafeLeft		= KeyCode.Q;
	static KeyCode s_eStrafeRight		= KeyCode.E;


    // Misc
    static KeyCode s_eReturnKey         = KeyCode.Return;


	static CUserInput s_cInstance 		= null;


};
