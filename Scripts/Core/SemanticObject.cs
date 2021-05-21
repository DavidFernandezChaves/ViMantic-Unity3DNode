using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class SemanticObject {

    public string id { get; private set; }
    public string type { get; private set; }
    public float score { get; private set; }
    public Dictionary<string, float> scores { get; private set; }    
    public Vector3 pose { get; private set; }
    public Vector3 size { get; private set; }
    public Quaternion rotation { get; private set; }
    public List<SemanticObject> associated { get; private set; }
    public int nDetections { get { return associated.Count; }  }
    public SemanticRoom room { get; private set; }

    public List<KeyValuePair<DateTime, Dictionary<string, float>>> historic;
    


    public SemanticObject(string id, Dictionary<string, float> scores, Vector3 pose, Quaternion rotation, Vector3 size, SemanticRoom room) {
        this.id = id;
        this.scores = scores;
        this.pose = pose;
        this.size = size;
        this.rotation = rotation;
        this.room = room;
        this.scores = Normalize(scores);
        associated = new List<SemanticObject>();
        historic = new List<KeyValuePair<DateTime, Dictionary<string,float>>>();
        associated.Add(this);
        UpdateType();
        historic.Add(new KeyValuePair<DateTime, Dictionary<string, float>>(DateTime.Now, scores));
    }

    public void SetId(string id) {
        if (this.id == "")
            this.id = id;
    }

    public void NewDetection(SemanticObject _newSemanticObject, Vector3 _pose, Quaternion _rotation, Vector3 _size) {
        
        SemanticObject old = new SemanticObject(id, scores, pose, rotation, size, room);

        List<string> dif = _newSemanticObject.scores.Keys.Except(scores.Keys).ToList();
        if (dif.Count > 0) {
            float newScore = scores["Default"] / (dif.Count + 1);
            foreach (string newKey in dif) {
                scores[newKey] = newScore;
            }
            scores["Default"] = newScore;
        }

        dif = scores.Keys.Except(_newSemanticObject.scores.Keys).ToList(); 
        if(dif.Count > 0) {
            float newScore = _newSemanticObject.scores["Default"] / (dif.Count + 1);
            foreach (string newKey in dif) {
                _newSemanticObject.scores[newKey] = newScore;
            }
            _newSemanticObject.scores["Default"] = newScore;
        }

        historic.Add(new KeyValuePair<DateTime, Dictionary<string, float>>(DateTime.Now, _newSemanticObject.scores));

        foreach (string detection in scores.Keys.ToList()) {
            scores[detection] *= _newSemanticObject.scores[detection];
        }
        scores = Normalize(scores);
        this.scores = scores.OrderByDescending(s => s.Value).Take(5).ToDictionary(s => s.Key, s => s.Value);
        UpdateType();
        this.pose = _pose;
        this.rotation = _rotation;
        this.size = _size;        

        if (nDetections == 1) {
            associated = new List<SemanticObject>() { old, _newSemanticObject };
            this.id = "";     
            OntologyManager.instance.AddNewDetectedObject(this);
            OntologyManager.instance.JoinSemanticObject(this, old);
        } else {
            associated.Add(_newSemanticObject);
            OntologyManager.instance.UpdateObject(old, this);
            OntologyManager.instance.JoinSemanticObject(this, _newSemanticObject);
        }
        
    }

    public void SetRoom(SemanticRoom room) {
        if(room != null && this.room == null) {
            this.room = room;
            OntologyManager.instance.ObjectInRoom(id,room.id);
        }
    }

    public void UpdateType() {
        type = scores.OrderByDescending(x => x.Value).FirstOrDefault().Key;
        score = scores[type];        
    }

    public static Dictionary<string,float> Normalize(Dictionary<string, float> dictionary) {
        Dictionary<string, float> result = dictionary;
        float sum = dictionary.Values.Sum();

        if (!dictionary.ContainsKey("Default")) {
            if (sum >= 1) {
                result.Add("Default",0.01f);
            } else {
                result.Add("Default",1-sum);
            }
            sum = result.Values.Sum();
        }

        foreach (string type in result.Keys.ToList()) {
            result[type] /= sum;
        }
        return result;
    }

    public override string ToString() {
        return "SemanticObject [id =" + id
                        + ", scores=" + score
                        + ", pose=" + pose.ToString()
                        + ", nDetections=" + nDetections
                        + ", size=" + size.ToString() + "]";
    }

    public string GetIdRoom() {
        if (room != null)
            return room.id;
        else
            return "";
    }

}
