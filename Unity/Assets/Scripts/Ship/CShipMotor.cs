//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CShipMotor.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public class CShipPilotState
{
	// Member Types
	public enum EShipMovementState : uint
	{
		MoveForward 	= 1 << 0,
		MoveBackward 	= 1 << 1,
		YawLeft 		= 1 << 2,
		YawRight 		= 1 << 3,
		StrafeLeft 		= 1 << 4,
		StrafeRight 	= 1 << 5,
	}
	
	// Member Fields
	uint m_CurrentMovementState = 0;
	Vector2 m_CurrentRotationState = Vector2.zero;
	float m_LastUpdateTimeStamp = 0.0f;
		
	// Member Properties
	public uint CurrentState 
	{ 
		get { return(m_CurrentMovementState); } 
	}
	
	public Vector2 CurrentRotationState 
	{ 
		get { return(m_CurrentRotationState); }
	}
	
	public float TimeStamp 
	{ 
		get { return(m_LastUpdateTimeStamp); }
	}
	
	public Vector2 Rotation
	{
		set 
		{ 
			m_CurrentRotationState = value; 
			m_LastUpdateTimeStamp = Time.time; 
		}
		get { return(m_CurrentRotationState); }
	}
	
	public bool MovingForward
	{
		set { SetState(value, EShipMovementState.MoveForward); }
		get { return(GetState(EShipMovementState.MoveForward)); }
	}
	
	public bool MovingBackward
	{
		set { SetState(value, EShipMovementState.MoveBackward); }
		get { return(GetState(EShipMovementState.MoveBackward)); }
	}
	
	public bool MovingLeft
	{
		set { SetState(value, EShipMovementState.YawLeft); }
		get { return(GetState(EShipMovementState.YawLeft)); }
	}
	
	public bool MovingRight
	{
		set { SetState(value, EShipMovementState.YawRight); }
		get { return(GetState(EShipMovementState.YawRight)); }
	}
	
	public bool RollLeft
	{
		set { SetState(value, EShipMovementState.StrafeLeft); }
		get { return(GetState(EShipMovementState.StrafeLeft)); }
	}
	
	public bool RollRight
	{
		set { SetState(value, EShipMovementState.StrafeRight); }
		get { return(GetState(EShipMovementState.StrafeRight)); }
	}
	
	// Memeber Methods
	private void SetState(bool _Value, EShipMovementState _State)
	{
		if(_Value)
		{
			m_CurrentMovementState |= (uint)_State;
		}
		else
		{
			m_CurrentMovementState &= ~(uint)_State;
		}
		
		m_LastUpdateTimeStamp = Time.time;
	}
	
	private bool GetState(EShipMovementState _State)
	{
		return((m_CurrentMovementState & (uint)_State) != 0);
	}
	
	public void SetCurrentState(uint _NewState, Vector2 _NewRotation, float _TimeStamp)
		{
			if(CNetwork.IsServer)
			{
				if(m_LastUpdateTimeStamp < _TimeStamp)
				{
					m_CurrentMovementState = _NewState;
					m_CurrentRotationState = _NewRotation;
				}
			}
			else
			{
				Logger.Write("CShipPilotState: Only server can direcly set the pilot state!");
			}
		}
	
	public void ResetStates()
	{
		m_CurrentRotationState = Vector2.zero;
		m_CurrentMovementState = 0;
	}
}

public class CShipMotor : CNetworkMonoBehaviour 
{
	// Member Types
	
	
	// Member Fields
	private GameObject m_PilotingCockpit = null;
	
	private Vector3 m_Acceleration = Vector3.zero;
	private Vector3 m_PreviousVelocity = Vector3.zero;
	
	// Member Properies
	public GameObject PilotingCockpit
	{
		set { m_PilotingCockpit = value; }
		get { return(m_PilotingCockpit); }
	}
	
	public Vector3 Acceleration
	{
		get { return(m_Acceleration); }
	}
	
	// Member Methods
	public override void InstanceNetworkVars()
	{
	}
	
	public void Update()
	{
		
	}
	
	public void FixedUpdate()
	{
		if(CNetwork.IsServer)
		{
			ProcessMovementsAndRotations();
		}
	}
	
	private void ProcessMovementsAndRotations()
    {
		Vector3 movementForce = Vector3.zero;
		Vector3 angularForce = Vector3.zero;
		
		// Calculate the acceleration of the ship
		m_Acceleration = (rigidbody.velocity - m_PreviousVelocity) / Time.fixedDeltaTime;
		m_PreviousVelocity = rigidbody.velocity;
		
		// Get the piloting state from the cockpit
		if(PilotingCockpit == null)
			return;
		
		CShipPilotState pilotingState = PilotingCockpit.GetComponent<CBridgeCockpit>().CockpitPilotState;
		
		// Exit early to avoid computations
		if(pilotingState.CurrentState == 0 && pilotingState.CurrentRotationState == Vector2.zero)
			return;
		
		// Moving 
        if (pilotingState.MovingForward &&
            !pilotingState.MovingBackward)
        {
            movementForce.z += 100.0f;
        }
        else if (pilotingState.MovingBackward &&
            	 !pilotingState.MovingForward)
        {
            movementForce.z -= 100.0f;
        }
		
		// Strafing
        if (pilotingState.RollLeft &&
        	!pilotingState.RollRight)
        {
            movementForce.x -= 100.0f;
        }
        else if (pilotingState.RollRight &&
        		 !pilotingState.RollLeft)
        {
            movementForce.x += 100.0f;
        }	
		
		// Yaw Rotation
        if (pilotingState.MovingLeft &&
        	!pilotingState.MovingRight)
        {
            angularForce.y -= 1.0f;
        }
        else if (pilotingState.MovingRight &&
        		 !pilotingState.MovingLeft)
        {
            angularForce.y += 1.0f;
        }
		
		// Roll rotation
		if(pilotingState.CurrentRotationState.x != 0.0f)
		{
			angularForce.z -= pilotingState.CurrentRotationState.x;
		}
		
		// Pitch rotation
		if(pilotingState.CurrentRotationState.y != 0.0f)
		{
			angularForce.x += pilotingState.CurrentRotationState.y;
		}
		
		// Apply the movement forces
		rigidbody.AddRelativeForce(movementForce, ForceMode.Acceleration);
		
		// Apply the torque force
		rigidbody.AddRelativeTorque(angularForce, ForceMode.Acceleration);
	}
}
