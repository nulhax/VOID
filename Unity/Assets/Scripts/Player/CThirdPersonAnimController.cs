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
	
    public bool IsPLayerJumping
    {
        get{return(m_bJumping);}
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
    static int m_iSpaceWalkState = Animator.StringToHash("Base Layer.Space Walk");
	
	//Player motor
	CPlayerMotor m_PlayerMotor;
	
	Animator m_ThirdPersonAnim;
	
	CapsuleCollider m_physCollider;
	const float m_kfColliderHeight = 1.8f;

	float m_fColliderLerpTimer = 0.0f;
	float m_fColliderLerpTime = 0.5f;

    ushort m_previousMovementState;
	ushort m_MovementState;
	
	bool m_bUsedSlide = false;
	bool m_bHoldTool = false;
	bool m_bInputDisabled = false;
    bool m_bJumping = false;
    bool m_bSpaceWalk = false;

    float m_fTimeLastGround = 0.0f;
    float m_fFallStateTriggerTime = 0.2f;
	
	//Timers
	float m_fTimeLastGrounded = 0.0f;
	const float m_kfFallStateEntryTime = 0.35f;

    bool bLogged = false;

	// Member Methods
	
	// Use this for initialization
	void Start () 
	{
		//Sign up to state change event in GroundMotor script
		m_PlayerMotor = gameObject.GetComponent<CPlayerMotor>();
		m_PlayerMotor.EventInputStatesChange += NotifyMovementStateChange;
		m_PlayerMotor.EventStateChange += NotifyMotorStateChange;
		
		//Get players animator
		m_ThirdPersonAnim = GetComponent<Animator>();
		
		//Get collider
		m_physCollider = GetComponent<CapsuleCollider>();	
	}

	void NotifyMotorStateChange(CPlayerMotor.EState _ePrevious, CPlayerMotor.EState _eNew) 
	{
		if(_eNew == CPlayerMotor.EState.AirThustersInSpace ||
		   _eNew == CPlayerMotor.EState.AirThustersInShip)
		{
			m_ThirdPersonAnim.SetLayerWeight(1,0.5f);
            m_bSpaceWalk = true;
			gameObject.GetComponent<CPlayerHeadBob>().ResetHeadPos();
			gameObject.GetComponent<CPlayerHeadBob>().ResetHeadRot();
			m_ThirdPersonAnim.SetBool("Space Walk Exit", false);
		}
		else
		{
			m_ThirdPersonAnim.SetLayerWeight(1,0);
            m_ThirdPersonAnim.SetBool("Space Walk", false);
			m_ThirdPersonAnim.SetBool("Space Walk Exit", true);
            m_bSpaceWalk = false;
		}
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
			if(CGamePlayers.SelfActor != gameObject)
			{
				int i = 0;
			}

			bool bWalkForward;
			bool bWalkBack;
			bool bSprint;
			bool bJump;
			bool bCrouch;
			bool bStrafeLeft;
			bool bStrafeRight;			
			
			bWalkForward = ((m_MovementState & (uint)CPlayerMotor.EInputState.Forward)     > 0) ? true : false;	
			bWalkBack    = ((m_MovementState & (uint)CPlayerMotor.EInputState.Backward)    > 0) ? true : false;	
			bJump        = ((m_MovementState & (uint)CPlayerMotor.EInputState.Jump)        > 0) ? true : false;	
			bCrouch      = ((m_MovementState & (uint)CPlayerMotor.EInputState.Crouch)      > 0) ? true : false;	
			bStrafeLeft  = ((m_MovementState & (uint)CPlayerMotor.EInputState.StrafeLeft)  > 0) ? true : false;	
			bStrafeRight = ((m_MovementState & (uint)CPlayerMotor.EInputState.StrafeRight) > 0) ? true : false;
			bSprint      = ((m_MovementState & (uint)CPlayerMotor.EInputState.Run)       > 0) ? true : false;	
			
			m_ThirdPersonAnim.SetBool("JogForward", bWalkForward);	
			m_ThirdPersonAnim.SetBool("WalkBack", bWalkBack);
			m_ThirdPersonAnim.SetBool("Sprint", bSprint);
			m_ThirdPersonAnim.SetBool("Jump", bJump);
			m_ThirdPersonAnim.SetBool("Crouch", bCrouch);

            if(m_PlayerMotor.IsGrounded)
            {
                m_fTimeLastGround = Time.time;
                m_ThirdPersonAnim.SetBool("Grounded", m_PlayerMotor.IsGrounded); 
                bLogged = false;
            }          	

			if(bStrafeLeft)
			{
				m_ThirdPersonAnim.SetFloat("Direction", -0.75f);	       
			}
			else if(bStrafeRight)
			{
				m_ThirdPersonAnim.SetFloat("Direction", 0.75f);	       
			}
			else if(bStrafeLeft == false && bStrafeRight == false)
			{
				m_ThirdPersonAnim.SetFloat("Direction", 0.0f);	       
			}

          

            AnimatorStateInfo currentBaseState = m_ThirdPersonAnim.GetCurrentAnimatorStateInfo(0);  // set our currentState variable to the current state of the Base Layer (0) of animation
			
            //Only enter fall state after a small amount of time, to avoid false positives.
            if(Time.time > m_fTimeLastGround + m_fFallStateTriggerTime && currentBaseState.nameHash != m_iSlideState && m_ThirdPersonAnim.GetBool("Space Walk") == false && !m_bSpaceWalk)
            {
                m_ThirdPersonAnim.SetBool("Grounded", false); 

            }  

			if(m_bSpaceWalk && currentBaseState.nameHash != m_iSpaceWalkState && currentBaseState.nameHash != m_iSlideState )
            {
                m_ThirdPersonAnim.SetBool("Space Walk", true);
				gameObject.GetComponent<CPlayerHeadBob>().ResetHeadPos();
				gameObject.GetComponent<CPlayerHeadBob>().ResetHeadRot();
            }
            else
            {
                m_ThirdPersonAnim.SetBool("Space Walk", false);
            }

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
	
					float fColliderHeight = m_ThirdPersonAnim.GetFloat("ColliderHeight");	

					m_physCollider.height = fColliderHeight;

					gameObject.GetComponent<CPlayerHead>().Head.transform.position = gameObject.GetComponent<CPlayerRagdoll>().m_RagdollHead.transform.position;

					Vector3 PlayerHeadRotation = gameObject.GetComponent<CPlayerHead>().Head.transform.rotation.eulerAngles;
					Vector3 RagdollHeadRotation = gameObject.GetComponent<CPlayerRagdoll>().m_RagdollHead.transform.rotation.eulerAngles;

					Quaternion newRotation = Quaternion.Euler(RagdollHeadRotation.x, PlayerHeadRotation.y, PlayerHeadRotation.z);

					gameObject.GetComponent<CPlayerHead>().Head.transform.rotation = newRotation;
					Vector3 offset = gameObject.GetComponent<CPlayerHead>().Head.transform.localPosition;
					offset.y += 0.3f;
					gameObject.GetComponent<CPlayerHead>().Head.transform.localPosition = offset;
				}		
				else
				{
					//Reset collider
					m_fColliderLerpTimer = 0.0f;
					gameObject.GetComponent<CPlayerHeadBob>().ResetHeadRot();
				}
			}

			if(currentBaseState.nameHash != m_iSlideState && currentBaseState.nameHash != m_iJumpState)
			{
				RestoreColliderHeight();
			}
			
			//UpdateFallingState();
			m_ThirdPersonAnim.StopPlayback();
		}
	}

	void RestoreColliderHeight()
	{
		if (m_fColliderLerpTimer < m_fColliderLerpTime) 
		{
			m_fColliderLerpTimer += Time.deltaTime;
			float fLerpFactor = m_fColliderLerpTimer / m_fColliderLerpTime;
			m_physCollider.height = Mathf.Lerp (m_physCollider.height, m_kfColliderHeight, fLerpFactor);
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

    public void OnBeginJumpingAnimation()
    {
        m_bJumping = true;
    }

    public void OnEndJumpingAnimation()
    {
        m_bJumping = false;
    }
}


