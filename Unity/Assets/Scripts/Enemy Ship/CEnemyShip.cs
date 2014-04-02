//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLASSNAME.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CNetworkView))]
public class CEnemyShip : CNetworkMonoBehaviour
{
	enum EState
	{
		none,
		attackingPrey,	// Includes moving and turning to face the prey.
		idling,	// Parked. Looks around occasionally.
		movingToDisturbance,	// Only if the disturbance is not in direct line of sight.
		scanningForPrey,	// Later feature - after going to the disturbance, the enemy ship scans the entire area, meaning the player ship has to hide behind asteroids to avoid being detected.
		travelling,	// Happens on spawn or after idling for a while.
		turningToFaceDisturbance,
		turningToFacePrey,
		any
	}

	public enum EEvent
	{
		none,

		// Internal events.
		transition_AttackPrey,
		transition_Idle,
		transition_MoveToDisturbance,
		transition_ScanForPrey,
		transition_Travel,
		transition_TurnToFaceDisturbance,
		transition_TurnToFacePrey,
		any
	}

	delegate bool StateFunction(CEnemyShip enemyShip);

	class CStateTransition
	{
		public CStateTransition(EState _state, EEvent _event, StateFunction _function) { m_State = _state; m_Event = _event; m_Function = _function; }
		public EState m_State;
		public EEvent m_Event;
		public StateFunction m_Function;
	}

	CStateTransition[] m_StateTransitionTable =
	{
		// Process.
		new CStateTransition(EState.attackingPrey,				EEvent.any,									Proc_AttackPrey),
		new CStateTransition(EState.idling,						EEvent.any,									Proc_Idle),
		new CStateTransition(EState.movingToDisturbance,		EEvent.any,									Proc_MoveToDisturbance),
		new CStateTransition(EState.scanningForPrey,			EEvent.any,									Proc_ScanForPrey),
		new CStateTransition(EState.travelling,					EEvent.any,									Proc_Travel),
		new CStateTransition(EState.turningToFaceDisturbance,	EEvent.any,									Proc_TurnToFaceDisturbance),
		new CStateTransition(EState.turningToFacePrey,			EEvent.any,									Proc_TurnToFacePrey),
		
		// Event.
		new CStateTransition(EState.any,						EEvent.disturbance,							Init_TurnToFaceDisturbance),

		// Transition.
		new CStateTransition(EState.any,						EEvent.transition_AttackPrey,				Init_AttackPrey),
		new CStateTransition(EState.any,						EEvent.transition_Idle,						Init_Idle),
		new CStateTransition(EState.any,						EEvent.transition_MoveToDisturbance,		Init_MoveToDisturbance),
		new CStateTransition(EState.any,						EEvent.transition_ScanForPrey,				Init_ScanForPrey),
		new CStateTransition(EState.any,						EEvent.transition_Travel,					Init_Travel),
		new CStateTransition(EState.any,						EEvent.transition_TurnToFaceDisturbance,	Init_TurnToFaceDisturbance),
		new CStateTransition(EState.any,						EEvent.transition_TurnToFacePrey,			Init_TurnToFacePrey),
		
		// Catch all.
		new CStateTransition(EState.any,						EEvent.any,									Init_Idle),
	};

	// State machine data set by state machine.
	EState m_State = EState.none;
	EEvent m_Event = EEvent.none;
	bool m_TargetDisturbance { get { return internal_TargetDisturbance; } set { m_LookingAtTarget = m_MovedToTarget = false; internal_TargetDisturbance = value; } }
	bool internal_TargetDisturbance = false;
	bool m_MoveToTarget = false;
	float m_Timeout = 0.0f;
	// State machine data set by physics.
	Transform m_Prey = null;
	Vector3 m_Disturbance;
	bool m_LookingAtTarget = false;
	bool m_MovedToTarget = false;
	public float viewConeRadiusInDegrees = 20.0f;
	public float detectionRadius = 200.0f;
	public float desiredDistanceToTarget = 100.0f;
	public float acceptableDistanceToTargetRatio = 0.2f;	// 20% deviation from desired distance to target is acceptable.

	// Physics data.
    //CPidController mPidAngleYaw = new CPidController(2000, 0, 0); // Correction for yaw angle to target.
    //CPidController mPidAnglePitch = new CPidController(2000, 0, 0); // Correction for pitch angle to target.
    ////CPidController mPidAngleRoll = new CPidController(2000, 0, 0); // Correction for pitch angle to target.
    //CPidController mPidVelocityYaw = new CPidController(2000, 1, 1); // Correction for yaw velocity to target.
    //CPidController mPidVelocityPitch = new CPidController(2000, 1, 1); // Correction for pitch velocity to target.
    //CPidController mPidVelocityRoll = new CPidController(2000, 1, 1); // Correction for roll velocity to target.

	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{

	}

	void Start()
	{
		rigidbody.maxAngularVelocity = 1;
	}

	void Update()
	{
		ProcessStateMachine();
	}

	void ProcessStateMachine()
	{
		// Process state machine.
		for (uint uiStateLoop = 0; uiStateLoop < m_StateTransitionTable.Length; ++uiStateLoop)
		{
			CStateTransition stateTransition = m_StateTransitionTable[uiStateLoop];
			if ((stateTransition.m_State == m_State || stateTransition.m_State == EState.any) && (stateTransition.m_Event == m_Event || stateTransition.m_Event == EEvent.any))
				if (stateTransition.m_Function(this))
					uiStateLoop = uint.MaxValue;	// Loop will increment making iterator restart.
				else
					break;
		}
	}

	void StateInitialisation(EState _state, bool _targetDisturbance, bool _moveToTarget)
	{
		m_State = _state;
		m_Event = EEvent.none;	// The event describes the state to switch to. It is always nulled after initialisation.
		m_TargetDisturbance = _targetDisturbance;
		m_MoveToTarget = _moveToTarget;
	}

	bool AttackPrey()
	{
		switch (m_State)
		{
			// Initialise state.
			case EState.none:
				StateInitialisation(EState.attackingPrey, false, true);
				return false;	// Init functions always return false.

			// Process state.
			case EState.attackingPrey:
				switch (m_Event)	// Process events.
				{
					case EEvent.none:	// Normal process.
						if (m_LookingAtTarget)	// If prey is in sight...
						{
							// Todo: Shoot at the prey.
							return false;
						}
						else	// Prey out of sight.
						{
							m_Event = EEvent.transition_TurnToFacePrey;
							return true;
						}

					default:	// Shutdown the state. An uncaught event is the only time the process returns true.
						m_State = EState.none;
						return true;	// Always return true for uncaught events.
				}

			default:	// An invalid state is set.
				Debug.LogError("CEnemyShip: State transition table describes incorrect function for a given state!");
				return false;
		}
	}

	bool Init_Idle()
	{
		if (m_State != EState.none) 

		// Initialise the state.
		/*Consume event			*/m_Event = EEvent.none;
		/*Target disturbance?	*/m_TargetDisturbance = false;
		/*Move to target?		*/m_MoveToTarget = false;
		/*Handle prey target	*/m_Prey = null;
		/*Set state				*/if (Random.Range(0, 2) == 0) m_State = EState.idling; else m_Event = EEvent.transition_Travel;	// 50/50 chance to idle or travel.

		return true;	// Init functions always return true.
	}

	bool Init_MoveToDisturbance()
	{
		if (m_State != EState.none) Debug.LogError("CEnemyShip: State should be null when initialising a new state!");

		// Initialise the state.
		/*Consume event			*/m_Event = EEvent.none;
		/*Target disturbance?	*/m_TargetDisturbance = true;
		/*Move to target?		*/m_MoveToTarget = true;
		/*Handle prey target	*/m_Prey = null;
		/*Set state				*/m_State = EState.movingToDisturbance;

		return true;	// Init functions always return true.
	}

	bool Init_ScanForPrey()
	{
		if (m_State != EState.none) Debug.LogError("CEnemyShip: State should be null when initialising a new state!");
		
		// Initialise the state.
		/*Consume event			*/m_Event = EEvent.none;
		/*Target disturbance?	*/m_TargetDisturbance = false;
		/*Move to target?		*/m_MoveToTarget = false;
		/*Handle prey target	*/m_Prey = null;
		/*Set state				*/m_State = EState.scanningForPrey;

		return true;	// Init functions always return true.
	}

	bool Init_Travel()
	{
		if (m_State != EState.none) Debug.LogError("CEnemyShip: State should be null when initialising a new state!");

		// Initialise the state.
		/*Consume event			*/m_Event = EEvent.none;
		/*Target disturbance?	*/m_TargetDisturbance = false;
		/*Move to target?		*/m_MoveToTarget = false;
		/*Handle prey target	*/m_Prey = null;
		/*Set state				*/m_State = EState.travelling;

		return true;	// Init functions always return true.
	}

	bool Init_TurnToFaceDisturbance()
	{
		if (m_State != EState.none) Debug.LogError("CEnemyShip: State should be null when initialising a new state!");

		// Initialise the state.
		/*Consume event			*/m_Event = EEvent.none;
		/*Target disturbance?	*/m_TargetDisturbance = true;
		/*Move to target?		*/m_MoveToTarget = false;
		/*Handle prey target	*/m_Prey = null;
		/*Set state				*/m_State = EState.turningToFaceDisturbance;

		return true;	// Init functions always return true.
	}

	bool Init_TurnToFacePrey()
	{
		if (m_State != EState.none) Debug.LogError("CEnemyShip: State should be null when initialising a new state!");

		// Initialise the state.
		/*Consume event			*/m_Event = EEvent.none;
		/*Target disturbance?	*/m_TargetDisturbance = false;
		/*Move to target?		*/m_MoveToTarget = false;
		/*Handle prey target	*/if (m_Prey == null) Debug.LogError("CEnemyShip: Can not face prey when there is no prey set!");
		/*Set state				*/m_State = EState.turningToFacePrey;

		return true;	// Init functions always return true.
	}

	bool Proc_AttackPrey()
	{
		

		return false;
	}

	bool Proc_Idle()
	{
		switch (m_Event)
		{
			case EEvent.none:	// Process the state.
				if (FindPrey())// Todo: Do this only periodically.
					m_Event = EEvent.transition_TurnToFacePrey;
				break;

			default:	// Shutdown the state.
				m_State = EState.none;
				return true;
		}

		return false;	// Proc functions return false unless the event is uncaught.
	}

	bool Proc_MoveToDisturbance()
	{
		switch (m_Event)
		{
			case EEvent.none:	// Process the state.
				if (m_MovedToTarget)
					m_Event = EEvent.transition_ScanForPrey;
				break;

			default:	// Shutdown the state.
				m_State = EState.none;
				return true;
		}

		return false;	// Proc functions return false unless the event is uncaught.
	}

	bool Proc_ScanForPrey()
	{
		switch (m_Event)
		{
			case EEvent.none:	// Process the state.
				// Todo: Scan for prey.
				// If prey is detected, set state_Prey, set event to transition_TurnToFacePrey, and return false.
				break;

			default:	// Shutdown the state.
				m_State = EState.none;
				return true;
		}

		return false;	// Proc functions return false unless the event is uncaught.
	}

	bool Proc_Travel()
	{
		switch (m_Event)
		{
			case EEvent.none:	// Process the state.
				break;

			default:	// Shutdown the state.
				m_State = EState.none;
				return true;
		}

		return false;	// Proc functions return false unless the event is uncaught.
	}

	bool Proc_TurnToFaceDisturbance()
	{
		switch (m_Event)
		{
			case EEvent.none:	// Process the state.
				if (m_LookingAtTarget)
					m_Event = FindPrey() ? EEvent.transition_TurnToFacePrey : EEvent.transition_MoveToDisturbance;
				break;

			default:	// Shutdown the state.
				m_State = EState.none;
				return true;
		}

		return false;	// Proc functions return false unless the event is uncaught.
	}

	bool Proc_TurnToFacePrey()
	{
		switch (m_Event)
		{
			case EEvent.none:	// Process the state.
				if (m_LookingAtTarget)	// Todo: Raycast check if there is a line of sight to the prey.
					m_Event = EEvent.transition_AttackPrey;
				break;

			default:	// Shutdown the state.
				m_State = EState.none;
				return true;
		}

		return false;	// Proc functions return false unless the event is uncaught.
	}

	/// <summary>
	/// Sets state_Prey to a target if prey is found within view cone or view radius.
	/// </summary>
	/// <returns>true if prey was found (state_Prey will be non-null)</returns>
	bool FindPrey()
	{
		if (m_Prey != null)
		{
			Debug.LogError("CEnemyShip: Should not be scanning for prey when prey has already been found!");
			return true;
		}

		GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
		System.Collections.Generic.List<GameObject> potentialPrey = new System.Collections.Generic.List<GameObject>();
		foreach (GameObject obj in allObjects)
			if ((obj.layer & 11) != 0)	// If on the galaxy layer...
				potentialPrey.Add(obj);

		foreach (GameObject prey in potentialPrey)
		{
			if (IsWithinViewCone(prey.transform.position) || IsWithinDetectionRadius(prey.transform.position))
				if (m_Prey == null)
					m_Prey = prey.transform;
				else if ((prey.transform.position - transform.position).sqrMagnitude < (m_Prey.position - transform.position).sqrMagnitude)
					m_Prey = prey.transform;
		}

		return m_Prey != null;
	}

	bool IsWithinViewCone(Vector3 pos)
	{
		Vector3 deltaPos = pos - transform.position;
		if (deltaPos == Vector3.zero)
			return true;
		else
		{
			float degreesToTarget = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(deltaPos));
			return degreesToTarget < viewConeRadiusInDegrees;	// Is looking at target if within view cone.
		}
	}

	bool IsWithinDetectionRadius(Vector3 pos)
	{
		return (pos - transform.position).sqrMagnitude <= detectionRadius * detectionRadius;
	}

    void FixedUpdate()
    {
		if (m_TargetDisturbance || (m_TargetDisturbance == false && m_Prey != null))	// If targeting anything...
		{
			Vector3 targetPos = m_TargetDisturbance ? m_Disturbance : m_Prey.position;

			transform.LookAt(targetPos);

			if (m_MoveToTarget)
				transform.position = (transform.position - targetPos).normalized * desiredDistanceToTarget;

			//// Get the position of the target in local space of this ship (i.e. relative position).
			//Vector3 targetPosition = (Quaternion.Inverse(transform.rotation) * (target.position - (transform.position + rigidbody.centerOfMass))).normalized;
			//float deltaYaw = Mathf.Atan2(targetPosition.x, targetPosition.z) * ();
			//float deltaPitch = Mathf.Atan2(targetPosition.y, targetPosition.z);
			////float deltaRoll = Mathf.Atan2(targetPosition.y, targetPosition.x);

			//float torqueYaw = mPidAngleYaw.GetOutput(deltaYaw, Time.fixedDeltaTime);
			//float torquePitch = mPidAnglePitch.GetOutput(deltaPitch, Time.fixedDeltaTime);
			////float torqueRoll = mPidAngleRoll.GetOutput(deltaRoll, Time.fixedDeltaTime);
			//float velocityYaw = mPidVelocityYaw.GetOutput(-rigidbody.angularVelocity.y, Time.fixedDeltaTime);
			//float velocityPitch = mPidVelocityPitch.GetOutput(rigidbody.angularVelocity.x, Time.fixedDeltaTime);
			//float velocityRoll = mPidVelocityRoll.GetOutput(rigidbody.angularVelocity.z, Time.fixedDeltaTime);
			//Debug.Log(velocityRoll.ToString());
			//rigidbody.AddTorque(transform.up * (torqueYaw + velocityYaw));
			//rigidbody.AddTorque(-transform.right * (torquePitch + velocityPitch));
			//rigidbody.AddTorque(-transform.forward * (/*torqueRoll + */velocityRoll));

			//targetPossss = targetPosition;
			//torque = Vector3.up * (torqueYaw + velocityYaw);
			//torque += Vector3.right * (torquePitch + velocityPitch);
			//torque += Vector3.forward * (/*torqueRoll + */velocityRoll);

			// Set state information saying if the disturbance/prey is being looked at and/or is within proximity.
			m_LookingAtTarget = IsWithinViewCone(targetPos);	// Is looking at target if within view cone.

			float distanceToTarget = (targetPos - transform.position).magnitude;
			m_MovedToTarget = distanceToTarget < desiredDistanceToTarget + (desiredDistanceToTarget * acceptableDistanceToTargetRatio) && distanceToTarget > desiredDistanceToTarget - (desiredDistanceToTarget * acceptableDistanceToTargetRatio);
		}
		else
			m_LookingAtTarget = false;
    }

    //void OnGUI()
    //{
    //    if (target == null)
    //        return;

    //    float dx = 200.0f;
    //    float dy = 300.0f;

    //    GUI.Box(new Rect(25 + dx, 5 + dy, 200, 40), "");

    //    mPidAngleYaw.Kp = GUI.HorizontalSlider(new Rect(25 + dx, 5 + dy, 200, 10), mPidAngleYaw.Kp, 50, 0);
    //    mPidAngleYaw.Ki = GUI.HorizontalSlider(new Rect(25 + dx, 20 + dy, 200, 10), mPidAngleYaw.Ki, 100, 0);
    //    mPidAngleYaw.Kd = GUI.HorizontalSlider(new Rect(25 + dx, 35 + dy, 200, 10), mPidAngleYaw.Kd, 1, 0);

    //    GUIStyle style1 = new GUIStyle();
    //    style1.alignment = TextAnchor.MiddleRight;
    //    style1.fontStyle = FontStyle.Bold;
    //    style1.normal.textColor = Color.yellow;
    //    style1.fontSize = 9;

    //    GUI.Label(new Rect(0 + dx, 5 + dy, 20, 10), "Kp", style1);
    //    GUI.Label(new Rect(0 + dx, 20 + dy, 20, 10), "Ki", style1);
    //    GUI.Label(new Rect(0 + dx, 35 + dy, 20, 10), "Kd", style1);

    //    GUIStyle style2 = new GUIStyle();
    //    style2.alignment = TextAnchor.MiddleLeft;
    //    style2.fontStyle = FontStyle.Bold;
    //    style2.normal.textColor = Color.yellow;
    //    style2.fontSize = 9;

    //    GUI.TextField(new Rect(235 + dx, 5 + dy, 60, 10), mPidAngleYaw.Kp.ToString(), style2);
    //    GUI.TextField(new Rect(235 + dx, 20 + dy, 60, 10), mPidAngleYaw.Ki.ToString(), style2);
    //    GUI.TextField(new Rect(235 + dx, 35 + dy, 60, 10), mPidAngleYaw.Kd.ToString(), style2);

    //    GUI.Label(new Rect(0 + dx, -8 + dy, 200, 10), name, style2);
    //}

    //void OnDrawGizmos()
    //{
    //    if (target == null)
    //        return;

    //    Debug.DrawLine(transform.position, transform.position + transform.rotation * targetPossss * (target.position - transform.position).magnitude, Color.white);
    //    Debug.DrawLine(transform.position, transform.position + transform.forward * 100, Color.green);
    //    Debug.DrawLine(transform.position + transform.forward * 100, transform.position + transform.forward * 100 + transform.up * torque.x, Color.red);
    //    Debug.DrawLine(transform.position + transform.forward * 100, transform.position + transform.forward * 100 + transform.right * torque.y, Color.red);
    //}
}
