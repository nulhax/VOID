using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CActorHealth))]
public class CActorBreakable : MonoBehaviour
{
    public delegate void Callback(GameObject gameObject);
    public event Callback Event_OnBroken;
    public event Callback Event_OnFixed;

    void Start()
    {
        CActorHealth health = gameObject.GetComponent<CActorHealth>();
        health.EventOnSetCallback += new CActorHealth.OnSetCallback(HealthModified);
    }

    public void Break()
    {
        if (Event_OnBroken != null)
            Event_OnBroken(gameObject);
    }

    public void Fix()
    {
        if (Event_OnFixed != null)
            Event_OnFixed(gameObject);
    }

    public static void HealthModified(GameObject gameObject, float prevHealth, float currHealth)
    {
        if (prevHealth > 0.0f && currHealth <= 0.0f)
            gameObject.GetComponent<CActorBreakable>().Break();
        else if (prevHealth <= 0.0f && currHealth > 0.0f)
            gameObject.GetComponent<CActorBreakable>().Fix();
    }
}
