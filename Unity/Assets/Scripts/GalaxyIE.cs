using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
//[AddComponentMenu("VOID/GalaxyIE")]

public class GalaxyIE : PostEffectsBase
{
    private bool mRegisteredWithGalaxy = false;

	private float CAMERA_NEAR = 0.5f;
	private float CAMERA_FAR = 50.0f;
	private float CAMERA_FOV = 60.0f;
	private float CAMERA_ASPECT_RATIO = 1.333333f;

	public Shader mGalaxyShader = Shader.Find("VOID/GalaxyShader");
    public Material mGalaxyMaterial = null;
    public float mFogStartDistance;
    public float mFogEndDistance;
    private RenderTexture mSkyboxBaked = new RenderTexture(Screen.width, Screen.height, 0);

    public override void Start()
    {
        base.Start();

        mGalaxyMaterial = new Material(mGalaxyShader);

        CGame game = CGame.Instance;
        if(game)
        {
            CGalaxy galaxy = game.GetComponent<CGalaxy>();
            if (galaxy)
            {
                galaxy.RegisterGalaxyIE(this);
                mRegisteredWithGalaxy = true;
            }
        }
    }

	public override bool CheckResources()
    {
		CheckSupport(true);

		//mFogMaterial = CheckShaderAndCreateMaterial(mGalaxyShader, mFogMaterial);

		if(!isSupported)
			ReportAutoDisable();
		return isSupported;
	}

    void OnPreRender()
    {
        // Ensure skybox RenderTexture is the same resolution as the screen window (it shouldn't matter too much).
        if (mSkyboxBaked.width != Screen.width || mSkyboxBaked.height != Screen.height)
            mSkyboxBaked = new RenderTexture(Screen.width, Screen.height, 0);

        // Render the skybox to the RenderTexture.
        RenderTexture oldTexture = RenderTexture.active;
        RenderTexture.active = mSkyboxBaked;

        mGalaxyMaterial.SetPass(0);

        GL.PushMatrix();
        GL.LoadOrtho();

        GL.Begin(GL.QUADS);

        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f); // BL

        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f); // BR

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f); // TR

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f); // TL

        GL.End();
        GL.PopMatrix();

        RenderTexture.active = oldTexture;
    }

	void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
		if(!CheckResources())
        {
			Graphics.Blit(source, destination);
			return;
		}

        if (!mRegisteredWithGalaxy)
        {
            CGame game = CGame.Instance;
            if (game)
            {
                CGalaxy galaxy = game.GetComponent<CGalaxy>();
                if (galaxy)
                {
                    galaxy.RegisterGalaxyIE(this);
                    mRegisteredWithGalaxy = true;
                }
            }
        }

		CAMERA_NEAR = camera.nearClipPlane;
		CAMERA_FAR = camera.farClipPlane;
		CAMERA_FOV = camera.fieldOfView;
		CAMERA_ASPECT_RATIO = camera.aspect;

		Matrix4x4 frustumCorners = Matrix4x4.identity;

		float fovWHalf = CAMERA_FOV * 0.5f;

		Vector3 toRight = camera.transform.right * CAMERA_NEAR * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * CAMERA_ASPECT_RATIO;
		Vector3 toTop = camera.transform.up * CAMERA_NEAR * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

		Vector3 topLeft = camera.transform.forward * CAMERA_NEAR - toRight + toTop;
		float CAMERA_SCALE = topLeft.magnitude * CAMERA_FAR/CAMERA_NEAR;

		topLeft.Normalize();
		topLeft *= CAMERA_SCALE;

		Vector3 topRight = (camera.transform.forward * CAMERA_NEAR + toRight + toTop).normalized * CAMERA_SCALE;
		Vector3 bottomRight = (camera.transform.forward * CAMERA_NEAR + toRight - toTop).normalized * CAMERA_SCALE;
		Vector3 bottomLeft = (camera.transform.forward * CAMERA_NEAR - toRight - toTop).normalized * CAMERA_SCALE;

		frustumCorners.SetRow(0, topLeft);
		frustumCorners.SetRow(1, topRight);
		frustumCorners.SetRow(2, bottomRight);
		frustumCorners.SetRow(3, bottomLeft);

		mGalaxyMaterial.SetMatrix("_FrustumCornersWS", frustumCorners);
        mGalaxyMaterial.SetVector("_CameraWS", camera.transform.position);
        mGalaxyMaterial.SetVector("_FogStartDistance", new Vector4(1.0f / mFogStartDistance, CAMERA_SCALE - mFogStartDistance));
        mGalaxyMaterial.SetVector("_FogEndDistance", new Vector4(1.0f / mFogEndDistance, CAMERA_SCALE - mFogEndDistance));

		CustomGraphicsBlit(source, destination, mGalaxyMaterial);
	}

    static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial)
    {
        RenderTexture.active = dest;

        fxMaterial.SetTexture("_MainTex", source);
        fxMaterial.SetPass(6);  // 0-5 are skybox passes.

        GL.PushMatrix();
        GL.LoadOrtho();

        GL.Begin(GL.QUADS);

        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f); // BL

        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f); // BR

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f); // TR

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f); // TL

        GL.End();
        GL.PopMatrix();
    }
}