using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
//[AddComponentMenu("VOID/GalaxyIE")]

public class GalaxyIE : PostEffectsBase
{
    //RenderTexture mSkyboxBaked = new RenderTexture(Screen.width, Screen.height, 0);
    GameObject mGalaxyCamera = (GameObject)Resources.Load("Prefabs/GalaxyCamera", typeof(GameObject));

	private float CAMERA_NEAR = 0.5f;
	private float CAMERA_FAR = 50.0f;
	private float CAMERA_FOV = 60.0f;
	private float CAMERA_ASPECT_RATIO = 1.333333f;

    public Skybox mSkybox = null;
    public Material mSkyboxMaterial { get { return mSkybox.material; } set { mSkybox.material = value; } }
    public Material mFogMaterial = null;
    public float mFogStartDistance;

    private bool mRegisteredWithGalaxy = false;
    public bool mRegisterWithGalaxy
    {
        get { return mRegisteredWithGalaxy; }
        set
        {
            if (mRegisteredWithGalaxy != value)
            {
                if (CGalaxy.instance)
                {
                    if (value)
                        mRegisteredWithGalaxy = CGalaxy.instance.RegisterGalaxyIE(this);
                    else
                    {
                        CGalaxy.instance.DeregisterGalaxyIE(this);
                        mRegisteredWithGalaxy = false;
                    }
                }
            }
        }
    }

    public override void Start()
    {
        base.Start();

        mFogMaterial = new Material(Shader.Find("VOID/TexturedFog"));

        mGalaxyCamera.camera.enabled = false;   // Disable camera to control when it renders.
        camera.clearFlags = CameraClearFlags.Depth;   // Actual camera clears only depth, as the entire image will be updated with a skybox manually.

        mSkybox = mGalaxyCamera.GetComponent<Skybox>();
        if (!mSkybox)
            mSkybox = mGalaxyCamera.AddComponent<Skybox>();
        mSkyboxMaterial = new Material(Shader.Find("VOID/MultitexturedSkybox"));
    }

    void OnDestroy()
    {
        mRegisterWithGalaxy = false;
    }

    public override bool CheckResources()
    {
        CheckSupport(true);

        //mFogMaterial = CheckShaderAndCreateMaterial(mGalaxyShader, mFogMaterial);

        if (!isSupported)
            ReportAutoDisable();
        return isSupported;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!CheckResources())
        {
            Graphics.Blit(source, destination);
            return;
        }

        mRegisterWithGalaxy = true;

        // Calculate stuff for fog.
        CAMERA_NEAR = camera.nearClipPlane;
        CAMERA_FAR = camera.farClipPlane;
        CAMERA_FOV = camera.fieldOfView;
        CAMERA_ASPECT_RATIO = camera.aspect;

        Matrix4x4 frustumCorners = Matrix4x4.identity;

        float fovWHalf = CAMERA_FOV * 0.5f;

        Vector3 toRight = camera.transform.right * CAMERA_NEAR * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * CAMERA_ASPECT_RATIO;
        Vector3 toTop = camera.transform.up * CAMERA_NEAR * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

        Vector3 topLeft = camera.transform.forward * CAMERA_NEAR - toRight + toTop;
        float CAMERA_SCALE = topLeft.magnitude * CAMERA_FAR / CAMERA_NEAR;

        topLeft.Normalize();
        topLeft *= CAMERA_SCALE;

        Vector3 topRight = (camera.transform.forward * CAMERA_NEAR + toRight + toTop).normalized * CAMERA_SCALE;
        Vector3 bottomRight = (camera.transform.forward * CAMERA_NEAR + toRight - toTop).normalized * CAMERA_SCALE;
        Vector3 bottomLeft = (camera.transform.forward * CAMERA_NEAR - toRight - toTop).normalized * CAMERA_SCALE;

        frustumCorners.SetRow(0, topLeft);
        frustumCorners.SetRow(1, topRight);
        frustumCorners.SetRow(2, bottomRight);
        frustumCorners.SetRow(3, bottomLeft);

        mFogMaterial.SetMatrix("_FrustumCornersWS", frustumCorners);
        mFogMaterial.SetVector("_CameraWS", camera.transform.position);
        mFogMaterial.SetVector("_FogStartDistance", new Vector4(1.0f / mFogStartDistance, CAMERA_SCALE - mFogStartDistance));
        mFogMaterial.SetTexture("_MainTex", source);

        // Render skybox to texture to use as the fog's texture.
        int oldCullingMask = mGalaxyCamera.camera.cullingMask;
        mGalaxyCamera.camera.CopyFrom(camera);
        mGalaxyCamera.camera.clearFlags = CameraClearFlags.Skybox;  // Actual camera clears with nothing - this clears with the skybox.
        mGalaxyCamera.camera.cullingMask = oldCullingMask;  // Restore old culling mask which is already set to render "Nothing".
        mGalaxyCamera.camera.depthTextureMode = DepthTextureMode.None;  // Without this, calling Render() would wipe the depth buffer of the original camera.
        RenderTexture bakedSkyboxTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);
        RenderTexture oldTargetTexture = mGalaxyCamera.camera.targetTexture;
        mGalaxyCamera.camera.targetTexture = bakedSkyboxTexture;
        mGalaxyCamera.camera.Render();
        mGalaxyCamera.camera.targetTexture = oldTargetTexture;
        mFogMaterial.SetTexture("_FogTex", bakedSkyboxTexture);
        RenderTexture.ReleaseTemporary(bakedSkyboxTexture);

        // Render the fog.
        mFogMaterial.SetPass(0);

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