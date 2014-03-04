using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
// Static class used to various component manipulation
public class NestedPrefabComponentUtility
{	
	// Add a component if not already there
	public static ComponentType GetOrCreate<ComponentType>(GameObject a_rGameObjectOwner) where ComponentType : Component
	{
		ComponentType rComponent = a_rGameObjectOwner.GetComponent<ComponentType>();
		if(rComponent == null)
		{
			rComponent = a_rGameObjectOwner.AddComponent<ComponentType>();
		}
		
		return rComponent;
	}
	
	/// \brief  Build a basic object with this component
	public static ComponentType Build<ComponentType>() where ComponentType : Component
	{
		return Build<ComponentType>(typeof(ComponentType).Name, null);
	}
	
	/// \brief  Build a basic object with this component
	public static ComponentType Build<ComponentType>(string a_rGameObjectName) where ComponentType : Component
	{
		return Build<ComponentType>(a_rGameObjectName, null);
	}
	
	/// \brief  Build a basic object with this component
	public static ComponentType Build<ComponentType>(GameObject a_rParent) where ComponentType : Component
	{
		return Build<ComponentType>(typeof(ComponentType).Name, a_rParent);
	}
	
	/// \brief  Build a basic object with this component at the same place of an other game object
	public static ComponentType BuildAtSamePlace<ComponentType>(Transform a_rObjectPlace) where ComponentType : Component
	{
		ComponentType rBuiltComponent = Build<ComponentType>(typeof(ComponentType).Name);
		
		Transform rBuiltComponentTransform = rBuiltComponent.transform;
		
		rBuiltComponentTransform.parent = a_rObjectPlace.parent;
		rBuiltComponentTransform.localPosition = a_rObjectPlace.localPosition;
		rBuiltComponentTransform.localRotation = a_rObjectPlace.localRotation;
		rBuiltComponentTransform.localScale = a_rObjectPlace.localScale;

		return rBuiltComponent;
	}
	
	/// \brief  Build a basic object with this component
	public static ComponentType Build<ComponentType>(string a_rGameObjectName, GameObject a_rParent) where ComponentType : Component
	{
		GameObject rNewGameObject;
		
		// Create a new game object to contain the component
		rNewGameObject = new GameObject(a_rGameObjectName);
		
		// If the new object have a parent
		if(a_rParent != null)
		{
			// Attach it to the parent
			rNewGameObject.transform.parent = a_rParent.transform;
		}
		
		// Add a new component to the action object
		return rNewGameObject.AddComponent<ComponentType>();
	}
	
	/// \brief  Build a basic object with this component
	public static Component Build(string a_rComponentTypeName)
	{
		return Build(a_rComponentTypeName, a_rComponentTypeName, null);
	}
	
	/// \brief  Build a basic object with this component
	public static Component Build(string a_rComponentTypeName, string a_rGameObjectName)
	{
		return Build(a_rComponentTypeName, a_rGameObjectName, null);
	}
	
	/// \brief  Build a basic object with this component
	public static Component Build(string a_rComponentTypeName, GameObject a_rParent)
	{
		return Build(a_rComponentTypeName, a_rComponentTypeName, a_rParent);
	}
	
	/// \brief  Build a basic object with this component
	public static Component Build(string a_rComponentTypeName, string a_rGameObjectName, GameObject a_rParent)
	{
		GameObject rNewGameObject;
		
		// Create a new game object to contain the component
		rNewGameObject = new GameObject(a_rGameObjectName);
		
		// If the new object have a parent
		if(a_rParent != null)
		{
			// Attach it to the parent
			rNewGameObject.transform.parent = a_rParent.transform;
		}
		
		// Add a new component to the action object
		return rNewGameObject.AddComponent(a_rComponentTypeName);
	}
}
#endif