//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerMotor.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/* Implementation */


public class CPlayerMotor : CNetworkMonoBehaviour
{

// Member Types


	public enum EPlayerMovementState : uint
	{
		MoveForward		= 1 << 0,
		MoveBackward	= 1 << 1,
		MoveLeft		= 1 << 2,
		MoveRight		= 1 << 3,
		Jump			= 1 << 4,
		Sprint			= 1 << 5,
		Crouch			= 1 << 6,
	}


// Member Delegates & Events


// Member Properties


	public float MovementSpeed
	{
		set { m_fMovementSpeed.Set(value); }
		get { return (m_fMovementSpeed.Get()); }
	}


	public float SprintSpeed
	{
		set { m_fSprintSpeed.Set(value); }
		get { return (m_fSprintSpeed.Get()); }
	}


	public float JumpSpeed
	{
		set { m_fJumpSpeed.Set(value); }
		get { return (m_fJumpSpeed.Get()); }
	}


	public bool InputDisabled
	{
		get { return (m_cInputDisableQueue.Count > 0); }
	}
	

	public bool UsingGravity
	{
		set { m_bUsingGravity.Set(value); }
		get { return (m_bUsingGravity.Get()); }
	}


	public bool IsGrounded
	{
		get { return (m_bGrounded); }
	}


// Member Methods


	public override void InstanceNetworkVars()
	{
		m_fGravity = new CNetworkVar<float>(OnNetworkVarSync, -9.81f);
		m_fMovementSpeed = new CNetworkVar<float>(OnNetworkVarSync, 6.5f);
		m_fSprintSpeed = new CNetworkVar<float>(OnNetworkVarSync, 8.0f);
		m_fJumpSpeed = new CNetworkVar<float>(OnNetworkVarSync, 1.0f);
		m_bUsingGravity = new CNetworkVar<bool>(OnNetworkVarSync, true);
	}


	public void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{

	}


	public void Start()
	{
		m_ThirdPersonAnim = GetComponent<Animator>();
		
		m_physCollider = GetComponent<CapsuleCollider>();
		
		AudioCue[] audioCues = gameObject.GetComponents<AudioCue>();
		foreach(AudioCue cue in audioCues)
		{
			if(cue.m_strCueName == "FootSteps")
			{
				m_cueFootSteps = 	cue;
			}
		}

        gameObject.GetComponent<Animator>().enabled = false;
	}


	public void Update()
	{
		// Process grounded check on server and client
		UpdateGrounded();

		// Process input only for client owned actors
		if (CGame.PlayerActor != null &&
			CGame.PlayerActor == gameObject)
		{
			UpdateInput();				
		}
		
		//Update animation and audio based on movement states.
		//UpdateThirdPersonAnimation();
		//UpdateAudio();
	}
	
	public void FixedUpdate()
	{
		// Needs to be in fixed update!
		
		// Process movement on server and client
		if (CNetwork.IsServer)
		{
			ProcessMovement();				
		}
	}


	public void DisableInput(object _cFreezeRequester)
	{
		m_cInputDisableQueue.Add(_cFreezeRequester.GetType());

		gameObject.GetComponent<Animator>().enabled = false;
	}


	public void UndisableInput(object _cFreezeRequester)
	{
		m_cInputDisableQueue.Remove(_cFreezeRequester.GetType());

		if (!InputDisabled)
		{
			gameObject.GetComponent<Animator>().enabled = true;
		}
	}


	public static void SerializePlayerState(CNetworkStream _cStream)
	{
		// Retrieve my actor motor
		CPlayerMotor cMyActorMotor = CGame.PlayerActor.GetComponent<CPlayerMotor>();

		// Write movement states
		_cStream.Write(cMyActorMotor.m_uiMovementStates);
		_cStream.Write(cMyActorMotor.transform.eulerAngles.y);
	}


	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		// Retrieve player actor motor
		CPlayerMotor cPlayerActorMotor = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<CPlayerMotor>();

		// Read movement states
		uint uiMovementStates = _cStream.ReadUInt();

		// Read rotation y
		cPlayerActorMotor.m_fRotationY = _cStream.ReadFloat();

		// Set movement states
		cPlayerActorMotor.m_uiMovementStates = uiMovementStates;
	}


	void UpdateGrounded()
	{
		Vector3 vPos = m_physCollider.transform.position; 
		vPos.y += 1.0f;
		float fRayLength = m_physCollider.bounds.extents.y + 0.5f;				
		Vector3 vTarget = vPos - (transform.up * fRayLength);
	
		//Grounded should only be set to false if the player hasn't touched the ground for a slight amount of time.
		
		m_bCurrentlyGround = Physics.Linecast(vPos, vTarget);
		
		Debug.DrawLine(vPos, vTarget, Color.magenta);		
		
		if(m_bCurrentlyGround)
		{
			m_fTimeLastGrounded = Time.time;
		}
		
		if(	Time.time > m_fTimeLastGrounded + 0.1f)
		{
			m_bGrounded = false;		
		}
		else
		{
			m_bGrounded = true;
		}
		
		RaycastHit hitInfo = new RaycastHit();
		m_bCurrentlyGround = Physics.Linecast(vPos, -transform.up, out hitInfo);
		
		m_ThirdPersonAnim.SetFloat("DistanceToGround", hitInfo.distance); 
	}


	void UpdateInput()
	{
		m_uiMovementStates  = 0;
		m_uiMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.MoveForward)	? (uint)EPlayerMovementState.MoveForward	: (uint)0;
		m_uiMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.MoveBackwards)	? (uint)EPlayerMovementState.MoveBackward	: (uint)0;
		m_uiMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.MoveLeft)		? (uint)EPlayerMovementState.MoveLeft		: (uint)0;
		m_uiMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.MoveRight)		? (uint)EPlayerMovementState.MoveRight		: (uint)0;
		m_uiMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.Jump)			? (uint)EPlayerMovementState.Jump			: (uint)0;
		m_uiMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.Sprint)			? (uint)EPlayerMovementState.Sprint			: (uint)0;
		m_uiMovementStates |= Input.GetKey(s_eCrouchKey)                                    ? (uint)EPlayerMovementState.Crouch         : (uint)0;			
	}


	void ProcessMovement()
	{
		if(CGame.PlayerActor != gameObject)
		{
			rigidbody.transform.eulerAngles = new Vector3(rigidbody.transform.eulerAngles.x, m_fRotationY, rigidbody.transform.eulerAngles.z);
		}

		// Direction movement

		Vector3 vMovementVelocity = new Vector3();
		vMovementVelocity += ((m_uiMovementStates & (uint)EPlayerMovementState.MoveForward)  > 0) ? transform.forward : Vector3.zero;
		vMovementVelocity -= ((m_uiMovementStates & (uint)EPlayerMovementState.MoveBackward) > 0) ? transform.forward : Vector3.zero;
		vMovementVelocity -= ((m_uiMovementStates & (uint)EPlayerMovementState.MoveLeft)     > 0) ? transform.right   : Vector3.zero;
		vMovementVelocity += ((m_uiMovementStates & (uint)EPlayerMovementState.MoveRight)    > 0) ? transform.right   : Vector3.zero;

		// Apply direction movement speed
		vMovementVelocity  = vMovementVelocity.normalized;
		vMovementVelocity *= ((m_uiMovementStates & (uint)EPlayerMovementState.Sprint) > 0) ? SprintSpeed : MovementSpeed;

		// Jump 
		if ((m_uiMovementStates & (uint)EPlayerMovementState.Jump) > 0 &&
			 IsGrounded)
		{
			vMovementVelocity.y = JumpSpeed;
		}

		// Apply movement velocity
        if (!rigidbody.isKinematic)
        {
            rigidbody.velocity = new Vector3(0.0f, rigidbody.velocity.y, 0.0f);
            rigidbody.AddForce(vMovementVelocity, ForceMode.VelocityChange);
        }

        /*
        // Set latest position
        if (CNetwork.IsServer)
        {
            GetComponent<CNetworkInterpolatedObject>().SetCurrentPosition(transform.position);
        }		
        */	
	}
	
		
	void UpdateThirdPersonAnimation()
	{	
		bool bWalkForward;
		bool bWalkBack;
		bool bSprint;
		bool bJump;
		bool bCrouch;
		bool bStrafeLeft;
		bool bStrafeRight;
		
				
		bWalkForward = ((m_uiMovementStates & (uint)EPlayerMovementState.MoveForward) > 0) ? true : false;	
		bWalkBack = ((m_uiMovementStates & (uint)EPlayerMovementState.MoveBackward) > 0) ? true : false;	
		bJump = ((m_uiMovementStates & (uint)EPlayerMovementState.Jump) > 0) ? true : false;	
		bCrouch = ((m_uiMovementStates & (uint)EPlayerMovementState.Crouch) > 0) ? true : false;	
		bStrafeLeft = ((m_uiMovementStates & (uint)EPlayerMovementState.MoveLeft) > 0) ? true : false;	
		bStrafeRight = ((m_uiMovementStates & (uint)EPlayerMovementState.MoveRight) > 0) ? true : false;
		bSprint = ((m_uiMovementStates & (uint)EPlayerMovementState.Sprint) > 0) ? true : false;	
		
		UpdateStrafe(bStrafeLeft, bStrafeRight);
		UpdateCrouch(bCrouch);
		
		//Players cannot sprint if they've used their slide move. It will be re-enabled when they exit their crouch state
		if(m_bUsedSlide)
		{
			bSprint = false;
		}
			
		m_ThirdPersonAnim.SetFloat("Direction",  m_fDirection);
		m_ThirdPersonAnim.SetBool("WalkForward", bWalkForward);	
		m_ThirdPersonAnim.SetBool("WalkBack", bWalkBack);
		m_ThirdPersonAnim.SetBool("Sprint", bSprint);
		m_ThirdPersonAnim.SetBool("Jump", bJump);
		m_ThirdPersonAnim.SetBool("Crouch", bCrouch);	
		m_ThirdPersonAnim.SetBool("Grounded", IsGrounded);	
			
		AnimatorStateInfo currentBaseState = m_ThirdPersonAnim.GetCurrentAnimatorStateInfo(0);	// set our currentState variable to the current state of the Base Layer (0) of animation
		
		//-------------------------------------------
		//----------------Jump State-----------------
		//-------------------------------------------
		if (currentBaseState.nameHash == m_iJumpState)
		{
			if(!m_ThirdPersonAnim.IsInTransition(0))
			{
				m_physCollider.height = m_ThirdPersonAnim.GetFloat("ColliderHeight");					
			}			
		}
		
		//-------------------------------------------
		//----------------Fall State-----------------
		//-------------------------------------------
		if (currentBaseState.nameHash == m_iFallState)
		{					
			Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
			RaycastHit hitInfo = new RaycastHit();
			
			if(Physics.Raycast(ray, out hitInfo))
			{
				//If the player was jumping and is about to hit the ground
				if(hitInfo.distance < 2.0f)
				{
					//Change their state to the rolling animation
					m_ThirdPersonAnim.SetBool("Landing", true);
					m_ThirdPersonAnim.SetFloat("DistanceToGround", hitInfo.distance); 
				
				}
				else
				{
					m_ThirdPersonAnim.SetBool("Landing", false);
				}
			}	
		}
		
		//-------------------------------------------
		//----------------Slide State----------------
		//-------------------------------------------
		
		if(currentBaseState.nameHash == m_iSlideState)
		{
			m_bUsedSlide = true;
			
			//Set collider to be oriented to the Z axis			
			m_physCollider.direction = 2;		
		}
		else
		{
			//Reset collider
			m_physCollider.direction = 1;
		}
	}
	
	
	void UpdateAudio()
	{
		AnimatorStateInfo currentBaseState = m_ThirdPersonAnim.GetCurrentAnimatorStateInfo(0);	// set our currentState variable to the current state of the Base Layer (0) of animation
		
		//Update audio here based on animations for now
		if(currentBaseState.nameHash == m_iRunState)
		{
			if(Time.time > m_fLastFootStep + 0.4f)
			{
				m_cueFootSteps.Play(0.8f, false, -1);
				m_fLastFootStep = Time.time;
			}
		}	
		if(	currentBaseState.nameHash == m_iWalkForwardState || 
			currentBaseState.nameHash == m_iWalkBackState)
		{
			if(Time.time > m_fLastFootStep + 0.6f)
			{
				m_cueFootSteps.Play(0.8f, false, -1);
				m_fLastFootStep = Time.time;			
			}
		}
	}
	
	void UpdateCrouch(bool _bCrouch)
	{
		if(m_bPreviousCrouchState != _bCrouch)
		{
			if(_bCrouch)
			{
				m_bPreviousCrouchState = _bCrouch;
				m_fCrouchLayerWeightTarget = 0.6f;
				m_fCrouchLerpTimer = 0.0f;
			}
			else
			{
				m_bPreviousCrouchState = _bCrouch;
				m_fCrouchLayerWeightTarget = 0.0f;
				m_fCrouchLerpTimer = 0.0f;
				
				//Once the player has stood up again, re-enable the slide
				m_bUsedSlide = false;
			}
		}
		
		if(m_fCurrentCrouchWeight != m_fCrouchLayerWeightTarget)
		{
			m_fCrouchLerpTimer += Time.deltaTime;
			float percentage = m_fCrouchLerpTimer / m_fCrouchLerpTime;
			
			m_fCurrentCrouchWeight = Mathf.Lerp(m_fCurrentCrouchWeight, m_fCrouchLayerWeightTarget, percentage);	
			
			m_ThirdPersonAnim.SetLayerWeight(1, m_fCurrentCrouchWeight);
		}
	}
	
	void UpdateStrafe(bool _bStrafeLeft, bool _bStrafeRight)
	{
		//Figure out strafe direction		
		if(_bStrafeLeft)
		{	
			if(m_fDirectionTarget !=  -0.9f)
			{
				m_fDirectionTarget = -0.9f;			
				m_fDirectionLerpTimer = 0.0f;
			}
		}
		else if(_bStrafeRight)
		{
			if(m_fDirectionTarget !=  0.9f)
			{
				m_fDirectionTarget = 0.9f;
				m_fDirectionLerpTimer = 0.0f;
			}
		}
		else if(!_bStrafeLeft && !_bStrafeRight)
		{
			if(m_fDirectionTarget !=  0.0f)
			{
				m_fDirectionTarget = 0.0f;
				m_fDirectionLerpTimer = 0.0f;
			}
		}
		
		if(m_fDirection != m_fDirectionTarget)
		{	
			m_fDirectionLerpTimer += Time.deltaTime;			
			m_fDirection = Mathf.Lerp(m_fDirection, m_fDirectionTarget, m_fDirectionLerpTimer);
		}	
		else 
		{
			m_fDirectionLerpTimer = 0.0f;
		}				
	}
	
// Member Fields


	List<Type> m_cInputDisableQueue = new List<Type>();


	CNetworkVar<float> m_fGravity = null;
	CNetworkVar<float> m_fMovementSpeed = null;
	CNetworkVar<float> m_fSprintSpeed = null;
	CNetworkVar<float> m_fJumpSpeed = null;
	CNetworkVar<bool> m_bUsingGravity = null;


	float m_fRotationY = 0.0f;


	uint m_uiMovementStates = 0;
	
	
	bool m_bInputDisabled = false;
	bool m_bCurrentlyGround = false;
	bool m_bGrounded = false;

	static KeyCode s_eCrouchKey = KeyCode.C;
	
	Animator m_ThirdPersonAnim;
	
	static int m_iIdleState = Animator.StringToHash("Base Layer.Idle");
	static int m_iJumpState = Animator.StringToHash("Base Layer.Jump");
	static int m_iRunState = Animator.StringToHash("Base Layer.Run");
	static int m_iWalkForwardState = Animator.StringToHash("Base Layer.WalkForward");
	static int m_iWalkBackState = Animator.StringToHash("Base Layer.WalkBack");
	static int m_iSlideState = Animator.StringToHash("Base Layer.Slide");
	static int m_iFallState = Animator.StringToHash("Base Layer.Fall");
	
	CapsuleCollider m_physCollider;
	float m_fDirection = 0;
	float m_fDirectionTarget = 0;
	float m_fDirectionLerpTimer = 0;
	
	float m_fCrouchLayerWeightTarget = 0;
	float m_fCrouchLerpTimer = 0;
	float m_fCrouchLerpTime = 0.5f;
	float m_fCurrentCrouchWeight = 0;
	bool m_bPreviousCrouchState = false;
	
	bool m_bUsedSlide = false;
	
	float m_fTimeLastGrounded = 0.0f;
	float m_fGroundedTimer = 0.0f;
	
	AudioCue m_cueFootSteps;
	bool m_bFootStepCoolDown = false;
	float m_fLastFootStep;
};
