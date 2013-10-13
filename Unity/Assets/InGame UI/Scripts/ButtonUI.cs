using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ButtonUI : MonoBehaviour 
{
    // Member Variables
    public event Action m_OnPress;

    // Member Methods
    public void ButtonPressed()
    {
        if (m_OnPress != null)
            m_OnPress();
    }
}
