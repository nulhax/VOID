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
		idling,		// Parked. Looks around occasionally.
		turningToFaceDisturbance,
		movingToDisturbance,	// Only if the disturbance is not in direct line of sight.
		turningToFacePrey,
		movingToPrey,
		scanningForPrey,
		travelling,	// Happens on spawn or after idling for a while.
		any
	}

	public enum EEvent
	{
		none,

		// Internal events.
		transition_Idle,
		transition_TurnToFaceDisturbance,
		transition_MoveToDisturbance,
		transition_TurnToFacePrey,
		transition_MoveToPrey,
		transition_ScanForPrey,
		transition_Travel,
		any
	}

	delegate bool StateFunction(CEnemyShip enemyShip);

	class CStateTransition
	{
		public CStateTransition(EState _state, EEvent _event, StateFunction _function) { mState = _state; mEvent = _event; mFunction = _function; }
		public EState mState;
		public EEvent mEvent;
		public StateFunction mFunction;
	}

	class CDisturbance
	{
		public Vector3 position;
		public float expireTime;
		public CDisturbance(Vector3 _position) { position = _position; expireTime = Time.time + 3.0f; }
		public bool Expired() { return expireTime <= Time.time; }
	}

	CStateTransition[] m_StateTransitionTable =
	{
		// Process.
		new CStateTransition(EState.idling,						EEvent.any,									Idle),
		new CStateTransition(EState.turningToFaceDisturbance,	EEvent.any,									TurnToFaceDisturbance),
		new CStateTransition(EState.movingToDisturbance,		EEvent.any,									MoveToDisturbance),
		new CStateTransition(EState.turningToFacePrey,			EEvent.any,									TurnToFacePrey),
		new CStateTransition(EState.movingToPrey,				EEvent.any,									MoveToPrey),
		new CStateTransition(EState.scanningForPrey,			EEvent.any,									ScanForPrey),
		new CStateTransition(EState.travelling,					EEvent.any,									Travel),
		
		// Event.
		//new CStateTransition(EState.any,						EEvent.disturbance,							Init_TurnToFaceDisturbance),

		// Transition.
		new CStateTransition(EState.any,						EEvent.transition_Idle,						Idle),
		new CStateTransition(EState.any,						EEvent.transition_TurnToFaceDisturbance,	TurnToFaceDisturbance),
		new CStateTransition(EState.any,						EEvent.transition_MoveToDisturbance,		MoveToDisturbance),
		new CStateTransition(EState.any,						EEvent.transition_TurnToFacePrey,			TurnToFacePrey),
		new CStateTransition(EState.any,						EEvent.transition_MoveToPrey,				MoveToPrey),
		new CStateTransition(EState.any,						EEvent.transition_ScanForPrey,				ScanForPrey),
		new CStateTransition(EState.any,						EEvent.transition_Travel,					Travel),
		
		// Catch all.
		//new CStateTransition(EState.any,						EEvent.any,									Idle),
	};

	// State machine data set by state machine.
	EState mState = EState.none;
	EEvent mEvent = EEvent.none;
	bool mLookAtTarget = false;
	bool mMoveToTarget = false;
	float mTimeout = 0.0f;
	// State machine data set by physics.
	Transform mPrey = null;
	CDisturbance mDisturbance;
	bool mLookingAtTarget = false;
	bool mMovedToTarget = false;
	public float viewConeRadiusInDegrees = 20.0f;
	public float viewConeLength = 400.0f;
	public float viewSphereRadius = 200.0f;
	public float desiredDistanceToTarget = 100.0f;
	public float acceptableDistanceToTargetRatio = 0.2f;	// 20% deviation from desired distance to target is acceptable.

	float mTimeBetweenLosCheck = 0.5f;
	float mTimeUntilNextLosCheck = 0.0f;

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
		//rigidbody.maxAngularVelocity = 1;
	}

	void Update()
	{
		ProcessStateMachine();

		if (mPrey != null)
		{
			// Todo: shoot prey. Bang bang.
		}
	}

	void FixedUpdate()
	{
		mTimeUntilNextLosCheck -= Time.fixedDeltaTime;
		if (mTimeUntilNextLosCheck <= 0.0f)
		{
			mTimeUntilNextLosCheck += mTimeBetweenLosCheck;
			FindDisturbancesInLineOfSight();
		}

		if ((mLookAtTarget || mMoveToTarget) && (mPrey != null || mDisturbance != null))	// If told to move to and/or look at the target, and there is prey or a disturbance to target...
		{
			Vector3 targetPos = mPrey != null ? mPrey.position : mDisturbance.position;	// The position of the target, regardless of whether it is prey or a disturbance.

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

			if(mLookAtTarget)
			{
				// Todo: Rotate to face target.
				transform.LookAt(targetPos);

				// Determine if the target is in view.
				mLookingAtTarget = IsWithinViewCone(targetPos);	// Is looking at target if within line of sight.
			}

			if(mMoveToTarget)
			{
				// Todo: Move to acceptable range.
				transform.position = (transform.position - targetPos).normalized * desiredDistanceToTarget;

				// Determine if the target is within acceptable range.
				float distanceToTarget = (targetPos - transform.position).magnitude;
				mMovedToTarget = distanceToTarget < desiredDistanceToTarget + (desiredDistanceToTarget * acceptableDistanceToTargetRatio) && distanceToTarget > desiredDistanceToTarget - (desiredDistanceToTarget * acceptableDistanceToTargetRatio);
			}
		}
	}

	/// <summary>
	/// Checks enemy ship line of sight for something interesting, and sets mDisturbance to the thing to check out.
	/// </summary>
	void FindDisturbancesInLineOfSight()
	{
		// Set disturbance only if there is no current disturbance, or the current disturbance is old enough to expire.
		if (mDisturbance != null)	// If there is an existing disturbance...
			if (mDisturbance.Expired())	// And it has not expired...
				mDisturbance = null;
			else
				return;	// Do not set a new disturbance, as the current one is still valid.

		// Find all objects within short range sphere and long-range cone.
		Collider[] colliders = Physics.OverlapSphere(transform.position, viewConeLength > viewSphereRadius ? viewConeLength : viewSphereRadius);
		for (int i = 0; i < colliders.Length; ++i)
		{
			GameObject entity = colliders[i].gameObject;
			if (entity == gameObject) continue;	// Ignore one's self.

			if(IsWithinLineOfSight(entity.transform.position))	// If the entity is within view...
			{
				// Check if the entity is a disturbance.
				// Todo: Refine this.
				Rigidbody entityBody = entity.GetComponent<Rigidbody>();
				if (entityBody == null) continue;	// Entities without a RigidBody can not move, thus can not have their velocity checked.

				if (entityBody.velocity.magnitude > 20.0f)	// If the gubbbin is moving faster than 20 units per second...
				{
					mDisturbance = new CDisturbance(entity.transform.position);
					break;
				}
			}
		}
	}

	void ProcessStateMachine()
	{
		// Process state machine.
		for (uint uiStateLoop = 0; uiStateLoop < m_StateTransitionTable.Length; ++uiStateLoop)
		{
			CStateTransition stateTransition = m_StateTransitionTable[uiStateLoop];
			if ((stateTransition.mState == mState || stateTransition.mState == EState.any) && (stateTransition.mEvent == mEvent || stateTransition.mEvent == EEvent.any))
				if (stateTransition.mFunction(this))
					uiStateLoop = uint.MaxValue;	// Loop will increment making iterator restart.
				else
					break;
		}
	}

	void StateInitialisation(EState _state, bool _lookAtTarget, bool _moveToTarget)
	{
		mState = _state;
		mEvent = EEvent.none;	// The event describes the state to switch to. It is always nulled after initialisation.
		mLookAtTarget = _lookAtTarget;
		mMoveToTarget = _moveToTarget;
	}

	static bool Idle(CEnemyShip enemyShip) { return enemyShip.Idle(); }
	bool Idle()
	{
		switch (mState)
		{
			// Initialise state.
			case EState.none:
				StateInitialisation(EState.idling, false, false);
				mTimeout = Time.time + 2.0f;	// Todo: Replace this example code.
				return false;	// Init functions always return false.

			// Process state.
			case EState.idling:
				switch (mEvent)	// Process events.
				{
					case EEvent.none:	// Normal process.

						// Switch to checking out prey or disturbances.
						if (mPrey != null || mDisturbance != null)
						{
							mEvent = mPrey != null ? EEvent.transition_TurnToFacePrey : EEvent.transition_TurnToFaceDisturbance;
							return true;
						}

						// Leave the area after a random amount of time.
						while (mTimeout < Time.time)
						{
							if (Random.Range(0, 2) == 0)
							{
								mEvent = EEvent.transition_Travel;
								return true;
							}
							else
								mTimeout += 2.0f;
						}

						return false;

					default:	// Shutdown the state. An uncaught event is the only time the process returns true.
						mState = EState.none;
						return true;	// Always return true for uncaught events.
				}

			default:	// An invalid state is set.
				Debug.LogError("CEnemyShip: State transition table describes incorrect function for a given state!");
				return false;
		}
	}

	static bool TurnToFaceDisturbance(CEnemyShip enemyShip) { return enemyShip.TurnToFaceDisturbance(); }
	bool TurnToFaceDisturbance()
	{
		switch (mState)
		{
			// Initialise state.
			case EState.none:
				StateInitialisation(EState.turningToFaceDisturbance, true, false);
				return false;	// Init functions always return false.

			// Process state.
			case EState.turningToFaceDisturbance:
				switch (mEvent)	// Process events.
				{
					case EEvent.none:	// Normal process.
						if (mLookingAtTarget)
						{
							mEvent = EEvent.transition_MoveToDisturbance;
							return true;
						}
						else if (mDisturbance == null)	// If the disturbance has expired...
						{
							mEvent = EEvent.transition_Idle;
							return true;
						}
						return false;

					default:	// Shutdown the state. An uncaught event is the only time the process returns true.
						mState = EState.none;
						return true;	// Always return true for uncaught events.
				}

			default:	// An invalid state is set.
				Debug.LogError("CEnemyShip: State transition table describes incorrect function for a given state!");
				return false;
		}
	}

	static bool MoveToDisturbance(CEnemyShip enemyShip) { return enemyShip.MoveToDisturbance(); }
	bool MoveToDisturbance()
	{
		switch (mState)
		{
			// Initialise state.
			case EState.none:
				StateInitialisation(EState.movingToDisturbance, true, true);
				return false;	// Init functions always return false.

			// Process state.
			case EState.movingToDisturbance:
				switch (mEvent)	// Process events.
				{
					case EEvent.none:	// Normal process.
						if (mMovedToTarget)
						{
							mEvent = EEvent.transition_ScanForPrey;	// Find something to shoot at.
							return true;
						}
						else if (mDisturbance == null)	// If the disturbance has expired...
						{
							mEvent = EEvent.transition_Idle;
							return true;
						}
						return false;

					default:	// Shutdown the state. An uncaught event is the only time the process returns true.
						mState = EState.none;
						return true;	// Always return true for uncaught events.
				}

			default:	// An invalid state is set.
				Debug.LogError("CEnemyShip: State transition table describes incorrect function for a given state!");
				return false;
		}
	}

	static bool TurnToFacePrey(CEnemyShip enemyShip) { return enemyShip.TurnToFacePrey(); }
	bool TurnToFacePrey()
	{
		switch (mState)
		{
			// Initialise state.
			case EState.none:
				StateInitialisation(EState.turningToFacePrey, false, false);
				return false;	// Init functions always return false.

			// Process state.
			case EState.turningToFacePrey:
				switch (mEvent)	// Process events.
				{
					case EEvent.none:	// Normal process.
						if (mLookingAtTarget)
						{
							mEvent = EEvent.transition_MoveToPrey;
							return true;
						}
						else if (mDisturbance == null)	// If the disturbance has expired...
						{
							mEvent = EEvent.transition_Idle;
							return true;
						}
						return false;

					default:	// Shutdown the state. An uncaught event is the only time the process returns true.
						mState = EState.none;
						return true;	// Always return true for uncaught events.
				}

			default:	// An invalid state is set.
				Debug.LogError("CEnemyShip: State transition table describes incorrect function for a given state!");
				return false;
		}
	}

	static bool MoveToPrey(CEnemyShip enemyShip) { return enemyShip.MoveToPrey(); }
	bool MoveToPrey()
	{
		switch (mState)
		{
			// Initialise state.
			case EState.none:
				StateInitialisation(EState.movingToPrey, false, true);
				return false;	// Init functions always return false.

			// Process state.
			case EState.movingToPrey:
				switch (mEvent)	// Process events.
				{
					case EEvent.none:	// Normal process.
						if (mLookingAtTarget)	// If prey is in sight...
						{
							// Todo: Reset expire timer.
							return false;
						}
						else	// Prey out of sight.
						{
							// Todo: If timer expires, TODO FINISH THIS SENTENCE.
							mEvent = EEvent.transition_TurnToFacePrey;
							return true;
						}

					default:	// Shutdown the state. An uncaught event is the only time the process returns true.
						mState = EState.none;
						return true;	// Always return true for uncaught events.
				}

			default:	// An invalid state is set.
				Debug.LogError("CEnemyShip: State transition table describes incorrect function for a given state!");
				return false;
		}
	}

	static bool ScanForPrey(CEnemyShip enemyShip) { return enemyShip.ScanForPrey(); }
	bool ScanForPrey()
	{
		switch (mState)
		{
			// Initialise state.
			case EState.none:
				StateInitialisation(EState.scanningForPrey, false, false);
				return false;	// Init functions always return false.

			// Process state.
			case EState.scanningForPrey:
				switch (mEvent)	// Process events.
				{
					case EEvent.none:	// Normal process.
						// Todo: Scan for prey.
						// Detection of prey (unlike detection of a disturbance) will throw an event to transition to facing prey, and eventually attacking it.
						return false;

					default:	// Shutdown the state. An uncaught event is the only time the process returns true.
						mState = EState.none;
						return true;	// Always return true for uncaught events.
				}

			default:	// An invalid state is set.
				Debug.LogError("CEnemyShip: State transition table describes incorrect function for a given state!");
				return false;
		}
	}

	static bool Travel(CEnemyShip enemyShip) { return enemyShip.Travel(); }
	bool Travel()
	{
		switch (mState)
		{
			// Initialise state.
			case EState.none:
				StateInitialisation(EState.travelling, false, false);
				return false;	// Init functions always return false.

			// Process state.
			case EState.travelling:
				switch (mEvent)	// Process events.
				{
					case EEvent.none:	// Normal process.
						// Todo: Fly forward.
						return false;

					default:	// Shutdown the state. An uncaught event is the only time the process returns true.
						mState = EState.none;
						return true;	// Always return true for uncaught events.
				}

			default:	// An invalid state is set.
				Debug.LogError("CEnemyShip: State transition table describes incorrect function for a given state!");
				return false;
		}
	}

	/// <summary>
	/// Sets state_Prey to a target if prey is found within view cone or view radius.
	/// </summary>
	/// <returns>true if prey was found (state_Prey will be non-null)</returns>
	bool FindPrey()
	{
		if (mPrey != null)
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
			if (IsWithinViewCone(prey.transform.position) || IsWithinViewRadius(prey.transform.position))
				if (mPrey == null)
					mPrey = prey.transform;
				else if ((prey.transform.position - transform.position).sqrMagnitude < (mPrey.position - transform.position).sqrMagnitude)
					mPrey = prey.transform;
		}

		return mPrey != null;
	}

	bool IsWithinLineOfSight(Vector3 pos)
	{
		return IsWithinViewCone(pos) || IsWithinViewRadius(pos);
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

	bool IsWithinViewRadius(Vector3 pos)
	{
		return (pos - transform.position).sqrMagnitude <= viewSphereRadius * viewSphereRadius;
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
