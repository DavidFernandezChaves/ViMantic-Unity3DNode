using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VirtualSemanticObject : MonoBehaviour
{
    public GameObject _canvasLabels;
    public LinesManager _lineManager;
    public Vector2 _heightCanvas;
    public Transform _cube;
    public SemanticObject _semanticObject;

    public List<Transform> _associatedDetections;

    private Transform _canvasLabel;
    private CanvasLabelClass _cavasLabelCass;
    private Transform _tfFrameID;
    private OntologyManager _ontologyManager;
    private DateTime _date;

    void OnCollisionEnter (Collision collision)
    {
        if (_ontologyManager == null)
        {
            _ontologyManager = FindObjectOfType<OntologyManager>();
        }

        if (collision.transform.tag.Equals("Object"))
        {
            _date = DateTime.Now;            
            var vso = collision.gameObject.GetComponent<VirtualSemanticObject>();
            if (transform.parent == _tfFrameID
                && vso != null && vso._semanticObject._type.Equals(_semanticObject._type)
                && collision.transform.parent != transform)
            {

                if (collision.transform.parent.parent == transform.parent)
                {
                    vso = collision.transform.parent.GetComponent<VirtualSemanticObject>();
                }

                //Join all equal objects
                Bounds bounds = _cube.GetComponent<Renderer>().bounds;

                foreach (Transform v in vso._associatedDetections)
                {
                    NewObjectPart(v);

                    var child = v.GetComponent<VirtualSemanticObject>();
                    _ontologyManager.RemoveSemanticObjectUnion(vso._semanticObject, child._semanticObject);
                    _ontologyManager.JoinSemanticObject(_semanticObject, child._semanticObject);

                    var bounds_child = v.GetComponentInChildren<Renderer>().bounds;
                    bounds_child.center += v.localPosition;
                    bounds.Encapsulate(bounds_child);
                }
                NewObjectPart(vso.transform);
                _ontologyManager.JoinSemanticObject(_semanticObject, vso._semanticObject);

                var bounds_2 = vso._cube.GetComponent<Renderer>().bounds;
                bounds_2.center += vso._cube.localPosition;
                bounds.Encapsulate(bounds_2);

                //Calculate the rotation and score mean
                var rotation = transform.rotation.eulerAngles;
                var score = _semanticObject._confidenceScore;
                VirtualSemanticObject[] objs = GetComponentsInChildren<VirtualSemanticObject>();
                foreach (VirtualSemanticObject so in objs)
                {
                    rotation += so._semanticObject._rotation.eulerAngles;
                    score += so._semanticObject._confidenceScore;
                }

                Vector3 pose = bounds.center;
                Vector3 size = bounds.size;
                rotation /= objs.Length + 1;
                score /= objs.Length + 1;

                //Debug.Log(name + "-"+ pose + "/" + size + "/"+ rotation + "/"+bounds);

                _semanticObject = _ontologyManager.UpdateObject(_semanticObject, pose, rotation, size, score, _semanticObject._nDetections + vso._semanticObject._nDetections);
                vso._semanticObject = _ontologyManager.UpdateNDetections(vso._semanticObject, 1);

                Destroy(vso._lineManager);
                Destroy(vso.GetComponent<LineRenderer>());

                if (vso._canvasLabel != null)
                    Destroy(vso._canvasLabel.gameObject);
                FindObjectOfType<ObjectManager>().AddTimeUnion((DateTime.Now - _date).Milliseconds);
            }

        }else if (collision.transform.tag.Equals("Room") && _semanticObject._semanticRoom == null)
        {
            var room = collision.gameObject.GetComponent<SemanticRoom>();
            if (room.PointInside(transform.position)){
                _semanticObject._semanticRoom = room;
                _ontologyManager.ObjectInRoom(_semanticObject);
            }
        }
    }

    public void NewObjectPart(Transform newPart) {
        _associatedDetections.Add(newPart);
        newPart.parent = transform;
        _lineManager.AddGameobjectLine(newPart.position);
    }


    public void Load(SemanticObject semanticObject, Transform tfFrameID, float joiningDistance)
    {
        _tfFrameID = tfFrameID;
        _semanticObject = semanticObject;
        transform.name = _semanticObject._ontologyID;
        transform.position = _semanticObject._pose;
        _cube.localScale = _semanticObject._size;
        transform.rotation = _semanticObject._rotation;
        GetComponent<BoxCollider>().size = _semanticObject._size + Vector3.one * joiningDistance;
        LoadCanvasLabel();
    }

    private void LoadCanvasLabel() {
        if (_canvasLabel == null)
        {
            _canvasLabel = Instantiate(_canvasLabels, transform).transform;
            _cavasLabelCass = _canvasLabel.GetComponent<CanvasLabelClass>();
            _lineManager.AddGameobjectLine(transform.position);
        }
        _canvasLabel.position = _semanticObject._pose + new Vector3(0, UnityEngine.Random.Range(_heightCanvas.x, _heightCanvas.y), 0);
        _cavasLabelCass.LoadLabel(_semanticObject._type, _semanticObject._confidenceScore);
        _lineManager.CanvasLine(_canvasLabel.position, _semanticObject._pose);
    }

    public void RemoveThisSemanticObject()
    {
        FindObjectOfType<OntologyManager>().RemoveSemanticObject(_semanticObject);
        Destroy(gameObject);
    }

}
