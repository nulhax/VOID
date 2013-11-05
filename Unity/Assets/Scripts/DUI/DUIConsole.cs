using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;

public class DUIConsole : MonoBehaviour 
{
    // Member Fields 
    public TextAsset m_mainviewXML;
    public GameObject m_screenObject;


    // Member Properties
    public DUIMainView m_DUIMV        { get; set; }
	

    public void Initialise()
    {
		// Set the screen to be on the right layer
		m_screenObject.layer = LayerMask.NameToLayer("Screen");

        // Set up the Diegetic User interface Object
        SetupDUI();
    }

    public void Deinitialise()
    {
        // Release the render texture
        m_DUIMV.m_renderTex.Release();

        // Destroy the game object
        Destroy(m_DUIMV.gameObject);
    }

    private void SetupDUI()
    {
        // Create the DUI game object
        GameObject duiGo = new GameObject();
        duiGo.name = name + "_DUI";
        duiGo.layer = LayerMask.NameToLayer("DUI");
		duiGo.transform.rotation = Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f);
        duiGo.transform.position = transform.position + duiGo.transform.rotation * new Vector3(0.0f, 0.0f, -1.0f);
		duiGo.transform.parent = transform.parent;
		
        // Add the DUI component
        m_DUIMV = duiGo.AddComponent<DUIMainView>();

        // Initialise the DUI Component
        m_DUIMV.Initialise(m_mainviewXML);

        // Attach the render texture
        m_DUIMV.AttatchRenderTexture(m_screenObject.renderer.material);
    }
	
	public void CheckScreenCollision(Vector3 _origin, Vector3 _direction)
    {
		RaycastHit hit = new RaycastHit();
		Ray ray = new Ray(_origin, _direction);
		
		// Test Mouse Down
		if (DidRayCollideWithScreen(ray, out hit))
		{
			DUIButton button = m_DUIMV.FindButtonCollisions(hit);
			
			if(button)
			{
				Debug.Log("ButPressed");
				button.OnPressDown();
			}
		}	    
    }
	
	private bool DidRayCollideWithScreen(Ray _ray, out RaycastHit _rh)
	{
		if (Physics.Raycast(_ray, out _rh, 2.0f, 1 << LayerMask.NameToLayer("Screen")))
		{
			if (_rh.transform.gameObject == m_screenObject)
			{
				return(true);
			}
		}
		
		return(false);
	}
}
