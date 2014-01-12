using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
//[AddComponentMenu("VOID/GalaxyIE")]

public class GalaxyIE : MonoBehaviour
{
    //RenderTexture mSkyboxBaked = new RenderTexture(Screen.width, Screen.height, 0);
    GameObject mGalaxyCamera = null;

    public Skybox mSkybox = null;
    public Material mSkyboxMaterial { get { return mSkybox.material; } set { mSkybox.material = value; } }
    //public Material mFogMaterial = null;

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

    void Start()
    {
        mGalaxyCamera = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Galaxy/GalaxyCamera", typeof(GameObject)));

        mGalaxyCamera.camera.enabled = false;   // Disable camera to control when it renders.
        camera.clearFlags = CameraClearFlags.Skybox;

        mSkybox = mGalaxyCamera.GetComponent<Skybox>();
        if (!mSkybox)
            mSkybox = mGalaxyCamera.AddComponent<Skybox>();
        mSkyboxMaterial = new Material(Shader.Find("VOID/MultitexturedSkybox"));

        RenderSettings.skybox = mSkyboxMaterial;
    }

	//void Update()
	//{
	//	if (camera != Camera.current && Camera.current != null)
	//	{
	//		Destroy(this);
	//		Camera.current.gameObject.AddComponent<GalaxyIE>();
	//	}
	//}

    void OnDestroy()
    {
        mRegisterWithGalaxy = false;
    }

    void OnPreRender()
    {
        mRegisterWithGalaxy = true;

        // Calculate stuff for fog.
        float CAMERA_NEAR = camera.nearClipPlane;
        float CAMERA_FAR = camera.farClipPlane;
        float CAMERA_FOV = camera.fieldOfView;
        float CAMERA_ASPECT_RATIO = camera.aspect;

        float fovWHalf = CAMERA_FOV * 0.5f;

		Vector3 toTop = camera.transform.up * CAMERA_NEAR * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);
		Vector3 toRight = toTop * CAMERA_ASPECT_RATIO;

        Vector3 topLeft = camera.transform.forward * CAMERA_NEAR - toRight + toTop;
        float CAMERA_SCALE = topLeft.magnitude * CAMERA_FAR / CAMERA_NEAR;

        topLeft.Normalize();
        topLeft *= CAMERA_SCALE;

        Vector3 topRight = (camera.transform.forward * CAMERA_NEAR + toRight + toTop).normalized * CAMERA_SCALE;
        Vector3 bottomRight = (camera.transform.forward * CAMERA_NEAR + toRight - toTop).normalized * CAMERA_SCALE;
        Vector3 bottomLeft = (camera.transform.forward * CAMERA_NEAR - toRight - toTop).normalized * CAMERA_SCALE;

        Shader.SetGlobalVector("void_FrustumCornerTopLeft", topLeft);
        Shader.SetGlobalVector("void_FrustumCornerTopRight", topRight);
        Shader.SetGlobalVector("void_FrustumCornerBottomRight", bottomRight);
        Shader.SetGlobalVector("void_FrustumCornerBottomLeft", bottomLeft);
        Shader.SetGlobalFloat("void_CameraScale", CAMERA_SCALE);
        //////////////////////////////////////

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
        Shader.SetGlobalTexture("void_FogTex", bakedSkyboxTexture);
        RenderTexture.ReleaseTemporary(bakedSkyboxTexture);
    }
}