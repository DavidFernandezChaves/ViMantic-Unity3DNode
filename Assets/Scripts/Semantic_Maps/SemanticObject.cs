using UnityEngine;

[System.Serializable]
public class SemanticObject {

    public string ontologyID;
    public string type;
    public double confidenceScore;
    public int nDetections;
    public Vector3 pose;
    public Vector3 size;
    public Quaternion rotation;
    public SemanticRoom semanticRoom;

    public SemanticObject(string type, double confidenceScore, Vector3 pose, Vector3 dimensions, Quaternion rotation) {
        ontologyID = "";
        this.type = type;
        this.confidenceScore = confidenceScore;
        nDetections = 1;
        this.pose = pose;
        size = dimensions;
        this.rotation = rotation;
        semanticRoom = null;
    }

    public SemanticObject(string id, string type, double confidenceScore, int nDetections, Vector3 pose, Vector3 dimensions, Quaternion rotation, SemanticRoom semanticRoom)
    {
        ontologyID = id;
        this.type = type;
        this.confidenceScore = confidenceScore;
        this.nDetections = nDetections;
        this.pose = pose;
        size = dimensions;
        this.rotation = rotation;
        this.semanticRoom = semanticRoom;
    }

    public string GetIdRoom() {
        if (semanticRoom != null)
            return semanticRoom.id;
        else
            return "";
    }

}
