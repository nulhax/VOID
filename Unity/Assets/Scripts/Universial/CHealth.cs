using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CNetworkView))]
public class CHealth : CNetworkMonoBehaviour
{
    public delegate void OnSetCallback(GameObject gameObject, CHealth health);
    public event OnSetCallback EventOnSetCallback;

    public bool destroyOnZeroHealth = false;
    public bool takeDamageOnImpact = false;

    protected CNetworkVar<float> m_health;
    public float health { get { return m_health.Get(); } set { m_health.Set(value); } }

    public override void InstanceNetworkVars()
    {
        m_health = new CNetworkVar<float>(OnNetworkVarSync, 1.0f);
    }

    void OnNetworkVarSync(INetworkVar sender)
    {
        if (EventOnSetCallback != null)
            EventOnSetCallback(gameObject, this);
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

            //Debug.Log("CHealth: " + gameObject.name + " (" + GetComponent<CNetworkView>().ViewId.ToString() + ") collided with " + collision.transform.gameObject.name + " (" + collision.transform.GetComponent<CNetworkView>().ViewId.ToString() + ") taking " + healthLost.ToString() + " damage to its health of " + health.ToString());

            health -= healthLost;

            if (health <= 0.0f && destroyOnZeroHealth)
            {
                CNetwork.Factory.DestoryObject(gameObject.GetComponent<CNetworkView>().ViewId);
                destroyOnZeroHealth = false;    // To be totes sure destroy doesn't get called again.
            }
        }
    }
}

//public class CHealth : MonoBehaviour
//{
//    public Health m_health = null;
//
//    public CHealth() { m_health = new Health(); }
//    public CHealth(GameObject gameObject = null, float initialHealth = 0.0f, Health.OnSetCallback onSetCallback = null, bool callCallbackNow = false) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(GameObject gameObject = null, float initialHealth = 0.0f, bool callCallbackNow = false, Health.OnSetCallback onSetCallback = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(GameObject gameObject = null, bool callCallbackNow = false, float initialHealth = 0.0f, Health.OnSetCallback onSetCallback = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(GameObject gameObject = null, bool callCallbackNow = false, Health.OnSetCallback onSetCallback = null, float initialHealth = 0.0f) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(GameObject gameObject = null, Health.OnSetCallback onSetCallback = null, float initialHealth = 0.0f, bool callCallbackNow = false) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(GameObject gameObject = null, Health.OnSetCallback onSetCallback = null, bool callCallbackNow = false, float initialHealth = 0.0f) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(float initialHealth = 0.0f, GameObject gameObject = null, Health.OnSetCallback onSetCallback = null, bool callCallbackNow = false) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(float initialHealth = 0.0f, GameObject gameObject = null, bool callCallbackNow = false, Health.OnSetCallback onSetCallback = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(float initialHealth = 0.0f, bool callCallbackNow = false, GameObject gameObject = null, Health.OnSetCallback onSetCallback = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(float initialHealth = 0.0f, bool callCallbackNow = false, Health.OnSetCallback onSetCallback = null, GameObject gameObject = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(float initialHealth = 0.0f, Health.OnSetCallback onSetCallback = null, GameObject gameObject = null, bool callCallbackNow = false) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(float initialHealth = 0.0f, Health.OnSetCallback onSetCallback = null, bool callCallbackNow = false, GameObject gameObject = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(Health.OnSetCallback onSetCallback = null, GameObject gameObject = null, float initialHealth = 0.0f, bool callCallbackNow = false) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(Health.OnSetCallback onSetCallback = null, GameObject gameObject = null, bool callCallbackNow = false, float initialHealth = 0.0f) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(Health.OnSetCallback onSetCallback = null, bool callCallbackNow = false, GameObject gameObject = null, float initialHealth = 0.0f) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(Health.OnSetCallback onSetCallback = null, bool callCallbackNow = false, float initialHealth = 0.0f, GameObject gameObject = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(Health.OnSetCallback onSetCallback = null, float initialHealth = 0.0f, GameObject gameObject = null, bool callCallbackNow = false) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(Health.OnSetCallback onSetCallback = null, float initialHealth = 0.0f, bool callCallbackNow = false, GameObject gameObject = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(bool callCallbackNow = false, GameObject gameObject = null, float initialHealth = 0.0f, Health.OnSetCallback onSetCallback = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(bool callCallbackNow = false, GameObject gameObject = null, Health.OnSetCallback onSetCallback = null, float initialHealth = 0.0f) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(bool callCallbackNow = false, Health.OnSetCallback onSetCallback = null, GameObject gameObject = null, float initialHealth = 0.0f) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(bool callCallbackNow = false, Health.OnSetCallback onSetCallback = null, float initialHealth = 0.0f, GameObject gameObject = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(bool callCallbackNow = false, float initialHealth = 0.0f, GameObject gameObject = null, Health.OnSetCallback onSetCallback = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//    public CHealth(bool callCallbackNow = false, float initialHealth = 0.0f, Health.OnSetCallback onSetCallback = null, GameObject gameObject = null) { m_health = new Health(gameObject, initialHealth, onSetCallback, callCallbackNow); }
//
//    public static implicit operator Health(CHealth rhs) { return rhs.m_health; } // CHealth to Health.
//    public static implicit operator float(CHealth rhs) { return rhs.m_health; } // CHealth to float health.
//    //public static CHealth operator +(CHealth lhs, float rhs) { return new Health(lhs.m_GameObject, lhs.m_health + rhs, lhs.m_OnSetCallback, true); }
//    //public static CHealth operator -(CHealth lhs, float rhs) { return new Health(lhs.m_GameObject, lhs.m_health - rhs, lhs.m_OnSetCallback, true); }
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