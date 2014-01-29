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

//[RequireComponent(typeof(CActorHealth))]
//public class CActorBreakable : MonoBehaviour
//{
//    public delegate void Callback(GameObject gameObject);
//    public event Callback Event_OnBreak;
//    public event Callback Event_OnFixed;

//    void Start()
//    {
//        CActorHealth health = gameObject.GetComponent<CActorHealth>();
//        health.EventOnSetCallback += new CActorHealth.OnSetCallback(HealthModified);
//    }

//    public void Break()
//    {
//        Debug.LogWarning("CActorBreakable: " + gameObject.ToString() + " broke");
//        if (Event_OnBreak != null)
//            Event_OnBreak(gameObject);
//    }

//    public void Fix()
//    {
//        Debug.LogWarning("CActorBreakable: " + gameObject.ToString() + " fixed");

//        if (Event_OnFixed != null)
//            Event_OnFixed(gameObject);
//    }

//    public static void HealthModified(GameObject gameObject, float prevHealth, float currHealth)
//    {
//        if (prevHealth > 0.0f && currHealth <= 0.0f)
//            gameObject.GetComponent<CActorBreakable>().Break();
//        else if (prevHealth <= 0.0f && currHealth > 0.0f)
//            gameObject.GetComponent<CActorBreakable>().Fix();
//    }
//}

[RequireComponent(typeof(CNetworkView))]
[RequireComponent(typeof(CActorHealth))]
public class CActorBreakable : CNetworkMonoBehaviour
{
    public delegate void Callback(GameObject gameObject, byte prevState, float currState);
    public event Callback Event_OnSet;

    [SerializeField] private byte initialState = 0;
    [SerializeField] public float[] stateTransitions;
    [HideInInspector] public byte state_previous;
    protected CNetworkVar<byte> state_internal = null;
    public byte state { get { return state_internal.Get(); } set { state_internal.Set(value); } }

    public override void InstanceNetworkVars()
    {
        state_internal = new CNetworkVar<byte>(OnSync, initialState);

        // Set before Start()
        state_previous = initialState;
    }

    void Start()
    {
        if (stateTransitions.Length > byte.MaxValue) Debug.LogError("More states than can hold!!!!");

        CActorHealth health = gameObject.GetComponent<CActorHealth>();
        HealthModified(gameObject, health.health, health.health);
        health.EventOnSetCallback += new CActorHealth.OnSetCallback(HealthModified);
    }

    void OnSync(INetworkVar sender)
    {
        if(Event_OnSet != null)
            Event_OnSet(gameObject, state_previous, state);
    }

    public static void HealthModified(GameObject gameObject, float prevHealth, float currHealth)
    {
        CActorBreakable actor = gameObject.GetComponent<CActorBreakable>();
        
        // Change state if necessary.
        if (actor.stateTransitions != null)
        {
            byte currentState = 0;

            for (int i = 0; i < actor.stateTransitions.Length; ++i)
            {
                if (currHealth < actor.stateTransitions[i] || i == actor.stateTransitions.Length - 1)
                {
                    currentState = (byte)i;
                    break;
                }
            }

            if (currentState != actor.state)
                actor.state = currentState;
        }
    }
}