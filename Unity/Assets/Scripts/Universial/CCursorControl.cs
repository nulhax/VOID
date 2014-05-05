//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CCursorControl.cs
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


public class CCursorControl : MonoBehaviour
{

// Member Types


// Member Delegates & Events


    public delegate void CursorLockedChangeHandler(CCursorControl _cSender, bool _bVisible);
    public event CursorLockedChangeHandler EventLockedChange;


// Member Properties


    public static CCursorControl Instance
    {
        get { return (s_cInstance); }
    }


    public static bool IsCursorLocked
    {
        get { return (Screen.lockCursor); }
    }


// Member Methods


    public void SetLocked(bool _bLocked)
    {
        if (_bLocked != m_bLocked)
        {
            m_bLocked = _bLocked;

            Screen.lockCursor = _bLocked;

            if (EventLockedChange != null)
                EventLockedChange(this, m_bLocked);
        }
    }


    void Awake()
    {
        s_cInstance = this;
    }

    
	void Start()
	{
	}


	void OnDestroy()
	{
	}


	void Update()
	{
        if (Screen.lockCursor != m_bLocked)
        {
            Screen.lockCursor = m_bLocked;
        }

        // Lock Cursor toggle
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Screen.lockCursor = !Screen.lockCursor;
        }
	}


    void OnApplicationFocus(bool _cFocused)
    {
    }


// Member Fields


    static CCursorControl s_cInstance = null;


    bool m_bLocked = true;


};
