using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ButtonUI : MonoBehaviour 
{
    // Member Variables
    private Action m_ButtonClicked;

    // Member Methods
    void Awake()
    {
        
    }

    void Update()
    {
        
    }

    public void RegisterListener(Action _Action)
    {
        m_ButtonClicked += _Action;
    }

    public void ButtonActivated()
    {
        if (m_ButtonClicked != null)
            m_ButtonClicked();
    }
}
