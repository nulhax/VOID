using UnityEngine;
using System.Collections;

public class GalaxyObserver : MonoBehaviour
{
    void Awake()
    {
        // If this is the server; find the galaxy instance and register this object as an observer.
        CNetwork network = CNetwork.Instance; System.Diagnostics.Debug.Assert(network);
        CGame game = CGame.Instance; System.Diagnostics.Debug.Assert(game);
        CGalaxy galaxy = game.GetComponent<CGalaxy>(); System.Diagnostics.Debug.Assert(galaxy);

        // Depending on the type of model; it may use a mesh renderer, an animator, or something else.
        float observationRadius = 1.0f;
        {
            Rigidbody body = gameObject.GetComponent<Rigidbody>();
            if (body)
                observationRadius = Mathf.Sqrt(body.collider.bounds.extents.sqrMagnitude);
            else
            {
                MeshRenderer mesh = gameObject.GetComponent<MeshRenderer>();
                if (mesh)
                    observationRadius = Mathf.Sqrt(mesh.bounds.extents.sqrMagnitude);
                else
                {
                    //Animator anim = gameObject.GetComponent<Animator>();
                    //if (anim)
                    //    observationRadius = Mathf.Sqrt(/*anim.renderer.bounds.extents.sqrMagnitude*//*anim.collider.bounds.extents.sqrMagnitude*//*anim.rigidbody.collider.bounds.extents.sqrMagnitude*/);
                    //else
                    {
                        Debug.LogWarning("GalaxyObserver: Can not determine model type for bounding sphere. Radius set to 1");
                    }
                }
            }
        }


        galaxy.RegisterObserver(this.gameObject, observationRadius/*Mathf.Sqrt(this.gameObject.rigidbody.collider.bounds.extents.sqrMagnitude)*/);

        //textObject = new GameObject();
        //textObject.transform.parent = this.gameObject.transform;
        //textObject.transform.localPosition = Vector3.zero;
        //textObject.transform.localRotation = Quaternion.identity;
        //textObject.layer = gameObject.layer;

        //// Add the mesh renderer
        //MeshRenderer mr = textObject.AddComponent<MeshRenderer>();
        //mr.material = (Material)Resources.Load("Fonts/Arial", typeof(Material));

        //// Add the text mesh
        //tm = textObject.AddComponent<TextMesh>();
        //tm.fontSize = 24;
        //tm.characterSize = .5f;
        //tm.color = Color.white;
        //tm.font = (Font)Resources.Load("Fonts/Arial", typeof(Font));
        //tm.anchor = TextAnchor.MiddleCenter;
        //tm.offsetZ = -0.01f;
        //tm.text = "OHAI";
        //tm.fontStyle = FontStyle.Italic;
    }

    //GameObject textObject;
    //TextMesh tm;
    //void Update()
    //{
    //    textObject.transform.position = gameObject.transform.position;
    //    if(Camera.current)
    //        textObject.transform.LookAt(-Camera.current.transform.position);

    //    CGalaxy.SGridCellPos cellPos = CGame.Instance.GetComponent<CGalaxy>().PointToTransformedCell(gameObject.transform.position);
    //    tm.text = string.Format("({0},{1},{2})", cellPos.x, cellPos.y, cellPos.z);
    //}

    void OnDestroy()
    {
        CNetwork network = CNetwork.Instance;
        if(network)
        {
            CGame game = CGame.Instance;
            if(game)
            {
                CGalaxy galaxy = game.GetComponent<CGalaxy>();
                if (galaxy)
                    galaxy.DeregisterObserver(this.gameObject);
            }
        }
    }
}

public class GalaxyObserver_Attachable
{
    GameObject mObserver;
    GalaxyObserver_Attachable(GameObject observer, float observationRadius)
    {
        mObserver = observer;

        // Find parent galaxy instance and register this object as an observer.
        CGame.Instance.GetComponent<CGalaxy>().RegisterObserver(observer, observationRadius);
    }

    ~GalaxyObserver_Attachable()
    {
        CGame.Instance.GetComponent<CGalaxy>().DeregisterObserver(mObserver);
    }
}
