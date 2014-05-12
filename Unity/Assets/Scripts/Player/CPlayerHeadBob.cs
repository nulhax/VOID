//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CHeadBob.cs
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

public class CPlayerHeadBob : MonoBehaviour {

	// Member Types

	// Member Delegates & Events

	// Member Properties

	// Member Fields

	//Headbob amounts   
	public float m_fHeadBobRunAmount = 0;
	public float m_fHeadBobSprintAmount = 0;
	
	//Headbob speeds
	public float m_fHeadBobRunSpeed = 8;
	public float m_fHeadBobSprintSpeed = 10;
	
	Vector3 m_initialOffset;
	ushort m_MovementState;
	
	float m_fHeadBobAmount = 0;
	float m_fHeadBobSpeed = 0;

    bool m_bUseHeadBob = true;

	// Use this for initialization
	void Start () 
	{
		//Initialise head bob
		gameObject.GetComponent<CPlayerMotor>().EventInputStatesChange += NotifyMovementStateChange;
		gameObject.GetComponent<CPlayerMotor>().EventStateChange += NotifyMotorStateChange;

		m_initialOffset = gameObject.GetComponent<CPlayerHead> ().Head.transform.localPosition;
	}

	void OnDestroy()
	{
		// Unregister
        gameObject.GetComponent<CPlayerMotor>().EventInputStatesChange -= NotifyMovementStateChange;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		if (CGamePlayers.SelfActor == gameObject && m_bUseHeadBob) 
		{
			if(gameObject.GetComponent<CPlayerMotor>().IsInputDisabled) return;

			HeadBob ();
		}
	}

	void NotifyMovementStateChange(ushort _usPreviousStates, ushort _usNewSates)
	{
		m_MovementState = _usNewSates;         
	}

	void NotifyMotorStateChange(CPlayerMotor.EState _ePrevious, CPlayerMotor.EState _eNew)
	{
		switch(_eNew) 
		{
            case CPlayerMotor.EState.WalkingShipExterior:
            {
                m_bUseHeadBob = true;
                break;
            }
            case CPlayerMotor.EState.WalkingWithinShip:
            {
                m_bUseHeadBob = true;
                break;
            }
            default:
            {
                m_bUseHeadBob = false;
                break;
            }
		}
	}

	void HeadBob()
	{
		//Determine current states
		bool bRunForward;
		bool bWalkBack;
		bool bSprint;
		bool bJump;
		bool bCrouch;
		bool bStrafeLeft;
		bool bStrafeRight;          
		
		bRunForward =  ((m_MovementState & (uint)CPlayerMotor.EInputState.Forward)     > 0) ? true : false;   
		bWalkBack    = ((m_MovementState & (uint)CPlayerMotor.EInputState.Backward)    > 0) ? true : false;   
		bJump        = ((m_MovementState & (uint)CPlayerMotor.EInputState.Jump)        > 0) ? true : false;   
		bCrouch      = ((m_MovementState & (uint)CPlayerMotor.EInputState.Crouch)      > 0) ? true : false;   
		bStrafeLeft  = ((m_MovementState & (uint)CPlayerMotor.EInputState.StrafeLeft)  > 0) ? true : false;   
		bStrafeRight = ((m_MovementState & (uint)CPlayerMotor.EInputState.StrafeRight) > 0) ? true : false;
		bSprint      = ((m_MovementState & (uint)CPlayerMotor.EInputState.Run)         > 0) ? true : false;   
		
        if (bJump)
        {
            ResetHeadPos();
            return;
        }

		//Determine head bob amount
		if((bRunForward || bWalkBack || bStrafeLeft || bStrafeRight) && !bJump)
		{
			m_fHeadBobAmount = m_fHeadBobRunAmount;
			m_fHeadBobSpeed = m_fHeadBobRunSpeed;
			ResetHeadPos();
		}
		if((bSprint && bRunForward) && !bJump)
		{
			m_fHeadBobAmount = m_fHeadBobSprintAmount;
			m_fHeadBobSpeed = m_fHeadBobSprintSpeed;
			ResetHeadPos();
		}
		
		//Only apply head bob if character is moving
		if ((bRunForward || bWalkBack || bStrafeLeft || bStrafeRight) && !bJump)
		{
			//Vector3 headPosition = gameObject.GetComponent<CPlayerHead> ().Head.transform.localPosition;
			//headPosition.y += Mathf.Cos(Time.fixedTime * m_fHeadBobSpeed) * m_fHeadBobAmount;        
			//gameObject.GetComponent<CPlayerHead> ().Head.transform.localPosition = headPosition;
			
			Vector3 headRotation = gameObject.GetComponent<CPlayerHead> ().Head.transform.localRotation.eulerAngles;
			headRotation.z += Mathf.Cos(Time.fixedTime * m_fHeadBobSpeed) * m_fHeadBobAmount;   
			gameObject.GetComponent<CPlayerHead> ().Head.transform.localRotation = Quaternion.Euler(headRotation);
		} 
		else
		{
			//Reset head bob
			ResetHeadPos();
		}
	}

	public void ResetHeadPos()
	{
		//Set position to inital offset
		gameObject.GetComponent<CPlayerHead>().Head.transform.localPosition = m_initialOffset;

		Vector3 headRotation = gameObject.GetComponent<CPlayerHead> ().Head.transform.localRotation.eulerAngles;
		//Reset roll
		headRotation.z = 0;
		gameObject.GetComponent<CPlayerHead> ().Head.transform.localRotation = Quaternion.Euler(headRotation);
	}

}
