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

public class CWeaponSway : MonoBehaviour {
	
	// Member Types
	
	// Member Delegates & Events
	
	// Member Properties
	
	// Member Fields
	
	//Headbob amounts   
	public float m_fHeadBobRunAmount = 0.5f;
	public float m_fHeadBobSprintAmount = 0.5f;
	
	//Headbob speeds
	public float m_fHeadBobRunSpeed = 8;
	public float m_fHeadBobSprintSpeed = 10;
	
	Vector3 m_initialOffset;
	ushort m_MovementState;
	
	float m_fHeadBobAmount = 0;
	float m_fHeadBobSpeed = 0;
	
	// Use this for initialization
	void Start () 
	{
		//Initialise head bob
		gameObject.GetComponent<CPlayerMotor>().EventInputStatesChange += NotifyMovementStateChange;		
		m_initialOffset = transform.localPosition;
	}
	
	void OnDestroy()
	{
		// Unregister
		gameObject.GetComponent<CPlayerMotor>().EventInputStatesChange -= NotifyMovementStateChange;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		HeadBob ();
	}
	
	void NotifyMovementStateChange(ushort _usPreviousStates, ushort _usNewSates)
	{
		m_MovementState = _usNewSates;         
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
		
		//Determine head bob amount
		if((bRunForward || bWalkBack || bStrafeLeft || bStrafeRight) && !bJump)
		{
			m_fHeadBobAmount = m_fHeadBobRunAmount;
			m_fHeadBobSpeed = m_fHeadBobRunSpeed;
		}
		if((bSprint && bRunForward) && !bJump)
		{
			m_fHeadBobAmount = m_fHeadBobSprintAmount;
			m_fHeadBobSpeed = m_fHeadBobSprintSpeed;
		}
		
		//Only apply head bob if character is moving
		if ((bRunForward || bWalkBack || bStrafeLeft || bStrafeRight) && !bJump)
		{
			Vector3 headPosition = transform.localPosition;
			headPosition.y += Mathf.Cos(Time.fixedTime * m_fHeadBobSpeed) * m_fHeadBobAmount;        
			transform.localPosition = headPosition;
			
			Vector3 headRotation = transform.localRotation.eulerAngles;
			headRotation.z += Mathf.Cos(Time.fixedTime * m_fHeadBobSpeed) * m_fHeadBobAmount;   
			transform.localRotation = Quaternion.Euler(headRotation);
		} 	
	}		
}
