//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFireHazard.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* Implementation */


public class CFireHazard : CNetworkMonoBehaviour
{
    // Member Types
    enum ENetworkAction
    {
        INVALID = -1,

        ActionFireChange,

        MAX
    }

    enum EFireChange
    {
        INVALID = -1,

        FireStart,
        FireEnd,
        FireSpread,

        MAX
    }


    // Member Delegates & Events
    public delegate void NotifyFireChange();

    public static event NotifyFireChange EventFireStart;
    public static event NotifyFireChange EventFireEnd;
    public static event NotifyFireChange EventFireSpread;


    // Member Properties
    public float Health     { get { return (m_fHealth); } }
    public float SpreadRate { get { return (m_fSpreadRate); } }


    // Member Methods
    public void Awake()
    {
        // Zero the timer
        m_fTimer = 0.0f;

        // Base health of a single 'unit' of fire
        m_fHealth = 100.0f;

        // Spread rate (in seconds)
        m_fSpreadRate = 5.0f;
    }

    void Start()
    {
        // Fire started
        EventFireStart();
    }

    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar) { }

    void Update()
    {
        // Update the timer
        m_fTimer += Time.deltaTime;

        // If the elapsed time is greater than the spread time
        if (m_fTimer >= m_fSpreadRate)
        {
            // Reset spread time
            m_fSpreadRate = 0.0f;

            // The fire spreads
            Spread();
        }
    }

    void Spread()
    {
        // Spread logic


        // Fire spreads
        EventFireSpread();
    }

    void OnDestroy()
    {
        // Fire extinguished
        EventFireEnd();
    }


    // Members
    float m_fTimer;
    float m_fHealth;
    float m_fSpreadRate;
}

//// Member Types


//// Member Delegates & Events


//// Member Properties


//// Member Functions


//public float Health
//{
//    set
//    { 
//        m_fHealth = value;
//        if (!m_bDead &&
//            m_fHealth < 0)
//        {
//            CNetwork.Factory.DestoryObject(gameObject.GetComponent<CNetworkView>().ViewId);
//            m_bDead = true;
//        }
//    }
//    get { return (m_fHealth); }
//}


//public float Damage
//{
//    get { return m_fDamage; }
//    set { m_fDamage = value; }
//}

//// Use this for initialization
//public void Start () 
//{

//}

//public void OnDestroy()
//{

//}

//// Update is called once per frame
//void Update () 
//{

//}

//// Get IsTrigger and get the collider of the player to check 
//// Send fire damage to player Hp 
//void OnTriggerStay(Collider _Entity)
//{
//   // Debug.Log("ontriggerstay function entered.");
//    if (CNetwork.IsServer)
//    {
//        //Debug.Log("Server detected in fire hazard");
//         // is player actor, does the object return character motor
//        if (_Entity.gameObject.name == "Player Actor(Clone)")
//        {
//            Debug.Log("Rigid body triggered. ---------------------------");
//            //Get actor health
//            float hp = _Entity.gameObject.GetComponent<CPlayerHealth>().HitPoints;

//            if(hp <= 0.0f)
//            {
//                // Do nothing
//            }
//            else
//            {
//                // Set the damage fire will do to the character
//                 Damage = 40.0f * Time.deltaTime;

//                 //apply damage
//                 _Entity.gameObject.GetComponent<CPlayerHealth>().ApplyDamage(Damage);
//            }
//        }
//    }
//}

////Members
//private float m_fDamage;
//float m_fHealth = 100;
//bool m_bDead = false;