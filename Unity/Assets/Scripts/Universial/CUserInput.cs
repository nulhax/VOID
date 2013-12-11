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
		Use,
		Reload,
		MoveForward,
		MoveBackwards,
		MoveLeft,
		MoveRight,
		Jump,
		Sprint,
	}


// Member Delegates & Events


	public delegate void NotifyMouseInput(float _fAmount);

	public event NotifyMouseInput EventMouseMoveX;
	public event NotifyMouseInput EventMouseMoveY;


	public delegate void NotifyKeyChange(bool _bDown);

	public event NotifyKeyChange EventPrimary;
	public event NotifyKeyChange EventSecondary;
	public event NotifyKeyChange EventUse;
	public event NotifyKeyChange EventMoveForward;
	public event NotifyKeyChange EventMoveBackward;
	public event NotifyKeyChange EventMoveLeft;
	public event NotifyKeyChange EventMoveRight;
	public event NotifyKeyChange EventMoveJump;
	public event NotifyKeyChange EventMoveSprint;


// Member Properties


	public float SensitivityX { get; set; }
	public float SensitivityY { get; set; }


	public float MouseMovementX { get; set; }
	public float MouseMovementY { get; set; }


// Member Methods


	public void Awake()
	{
		SensitivityX = 6.0f;
		SensitivityY = 6.0f;
	}


	public void Start()
	{
		// Empty
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
		UpdateMouseMove();
		UpdatePrimary();
		UpdateSecondary();
		UpdateUse();
		UpdateMovement();
		UpdateMovementSpecial();
	}


	public bool IsInputDown(EInput _eInput)
	{
		KeyCode eKeyCode = KeyCode.A;

		switch (_eInput)
		{
			case EInput.PrimaryDown:
			case EInput.PrimaryUp: eKeyCode = s_ePrimaryKey; break;
			case EInput.SecondaryDown:
			case EInput.SecondaryUp: eKeyCode = s_eSecondaryKey; break;
			case EInput.Use: eKeyCode = s_eUseKey; break;
			case EInput.Reload: eKeyCode = s_eReloadToolKey; break;
			case EInput.MoveForward: eKeyCode = s_eMoveForwardKey; break;
			case EInput.MoveBackwards: eKeyCode = s_eMoveBackwardsKey; break;
			case EInput.MoveLeft: eKeyCode = s_eMoveLeftKey; break;
			case EInput.MoveRight: eKeyCode = s_eMoveRightKey; break;
			case EInput.Jump: eKeyCode = s_eJumpKey; break;
			case EInput.Sprint: eKeyCode = s_eSprintKey; break;

			default: Debug.LogError(string.Format("Unknown input type ({0})", _eInput)); break;
		}

		return (Input.GetKey(eKeyCode));
	}


	[AClientMethod]
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


	[AClientMethod]
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
	}


	[AClientMethod]
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
	}


	[AClientMethod]
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
	}


	[AClientMethod]
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

		// Backwards
		if (Input.GetKeyDown(s_eMoveBackwardsKey))
		{
			if (EventMoveBackward != null) EventMoveBackward(true);
		}
		else if (Input.GetKeyUp(s_eMoveBackwardsKey))
		{
			if (EventMoveBackward != null) EventMoveBackward(false);
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

		// Right
		if (Input.GetKeyDown(s_eMoveRightKey))
		{
			if (EventMoveRight != null) EventMoveRight(true);
		}
		else if (Input.GetKeyUp(s_eMoveRightKey))
		{
			if (EventMoveRight != null) EventMoveRight(false);
		}
	}


	[AClientMethod]
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

		// Sprint
		if (Input.GetKeyDown(s_eSprintKey))
		{
			if (EventMoveSprint != null) EventMoveSprint(true);
		}
		else if (Input.GetKeyUp(s_eSprintKey))
		{
			if (EventMoveSprint != null) EventMoveSprint(false);
		}
	}


// Member Fields


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


	// Movement Special
	static KeyCode s_eJumpKey			= KeyCode.Space;
	static KeyCode s_eSprintKey			= KeyCode.LeftShift;


	// Tools
	static KeyCode s_eReloadToolKey		= KeyCode.R;
	static KeyCode s_eDropToolKey		= KeyCode.G;


	// Cockpits
	static KeyCode s_eStrafeLeft		= KeyCode.Q;
	static KeyCode s_eStrafeRight		= KeyCode.E;


};
