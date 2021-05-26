using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VirtualObjectBox : MonoBehaviour
{
    public int verbose;
    public float radius = 1f;

    public float joiningDistance = 1f;
    public float similarityRate = 0.1f;
    public float erodeRate = 0.05f;

    public int detectionsForVisibility = 2;

    public CanvasLabelClass canvasLabel;
    public LineRenderer lineRender;
    public Vector2 heightCanvas;
    public SemanticObject semanticObject;

    public DateTime dateTime { get; private set; } 

    #region Unity Functions

    private void OnTriggerStay(Collider other) {
        VirtualObjectBox vob = other.gameObject.GetComponent<VirtualObjectBox>();

        if (vob != null &&
        dateTime <= vob.dateTime &&
        vob.semanticObject.room == semanticObject.room &&
        vob.semanticObject.scores.ContainsKey(semanticObject.type) &&
        vob.semanticObject.scores[semanticObject.type] >= (semanticObject.score - similarityRate)) { 

            if (dateTime == vob.dateTime && semanticObject.score < vob.semanticObject.score) {
                dateTime.AddMilliseconds(1);
                return;
            }

            Vector3 rotation = (semanticObject.rotation.eulerAngles + vob.semanticObject.rotation.eulerAngles) / 2;

            transform.parent.rotation = Quaternion.identity;
            vob.transform.parent.rotation = Quaternion.identity;

            Bounds bounds = GetComponent<MeshRenderer>().bounds;
            //Debug.Log(semanticObject.pose+"/"+semanticObject.size+"/"+bounds.center + "/" + bounds.size);
            bounds.Encapsulate(vob.GetComponent<MeshRenderer>().bounds);
            //bounds.Encapsulate(vso.semanticObject.pose);

            semanticObject.NewDetection(vob.semanticObject, bounds.center, Quaternion.Euler(rotation), bounds.size * (1 - erodeRate));
            UpdateObject();

            Destroy(vob.transform.parent.gameObject);
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmos() {
        if (Application.isPlaying && this.enabled && verbose > 1) {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
#endif

    #endregion

    #region Public Functions
    public void InitializeObject(SemanticObject _semanticObject)
    {        
        dateTime = DateTime.Now;
        semanticObject = _semanticObject;
        GetComponent<BoxCollider>().size = Vector3.one * joiningDistance;       

        UpdateObject();

        semanticObject.SetRoom(GetRoom(transform.position));
        GetComponent<BoxCollider>().enabled = true;
    }

    public void UpdateObject() {
        if (semanticObject.type == "Default") {
            Destroy(transform.parent.gameObject);
            return;
        }

        transform.parent.name = semanticObject.id+"_"+semanticObject.nDetections;

        //Load Object
        transform.parent.position = semanticObject.pose;
        transform.localScale = semanticObject.size;
        Vector3 rotation = semanticObject.rotation.eulerAngles;
        transform.parent.rotation = Quaternion.Euler(0, rotation.y, 0);

        if (semanticObject.nDetections >= detectionsForVisibility) {
            GetComponent<MeshRenderer>().enabled = true;

            canvasLabel.gameObject.SetActive(true);
            lineRender.enabled = true;

            //Load Canvas
            canvasLabel.transform.position = semanticObject.pose + new Vector3(0, UnityEngine.Random.Range(heightCanvas.x, heightCanvas.y), 0);
            canvasLabel.LoadLabel(semanticObject.type, semanticObject.score);

            lineRender.SetPosition(0, canvasLabel.transform.position - new Vector3(0, 0.2f, 0));
            lineRender.SetPosition(1, transform.parent.position);
        }
    }

    public void RemoveVirtualBox()
    {
        OntologyManager.instance.RemoveSemanticObject(semanticObject);
        Destroy(transform.parent.gameObject);
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
