using UnityEngine;
using System.Collections;

public class CNOSPanelFocus : MonoBehaviour 
{
	private GameObject m_Widget = null;
	private CNOSPanelRoot m_PanelRoot = null;

	private void Start()
	{
		m_Widget = CUtility.FindInParents<CNOSWidget>(gameObject).gameObject;
		m_PanelRoot = CUtility.FindInParents<CNOSPanelRoot>(gameObject);
	}

	private void OnClick()
	{
		// Focus this widget
		m_PanelRoot.FocusWidget(m_Widget);
	}
}
