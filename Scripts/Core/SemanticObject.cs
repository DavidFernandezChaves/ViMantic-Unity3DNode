using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;

public class SemanticObject {

    public string id { get; private set; }
    public string type { get; private set; }
    public float score { get; private set; }
    public Dictionary<string, float> scores { get; private set; }    
    public Vector3 position { get; private set; }
    public Vector3 size { get; private set; }
    public Quaternion rotation { get; private set; }
    public int nDetections { get; private set; }
    public SemanticRoom room { get; private set; }

    public SemanticObject(Dictionary<string, float> _scores, Vector3 _position, Quaternion _rotation, Vector3 _size) {
        id = "";
        size = _size;
        rotation = _rotation;
        position = _position;
        nDetections = 1;
        scores = new Dictionary<string, float>();
        float defaultValue =  (1-scores.Values.Sum()) / OntologySystem.instance.objectClassInOntology.Count;

        scores.Add("Other", defaultValue);
        foreach(string objectClass in OntologySystem.instance.objectClassInOntology) {
            scores.Add(objectClass, defaultValue);
        }

        foreach(KeyValuePair<string,float> s in _scores) {
            scores[CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.Key).Replace(" ", "_")] = s.Value;
        }

        UpdateType();
    }

    public void SetId(string id) {
        if (this.id == "")
            this.id = id;
    }

    public void SetRoom(SemanticRoom room) {
        if(room != null && this.room == null) {
            this.room = room;
            OntologySystem.instance.ObjectInRoom(id,room.id);
        }
    }

    public void UpdateType() {
        type = scores.OrderByDescending(x => x.Value).FirstOrDefault().Key;
        score = scores[type] / nDetections;
    }

    public void NewDetection(SemanticObject newDetection) {

        SemanticObject oldObj = GetDeepCopy();

        if (newDetection != null) {        

            // Update bounding box
            position = (nDetections * position + newDetection.position) / (nDetections + 1);
            size = (nDetections * size + newDetection.size) / (nDetections + 1);

            Vector3 r = rotation.eulerAngles;
            r = (nDetections * r + newDetection.rotation.eulerAngles) / (nDetections + 1);
            rotation = Quaternion.Euler(r);

            // Update scores
            foreach (KeyValuePair<string, float> s in newDetection.scores)
            {
                string name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.Key).Replace(" ", "_");
                if (!scores.Keys.Contains(name))
                {
                    Debug.LogWarning(name);
                }
                scores[name] += s.Value;
            }

            OntologySystem.instance.JoinSemanticObject(this, newDetection);
        }
        else {
            scores["Other"] += 0.8f;
        }

        UpdateType();

        // Update ontology
        OntologySystem.instance.UpdateObject(oldObj, this);
        nDetections++;

    }

    

    //public static Dictionary<string,float> Normalize(Dictionary<string, float> dictionary) {
    //    Dictionary<string, float> result = dictionary;
    //    float sum = dictionary.Values.Sum();

    //    if (!dictionary.ContainsKey("Default")) {
    //        if (sum >= 1) {
    //            result.Add("Default",0.01f);
    //        } else {
    //            result.Add("Default",1-sum);
    //        }
    //        sum = result.Values.Sum();
    //    }

    //    foreach (string type in result.Keys.ToList()) {
    //        result[type] /= sum;
    //    }
    //    return result;
    //}

    public override string ToString() {
        return "SemanticObject [id =" + id
                        + ", scores=" + score
                        + ", pose=" + position.ToString()
                        + ", nDetections=" + nDetections
                        + ", size=" + size.ToString() + "]";
    }

    public string GetIdRoom() {
        if (room != null)
            return room.id;
        else
            return "";
    }

    public SemanticObject GetDeepCopy() {
        SemanticObject newSO = new SemanticObject(scores, position, rotation, size);
        newSO.SetId(id);
        return newSO;
    }
}
