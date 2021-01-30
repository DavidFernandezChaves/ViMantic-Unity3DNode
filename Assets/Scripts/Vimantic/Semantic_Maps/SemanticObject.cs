using UnityEngine;

[System.Serializable]
public class SemanticObject {

    public string ontologyId;
    public string type;
    public float score;
    public int nDetections;
    public Vector3 pose;
    public Vector3 size;
    public Quaternion rotation;
    public SemanticRoom semanticRoom;

    public SemanticObject(string type, float confidenceScore, Vector3 pose, Vector3 dimensions, Quaternion rotation) {
        ontologyId = "";
        this.type = type;
        this.score = confidenceScore;
        nDetections = 1;
        this.pose = pose;
        size = dimensions;
        this.rotation = rotation;
        semanticRoom = null;
    }

    public SemanticObject(string id, string type, float confidenceScore, int nDetections, Vector3 pose, Vector3 dimensions, Quaternion rotation, SemanticRoom semanticRoom)
    {
        ontologyId = id;
        this.type = type;
        this.score = confidenceScore;
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
