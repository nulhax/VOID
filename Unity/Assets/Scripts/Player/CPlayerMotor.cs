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
		set { m_bInputDisabled = value; }
		get { return (m_bInputDisabled); }
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
		m_fRotationY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_fGravity = new CNetworkVar<float>(OnNetworkVarSync, -9.81f);
		m_fMovementSpeed = new CNetworkVar<float>(OnNetworkVarSync, 6.5f);
		m_fSprintSpeed = new CNetworkVar<float>(OnNetworkVarSync, 8.0f);
		m_fJumpSpeed = new CNetworkVar<float>(OnNetworkVarSync, 5.0f);
		m_bUsingGravity = new CNetworkVar<bool>(OnNetworkVarSync, true);


		m_vPosition = new CNetworkVar<Vector3>(OnNetworkVarSync);
	}


	public void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		if (_cSyncedNetworkVar == m_fRotationY &&
			CGame.PlayerActor != gameObject)
		{
			transform.eulerAngles = new Vector3(transform.eulerAngles.x, m_fRotationY.Get(), transform.eulerAngles.z);
		}
		else if (_cSyncedNetworkVar == m_vPosition)
		{
			transform.position = m_vPosition.Get();
		}
	}


	public void Start()
	{
		// Empty
		if (!CNetwork.IsServer)
		{
			rigidbody.isKinematic = true;
			rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		}
		
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
		UpdateThirdPersonAnimation();
		UpdateAudio();
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
		float fRotationY = _cStream.ReadFloat();

		// Set movement states
		cPlayerActorMotor.m_uiMovementStates = uiMovementStates;

		// Set rotation y
		cPlayerActorMotor.m_fRotationY.Set(fRotationY);		
	}


	void UpdateGrounded()
	{
		m_bGrounded = Physics.Raycast(transform.position, -Vector3.up, m_physCollider.bounds.extents.y + 0.5f);
	}


	void UpdateInput()
	{
		m_uiMovementStates  = 0;
		m_uiMovementStates |= Input.GetKey(s_eMoveForwardKey)   ? (uint)EPlayerMovementState.MoveForward  : (uint)0;
		m_uiMovementStates |= Input.GetKey(s_eMoveBackwardsKey) ? (uint)EPlayerMovementState.MoveBackward : (uint)0;
		m_uiMovementStates |= Input.GetKey(s_eMoveLeftKey)	    ? (uint)EPlayerMovementState.MoveLeft     : (uint)0;
		m_uiMovementStates |= Input.GetKey(s_eMoveRightKey)     ? (uint)EPlayerMovementState.MoveRight    : (uint)0;
		m_uiMovementStates |= Input.GetKeyDown(s_eJumpKey)      ? (uint)EPlayerMovementState.Jump         : (uint)0;
		m_uiMovementStates |= Input.GetKey(s_eSprintKey)        ? (uint)EPlayerMovementState.Sprint       : (uint)0;
		m_uiMovementStates |= Input.GetKey(s_eCrouchKey)        ? (uint)EPlayerMovementState.Crouch       : (uint)0;			
	}


	void ProcessMovement()
	{
		// Direction movement
		/*
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
		rigidbody.velocity = new Vector3(0.0f, rigidbody.velocity.y, 0.0f);
		rigidbody.AddForce(vMovementVelocity, ForceMode.VelocityChange);

		// Set latest position
		if (CNetwork.IsServer)
		{
			GetComponent<CNetworkInterpolatedObject>().SetCurrentPosition(transform.position);
		}		
		*/	
		
		m_vPosition.Set(transform.position);
	}
	
		
	void UpdateThirdPersonAnimation()
	{	
		bool WalkForward;
		bool WalkBack;
		bool Sprint;
		bool Jump;
		bool Crouch;
		bool StrafeLeft;
		bool StrafeRight;
		
				
		WalkForward = ((m_uiMovementStates & (uint)EPlayerMovementState.MoveForward) > 0) ? true : false;	
		WalkBack = ((m_uiMovementStates & (uint)EPlayerMovementState.MoveBackward) > 0) ? true : false;	
		Sprint = ((m_uiMovementStates & (uint)EPlayerMovementState.Sprint) > 0) ? true : false;	
		Jump = ((m_uiMovementStates & (uint)EPlayerMovementState.Jump) > 0) ? true : false;	
		Crouch = ((m_uiMovementStates & (uint)EPlayerMovementState.Crouch) > 0) ? true : false;	
		StrafeLeft = ((m_uiMovementStates & (uint)EPlayerMovementState.MoveLeft) > 0) ? true : false;	
		StrafeRight = ((m_uiMovementStates & (uint)EPlayerMovementState.MoveRight) > 0) ? true : false;	
		
		//Figure out strafe direction		
		if(StrafeLeft)
		{	
			if(m_fDirectionTarget !=  -0.9f)
			{
				m_fDirectionTarget = -0.9f;			
				m_fDirectionLerpTimer = 0.0f;
			}
		}
		else if(StrafeRight)
		{
			if(m_fDirectionTarget !=  0.9f)
			{
				m_fDirectionTarget = 0.9f;
				m_fDirectionLerpTimer = 0.0f;
			}
		}
		else if(!StrafeLeft && !StrafeRight)
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
		
		m_ThirdPersonAnim.SetFloat("Direction",  m_fDirection);
		m_ThirdPersonAnim.SetBool("WalkForward", WalkForward);	
		m_ThirdPersonAnim.SetBool("WalkBack", WalkBack);
		m_ThirdPersonAnim.SetBool("Sprint", Sprint);
		m_ThirdPersonAnim.SetBool("Jump", Jump);
		m_ThirdPersonAnim.SetBool("Crouch", Crouch);	
		m_ThirdPersonAnim.SetBool("Grounded", IsGrounded);	
			
		AnimatorStateInfo currentBaseState = m_ThirdPersonAnim.GetCurrentAnimatorStateInfo(0);	// set our currentState variable to the current state of the Base Layer (0) of animation
		
		if (currentBaseState.nameHash == m_iJumpState)
		{
			if(!m_ThirdPersonAnim.IsInTransition(0))
			{
				m_physCollider.height = m_ThirdPersonAnim.GetFloat("ColliderHeight");					
			}	
			
			Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
			RaycastHit hitInfo = new RaycastHit();
			
			if(Physics.Raycast(ray, out hitInfo))
			{
				if(hitInfo.distance > 1.75f)
				{
					m_ThirdPersonAnim.MatchTarget(hitInfo.point, Quaternion.identity, AvatarTarget.Root, new MatchTargetWeightMask(new Vector3(0,1,0), 0), 0.35f, 0.5f);
				}
			}				
		}
		else if(currentBaseState.nameHash == m_iSlideState)
		{
			if(!m_ThirdPersonAnim.IsInTransition(0))
			{				
				m_physCollider.direction = 2;
			}
			else
			{
				m_physCollider.direction = 1;
			}
			
			Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
			RaycastHit hitInfo = new RaycastHit();
			
			if(Physics.Raycast(ray, out hitInfo))
			{
				if(hitInfo.distance > 1.5f)
				{
					m_ThirdPersonAnim.MatchTarget(hitInfo.point, Quaternion.identity, AvatarTarget.Root, new MatchTargetWeightMask(new Vector3(0,1,0), 0), 0.1f, 0.6f);
				}
			}
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
	
// Member Fields


	CNetworkVar<float> m_fRotationY = null;
	CNetworkVar<float> m_fGravity = null;
	CNetworkVar<float> m_fMovementSpeed = null;
	CNetworkVar<float> m_fSprintSpeed = null;
	CNetworkVar<float> m_fJumpSpeed = null;
	CNetworkVar<bool> m_bUsingGravity = null;


	CNetworkVar<Vector3> m_vPosition = null;


	uint m_uiMovementStates = 0;
	
	
	bool m_bInputDisabled = false;
	bool m_bGrounded = false;
	

    static KeyCode s_eMoveForwardKey = KeyCode.W;
    static KeyCode s_eMoveBackwardsKey = KeyCode.S;
    static KeyCode s_eMoveLeftKey = KeyCode.A;
    static KeyCode s_eMoveRightKey = KeyCode.D;
	static KeyCode s_eJumpKey = KeyCode.Space;
	static KeyCode s_eSprintKey = KeyCode.LeftShift;
	static KeyCode s_eCrouchKey = KeyCode.C;
	
	Animator m_ThirdPersonAnim;
	
	static int m_iIdleState = Animator.StringToHash("Base Layer.Idle");
	static int m_iJumpState = Animator.StringToHash("Base Layer.Jump");
	static int m_iRunState = Animator.StringToHash("Base Layer.Run");
	static int m_iWalkForwardState = Animator.StringToHash("Base Layer.WalkForward");
	static int m_iWalkBackState = Animator.StringToHash("Base Layer.WalkBack");
	static int m_iSlideState = Animator.StringToHash("Base Layer.Slide");
	
	CapsuleCollider m_physCollider;
	float m_fDirection = 0;
	float m_fDirectionTarget = 0;
	float m_fDirectionLerpTimer = 0;
	
	const float m_kfCrouchLayerWeight = 0.6f;
	float m_fCrouchLerpTimer = 0;
	float m_fCurrentCrouchWeight = 0;
		
	AudioCue m_cueFootSteps;
	bool m_bFootStepCoolDown = false;
	float m_fLastFootStep;
};
