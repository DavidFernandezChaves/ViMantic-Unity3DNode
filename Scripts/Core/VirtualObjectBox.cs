using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VirtualObjectBox : MonoBehaviour
{
    public int verbose;
    public Vector2 heightCanvas;

    public SemanticObject semanticObject;
    public Color boxColor { get; private set; }

    public CanvasLabelClass canvasLabel;
    private LineRenderer lineRender;

    #region Unity Functions

#if UNITY_EDITOR
    private void OnDrawGizmos() {
        if (Application.isPlaying && this.enabled && verbose > 1) {
            for(int i=0;i<8;i++) {

                if((semanticObject.fixed_corners & (1 << i)) > 0)
                    Gizmos.color = Color.green;
                else
                    Gizmos.color = Color.red;

                //if (i == 0) Gizmos.color = Color.blue;
                //if (i == 1) Gizmos.color = Color.magenta;
                //if (i == 2) Gizmos.color = Color.gray;
                //if (i == 3) Gizmos.color = Color.yellow;
                //if (i == 4) Gizmos.color = Color.green;
                //if (i == 5) Gizmos.color = Color.red;
                //if (i == 6) Gizmos.color = Color.white;
                //if (i == 7) Gizmos.color = Color.cyan;

                Gizmos.DrawSphere(semanticObject.corners[i], 0.05f);
            }            
        }
    }
#endif


    private void Start() {        
        boxColor = VirtualObjectSystem.instance.GetColorObject(this);
        Color transparentColor = new Color(boxColor.r, boxColor.g, boxColor.b,0.3f);
        Material material = new Material(Shader.Find("Standard"));
        GetComponent<Renderer>().materials[0] = material;
        GetComponent<Renderer>().material.SetFloat("_Mode", 2.0f);
        GetComponent<Renderer>().material.SetColor("_Color", transparentColor);
        GetComponent<Renderer>().material.SetColor("_UnlitColor", boxColor);
        lineRender = GetComponentInParent<LineRenderer>();
        lineRender.startColor = boxColor;
        canvasLabel.SetColor(boxColor);
        UpdateObject();
        semanticObject.SetRoom(GetRoom(transform.position));
    }
    #endregion

    #region Public Functions
    public void InitializeSemanticObject(SemanticObject _semanticObject)
    {        
        semanticObject = _semanticObject;   
    }

    public void UpdateObject() {
        if (semanticObject.type == "Other") {
            RemoveVirtualBox();
            return;
        }

        transform.parent.name = semanticObject.nDetections + "_" + semanticObject.type + "_" + semanticObject.id;

        //Load Object
        transform.parent.position = semanticObject.position;
        transform.localScale = semanticObject.size;
        transform.parent.rotation = semanticObject.rotation;

        GetComponent<MeshRenderer>().enabled = true;

        canvasLabel.gameObject.SetActive(true);
        lineRender.enabled = true;

        //Load Canvas
        canvasLabel.transform.position = semanticObject.position + new Vector3(0, UnityEngine.Random.Range(heightCanvas.x, heightCanvas.y), 0);
        canvasLabel.LoadLabel(semanticObject.type, semanticObject.score);

        lineRender.SetPosition(0, canvasLabel.transform.position - new Vector3(0, 0.2f, 0));
        lineRender.SetPosition(1, transform.parent.position);
    }

    public void RemoveVirtualBox()
    {
        //OntologySystem.instance.RemoveSemanticObject(semanticObject);
        VirtualObjectSystem.instance.UnregisterColor(boxColor);
        Destroy(transform.parent.gameObject);
    }

    public void NewDetection(SemanticObject newDetection, List<VirtualObjectBox> matches = null) {
        semanticObject.NewDetection(newDetection, matches);
        if (matches != null)
        {
            matches.ForEach(m => m.RemoveVirtualBox());
        }
        UpdateObject();
    }

    public static SemanticRoom GetRoom(Vector3 position) {
        RaycastHit hit;
        position.y = -100;
        if (Physics.Raycast(position, Vector3.up, out hit)) {
            return hit.transform.GetComponent<SemanticRoom>();            
        }
        return null;
    }
    #endregion

    #region Private Functions
    private void Log(string _msg) {
        if (verbose > 1)
            Debug.Log("[Virtual Object Box]: " + _msg);
    }

    private void LogWarning(string _msg) {
        if (verbose > 0)
            Debug.LogWarning("[Virtual Object Box]: " + _msg);
    }
    #endregion

}
