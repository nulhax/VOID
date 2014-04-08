//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CThirdPersonAnimController.cs
//  Description :   Controls third person animations
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//

//Namespaces

using UnityEngine;
using System.Collections;

/* Implementation */

public class CThirdPersonAnimController : MonoBehaviour 
{
	
	// Member Types
	
	// Member Delegates & Events
	
	// Member Properties
	public bool IsInputDisabled
	{
		get { return(m_bInputDisabled); }
		set { m_bInputDisabled = value; }
	}
	
	// Member Fields
	
	//Animation States
	static int m_iIdleState = Animator.StringToHash("Base Layer.Idle");
	static int m_iJumpState = Animator.StringToHash("Base Layer.Jump");
	static int m_iRunState = Animator.StringToHash("Base Layer.Run");
	static int m_iWalkForwardState = Animator.StringToHash("Base Layer.WalkForward");
	static int m_iWalkBackState = Animator.StringToHash("Base Layer.WalkBack");
	static int m_iSlideState = Animator.StringToHash("Base Layer.Slide");
	static int m_iFallState = Animator.StringToHash("Base Layer.Fall");
	
	//Player motor
	CPlayerGroundMotor m_PlayerMotor;
	
	Animator m_ThirdPersonAnim;
	
	CapsuleCollider m_physCollider;

    ushort m_previousMovementState;
	ushort m_MovementState;
	
	bool m_bUsedSlide = false;
	bool m_bHoldTool = false;
	bool m_bInputDisabled = false;
	
	//Timers
	float m_fTimeLastGrounded = 0.0f;
	const float m_kfFallStateEntryTime = 0.3f;
	
	// Member Methods
	
	// Use this for initialization
	void Start () 
	{
		//Sign up to state change event in GroundMotor script
		m_PlayerMotor = gameObject.GetComponent<CPlayerGroundMotor>();
		m_PlayerMotor.EventInputStatesChange += NotifyMovementStateChange;
		
		//Get players animator
		m_ThirdPersonAnim = GetComponent<Animator>();
		
		//Get collider
		m_physCollider = GetComponent<CapsuleCollider>();	
	}

    void NotifyMovementStateChange(ushort _usPreviousStates, ushort _usNewSates)
	{
		m_MovementState = _usNewSates;
        //m_previousMovementState = _usNewSates;	
	}	
	// Update is called once per frame
	void Update () 
	{
		if(m_bInputDisabled == false)
		{			
			bool bWalkForward;
			bool bWalkBack;
			bool bSprint;
			bool bJump;
			bool bCrouch;
			bool bStrafeLeft;
			bool bStrafeRight;			
			
			bWalkForward = ((m_MovementState & (uint)CPlayerGroundMotor.EInputState.Forward)     > 0) ? true : false;	
			bWalkBack    = ((m_MovementState & (uint)CPlayerGroundMotor.EInputState.Backward)    > 0) ? true : false;	
			bJump        = ((m_MovementState & (uint)CPlayerGroundMotor.EInputState.Jump)        > 0) ? true : false;	
			bCrouch      = ((m_MovementState & (uint)CPlayerGroundMotor.EInputState.Crouch)      > 0) ? true : false;	
			bStrafeLeft  = ((m_MovementState & (uint)CPlayerGroundMotor.EInputState.StrafeLeft)  > 0) ? true : false;	
			bStrafeRight = ((m_MovementState & (uint)CPlayerGroundMotor.EInputState.StrafeRight) > 0) ? true : false;
			bSprint      = ((m_MovementState & (uint)CPlayerGroundMotor.EInputState.Turbo)       > 0) ? true : false;	
			
			m_ThirdPersonAnim.SetBool("JogForward", bWalkForward);	
			m_ThirdPersonAnim.SetBool("WalkBack", bWalkBack);
			m_ThirdPersonAnim.SetBool("Sprint", bSprint);
			m_ThirdPersonAnim.SetBool("Jump", bJump);
			m_ThirdPersonAnim.SetBool("Crouch", bCrouch);	
			m_ThirdPersonAnim.SetBool("Grounded", m_PlayerMotor.IsGrounded);	       
			
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
				if(!m_ThirdPersonAnim.IsInTransition(0))
				{
					m_bUsedSlide = true;
	
					float fOrientation = m_ThirdPersonAnim.GetFloat("ColliderHeight");
	
					//Set collider to be oriented to the Z axis	
					if(fOrientation < 1)
					{
						m_physCollider.direction = 2;	
					}
					else
					{
						m_physCollider.direction = 1;
					}
				}		
				else
				{
					//Reset collider
					m_physCollider.direction = 1;
				}
			}
			
			UpdateFallingState();
			m_ThirdPersonAnim.StopPlayback();
		}
	}
	
	void UpdateFallingState()
	{
		if(!m_PlayerMotor.IsGrounded)
		{
			if(m_fTimeLastGrounded + m_kfFallStateEntryTime > Time.time )
			{
				m_ThirdPersonAnim.SetBool("Falling", true);	
			}
			else
			{
				m_ThirdPersonAnim.SetBool("Falling", false);	
			}
		}
		else
		{
			m_fTimeLastGrounded = Time.time;
		}
	}
	
	public void DisableAnimation()
	{
        if (m_ThirdPersonAnim != null)
        {
            IsInputDisabled = true;
            m_ThirdPersonAnim.enabled = false;
        }
	}
	
    public void EnableAnimation()
	{
        if (m_ThirdPersonAnim != null)
        {
            IsInputDisabled = false;
            m_ThirdPersonAnim.enabled = true;
        }
	}
}


