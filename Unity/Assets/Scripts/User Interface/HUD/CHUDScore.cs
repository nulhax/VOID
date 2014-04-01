//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CHazardSystem.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;

public class CHUDScore : MonoBehaviour
{
// Member Properties
    public uint Score
    {
        // Get
        get { return (m_uiScore); }
    }

// Member Functions
	void Start()
    {
        foreach (GameObject Actor in CGamePlayers.PlayerActors)
        {
            Actor.GetComponent<CPlayerHealth>().m_EventHealthChanged += TESTDamage;
        }
	}

    private void TESTDamage(GameObject _TargetPlayer, float _fHealthCurrentValue, float _fHealthPreviousValue)
    {
        m_uiScore += 10;

        Debug.Log("Score: " + Score);
    }

    private void OnDestroy()
    {
        
    }
	
	

// Unused Functions
    void Awake(){}
    void Update(){}

// Member Fields
    private uint m_uiScore = 0;
}
