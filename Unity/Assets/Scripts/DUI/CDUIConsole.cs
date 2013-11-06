using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;

public class CDUIConsole : MonoBehaviour 
{
	// Retarded public fields for the editor
	public GameObject m_ScreenObject = null;
	
    // Member Fields 
	private GameObject m_MainView = null;

    // Member Properties
	public CDUIMainView MainView { get { return(m_MainView.GetComponent<CDUIMainView>()); } }
	
	// Member Methods
    public void Initialise(EQuality _Quality, ELayoutStyle _Layout, Vector2 _Dimensions)
    {
		// Check the screen is assigned in the editor
		if(m_ScreenObject == null)
		{
			Debug.LogError("CDUIConsole Initialise failed. ScreenObj hasn't been assigned!");
		}
		
		// Set the screen to be on the right layer
		m_ScreenObject.layer = LayerMask.NameToLayer("Screen");
		
		// Create the mainview
		CreateMainView(_Quality, _Layout, _Dimensions);
    }
	
	public bool CheckScreenRaycast(Vector3 _origin, Vector3 _direction, float _fDistance, out RaycastHit _rh)
    {
		Ray ray = new Ray(_origin, _direction);
		
		if (Physics.Raycast(ray, out _rh, _fDistance, 1 << LayerMask.NameToLayer("Screen")))
		{
			if (_rh.transform.gameObject == m_ScreenObject)
			{
				return(true);
			}
		}
		
		return(false); 
    }
	
    private void CreateMainView(EQuality _Quality, ELayoutStyle _Layout, Vector2 _Dimensions)
    {
        // Create the DUI game object
        m_MainView = new GameObject();
        m_MainView.name = name + "_DUI";
        m_MainView.layer = LayerMask.NameToLayer("DUI");
		m_MainView.transform.rotation = Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f);
        m_MainView.transform.position = transform.position + m_MainView.transform.rotation * new Vector3(0.0f, 0.0f, -1.0f);
		m_MainView.transform.parent = transform.parent;
		
        // Add the DUI component
        m_MainView.AddComponent<CDUIMainView>();

        // Initialise the DUI Component
        MainView.Initialise(_Quality, _Layout, _Dimensions);

        // Attach the render texture
        MainView.AttatchRenderTexture(m_ScreenObject.renderer.material);
    }
}
