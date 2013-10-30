//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CActorCamera.cs
//  Description :   --------------------------
//
//  Author  	:  Programming Team
//  Mail    	:  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public class CActorHead : MonoBehaviour
{

// Member Types


// Member Fields	
	public float m_SensitivityX = 0.5f;
	public float m_SensitivityY = 0.5f;

	public float m_MinimumX = -360.0f;
	public float m_MaximumX = 360.0f;

	public float m_MinimumY = -60.0f;
	public float m_MaximumY = 60.0f;

    public Texture m_CrossHair = null;

	public float m_RotationX = 0.0f;
	public float m_RotationY = 0.0f;
	
	private GameObject m_camera;
	
// Member Methods
    void Start()
    {
		// Disable the current camera
        Camera.main.enabled = false;
		
		// Create te camera object for the camera
		m_camera = GameObject.Instantiate(Resources.Load("Prefabs/Player/Actor Camera", typeof(GameObject))) as GameObject;
        m_camera.transform.parent = transform;
        m_camera.transform.localPosition = Vector3.zero;
    }

	void Update ()
	{
		// Lock the cursor to the screen
		Screen.lockCursor = true;
		
		// Yaw rotation
		m_RotationX += Input.GetAxis("Mouse X") * m_SensitivityX;
		
		if(m_RotationX > 360.0f)
			m_RotationX -= 360.0f;
		else if(m_RotationX < -360.0f)
			m_RotationX += 360.0f;
			
		m_RotationX = Mathf.Clamp (m_RotationX, m_MinimumX, m_MaximumX);
		
		// Pitch rotation
		m_RotationY += Input.GetAxis("Mouse Y") * m_SensitivityY;
		m_RotationY = Mathf.Clamp (m_RotationY, m_MinimumY, m_MaximumY);
		
		// Apply the yaw to the camera
		m_camera.transform.eulerAngles = new Vector3(-m_RotationY, m_RotationX, 0.0f);
		
		// Apply the pitch to the actor
		transform.eulerAngles = new Vector3(0.0f, m_RotationX, 0.0f);
	}
	
	void OnGUI()
    {
        //GUI.DrawTexture(new Rect((Screen.width - m_CrossHair.width) * 0.5f, (Screen.height * 0.5f - m_CrossHair.height * 0.75f), m_CrossHair.width, m_CrossHair.height), m_CrossHair);
    }
};
