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
	public delegate void OnSetHealth(GameObject gameObject, float prevHealth, float currHealth);
	public event OnSetHealth EventOnSetHealth;

	public delegate void OnSetState(GameObject gameObject, byte prevState, byte currState);
	public event OnSetState EventOnSetState;

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
		health_internal = _cRegistrar.CreateNetworkVar<float>(OnSyncHealth, health_initial);
		state_internal = _cRegistrar.CreateNetworkVar<byte>(OnSyncState, state_initial);
		// Set before Start()
		health_previous = health_current = health_initial;
		state_previous = state_current = state_initial;
	}

	void Start()
	{
		if (callEventsOnStart)
		{
			if (EventOnSetHealth != null)
				EventOnSetHealth(gameObject, health_previous, health_current);

			if (EventOnSetState != null)
				EventOnSetState(gameObject, state_previous, state_current);
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
			EventOnSetHealth(gameObject, health_previous, health_current);

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
			EventOnSetState(gameObject, state_previous, state_current);

		state_previous = state_current;
	}

    void OnCollisionEnter(Collision collision)
    {
        if (CNetwork.IsServer && takeDamageOnImpact)
        {
            float impulse = 0.0f;
            if (collision.transform.rigidbody != null)
                impulse = (collision.rigidbody.mass * collision.relativeVelocity / (rigidbody.mass + collision.transform.rigidbody.mass)).magnitude;
            else
				Debug.LogError(string.Format("Put a Rigidbody on " + collision.transform.gameObject.name + " else there is no force in impacts. GameObject1({0}) GameObject2({1})", gameObject.name, collision.transform.gameObject.name));

            //Debug.Log("CActorHealth: " + gameObject.name + " (" + GetComponent<CNetworkView>().ViewId.ToString() + ") collided with " + collision.transform.gameObject.name + " (" + collision.transform.GetComponent<CNetworkView>().ViewId.ToString() + ") taking " + healthLost.ToString() + " damage to its health of " + health.ToString());

            health -= impulse;
        }
    }
}

//public class CActorHealth : MonoBehaviour
//{
//    public Health m_health = null;
//
//    public CActorHealth() { m_health = new Health(); }
//    public CActorHealth(GameObject gameObject = null, float initialHealth = 0.0f, Health.OnSetCallback onSetCallback = null, bool callCallbackNow = false) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(GameObject gameObject = null, float initialHealth = 0.0f, bool callCallbackNow = false, Health.OnSetCallback onSetCallback = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(GameObject gameObject = null, bool callCallbackNow = false, float initialHealth = 0.0f, Health.OnSetCallback onSetCallback = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(GameObject gameObject = null, bool callCallbackNow = false, Health.OnSetCallback onSetCallback = null, float initialHealth = 0.0f) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(GameObject gameObject = null, Health.OnSetCallback onSetCallback = null, float initialHealth = 0.0f, bool callCallbackNow = false) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(GameObject gameObject = null, Health.OnSetCallback onSetCallback = null, bool callCallbackNow = false, float initialHealth = 0.0f) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(float initialHealth = 0.0f, GameObject gameObject = null, Health.OnSetCallback onSetCallback = null, bool callCallbackNow = false) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(float initialHealth = 0.0f, GameObject gameObject = null, bool callCallbackNow = false, Health.OnSetCallback onSetCallback = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(float initialHealth = 0.0f, bool callCallbackNow = false, GameObject gameObject = null, Health.OnSetCallback onSetCallback = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(float initialHealth = 0.0f, bool callCallbackNow = false, Health.OnSetCallback onSetCallback = null, GameObject gameObject = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(float initialHealth = 0.0f, Health.OnSetCallback onSetCallback = null, GameObject gameObject = null, bool callCallbackNow = false) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(float initialHealth = 0.0f, Health.OnSetCallback onSetCallback = null, bool callCallbackNow = false, GameObject gameObject = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(Health.OnSetCallback onSetCallback = null, GameObject gameObject = null, float initialHealth = 0.0f, bool callCallbackNow = false) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(Health.OnSetCallback onSetCallback = null, GameObject gameObject = null, bool callCallbackNow = false, float initialHealth = 0.0f) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(Health.OnSetCallback onSetCallback = null, bool callCallbackNow = false, GameObject gameObject = null, float initialHealth = 0.0f) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(Health.OnSetCallback onSetCallback = null, bool callCallbackNow = false, float initialHealth = 0.0f, GameObject gameObject = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(Health.OnSetCallback onSetCallback = null, float initialHealth = 0.0f, GameObject gameObject = null, bool callCallbackNow = false) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(Health.OnSetCallback onSetCallback = null, float initialHealth = 0.0f, bool callCallbackNow = false, GameObject gameObject = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(bool callCallbackNow = false, GameObject gameObject = null, float initialHealth = 0.0f, Health.OnSetCallback onSetCallback = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(bool callCallbackNow = false, GameObject gameObject = null, Health.OnSetCallback onSetCallback = null, float initialHealth = 0.0f) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(bool callCallbackNow = false, Health.OnSetCallback onSetCallback = null, GameObject gameObject = null, float initialHealth = 0.0f) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(bool callCallbackNow = false, Health.OnSetCallback onSetCallback = null, float initialHealth = 0.0f, GameObject gameObject = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(bool callCallbackNow = false, float initialHealth = 0.0f, GameObject gameObject = null, Health.OnSetCallback onSetCallback = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CActorHealth(bool callCallbackNow = false, float initialHealth = 0.0f, Health.OnSetCallback onSetCallback = null, GameObject gameObject = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//
//    public static implicit operator Health(CActorHealth rhs) { return rhs.m_health; } // CActorHealth to Health.
//    public static implicit operator float(CActorHealth rhs) { return rhs.m_health; } // CActorHealth to float health.
//    //public static CActorHealth operator +(CActorHealth lhs, float rhs) { return new Health(lhs.m_GameObject, lhs.m_health + rhs, lhs.m_OnSetCallback, true); }
//    //public static CActorHealth operator -(CActorHealth lhs, float rhs) { return new Health(lhs.m_GameObject, lhs.m_health - rhs, lhs.m_OnSetCallback, true); }
//}
//
//public class Health
//{
//    public delegate void OnSetCallback(GameObject gameObject, Health health);
//
//    private GameObject m_GameObject;
//    public OnSetCallback m_OnSetCallback;
//    private float m_health;
//
//    public Health() { }
//    public Health(GameObject gameObject = null, float initialHealth = 0.0f, OnSetCallback onSetCallback = null, bool callCallbackNow = false) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(GameObject gameObject = null, float initialHealth = 0.0f, bool callCallbackNow = false, OnSetCallback onSetCallback = null) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(GameObject gameObject = null, bool callCallbackNow = false, float initialHealth = 0.0f, OnSetCallback onSetCallback = null) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(GameObject gameObject = null, bool callCallbackNow = false, OnSetCallback onSetCallback = null, float initialHealth = 0.0f) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(GameObject gameObject = null, OnSetCallback onSetCallback = null, float initialHealth = 0.0f, bool callCallbackNow = false) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(GameObject gameObject = null, OnSetCallback onSetCallback = null, bool callCallbackNow = false, float initialHealth = 0.0f) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(float initialHealth = 0.0f, GameObject gameObject = null, OnSetCallback onSetCallback = null, bool callCallbackNow = false) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(float initialHealth = 0.0f, GameObject gameObject = null, bool callCallbackNow = false, OnSetCallback onSetCallback = null) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(float initialHealth = 0.0f, bool callCallbackNow = false, GameObject gameObject = null, OnSetCallback onSetCallback = null) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(float initialHealth = 0.0f, bool callCallbackNow = false, OnSetCallback onSetCallback = null, GameObject gameObject = null) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(float initialHealth = 0.0f, OnSetCallback onSetCallback = null, GameObject gameObject = null, bool callCallbackNow = false) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(float initialHealth = 0.0f, OnSetCallback onSetCallback = null, bool callCallbackNow = false, GameObject gameObject = null) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(OnSetCallback onSetCallback = null, GameObject gameObject = null, float initialHealth = 0.0f, bool callCallbackNow = false) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(OnSetCallback onSetCallback = null, GameObject gameObject = null, bool callCallbackNow = false, float initialHealth = 0.0f) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(OnSetCallback onSetCallback = null, bool callCallbackNow = false, GameObject gameObject = null, float initialHealth = 0.0f) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(OnSetCallback onSetCallback = null, bool callCallbackNow = false, float initialHealth = 0.0f, GameObject gameObject = null) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(OnSetCallback onSetCallback = null, float initialHealth = 0.0f, GameObject gameObject = null, bool callCallbackNow = false) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(OnSetCallback onSetCallback = null, float initialHealth = 0.0f, bool callCallbackNow = false, GameObject gameObject = null) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(bool callCallbackNow = false, GameObject gameObject = null, float initialHealth = 0.0f, OnSetCallback onSetCallback = null) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(bool callCallbackNow = false, GameObject gameObject = null, OnSetCallback onSetCallback = null, float initialHealth = 0.0f) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(bool callCallbackNow = false, OnSetCallback onSetCallback = null, GameObject gameObject = null, float initialHealth = 0.0f) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(bool callCallbackNow = false, OnSetCallback onSetCallback = null, float initialHealth = 0.0f, GameObject gameObject = null) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(bool callCallbackNow = false, float initialHealth = 0.0f, GameObject gameObject = null, OnSetCallback onSetCallback = null) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//    public Health(bool callCallbackNow = false, float initialHealth = 0.0f, OnSetCallback onSetCallback = null, GameObject gameObject = null) { m_health = initialHealth; m_OnSetCallback = onSetCallback; m_GameObject = gameObject; if (callCallbackNow && m_OnSetCallback != null) { m_OnSetCallback(m_GameObject, this); } }
//
//    public static implicit operator float(Health rhs) { return rhs.m_health; } // Health to float health.
//    public static Health operator +(Health lhs, float rhs) { return new Health(lhs.m_GameObject, lhs.m_health + rhs, lhs.m_OnSetCallback, true); }
//    public static Health operator -(Health lhs, float rhs) { return new Health(lhs.m_GameObject, lhs.m_health - rhs, lhs.m_OnSetCallback, true); }
//}