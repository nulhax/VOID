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
	public enum EState
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
		HostileTarget,

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
		new CStateTransition(EState.none,						EEvent.HostileTarget,					AttackTarget),

		// Transition.
		new CStateTransition(EState.none,						EEvent.transition_Idle,					Idle),
		new CStateTransition(EState.none,						EEvent.transition_ExamineTarget,		ExamineTarget),
		new CStateTransition(EState.none,						EEvent.transition_AttackTarget,			AttackTarget),
		new CStateTransition(EState.none,						EEvent.transition_ScanForHeatSignature,	ScanForHeatSignature),
		new CStateTransition(EState.none,						EEvent.transition_Travel,				Travel),
		
		// Catch all.
		new CStateTransition(EState.any,						EEvent.any,								Idle),
	};

	// State machine data set by state machine.
	public EState mState = EState.none;
	public EEvent mEvent = EEvent.none;
	public float mTimeout = 0.0f;
	public float mTimeoutSecondary = 0.0f;
	public float mTimeSpentIdling = 2.0f;
	public float mTimeSpentScanningForHeatSignatures = 15.0f;
	public float mTimeSpentExaminingTarget = 10.0f;
	public float mTimeSpentAttackingTargetBeforeStopping = 15.0f;	// Chase for x seconds.
	public float mTimeSpentAttackingTargetAfterStopping = 20.0f;	// Remain hostile to the target if it approaches within x seconds.
	public float mTimeSpentTravelling = 5.0f;
	public float mFireRate = 1.5f;
	public float mMinVelocityOfSuspiciousGubbin = 0.0f;
	public float mTimeUntilSuspiciousGubbinExpires = 15.0f;

	public Rigidbody mTarget { get { return mTarget_InternalSource != null ? mVisibleTarget ? mTarget_InternalSource : mTarget_InternalLastKnownPosition : null; } set { InvalidateCache(); mTarget_InternalSource = value; } }
	public Rigidbody mTarget_InternalSource = null;	// Will be valid for as long as something is being targeted, visible or not.
	public Rigidbody mTarget_InternalLastKnownPosition = null;	// Always valid, but only ever referenced if there is a real target.
	public bool mFaceTarget = false;
	public bool mFollowTarget = false;
	public float mTargetExpireTime = 0.0f;

	public int mAudioWeaponFireID = -1;

	// State machine data set by physics.
	public bool mFacingTarget = false;		// Is looking in the general direction of the target.
	public bool mCloseToTarget = false;	// Is within acceptable range of the target.
	public bool mVisibleTarget = false;	// Has direct line of sight to the target.
	public float viewConeRadiusInDegrees = 30.0f;
	public float viewConeLength_Extension = 1000.0f;
	public float viewConeLength { get { return mBoundingRadius + viewConeLength_Extension; } }
	public float viewSphereRadius_Extension = 300.0f;
	public float viewSphereRadius { get { return mBoundingRadius + viewSphereRadius_Extension; } }
	public float desiredDistanceToTarget_Extension = 100.0f;
	public float desiredDistanceToTarget { get { return mBoundingRadius + desiredDistanceToTarget_Extension; } }
	public float minAcceptableDistanceToTarget { get { return desiredDistanceToTarget - desiredDistanceToTarget_Extension * acceptableDistanceToTargetRatio; } }
	public float maxAcceptableDistanceToTarget { get { return desiredDistanceToTarget + desiredDistanceToTarget_Extension * acceptableDistanceToTargetRatio; } }
	public float acceptableDistanceToTargetRatio = 0.2f;	// 20% deviation from desired distance to target is acceptable.
	public const float maxLinearAcceleration = 100000.0f;
	public const float maxAngularAcceleration = 100000000.0f;
	public uint mNumWhiskers = 16;

	public float mTimeBetweenLosCheck = 0.5f;
	public float mTimeBetweenWhiskerCheck = 0.5f;
	public float mTimeUntilNextLosCheck = 0.5f;
	public float mTimeUntilNextWhiskerCheck = 0.0f;

	// Physics data.
	public float mBoundingRadius = 0.0f;	// Set at rumtime.
	public Vector3 mTorque;
	public Vector3 mRepulsionForce;

	public bool debug_Display = false;
	public string debug_StateName;

	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{

	}

	void Awake()
	{
		if (viewConeLength < viewSphereRadius) Debug.LogError("CEnemyShip: View cone length must be greater than view sphere radius");
		if (viewSphereRadius < maxAcceptableDistanceToTarget) Debug.LogError("CEnemyShip: View sphere radius must be greater than desired distance to target");

		mTarget_InternalLastKnownPosition = ((GameObject)GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/EnemyShips/DummyTarget"))).rigidbody;	// Create the RigidBody mTarget_InternalLastKnownPosition.

		mAudioWeaponFireID = GetComponent<CAudioCue>().AddSound("Audio/BulletFire", 0.0f, 0.0f, false);
	}

	void Start()
	{
		mBoundingRadius = CUtility.GetBoundingRadius(gameObject);
	}

	void OnDestroy()
	{
		Destroy(mTarget_InternalLastKnownPosition);	// Destroy the GameObject mTarget_InternalLastKnownPosition.
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.KeypadDivide))
			debug_Display = !debug_Display;

		mTimeout -= Time.deltaTime;

		if (mTargetExpireTime <= Time.time)	// If the target expire time is met...
			mTarget = null;	// Expire the target.

		ProcessStateMachine();
	}

	void FixedUpdate()
	{
		ProcessWhiskers();

		mTimeUntilNextLosCheck -= Time.fixedDeltaTime;
		if (mTimeUntilNextLosCheck <= 0.0f)
		{
			mTimeUntilNextLosCheck += mTimeBetweenLosCheck;
			FindTargetsInLineOfSight();
		}

		if (mTarget != null && (mFaceTarget || mFollowTarget))	// If told to move to and/or look at the target, and there is prey or a disturbance to target...
		{
			Vector3 targetPos = mTarget.rigidbody.worldCenterOfMass;

			// Get the location of the target in local space of this ship (i.e. relative location).
			Vector3 absoluteDeltaPosition = targetPos - rigidbody.worldCenterOfMass;
			float distanceToTarget = absoluteDeltaPosition.magnitude;
			Vector3 absoluteDirection = absoluteDeltaPosition.normalized;
			Vector3 relativeDirection = (Quaternion.Inverse(transform.rotation) * (targetPos - rigidbody.worldCenterOfMass)).normalized;
			Vector3 relativeRotation = new Vector3(Mathf.Atan2(relativeDirection.y, relativeDirection.z), Mathf.Atan2(relativeDirection.x, relativeDirection.z), Mathf.Atan2(relativeDirection.x, relativeDirection.y));

			mTorque.x = relativeRotation.x * -maxAngularAcceleration / Mathf.PI;
			mTorque.y = relativeRotation.y * +maxAngularAcceleration / Mathf.PI;
			mTorque.z = relativeRotation.z * 0.0f;

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

			// Determine if the target is within range.
			mCloseToTarget = distanceToTarget < maxAcceptableDistanceToTarget;

			// Check if there is a direct line of sight to the target.
			mVisibleTarget = IsWithinLineOfSight(mTarget);
			if(mVisibleTarget)
				mTarget_InternalLastKnownPosition.transform.position = mTarget.rigidbody.worldCenterOfMass;
		}
	}

	void ProcessWhiskers()
	{
		for (mTimeUntilNextWhiskerCheck -= Time.fixedDeltaTime; mTimeUntilNextWhiskerCheck <= 0.0f; mTimeUntilNextWhiskerCheck = mTimeBetweenWhiskerCheck)
		{
			float whiskerLength = minAcceptableDistanceToTarget;
			mRepulsionForce = Vector3.zero;
			float increment = Mathf.PI * (3.0f - Mathf.Sqrt(5));
			float offset = 2.0f / mNumWhiskers;
			for (uint ui = 0; ui < mNumWhiskers; ++ui)
			{
				float y = ui * offset - 1.0f + (offset / 2.0f);
				float radians = Mathf.Sqrt(1.0f - y * y);
				float phi = ui * increment;
				Vector3 direction = new Vector3(Mathf.Cos(phi) * radians, y, Mathf.Sin(phi) * radians);
				RaycastHit[] rayHits = Physics.RaycastAll(rigidbody.worldCenterOfMass, direction, whiskerLength, 1 << LayerMask.NameToLayer("Galaxy"));
				for (int i = 0; i < rayHits.Length; ++i )
				{
					RaycastHit rayHit = rayHits[i];

					if (rayHit.collider.isTrigger)	// Ignore triggers.
						continue;

					mRepulsionForce += -direction * (whiskerLength - rayHit.distance) * maxLinearAcceleration * 0.25f / mNumWhiskers;
				}
			}

			float repulsionForceLength = Mathf.Min(mRepulsionForce.magnitude, whiskerLength);
			mRepulsionForce = mRepulsionForce.normalized * maxLinearAcceleration * repulsionForceLength / whiskerLength;
		}

		rigidbody.AddForce(mRepulsionForce, ForceMode.Force);
	}

	void OnCollisionEnter(Collision collision)
	{
		mEvent = EEvent.HostileTarget;
		Rigidbody targetRigidbody = collision.gameObject.rigidbody;
		if (targetRigidbody == null)
			targetRigidbody = CUtility.FindInParents<Rigidbody>(collision.gameObject);

		if(targetRigidbody != null)
			mTarget = targetRigidbody;
	}

	void InvalidateCache()
	{
		mFacingTarget = false;
		mCloseToTarget = false;
		mVisibleTarget = false;
	}

	bool IsWithinLineOfSight(Rigidbody target)
	{
		return (IsWithinViewCone(target) || IsWithinViewRadius(target)) && HasDirectLineOfSight(target);
	}

	bool IsWithinViewCone(Rigidbody target) { return IsWithinViewCone(target.worldCenterOfMass); }
	bool IsWithinViewCone(Vector3 pos)
	{
		Vector3 deltaPos = pos - rigidbody.worldCenterOfMass;
		if (deltaPos == Vector3.zero)
			return true;
		else
		{
			float degreesToTarget = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(deltaPos));
			return degreesToTarget < viewConeRadiusInDegrees;	// Is looking at target if within view cone.
		}
	}

	bool IsWithinViewRadius(Rigidbody target) { return IsWithinViewRadius(target.worldCenterOfMass); }
	bool IsWithinViewRadius(Vector3 pos)
	{
		return (pos - rigidbody.worldCenterOfMass).sqrMagnitude <= viewSphereRadius * viewSphereRadius;
	}

	bool HasDirectLineOfSight(Rigidbody target)
	{
		Vector3 deltaPos = target.worldCenterOfMass - rigidbody.worldCenterOfMass;
		RaycastHit[] rayHits = Physics.RaycastAll(rigidbody.worldCenterOfMass, deltaPos.normalized, deltaPos.magnitude, 1 << LayerMask.NameToLayer("Galaxy"));
		for (int i = 0; i < rayHits.Length; ++i)	// For each object the ray hit...
		{
			RaycastHit rayHit = rayHits[i];

			if (rayHit.collider.isTrigger)	// Ignore triggers.
				continue;

			bool isAnIgnoredObject = false;
			for (Transform collidedObjectTransform = rayHit.collider.transform; collidedObjectTransform != null; collidedObjectTransform = collidedObjectTransform.parent)	// For the object, and each of its parents in the hierarchy...
			{
				Rigidbody collidedObjectRigidbody = collidedObjectTransform.rigidbody != null ? collidedObjectTransform.rigidbody : CUtility.FindInParents<Rigidbody>(collidedObjectTransform);	// Shortcut to the object, since iteration uses its transform.
				if (collidedObjectRigidbody == rigidbody || collidedObjectRigidbody == target)	// If the object in the ray is this enemy ship, the target, or part of either...
				{
					isAnIgnoredObject = true;	// Ignore it, thus continuing the search.
					break;	// Stop searching the hierarchy.
				}
			}

			if (!isAnIgnoredObject)	// If the object was not one of the ignored objects...
			{
				Debug.Log(rayHit.collider.gameObject.name + " was in between " + gameObject.name + " and " + target.name);
				return false;	// There was something between the ship casting the ray and the object being raycasted to, thus there is no line of sight.
			}
		}

		return true;	// There are no objects between the enemy ship and the target.
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
		Collider[] colliders = Physics.OverlapSphere(rigidbody.worldCenterOfMass, viewConeLength);
		for (int i = 0; i < colliders.Length; ++i)
		{
			Rigidbody entityRigidbody = colliders[i].rigidbody != null ? colliders[i].rigidbody : CUtility.FindInParents<Rigidbody>(colliders[i].gameObject);
			if (entityRigidbody == null || entityRigidbody == rigidbody)	// If the object has no rigidbody, or it is this enemy ship...
				continue;	// Ignore this object and look for others.

			if (IsWithinLineOfSight(entityRigidbody))	// If the entity is within view cone or view sphere (and is not this ship)...
			{
				// Check if the entity is worthy of being targeted.
				if (entityRigidbody.velocity.magnitude >= mMinVelocityOfSuspiciousGubbin)	// If the gubbbin is moving faster than x units per second...
				{
					mTarget = entityRigidbody;
					mTargetExpireTime = Time.time + mTimeUntilSuspiciousGubbinExpires;
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
		mTimeoutSecondary = 0.0f;

		debug_StateName = _stateName;
	}

	static bool Idle(CEnemyShip enemyShip) { return enemyShip.Idle(); }
	bool Idle()
	{
		switch (mState)
		{
			// Initialise state.
			case EState.none:
				StateInitialisation(EState.idling, false, false, mTimeSpentIdling, "Idling");
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
								mTimeout += mTimeSpentIdling * 0.5f;
						}

						return false;

					default:	// Shutdown the state. An uncaught event is the only time the process returns true.
						mState = EState.none;
						return true;	// Always return true for uncaught events.
				}
		}

		return false;
	}

	static bool ExamineTarget(CEnemyShip enemyShip) { return enemyShip.ExamineTarget(); }
	bool ExamineTarget()
	{
		switch (mState)
		{
			// Initialise state.
			case EState.none:
				StateInitialisation(EState.examiningTarget, true, false, mTimeSpentExaminingTarget, "Examining Target");
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
		}

		return false;
	}

	static bool AttackTarget(CEnemyShip enemyShip) { return enemyShip.AttackTarget(); }
	bool AttackTarget()
	{
		switch (mState)
		{
			// Initialise state.
			case EState.none:
				StateInitialisation(EState.attackingTarget, true, true, mTimeSpentAttackingTargetBeforeStopping, "Attacking Target");
				return false;	// Init functions always return false.

			// Process state.
			case EState.attackingTarget:
				switch (mEvent)	// Process events.
				{
					case EEvent.none:	// Normal process.
						
						if (mTarget == null)	// If the target has expired...
						{
							mEvent = EEvent.transition_ScanForHeatSignature;	// Search for something to kill.
							return true;
						}

						mTimeoutSecondary -= Time.deltaTime;	// Time between firing bullets.

						if(mVisibleTarget)	// If the target is in sight...
						{
							mTargetExpireTime = Time.time + 20.0f;	// Reset the expire time.

							while (mTimeoutSecondary <= 0.0f)
							{
								mTimeoutSecondary += mFireRate != 0.0f ? 1.0f / mFireRate : float.PositiveInfinity;

								// Todo: Shoot prey. Bang bang.
								GetComponent<CAudioCue>().Play(transform, 1.0f, false, mAudioWeaponFireID);
							}

							// Todo: If this ship's health is low, it could run off.
						}
						else	// Target can not be seen.
						{
							// Ensure fire rate does not build up a backlog of shots to fire.
							if (mTimeoutSecondary < 0.0f)
								mTimeoutSecondary = 0.0f;
						}

						if(mTimeout <= 0.0f)	// If the timer runs out...
						{
							if(mFollowTarget)	// If chasing the target...
							{
								mFollowTarget = false;	// Stop chasing the target
								mTimeout = mTimeSpentAttackingTargetAfterStopping;	// Stop chasing for x seconds.
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
		}

		return false;
	}

	static bool ScanForHeatSignature(CEnemyShip enemyShip) { return enemyShip.ScanForHeatSignature(); }
	bool ScanForHeatSignature()
	{
		switch (mState)
		{
			// Initialise state.
			case EState.none:
				StateInitialisation(EState.scanningForHeatSignature, false, false, mTimeSpentScanningForHeatSignatures, "Scanning For Heat Signature");
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
						Rigidbody targetWithHeatSignature = null;	// Todo: call function that returns a player, enemy, ship, or nothing.
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
		}

		return false;
	}

	static bool Travel(CEnemyShip enemyShip) { return enemyShip.Travel(); }
	bool Travel()
	{
		switch (mState)
		{
			// Initialise state.
			case EState.none:
				if (mTarget == null)
				{
					StateInitialisation(EState.travelling, true, true, mTimeSpentTravelling, "Travelling");

					mTarget = mTarget_InternalLastKnownPosition;
					mTarget_InternalLastKnownPosition.transform.position = rigidbody.worldCenterOfMass + gameObject.transform.forward * (desiredDistanceToTarget + (viewConeLength - desiredDistanceToTarget) * 0.75f);
					mTarget_InternalLastKnownPosition.transform.parent = gameObject.transform;

					mTargetExpireTime = Time.time + mTimeSpentTravelling;
					return false;	// Init functions always return false.
				}
				else
				{
					mEvent = EEvent.transition_ExamineTarget;
					return true;	// There is a valid target.
				}

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

						return false;

					default:	// Shutdown the state. An uncaught event is the only time the process returns true.
						mTarget_InternalLastKnownPosition.transform.parent = null;
						mState = EState.none;
						return true;	// Always return true for uncaught events.
				}
		}

		return false;
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
			Gizmos.DrawLine(rigidbody.worldCenterOfMass, mTarget.worldCenterOfMass);	// Point to target.
			Gizmos.DrawSphere(mTarget.worldCenterOfMass, 1.0f);
			Gizmos.color = new Color(0, 1, 0, 1);
			Gizmos.DrawSphere(mTarget_InternalLastKnownPosition.worldCenterOfMass, 50.0f);
		}

		// Angular forces.
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(rigidbody.worldCenterOfMass + transform.up, rigidbody.worldCenterOfMass - transform.up);
		Gizmos.DrawLine(rigidbody.worldCenterOfMass + transform.right, rigidbody.worldCenterOfMass - transform.right);
		Gizmos.DrawLine(rigidbody.worldCenterOfMass + transform.forward, rigidbody.worldCenterOfMass - transform.forward);

		Gizmos.color = Color.cyan;
		Vector3 torqueScaled = mTorque / maxAngularAcceleration;
		Gizmos.DrawLine(rigidbody.worldCenterOfMass + transform.up, rigidbody.worldCenterOfMass + transform.up + transform.forward * torqueScaled.x);		// X
		Gizmos.DrawLine(rigidbody.worldCenterOfMass - transform.up, rigidbody.worldCenterOfMass - transform.up - transform.forward * torqueScaled.x);		// X
		Gizmos.DrawLine(rigidbody.worldCenterOfMass + transform.forward, rigidbody.worldCenterOfMass + transform.forward - transform.up * torqueScaled.x);	// X
		Gizmos.DrawLine(rigidbody.worldCenterOfMass - transform.forward, rigidbody.worldCenterOfMass - transform.forward + transform.up * torqueScaled.x);	// X
		Gizmos.DrawLine(rigidbody.worldCenterOfMass + transform.forward, rigidbody.worldCenterOfMass + transform.forward + transform.right * torqueScaled.y);	// Y
		Gizmos.DrawLine(rigidbody.worldCenterOfMass - transform.forward, rigidbody.worldCenterOfMass - transform.forward - transform.right * torqueScaled.y);	// Y
		Gizmos.DrawLine(rigidbody.worldCenterOfMass + transform.right, rigidbody.worldCenterOfMass + transform.right - transform.forward * torqueScaled.y);	// Y
		Gizmos.DrawLine(rigidbody.worldCenterOfMass - transform.right, rigidbody.worldCenterOfMass - transform.right + transform.forward * torqueScaled.y);	// Y
		Gizmos.DrawLine(rigidbody.worldCenterOfMass + transform.up, rigidbody.worldCenterOfMass + transform.up + transform.right * torqueScaled.z);	// Z
		Gizmos.DrawLine(rigidbody.worldCenterOfMass - transform.up, rigidbody.worldCenterOfMass - transform.up - transform.right * torqueScaled.z);	// Z
		Gizmos.DrawLine(rigidbody.worldCenterOfMass + transform.right, rigidbody.worldCenterOfMass + transform.right - transform.up * torqueScaled.z);	// Z
		Gizmos.DrawLine(rigidbody.worldCenterOfMass - transform.right, rigidbody.worldCenterOfMass - transform.right + transform.up * torqueScaled.z);	// Z

		// Field of view.
		Gizmos.color = new Color(0, 0.5f, 0.5f, 0.25f);
		Gizmos.DrawSphere(rigidbody.worldCenterOfMass, mBoundingRadius);
		Gizmos.DrawSphere(rigidbody.worldCenterOfMass, viewSphereRadius);
		Matrix4x4 oldMatrix = Gizmos.matrix;
		Gizmos.matrix = Matrix4x4.TRS(rigidbody.worldCenterOfMass, transform.rotation, Vector3.one);
		Gizmos.DrawFrustum(Vector3.zero, viewConeRadiusInDegrees, viewConeLength, 1.0f, 1.0f);
		Gizmos.matrix = oldMatrix;
	}
}
