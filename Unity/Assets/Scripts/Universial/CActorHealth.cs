using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CNetworkView))]
[RequireComponent(typeof(Rigidbody))]
public class CActorHealth : CNetworkMonoBehaviour
{
    public delegate void OnSetCallback(GameObject gameObject, float prevHealth, float currHealth);
    public event OnSetCallback EventOnSetCallback;

    public bool destroyOnZeroHealth = false;
    public bool takeDamageOnImpact = false;

    [SerializeField] private float initialHealth = 1.0f;
    [HideInInspector] public float health_previous;
    protected CNetworkVar<float> health_internal;
    public float health { get { return health_internal.Get(); } set { health_internal.Set(value); } }

    public override void InstanceNetworkVars()
    {
        health_internal = new CNetworkVar<float>(OnNetworkVarSync, initialHealth);

        // Set before Start()
        health_previous = health;
    }

    void OnNetworkVarSync(INetworkVar sender)
    {
        if (EventOnSetCallback != null)
            EventOnSetCallback(gameObject, health_previous, health);

        health_previous = health;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (CNetwork.IsServer && takeDamageOnImpact)
        {
            float healthLost = 0.0f;
            if (collision.transform.rigidbody != null)
                healthLost = collision.relativeVelocity.magnitude * (collision.transform.rigidbody.mass / rigidbody.mass);
            else
                Debug.LogError("Put a Rigidbody on " + collision.transform.gameObject.name + " else there is no force in impacts.");

            //Debug.Log("CActorHealth: " + gameObject.name + " (" + GetComponent<CNetworkView>().ViewId.ToString() + ") collided with " + collision.transform.gameObject.name + " (" + collision.transform.GetComponent<CNetworkView>().ViewId.ToString() + ") taking " + healthLost.ToString() + " damage to its health of " + health.ToString());

            health -= healthLost;

            if (health <= 0.0f && destroyOnZeroHealth)
            {
                CNetwork.Factory.DestoryObject(gameObject.GetComponent<CNetworkView>().ViewId);
                destroyOnZeroHealth = false;    // To be totes sure destroy doesn't get called again.
            }
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