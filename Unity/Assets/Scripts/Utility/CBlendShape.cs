using UnityEngine;
using System.Collections;

public class CBlendShape : MonoBehaviour
{
	int blendShapeCount;
	SkinnedMeshRenderer skinnedMeshRenderer;
	Mesh skinnedMesh;
	float blendOne = 0f;
	float blendTwo = 0f;
	float blendSpeed = 100.0f;
	bool blendOneFinished = false;

	public Light light1 = null;
	public Light light2 = null;

	void Awake ()
	{
		skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer> ();
		skinnedMesh = GetComponent<SkinnedMeshRenderer> ().sharedMesh;
	}
	
	void Start ()
	{
		blendShapeCount = skinnedMesh.blendShapeCount; 
	}
	
	void Update ()
	{
		if (blendOne < 100f && !blendOneFinished) 
		{
			skinnedMeshRenderer.SetBlendShapeWeight (0, blendOne);
			blendOne += blendSpeed * Time.deltaTime;
		} 
		else 
		{
			blendOneFinished = true;
		}
		
		if (blendOneFinished == true && blendOne > 0.0f) 
		{
			skinnedMeshRenderer.SetBlendShapeWeight (0, blendOne);
			blendOne -= blendSpeed * Time.deltaTime;
		}
		else
		{
			blendOneFinished = false;
		}

		light1.intensity = Mathf.Lerp(0.0f, 1.0f, blendOne/100.0f);
		light2.intensity = Mathf.Lerp(0.0f, 1.0f, blendOne/100.0f);

		Color c = skinnedMeshRenderer.materials[0].color;
		c.a = Mathf.Lerp(0.0f, 1.0f, blendOne/100.0f);
		skinnedMeshRenderer.materials[0].color = c;

		c.a = 1.0f;
		light1.color = c;
		light2.color = c;
		
	}
}