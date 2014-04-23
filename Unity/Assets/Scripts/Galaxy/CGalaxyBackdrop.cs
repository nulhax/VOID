using UnityEngine;
using System.Collections;

public class CGalaxyBackdrop
{
	enum ESkybox : uint
	{
		Composite,
		Solid,
		Stars,
		MAX
	}

	private Cubemap[] mSkyboxes = new Cubemap[(uint)ESkybox.MAX];
	private CGalaxy mGalaxy = null;

	private float mFogStart = 3000.0f;
	public float fogStart { get { return mFogStart; } }
	private float mFogEnd = 4000.0f;
	public float fogEnd { get { return mFogEnd; } }

	public CGalaxyBackdrop(CGalaxy _galaxy)
	{
		mGalaxy = _galaxy;

		Reinitialise();
	}

	void Reinitialise()
	{
		RenderSettings.fog = false;
		RenderSettings.skybox = null;

		// Load skyboxes.
		string[] skyboxFaces = new string[] {"Left", "Right", "Down", "Up", "Front", "Back"};

		if (true)	// Load the texture from file.
		{
			for (uint uiSkybox = 0; uiSkybox < (uint)ESkybox.MAX; ++uiSkybox)    // For each skybox...
			{
				for (uint uiFace = 0; uiFace < 6; ++uiFace)  // For each face on the skybox...
				{
					Texture2D skyboxFace = Resources.Load("Textures/Galaxy/" + uiSkybox.ToString() + skyboxFaces[uiFace], typeof(Texture2D)) as Texture2D;
					if (!mSkyboxes[uiSkybox])
						mSkyboxes[uiSkybox] = new Cubemap(skyboxFace.width, skyboxFace.format, false);
					mSkyboxes[uiSkybox].SetPixels(skyboxFace.GetPixels(), (CubemapFace)uiFace);
					Resources.UnloadAsset(skyboxFace);
				}

				mSkyboxes[uiSkybox].Apply(false, true);
			}
		}
		else	// Load the cubemap texture from file.
		{
			for (uint uiSkybox = 0; uiSkybox < (uint)ESkybox.MAX; ++uiSkybox)    // For each skybox...
				mSkyboxes[uiSkybox] = Resources.Load("Textures/Galaxy/" + uiSkybox.ToString() + "Cubemap", typeof(Cubemap)) as Cubemap;
		}

		// Set initial state.
		UpdateBackdrop(mGalaxy.centreCell);
	}

	public void UpdateBackdrop(CGalaxy.SCellPos absoluteCell)
	{
		Shader.SetGlobalFloat("void_FogStartDistance", mFogStart);
		Shader.SetGlobalFloat("void_FogEndDistance", mFogEnd);
		Shader.SetGlobalFloat("void_FogDensity", 0.01f);

		Shader.SetGlobalTexture("void_Skybox1", mSkyboxes[(uint)ESkybox.Stars]);

		if (RenderSettings.skybox == null)
			RenderSettings.skybox = new Material(Shader.Find("VOID/MultitexturedSkybox"));
		RenderSettings.skybox.SetVector("_Tint", Color.grey);
	}
}
