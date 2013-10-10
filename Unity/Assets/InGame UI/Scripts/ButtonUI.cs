using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ButtonUI : MonoBehaviour 
{
    // Member Variables
    public event Action m_OnClick;

    // Member Methods
    public void ButtonClicked()
    {
        if (m_OnClick != null)
            m_OnClick();
    }
}
