using UnityEngine;
using System.Collections;

public class GalaxyObserver : MonoBehaviour
{
    void Start()
    {
        if (CNetwork.IsServer)   // If this is the server...
        {
            // Find the galaxy instance and register this object as an observer.
            CNetwork network = CNetwork.Instance; System.Diagnostics.Debug.Assert(network);
            CGame game = CGame.Instance; System.Diagnostics.Debug.Assert(game);
            CGalaxy galaxy = CGalaxy.instance; System.Diagnostics.Debug.Assert(galaxy);

            if (galaxy != null)
            galaxy.RegisterObserver(this.gameObject, CGalaxy.GetBoundingRadius(gameObject));

            //textObject = new GameObject();
            //textObject.transform.parent = this.gameObject.transform;
            //textObject.transform.localPosition = Vector3.zero;
            //textObject.transform.localRotation = Quaternion.identity;
            //textObject.layer = gameObject.layer;

            //// Add the mesh renderer
            //MeshRenderer mr = textObject.AddComponent<MeshRenderer>();
            //mr.material = (Material)Resources.Load("Fonts/Couri", typeof(Material));

            //// Add the text mesh
            //tm = textObject.AddComponent<TextMesh>();
            //tm.fontSize = 72;
            //tm.characterSize = .125f;
            //tm.color = Color.white;
            //tm.font = (Font)Resources.Load("Fonts/Couri", typeof(Font));
            //tm.anchor = TextAnchor.MiddleCenter;
            //tm.offsetZ = 0.0f;
            //tm.text = "OHAI";
            //tm.fontStyle = FontStyle.Italic;
        }
    }

    //GameObject textObject;
    //TextMesh tm;
    //void Update()
    //{
    //    textObject.transform.position = gameObject.transform.position;
    //    if (Camera.current)
    //        textObject.transform.rotation = Quaternion.LookRotation(gameObject.transform.position - Camera.current.transform.position);

    //    CGalaxy galaxy = CGalaxy.instance;
    //    CGalaxy.SGridCellPos transformedCellPos = galaxy.PointToRelativeCell(gameObject.transform.position);
    //    CGalaxy.SGridCellPos untransformedCellPos = galaxy.PointToAbsoluteCell(gameObject.transform.position);
    //    tm.text = string.Format("Rel({0},{1},{2})\nAbs({3},{4},{5})", transformedCellPos.x, transformedCellPos.y, transformedCellPos.z, untransformedCellPos.x, untransformedCellPos.y, untransformedCellPos.z);
    //}

    void OnDestroy()
    {
        CGalaxy galaxy = CGalaxy.instance;
        if (galaxy)
            galaxy.DeregisterObserver(this.gameObject);
    }
}
