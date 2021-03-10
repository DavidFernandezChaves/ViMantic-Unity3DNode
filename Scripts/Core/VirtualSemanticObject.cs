using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VirtualSemanticObject : MonoBehaviour
{

    public int verbose;
    public float joiningDistance = 1;
    public float erodeRate = 0.1f;
    public CanvasLabelClass canvasLabel;
    public LineRenderer lineRender;
    public Vector2 heightCanvas;
    public SemanticObject semanticObject;

    public DateTime dateTime { get; private set; }
    public List<SemanticObject> associatedDetections;    
    private DateTime time;

    #region Unity Functions

    private void OnTriggerStay(Collider other) {
        VirtualSemanticObject vso = other.gameObject.GetComponent<VirtualSemanticObject>();

        if (vso != null && vso.semanticObject.type.Equals(semanticObject.type) && dateTime <= vso.dateTime && null == associatedDetections.Find(O=>O.ontologyId.Equals(vso.semanticObject.ontologyId)) && vso.semanticObject.semanticRoom == semanticObject.semanticRoom) {
            if(dateTime == vso.dateTime && semanticObject.score < vso.semanticObject.score) {
                dateTime.AddMilliseconds(1);
                return;
            }
            
            time = DateTime.Now;
            associatedDetections.Add(vso.semanticObject);
            OntologyManager.instance.JoinSemanticObject(semanticObject, vso.semanticObject);
            semanticObject.nDetections++;

            float score = 0;
            foreach (SemanticObject so in associatedDetections) {
                score += so.score;
            }
            semanticObject.score = score / associatedDetections.Count;

            transform.parent.rotation = Quaternion.identity;
            vso.transform.parent.rotation = Quaternion.identity;

            Bounds bounds = GetComponent<MeshRenderer>().bounds;
            //Debug.Log(semanticObject.pose+"/"+semanticObject.size+"/"+bounds.center + "/" + bounds.size);
            bounds.Encapsulate(vso.GetComponent<MeshRenderer>().bounds);
            //bounds.Encapsulate(vso.semanticObject.pose);

            semanticObject.pose = bounds.center;
            semanticObject.size = bounds.size*(1-erodeRate);
            UpdateObject();
            Destroy(vso.transform.parent.gameObject);
            ObjectManager.instance.AddTimeUnion((DateTime.Now - time).Milliseconds);
        }
    }


    private void OnDestroy() {
        if (associatedDetections.Count > 1) {
            OntologyManager.instance.UpdateObject(semanticObject, semanticObject.pose, semanticObject.rotation, semanticObject.size, semanticObject.score, semanticObject.nDetections);
        }
    }

    #endregion

    #region Public Functions
    public void InitializeObject(SemanticObject _semanticObject, Transform _robot)
    {        
        dateTime = DateTime.Now;
        semanticObject = _semanticObject;
        associatedDetections.Add(semanticObject);
        GetComponent<BoxCollider>().size = Vector3.one * joiningDistance;
        

        UpdateObject();

        semanticObject.semanticRoom = GetRoom(transform.position);
        if (semanticObject.semanticRoom != null) {
            OntologyManager.instance.ObjectInRoom(semanticObject);
        }

        GetComponent<BoxCollider>().enabled = true;
    }

    public void UpdateObject() {
        transform.parent.name = semanticObject.ontologyId+"_"+semanticObject.nDetections;

        //Load Object
        transform.parent.position = semanticObject.pose;
        transform.localScale = semanticObject.size;
        Vector3 rotation = semanticObject.rotation.eulerAngles;
        transform.parent.rotation = Quaternion.Euler(0, rotation.y, 0);
        //transform.parent.rotation = semanticObject.rotation;

        //Load Canvas
        canvasLabel.transform.position = semanticObject.pose + new Vector3(0, UnityEngine.Random.Range(heightCanvas.x, heightCanvas.y), 0);
        canvasLabel.LoadLabel(semanticObject.type, semanticObject.score);

        lineRender.SetPosition(0, canvasLabel.transform.position - new Vector3(0, 0.2f, 0));
        lineRender.SetPosition(1, transform.parent.position);
    }

    public void RemoveSemanticObject()
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
            Debug.Log("[Object Manager]: " + _msg);
    }

    private void LogWarning(string _msg) {
        if (verbose > 0)
            Debug.LogWarning("[Object Manager]: " + _msg);
    }
    #endregion

}
