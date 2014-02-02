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
		
	// Member Properties
	public uint CurrentState 
	{ 
		get { return(m_CurrentMovementState); } 
	}
	
	public Vector2 CurrentRotationState 
	{ 
		get { return(m_CurrentRotationState); }
	}
	
	public Vector2 Rotation
	{
		set { m_CurrentRotationState = value; }
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
	}
	
	private bool GetState(EShipMovementState _State)
	{
		return((m_CurrentMovementState & (uint)_State) != 0);
	}

	[ANetworkRpc]
	public void SetCurrentState(uint _NewState, Vector2 _NewRotation)
		{
			if(CNetwork.IsServer)
			{
				m_CurrentMovementState = _NewState;
				m_CurrentRotationState = _NewRotation;
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

public class CGalaxyShipMotor : CNetworkMonoBehaviour 
{
	// Member Types
	
	
	// Member Fields
	private GameObject m_PilotingCockpit = null;
	private CShipPilotState m_CurrentPilotState = null;

	private Vector3 m_Acceleration = Vector3.zero;
	private Vector3 m_PreviousVelocity = Vector3.zero;

	private CNetworkVar<Vector3> m_Velocity = null;
	private CNetworkVar<Vector3> m_AngularVelocity = null;

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
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_Velocity = _cRegistrar.CreateNetworkVar<Vector3>(OnNetworkVarSync, Vector3.zero);
		m_AngularVelocity = _cRegistrar.CreateNetworkVar<Vector3>(OnNetworkVarSync, Vector3.zero);
	}

	public void OnNetworkVarSync(INetworkVar _rSender)
	{

	}

	public void Start()
	{
		if(!CNetwork.IsServer)
		{
			rigidbody.isKinematic = true;
		}
	}

	public void LateUpdate()
	{
		if(CNetwork.IsServer)
		{
			if(PilotingCockpit != null)
			{
				m_CurrentPilotState = PilotingCockpit.GetComponent<CBridgeCockpit>().CockpitPilotState;
			}
		}
	}
	
	public void FixedUpdate()
	{
		if(CNetwork.IsServer)
		{
			if(m_CurrentPilotState != null)
				ProcessMovementsAndRotations();
		}
	}

	public Vector3 GetRelativePointVelocity(Vector3 _GalaxyPos)
	{
		Vector3 velocity = Vector3.zero;

		if(CNetwork.IsServer)
		{
			velocity = rigidbody.GetRelativePointVelocity(_GalaxyPos - CGameShips.GalaxyShip.transform.position);
		}
		else
		{
			// Set the rigidbody to dynamic and apply current velocities temporarily
			rigidbody.isKinematic = false;
			rigidbody.velocity = m_Velocity.Get();
			rigidbody.angularVelocity = m_AngularVelocity.Get();

			velocity = rigidbody.GetRelativePointVelocity(_GalaxyPos - CGameShips.GalaxyShip.transform.position);

			rigidbody.isKinematic = true;
		}

		return(velocity);
	}
	
	private void ProcessMovementsAndRotations()
    {
		Vector3 movementForce = Vector3.zero;
		Vector3 angularForce = Vector3.zero;
		
		// Calculate the acceleration of the ship
		m_Acceleration = (rigidbody.velocity - m_PreviousVelocity) / Time.fixedDeltaTime;
		m_PreviousVelocity = rigidbody.velocity;
		
		// Moving 
		if (m_CurrentPilotState.MovingForward &&
		    !m_CurrentPilotState.MovingBackward)
        {
            movementForce.z += 300.0f;
        }
		else if (m_CurrentPilotState.MovingBackward &&
		         !m_CurrentPilotState.MovingForward)
        {
            movementForce.z -= 100.0f;
        }
		
		// Strafing
		if (m_CurrentPilotState.RollLeft &&
		    !m_CurrentPilotState.RollRight)
        {
            movementForce.x -= 100.0f;
        }
		else if (m_CurrentPilotState.RollRight &&
		         !m_CurrentPilotState.RollLeft)
        {
            movementForce.x += 100.0f;
        }	
		
		// Yaw Rotation
		if (m_CurrentPilotState.MovingLeft &&
		    !m_CurrentPilotState.MovingRight)
        {
            angularForce.y -= 1.0f;
        }
		else if (m_CurrentPilotState.MovingRight &&
		         !m_CurrentPilotState.MovingLeft)
        {
            angularForce.y += 1.0f;
        }
		
		// Roll rotation
		if(m_CurrentPilotState.CurrentRotationState.x != 0.0f)
		{
			angularForce.z -= m_CurrentPilotState.CurrentRotationState.x;
		}
		
		// Pitch rotation
		if(m_CurrentPilotState.CurrentRotationState.y != 0.0f)
		{
			angularForce.x += m_CurrentPilotState.CurrentRotationState.y;
		}
		
		// Apply the movement forces
		rigidbody.AddRelativeForce(movementForce, ForceMode.Acceleration);
		
		// Apply the torque force
		rigidbody.AddRelativeTorque(angularForce, ForceMode.Acceleration);
	}
}

