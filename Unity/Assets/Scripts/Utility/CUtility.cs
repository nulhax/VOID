//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   Utility.cs
//  Description :   Utility class containing helper functions
//                  and plain text resource locations
//
//  Author  	:  Nathan Boon
//  Mail    	:  Nathan.Boon@gmail.com
//

// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


/* Implementation */


public class CUtility
{
    // Member Types
    static readonly string s_sXmlPath           = "C:/VOID/Unity/Assets/Resources/XMLs/";
    static readonly string s_sXmlPathTools      = s_sXmlPath + "Tools.xml";
    static readonly string s_sXmlPathComponents = s_sXmlPath + "Components.xml";
    static readonly string s_sXmlPathFacilities = s_sXmlPath + "Facilities.xml";

    enum ETool
    {
        TOOL_INVALID = -1,
        TOOL_BLOWTORCH,
        TOOL_DETX,
        TOOL_EXTINGUISHER,
        TOOL_MEDPACK,
        TOOL_NANITEGUN,
        TOOL_PISTOL,
        TOOL_SEALER,
        TOOL_TECHKIT,
        TOOL_TORCH,
        TOOL_MAX
    };

    // Member Delegates & Events

    // Member Properties

    // Member Functions
    static public string GetXmlPath()           { return (s_sXmlPath);           }
    static public string GetXmlPathTools()      { return (s_sXmlPathTools);      }
    static public string GetXmlPathComponents() { return (s_sXmlPathComponents); }
    static public string GetXmlPathFacilities() { return (s_sXmlPathFacilities); }

    // Member Methods
	static public IEnumerable<IEnumerable<T>> GetPowerSet<T>(List<T> list)  
	{  
		return from m in Enumerable.Range(0, 1 << list.Count)  
			select  
				from i in Enumerable.Range(0, list.Count)  
				where (m & (1 << i)) != 0  
				select list[i];  
	}

	static public GameObject CreateNewGameObject(Transform _Parent, string _Name)
	{
		GameObject go = new GameObject(_Name);

		if(_Parent != null)
			go.transform.parent = _Parent;

		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;

		return(go);
	}

	static public void SetLayerRecursively(GameObject _Obj, int _Layer)
	{
		_Obj.layer = _Layer;
		
		for(int i = 0; i < _Obj.transform.childCount; ++i)
		{
			SetLayerRecursively(_Obj.transform.GetChild(i).gameObject, _Layer);
		}
	}

	static public string LoremIpsum(int minWords, int maxWords,
	                         int minSentences, int maxSentences,
	                         int numParagraphs) {
		
		var words = new[]{"lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
			"adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
			"tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"};

		var rand = new System.Random();
		int numSentences = rand.Next(maxSentences - minSentences)
			+ minSentences + 1;
		int numWords = rand.Next(maxWords - minWords) + minWords + 1;
		
		string result = string.Empty;
		
		for(int p = 0; p < numParagraphs; p++) 
		{
			for(int s = 0; s < numSentences; s++) 
			{
				for(int w = 0; w < numWords; w++) 
				{
					string word = words[rand.Next(words.Length)];
					if (w > 0) 
					{ 
						result += " "; 
					}
					else if(w == 0)
					{
						char[] copy = word.ToCharArray();
						copy[0] = char.ToUpper(copy[0]);
						word = new string(copy);
					}

					result += word;

				}
				result += ". ";
			}
		}
		
		return result;
	}

	/// <summary>
	/// Finds the specified component on the game object or one of its parents.
	/// </summary>
	
	static public T FindInParents<T> (GameObject go) where T : Component
	{
		if (go == null) return null;
		object comp = null;

		Transform t = go.transform.parent;
		
		while (t != null && comp == null)
		{
			comp = t.gameObject.GetComponent<T>();
			t = t.parent;
		}

		return (T)comp;
	}
	
	/// <summary>
	/// Finds the specified component on the game object or one of its parents.
	/// </summary>
	
	static public T FindInParents<T> (Transform trans) where T : Component
	{
		if (trans == null) return null;
		object comp = null;

		Transform t = trans.transform.parent;
		
		while (t != null && comp == null)
		{
			comp = t.gameObject.GetComponent<T>();
			t = t.parent;
		}

		return (T)comp;
	}

	static public Mesh CreateCombinedMesh(GameObject _GameObject)
	{
		Vector3 oldPos = _GameObject.transform.position;
		Quaternion oldRot = _GameObject.transform.rotation;

		_GameObject.transform.position = Vector3.zero;
		_GameObject.transform.rotation = Quaternion.identity;

		MeshFilter[] meshFilters = _GameObject.GetComponentsInChildren<MeshFilter>();
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];
		
		for(int i = 0; i < meshFilters.Length; ++i) 
		{
			combine[i].mesh = meshFilters[i].sharedMesh;
			combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
		}
		
		Mesh mesh = new Mesh();
		mesh.name = _GameObject.name + "_Combined";
		mesh.CombineMeshes(combine);

		_GameObject.transform.position = oldPos;
		_GameObject.transform.rotation = oldRot;

		return(mesh);
	}

	static public void DestroyAllNonRenderingComponents(GameObject _GameObject)
	{
		// Get all the monobehaviours that exsist on the prefab, reverse the order to delete dependant components first
		List<Component> components = new List<Component>(_GameObject.GetComponents<Component>());
		components.Reverse();
		
		// Get all the monobehaviours of all of the children too
		List<Component> childrenComponents = new List<Component>(_GameObject.GetComponentsInChildren<Component>());
		childrenComponents.Reverse();
		components.AddRange(childrenComponents);
		
		// Remove any scripts that arent rendering related
		foreach(Component comp in components)
		{
			System.Type behaviourType = comp.GetType();
			
			if(behaviourType != typeof(MeshRenderer) &&
			   behaviourType != typeof(MeshFilter) &&
			   behaviourType != typeof(Transform))
			{
				GameObject.Destroy(comp);
			}
		}
	}

	static public string SplitCamelCase(string _Original)
	{
		for(var i = 1; i < _Original.Length - 1; i++)
		{
			if (char.IsLower(_Original[i - 1]) && char.IsUpper(_Original[i]) ||
			    _Original[i - 1] != ' ' && char.IsUpper(_Original[i]) && char.IsLower(_Original[i + 1]))
			{
				_Original = _Original.Insert(i, " ");
			}
		}

		return(_Original);
	}

	public static float GetBoundingRadius(GameObject gameObject)
	{
		float result = 1.0f;

		// Depending on the type of model; it may use a collider, mesh renderer, animator, or something else.
		Collider collider = gameObject.GetComponent<Collider>();
		if (collider)
			result = collider.bounds.extents.magnitude;
		else
		{
			Renderer renderer = gameObject.GetComponent<Renderer>();
			if (renderer)
				result = renderer.bounds.extents.magnitude;
			else
			{
				bool gotSomethingFromAnimator = false;
				Animator anim = gameObject.GetComponent<Animator>();
				if (anim)
				{
					gotSomethingFromAnimator = anim.renderer || anim.collider || anim.rigidbody;
					if (anim.renderer) result = anim.renderer.bounds.extents.magnitude;
					else if (anim.collider) result = anim.collider.bounds.extents.magnitude;
					else if (anim.rigidbody) result = anim.rigidbody.collider.bounds.extents.magnitude;
				}

				if (!gotSomethingFromAnimator)
				{

				}
			}
		}

		return result;
	}

	public static float GetMass(GameObject gameObject)
	{
		Transform gameObjectTransform = gameObject.transform;
		while (gameObjectTransform != null)
		{
			Rigidbody rigidBody = gameObjectTransform.rigidbody;
			if (rigidBody)
				return rigidBody.mass;

			gameObjectTransform = gameObjectTransform.parent;
		}

		return 1.0f;
	}

	/// <summary>
	/// http://forum.unity3d.com/threads/117388-figuring-out-the-volume-and-surface-area
	/// </summary>
	/// <param name="mesh"></param>
	/// <returns></returns>
	public static float GetMeshSurfaceArea(Mesh mesh, Vector3 scale)
	{
		int[] triangles = mesh.triangles;
		Vector3[] vertices = mesh.vertices;

		float surfaceArea = 0;

		for (int i = 0; i < triangles.Length; i += 3)
		{
			Vector3 a = Vector3.Scale(vertices[triangles[i + 0]], scale);
			Vector3 b = Vector3.Scale(vertices[triangles[i + 1]], scale);
			Vector3 c = Vector3.Scale(vertices[triangles[i + 2]], scale);
			float triangleHeight = Vector3.Cross((c - b).normalized, a - b).magnitude;
			float triangleBase = (c - b).magnitude;
			surfaceArea += 0.5f * triangleBase * triangleHeight;
		}

		return surfaceArea;
	}
}

/*
    MeshSmoothFilter
 
	Laplacian Smooth Filter, HC-Smooth Filter
 
	MarkGX, Jan 2011
*/

/*
	Useful mesh functions
*/
