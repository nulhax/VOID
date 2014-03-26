using UnityEngine;
using System.Collections;

public class TileBehaviour : MonoBehaviour 
{
	public Tile tile;

	public Vector2 screenPos;
	public bool onScreen;
	public bool selected = false;

	void Update()
	{
		//Track Screen position
		screenPos = Camera.main.WorldToScreenPoint(this.transform.position);

		//if within screen space
		if (GridManager.I.NodeWithinScreenSpace(screenPos))
		{
			if(!onScreen)
			{
				GridManager.I.m_TilesOnScreen.Add(this);
				onScreen = true;
			}
		}
		else if(onScreen)
		{
			GridManager.I.RemoveFromOnScreenUnts(this);
		}
	}
	
}

	[System.Serializable]
	public class Point
	{
		public int X, Y, Z;
		public Point (int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}
	}
