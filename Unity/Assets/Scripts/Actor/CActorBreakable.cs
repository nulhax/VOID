using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CActorHealth))]
public class CActorBreakable : MonoBehaviour
{
    public delegate void Callback(GameObject gameObject);
    public event Callback Event_OnBreak;
    public event Callback Event_OnFixed;

    void Start()
    {
        CActorHealth health = gameObject.GetComponent<CActorHealth>();
        health.EventOnSetCallback += new CActorHealth.OnSetCallback(HealthModified);
    }

    public void Break()
    {
        Debug.LogWarning("CActorBreakable: " + gameObject.ToString() + " broke");
        if (Event_OnBreak != null)
            Event_OnBreak(gameObject);
    }

    public void Fix()
    {
        Debug.LogWarning("CActorBreakable: " + gameObject.ToString() + " fixed");

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
