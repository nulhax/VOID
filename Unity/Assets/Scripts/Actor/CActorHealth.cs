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
[System.Serializable]
public class CActorHealth : CNetworkMonoBehaviour
{
	public delegate void OnSetHealth(float prevHealth, float currHealth);
	public event OnSetHealth EventOnSetHealth;

	public delegate void OnSetState(byte prevState, byte currState);
	public event OnSetState EventOnSetState;

	[SerializeField] public bool flammable = true;
	[SerializeField] public bool callEventsOnStart = false;
	[SerializeField] public bool syncNetworkHealth = true;
	[SerializeField] public bool destroyOnZeroHealth = false;
	[SerializeField] public bool takeDamageOnImpact = false;
	[SerializeField] public bool syncNetworkState = true;

	[SerializeField] public float health_max = 100.0f;
	[SerializeField] public float health_min = 0.0f;
	[SerializeField] public float health_initial = 100.0f;
	[HideInInspector] public float health_previous;
	private float health_current;
	protected CNetworkVar<float> health_internal = null;
	public float health { get { return health_current; } set { value = value > health_max ? health_max : value < health_min ? health_min : value; if (value == health) return; if (syncNetworkHealth)health_internal.Set(value); else { health_current = value; OnSyncHealth(null); } } }

	[SerializeField] public byte state_initial = 0;
	[SerializeField] public float[] stateTransitions;
	[HideInInspector] public byte state_previous;
	private byte state_current;
	protected CNetworkVar<byte> state_internal = null;
	public byte state { get { return state_current; } set { if (syncNetworkState)state_internal.Set(value); else { state_current = value; OnSyncState(null); } } }

	[SerializeField] public float timeBetweenNetworkSyncs = 0.1f;
	private float timeUntilNextNetworkSync = 0.0f;

	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		health_internal = _cRegistrar.CreateReliableNetworkVar<float>(OnSyncHealth, health_initial);
		state_internal = _cRegistrar.CreateReliableNetworkVar<byte>(OnSyncState, state_initial);
	}

	void Awake()
	{
		health_previous = health_current = health_initial;
		state_previous = state_current = state_initial;
	}

	void Start()
	{
		if (callEventsOnStart)
		{
			if (EventOnSetHealth != null)
				EventOnSetHealth(health_previous, health_current);

			if (EventOnSetState != null)
				EventOnSetState(state_previous, state_current);
		}
	}

	void Update()
	{
		if (CNetwork.IsServer)
		{
			timeUntilNextNetworkSync -= Time.deltaTime;
			if (timeUntilNextNetworkSync <= 0.0f && (syncNetworkHealth || syncNetworkState))
			{
				if (syncNetworkHealth && health_current != health_internal.Get())
					health_internal.Set(health_current);

				if (syncNetworkState && state_current != state_internal.Get())
					state_internal.Set(state_current);

				timeUntilNextNetworkSync = timeBetweenNetworkSyncs;
			}
		}
	}

	void OnSyncHealth(INetworkVar sender)
	{
		if (syncNetworkHealth)
			health_current = health_internal.Get();

		if (stateTransitions != null)
		{
			byte currentState = (byte)stateTransitions.Length;

			for (int i = 0; i < stateTransitions.Length; ++i)
			{
				if (health_current < stateTransitions[i])
				{
					currentState = (byte)i;
					break;
				}
			}

			if (currentState != state)
				state = currentState;
		}

		if (EventOnSetHealth != null && health_current != health_previous)
			EventOnSetHealth(health_previous, health_current);

		health_previous = health_current;

		if (health <= 0.0f && destroyOnZeroHealth)
		{
			CNetwork.Factory.DestoryObject(gameObject.GetComponent<CNetworkView>().ViewId);
			destroyOnZeroHealth = false;    // To be totes sure destroy doesn't get called again.
		}
	}

	void OnSyncState(INetworkVar sender)
	{
		if (syncNetworkState)
			state_current = state_internal.Get();

		if (EventOnSetState != null && state_current != state_previous)
			EventOnSetState(state_previous, state_current);

		state_previous = state_current;
	}

	void OnCollisionEnter(Collision collision)
	{
		if (CNetwork.IsServer && takeDamageOnImpact)
		{
			float colliderMass = CGalaxy.GetMass(collision.gameObject);
			float impulse = (colliderMass * collision.relativeVelocity / (rigidbody.mass + colliderMass)).magnitude;

			//Debug.Log("CActorHealth: " + gameObject.name + " (" + GetComponent<CNetworkView>().ViewId.ToString() + ") collided with " + collision.transform.gameObject.name + " (" + collision.transform.GetComponent<CNetworkView>().ViewId.ToString() + ") taking " + healthLost.ToString() + " damage to its health of " + health.ToString());

			health -= impulse;
		}
	}
}