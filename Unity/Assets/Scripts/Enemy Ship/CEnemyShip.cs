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
		examiningTarget,
		attackingTarget,
		scanningForHeatSignature,	// Will detect players, enemies, player ships, and enemy ships within a range relative to the intesity of their heat signatures.
		travelling,	// Happens on spawn or after idling for a while.
		any
	}

	public enum EEvent
	{
		none,

		// Internal events.
		transition_Idle,
		transition_ExamineTarget,
		transition_AttackTarget,
		transition_ScanForHeatSignature,
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

	class SDisturbance
	{
		public GameObject target;
		public Vector3 lastObservedPosition;
		public float expireTime;
		public void Set(GameObject _target, float _expireTime) { target = _target; expireTime = _expireTime; }
		public bool Invalid() { return target == null || expireTime <= Time.time; }
		public bool Valid() { return target != null && expireTime > Time.time; }
	}

	CStateTransition[] m_StateTransitionTable =
	{
		// Process.
		new CStateTransition(EState.idling,						EEvent.any,								Idle),
		new CStateTransition(EState.examiningTarget,			EEvent.any,								ExamineTarget),
		new CStateTransition(EState.attackingTarget,			EEvent.any,								AttackTarget),
		new CStateTransition(EState.scanningForHeatSignature,	EEvent.any,								ScanForHeatSignature),
		new CStateTransition(EState.travelling,					EEvent.any,								Travel),
		
		// Event.
		//new CStateTransition(EState.any,						EEvent.disturbance,				Init_TurnToFaceDisturbance),

		// Transition.
		new CStateTransition(EState.any,						EEvent.transition_Idle,					Idle),
		new CStateTransition(EState.any,						EEvent.transition_ExamineTarget,		ExamineTarget),
		new CStateTransition(EState.any,						EEvent.transition_AttackTarget,			AttackTarget),
		new CStateTransition(EState.any,						EEvent.transition_ScanForHeatSignature,	ScanForHeatSignature),
		new CStateTransition(EState.any,						EEvent.transition_Travel,				Travel),
		
		// Catch all.
		new CStateTransition(EState.any,						EEvent.any,									Idle),
	};

	// State machine data set by state machine.
	EState mState = EState.none;
	EEvent mEvent = EEvent.none;
	float mTimeout = 0.0f;
	float timeSpentIdling = 2.0f;
	float timeSpentScanningForHeatSignatures = 15.0f;
	float timeSpentExaminingTarget = 10.0f;
	float timeSpentAttackingTargetBeforeStopping = 15.0f;	// Chase for x seconds.
	float timeSpentAttackingTargetAfterStopping = 20.0f;	// Remain hostile to the target if it approaches within x seconds.
	float timeSpentTravelling = 5.0f;

	GameObject mTarget { get { return mTarget_InternalSource != null ? mVisibleTarget ? mTarget_InternalSource : mTarget_InternalLastKnownPosition : null; } set { InvalidateCache(); mTarget_InternalSource = value; } }
	GameObject mTarget_InternalSource = null;	// Will be valid for as long as something is being targeted, visible or not.
	GameObject mTarget_InternalLastKnownPosition = null;	// Always valid, but only ever referenced if there is a real target.
	bool mFaceTarget = false;
	bool mFollowTarget = false;
	float mTargetExpireTime = 0.0f;

	// State machine data set by physics.
	bool mFacingTarget = false;		// Is looking in the general direction of the target.
	bool mCloseToTarget = false;	// Is within acceptable range of the target.
	bool mVisibleTarget = false;	// Has direct line of sight to the target.
	float viewConeRadiusInDegrees = 20.0f;
	float viewConeLength = 400.0f;
	float viewSphereRadius = 200.0f;
	float desiredDistanceToTarget = 100.0f;
	float acceptableDistanceToTargetRatio = 0.2f;	// 20% deviation from desired distance to target is acceptable.
	float maxLinearAcceleration = 100000.0f;

	float mTimeBetweenLosCheck = 0.5f;
	float mTimeUntilNextLosCheck = 0.0f;

	// Physics data.
	CPidController mPidAngularAccelerationX = new CPidController(-2, 0, 0);
	CPidController mPidAngularAccelerationY = new CPidController(2, 0, 0);
	CPidController mPidAngularAccelerationZ = new CPidController(0, 0, 0);
	Vector3 mTorque;

	bool debug_Display = true;
	string debug_StateName;

	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{

	}

	void Awake()
	{
		if (viewConeLength < viewSphereRadius) Debug.LogError("CEnemyShip: View cone length must be greater than view sphere radius");
		if (viewSphereRadius < desiredDistanceToTarget + desiredDistanceToTarget * acceptableDistanceToTargetRatio) Debug.LogError("CEnemyShip: View sphere radius must be greater than desired distance to target");

		// Create the GameObject mTarget_InternalLastKnownPosition.
		mTarget_InternalLastKnownPosition = (GameObject)GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Enemy Ship/DummyTarget"));
	}

	void Start()
	{
		// Test.

	}

	void OnDestroy()
	{
		// Destroy the GameObject mTarget_InternalLastKnownPosition.
		Destroy(mTarget_InternalLastKnownPosition);
	}

	void Update()
	{
		mTimeout -= Time.deltaTime;

		if (mState == EState.attackingTarget && mTarget != null && mVisibleTarget)	// If attacking the target, and the target is visible...
		{
			mTargetExpireTime = Time.time + 20.0f;	// Reset the expire time.
			// Todo: Shoot prey. Bang bang.
		}

		if (mTargetExpireTime <= Time.time)	// If the target expire time is met...
			mTarget = null;	// Expire the target.

		ProcessStateMachine();
	}

	void FixedUpdate()
	{
		mTimeUntilNextLosCheck -= Time.fixedDeltaTime;
		if (mTimeUntilNextLosCheck <= 0.0f)
		{
			mTimeUntilNextLosCheck += mTimeBetweenLosCheck;
			FindTargetsInLineOfSight();
		}

		if (mTarget != null && (mFaceTarget || mFollowTarget))	// If told to move to and/or look at the target, and there is prey or a disturbance to target...
		{
			Vector3 targetPos = mTarget.transform.position;

			// Get the position of the target in local space of this ship (i.e. relative position).
			Vector3 absoluteDeltaPosition = targetPos - transform.position;
			float distanceToTarget = absoluteDeltaPosition.magnitude;
			Vector3 absoluteDirection = absoluteDeltaPosition.normalized;
			Vector3 relativeDirection = (Quaternion.Inverse(transform.rotation) * (targetPos - rigidbody.worldCenterOfMass)).normalized;
			Vector3 relativeRotation = new Vector3(Mathf.Atan2(relativeDirection.y, relativeDirection.z), Mathf.Atan2(relativeDirection.x, relativeDirection.z), Mathf.Atan2(relativeDirection.x, relativeDirection.y));

			mTorque.x = mPidAngularAccelerationX.GetOutput(relativeRotation.x, Time.fixedDeltaTime);
			mTorque.y = mPidAngularAccelerationY.GetOutput(relativeRotation.y, Time.fixedDeltaTime);
			mTorque.z = mPidAngularAccelerationZ.GetOutput(relativeRotation.z, Time.fixedDeltaTime);

			if(mFaceTarget)
			{
				// Rotate to face target.
				rigidbody.AddRelativeTorque(mTorque, ForceMode.Force);
			}

			if(mFollowTarget)
			{
				// Move to acceptable range.
				float distanceToDesiredDistance = distanceToTarget - desiredDistanceToTarget;
				float maxLinearAccelerationScale = Mathf.Clamp(distanceToDesiredDistance / desiredDistanceToTarget, -1.0f, 1.0f);	// From no deviation to maximum deviation from desired distance; acceleration is scaled from 0 to max.
				rigidbody.AddForce(absoluteDirection * maxLinearAcceleration * maxLinearAccelerationScale, ForceMode.Force);
			}

			// Determine if the target is in view.
			mFacingTarget = IsWithinViewCone(targetPos);	// Is looking at target if within line of sight.

			// Determine if the target is within acceptable range.
			mCloseToTarget = distanceToTarget < desiredDistanceToTarget + (desiredDistanceToTarget * acceptableDistanceToTargetRatio) && distanceToTarget > desiredDistanceToTarget - (desiredDistanceToTarget * acceptableDistanceToTargetRatio);

			// Check if there is a direct line of sight to the target.
			mVisibleTarget = IsWithinLineOfSight(mTarget);
		}
	}

	void InvalidateCache()
	{
		mFacingTarget = false;
		mCloseToTarget = false;
		mVisibleTarget = false;
	}

	bool IsWithinLineOfSight(GameObject target)
	{
		return (IsWithinViewCone(target) || IsWithinViewRadius(target)) && HasDirectLineOfSight(target);
	}

	bool IsWithinViewCone(GameObject target) { return IsWithinViewCone(target.transform.position); }
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

	bool IsWithinViewRadius(GameObject target) { return IsWithinViewRadius(target.transform.position); }
	bool IsWithinViewRadius(Vector3 pos)
	{
		return (pos - transform.position).sqrMagnitude <= viewSphereRadius * viewSphereRadius;
	}

	bool HasDirectLineOfSight(GameObject target)
	{
		Vector3 deltaPos = target.transform.position - transform.position;
		RaycastHit[] rayHits = Physics.RaycastAll(transform.position, deltaPos.normalized, deltaPos.magnitude, LayerMask.NameToLayer("Galaxy"));
		for (int i = 0; i < rayHits.Length; ++i)
		{
			if (rayHits[i].collider.gameObject != gameObject || rayHits[i].collider.gameObject != target)
				return false;
		}

		return true;
	}

	/// <summary>
	/// Checks enemy ship line of sight for something interesting, and sets mTarget to the thing to check out.
	/// </summary>
	void FindTargetsInLineOfSight()
	{
		// Set disturbance only if there is no current disturbance, or the current disturbance is old enough to expire.
		if (mTarget != null)	// If there is an existing target...
			return;	// Do not set a new target, as the current one is still valid.

		// Find all objects within short range sphere and long-range cone.
		Collider[] colliders = Physics.OverlapSphere(transform.position, viewConeLength);
		for (int i = 0; i < colliders.Length; ++i)
		{
			GameObject entity = colliders[i].gameObject;
			if(entity != gameObject && IsWithinLineOfSight(entity))	// If the entity is within view cone or view sphere (and is not this ship)...
			{
				// Check if the entity is worthy of being targeted.
				Rigidbody entityBody = entity.GetComponent<Rigidbody>();
				if (entityBody == null) continue;	// Entities without a RigidBody can not move, thus can not have their velocity checked.

				if (entityBody.velocity.magnitude > 20.0f)	// If the gubbbin is moving faster than 20 units per second...
				{
					mTarget = entity;
					mTargetExpireTime = Time.time + 15.0f;
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

	void StateInitialisation(EState _state, bool _lookAtTarget, bool _moveToTarget, float _timeout, string _stateName)
	{
		mState = _state;
		mEvent = EEvent.none;	// The event describes the state to switch to. It is always nulled after initialisation.
		mFaceTarget = _lookAtTarget;
		mFollowTarget = _moveToTarget;
		mTimeout = _timeout;

		debug_StateName = _stateName;
	}

	static bool Idle(CEnemyShip enemyShip) { return enemyShip.Idle(); }
	bool Idle()
	{
		switch (mState)
		{
			// Initialise state.
			case EState.none:
				StateInitialisation(EState.idling, false, false, timeSpentIdling, "Idling");
				return false;	// Init functions always return false.

			// Process state.
			case EState.idling:
				switch (mEvent)	// Process events.
				{
					case EEvent.none:	// Normal process.

						// Switch to checking out the target if there is one.
						if (mTarget != null)
						{
							mEvent = EEvent.transition_ExamineTarget;
							return true;
						}

						// 50/50 chance every (timeSpentIdling/2) seconds to leave the area, beginning after (timeSpentIdling) seconds.
						while (mTimeout <= 0.0f)
						{
							if (Random.Range(0, 2) == 0)
							{
								mEvent = EEvent.transition_Travel;
								return true;
							}
							else
								mTimeout += timeSpentIdling * 0.5f;
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

	static bool ExamineTarget(CEnemyShip enemyShip) { return enemyShip.ExamineTarget(); }
	bool ExamineTarget()
	{
		switch (mState)
		{
			// Initialise state.
			case EState.none:
				StateInitialisation(EState.examiningTarget, true, false, timeSpentExaminingTarget, "Examining Target");
				return false;	// Init functions always return false.

			// Process state.
			case EState.examiningTarget:
				switch (mEvent)	// Process events.
				{
					case EEvent.none:	// Normal process.

						if (mTarget == null)	// If the target has expired...
						{
							mEvent = EEvent.transition_Idle;	// Move on to other things.
							return true;
						}

						if (mFacingTarget)	// If facing the target...
						{
							if (mVisibleTarget)	// If there is a direct line of sight to the target...
							{
								mEvent = EEvent.transition_ScanForHeatSignature;	// Scan for heat signatures.
								return true;
							}
							else	// Facing the target, but target is not visible...
							{
								if (mCloseToTarget)	// If already close to the target...
								{
									mEvent = EEvent.transition_Idle;	// Ignore the target.
									return true;
								}
								else if(mFollowTarget == false)	// If not following the target...
								{
									mFollowTarget = true;	// Go to the target.
									mTimeout = 10.0f;	// 10 seconds to reach the target.
								}
								else if (mTimeout < 0.0f)	// Following the target; If it took so long to reach the target the timer timed out...
								{
									mEvent = EEvent.transition_Idle;	// Give up (can't reach the target).
									return true;
								}
							}
						}
						else if (mTimeout <= 0.0f)	// Not facing the target; If it took so long to face the target the timer timed out...
						{
							mEvent = EEvent.transition_Idle;	// Give up (can't face the target).
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

	static bool AttackTarget(CEnemyShip enemyShip) { return enemyShip.AttackTarget(); }
	bool AttackTarget()
	{
		switch (mState)
		{
			// Initialise state.
			case EState.none:
				StateInitialisation(EState.examiningTarget, true, true, timeSpentAttackingTargetBeforeStopping, "Attacking Target");
				return false;	// Init functions always return false.

			// Process state.
			case EState.examiningTarget:
				switch (mEvent)	// Process events.
				{
					case EEvent.none:	// Normal process.
						
						if (mTarget == null)	// If the target has expired...
						{
							mEvent = EEvent.transition_ScanForHeatSignature;	// Search for something to kill.
							return true;
						}

						if(mVisibleTarget)	// If the target is in sight...
						{
							// Will be blasting away at the target.
							// Todo: If this ship's health is low, it could run off.
						}
						if(mTimeout <= 0.0f)	// If the timer runs out...
						{
							if(mFollowTarget)	// If chasing the target...
							{
								mFollowTarget = false;	// Stop chasing the target
								mTimeout = timeSpentAttackingTargetAfterStopping;	// Stop chasing for x seconds.
							}
							else	// Not chasing the target...
							{
								if (mTimeout <= 0.0f)	// once the timer runs out...
								{
									mEvent = EEvent.transition_ScanForHeatSignature;	// Search for something nearby to kill.
									return true;
								}
							}
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

	static bool ScanForHeatSignature(CEnemyShip enemyShip) { return enemyShip.ScanForHeatSignature(); }
	bool ScanForHeatSignature()
	{
		switch (mState)
		{
			// Initialise state.
			case EState.none:
				StateInitialisation(EState.scanningForHeatSignature, false, false, timeSpentScanningForHeatSignatures, "Scanning For Heat Signature");
				return false;	// Init functions always return false.

			// Process state.
			case EState.scanningForHeatSignature:
				switch (mEvent)	// Process events.
				{
					case EEvent.none:	// Normal process.

						// Switch to checking out the target if there is one.
						if (mTarget != null)
						{
							mEvent = EEvent.transition_ExamineTarget;
							return true;
						}

						// Todo: Scan for heat signature.
						GameObject targetWithHeatSignature = null;	// Todo: call function that returns a player, enemy, ship, or nothing.
						if (targetWithHeatSignature != null)
						{
							mTarget = targetWithHeatSignature;
							mEvent = EEvent.transition_AttackTarget;
							return true;
						}

						if (mTimeout <= 0.0f)	// If the timer times out...
						{
							mEvent = EEvent.transition_Idle;	// Nothing found - give up.
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

	static bool Travel(CEnemyShip enemyShip) { return enemyShip.Travel(); }
	bool Travel()
	{
		switch (mState)
		{
			// Initialise state.
			case EState.none:
				StateInitialisation(EState.travelling, true, true, timeSpentTravelling, "Travelling");
				if (mTarget == null)
				{
					mTarget = mTarget_InternalLastKnownPosition;
					mTargetExpireTime = Time.time + timeSpentTravelling;
				}
				return false;	// Init functions always return false.

			// Process state.
			case EState.travelling:
				switch (mEvent)	// Process events.
				{
					case EEvent.none:	// Normal process.
						if (mTarget != mTarget_InternalLastKnownPosition)
						{
							if (mTarget == null)
								mEvent = EEvent.transition_Idle;
							else
								mEvent = EEvent.transition_ExamineTarget;
							return true;
						}

						mTarget_InternalLastKnownPosition.transform.position = gameObject.transform.position + gameObject.transform.forward * (desiredDistanceToTarget + (viewConeLength - desiredDistanceToTarget) * 0.75f);

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

	//void OnGUI()
	//{
	//    if (!debug_Display)
	//        return;

	//    float dx = 200.0f;
	//    float dy = 300.0f;

	//    GUIStyle middleRightText = new GUIStyle();
	//    middleRightText.alignment = TextAnchor.MiddleRight;
	//    middleRightText.fontStyle = FontStyle.Bold;
	//    middleRightText.normal.textColor = Color.yellow;
	//    middleRightText.fontSize = 9;

	//    GUIStyle middleLeftText = new GUIStyle();
	//    middleLeftText.alignment = TextAnchor.MiddleLeft;
	//    middleLeftText.fontStyle = FontStyle.Bold;
	//    middleLeftText.normal.textColor = Color.yellow;
	//    middleLeftText.fontSize = 9;

	//    GUI.Box(new Rect(25 + dx, 05 + dy, 200, 100), "\n\n" + debug_StateName + "\nState Timeout: " + mTimeout.ToString("F2") + "\nTarget Timeout: " + (mTargetExpireTime - Time.time).ToString("F2"));

	//    GUI.Label(new Rect(0 + dx, 05 + dy, 20, 10), "Angular Acceleration", middleRightText);
	//    GUI.Label(new Rect(0 + dx, 20 + dy, 20, 10), "Linear Acceleration", middleRightText);

	//    //mPidAngularAccelerationX.Kp = mPidAngularAccelerationY.Kp = mPidAngularAccelerationZ.Kp = GUI.HorizontalSlider(new Rect(25 + dx, 05 + dy, 200, 10), mPidAngularAccelerationX.Kp, 0, 1000);
	//    maxLinearAcceleration = GUI.HorizontalSlider(new Rect(25 + dx, 20 + dy, 200, 10), maxLinearAcceleration, 0, 100000);

	//    GUI.TextField(new Rect(235 + dx, 05 + dy, 60, 10), mPidAngularAccelerationY.Kp.ToString("F0"), middleLeftText);
	//    GUI.TextField(new Rect(235 + dx, 20 + dy, 60, 10), maxLinearAcceleration.ToString("F0"), middleLeftText);

	//    GUI.Label(new Rect(0 + dx, -8 + dy, 200, 10), name, middleLeftText);
	//}

	void OnDrawGizmos()
	{
		if (!debug_Display)
			return;

		Color oldColour = Gizmos.color;

		if(mTarget != null)
		{
			Gizmos.color = new Color(1,0,0,Mathf.Clamp01(0.1f + mTargetExpireTime * 0.5f));	// White line, fading out in the last two seconds of expiry, but still 10% visible when it finally expires.
			Gizmos.DrawLine(transform.position, mTarget.transform.position);	// Point to target.
			Gizmos.DrawSphere(mTarget.transform.position, 1.0f);
			Gizmos.color = new Color(0, 1, 0, 1);
			Gizmos.DrawSphere(mTarget_InternalLastKnownPosition.transform.position, 0.5f);
		}

		// Angular forces.
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position + transform.up, transform.position - transform.up);
		Gizmos.DrawLine(transform.position + transform.right, transform.position - transform.right);
		Gizmos.DrawLine(transform.position + transform.forward, transform.position - transform.forward);

		// All the following assume clockwise rotation for positive torque.
		// Y may be incorrect.
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(transform.position + transform.up, transform.position + transform.up + transform.forward * mTorque.x);		// X
		Gizmos.DrawLine(transform.position - transform.up, transform.position - transform.up - transform.forward * mTorque.x);		// X
		Gizmos.DrawLine(transform.position + transform.forward, transform.position + transform.forward - transform.up * mTorque.x);	// X
		Gizmos.DrawLine(transform.position - transform.forward, transform.position - transform.forward + transform.up * mTorque.x);	// X
		Gizmos.DrawLine(transform.position + transform.forward, transform.position + transform.forward + transform.right * mTorque.y);	// Y
		Gizmos.DrawLine(transform.position - transform.forward, transform.position - transform.forward - transform.right * mTorque.y);	// Y
		Gizmos.DrawLine(transform.position + transform.right, transform.position + transform.right - transform.forward * mTorque.y);	// Y
		Gizmos.DrawLine(transform.position - transform.right, transform.position - transform.right + transform.forward * mTorque.y);	// Y
		Gizmos.DrawLine(transform.position + transform.up, transform.position + transform.up + transform.right * mTorque.z);	// Z
		Gizmos.DrawLine(transform.position - transform.up, transform.position - transform.up - transform.right * mTorque.z);	// Z
		Gizmos.DrawLine(transform.position + transform.right, transform.position + transform.right - transform.up * mTorque.z);	// Z
		Gizmos.DrawLine(transform.position - transform.right, transform.position - transform.right + transform.up * mTorque.z);	// Z
	}
}
