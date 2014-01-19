using UnityEngine;
using System.Collections;

public class TestNGUIEventHandler : MonoBehaviour 
{
	private bool m_IgnoreSelf = false;

	private void OnClick () 
	{
		//Debug.Log("OnClick " + UICamera.currentTouchID.ToString());
	}

	private void OnDoubleClick () 
	{
		//Debug.Log("OnDoubleClick " + UICamera.currentTouchID.ToString());
	}

	private void OnPress (bool isPressed)
	{
		if(!m_IgnoreSelf)
		{
			Debug.Log("OnPress " + isPressed.ToString());
			m_IgnoreSelf = true;

			// Send the message to myself
			gameObject.SendMessage("OnPress", isPressed, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			m_IgnoreSelf = false;
		}
	}

	private void OnHover (bool b) 
	{
		//Debug.Log("OnHover " + b.ToString());
	}

	private void OnDrag (Vector2 d) 
	{
		//Debug.Log("OnDrag " + d.ToString());
	}

	private void OnDragStart () 
	{
		//Debug.Log("OnDragStart");
	}

	private void OnDragEnd () 
	{
		//Debug.Log("OnDragEnd");
	}

	private void OnScroll (float d)
	{
		//Debug.Log("OnScroll " + d.ToString());
	}
}
