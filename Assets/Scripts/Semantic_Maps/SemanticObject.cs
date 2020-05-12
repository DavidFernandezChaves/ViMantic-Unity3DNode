using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SemanticObject {

    public string _ontologyID;
    public string _type;
    public double _confidenceScore;
    public int _nDetections;
    public Vector3 _pose;
    public Vector3 _size;
    public Quaternion _rotation;
    public SemanticRoom _semanticRoom;

    public SemanticObject(string type, double confidenceScore, Vector3 pose, Vector3 dimensions, Quaternion rotation) {
        _ontologyID = "";
        _type = type;
        _confidenceScore = confidenceScore;
        _nDetections = 1;
        _pose = pose;
        _size = dimensions;
        _rotation = rotation;
        _semanticRoom = null;
    }

    public SemanticObject(string id, string type, double confidenceScore, int nDetections, Vector3 pose, Vector3 dimensions, Quaternion rotation, SemanticRoom semanticRoom)
    {
        _ontologyID = id;
        _type = type;
        _confidenceScore = confidenceScore;
        _nDetections = nDetections;
        _pose = pose;
        _size = dimensions;
        _rotation = rotation;
        _semanticRoom = semanticRoom;
    }

    public void SetRoom(SemanticRoom semanticRoom) {
        _semanticRoom = semanticRoom;
    }

    public string GetIdRoom() {
        if (_semanticRoom != null)
            return _semanticRoom._id;
        else
            return "";
    }

}
