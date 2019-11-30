using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SemanticObject {

    public string _id;
    public double _score;
    public Vector3 _pose;
    public Vector3 _dimensions;
    public Quaternion _rotation;
    public SemanticRoom _semanticRoom;
    public string _ontologyId;
    public string _fatherId;

    public SemanticObject(string id, double probability, Vector3 pose, Vector3 dimensions, Quaternion rotation, SemanticRoom semanticRoom)
    {
        _id = id;
        _score = probability;
        _pose = pose;
        _dimensions = dimensions;
        _rotation = rotation;
        _semanticRoom = semanticRoom;
    }


}
