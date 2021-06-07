using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;

public class SemanticObject {
    public Dictionary<string, float> scores { get; private set; }
    public List<Vector3> corners { get; private set; }
    public byte fixed_corners { get; private set; }

    public string id { get; private set; }
    public string type { get; private set; }
    public float score { get; private set; }
    public Vector3 position { get; private set; }
    public Vector3 size { get; private set; }
    public Quaternion rotation { get; private set; }
    public int nDetections { get; private set; }
    public SemanticRoom room { get; private set; }


    public SemanticObject(Dictionary<string, float> _scores, List<Vector3> _corners, byte _fixed_corners) {
        id = "";
        corners = _corners;
        fixed_corners = _fixed_corners;
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

        UpdateProperties();
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

    public void UpdateProperties() {
        type = scores.OrderByDescending(x => x.Value).FirstOrDefault().Key;
        score = scores[type] / nDetections;

        position = new Vector3(corners.Average(p=>p.x), corners.Average(p => p.y), corners.Average(p => p.z));
        size = new Vector3(corners.Max(p => p.x)- corners.Min(p => p.x),size.y, corners.Max(p => p.z) - corners.Min(p => p.z)) ;
        float r = Vector3.Angle(corners[6], corners[3]);
        rotation = Quaternion.Euler(0, r, 0);
    }

    public void NewDetection(SemanticObject newDetection, List<VirtualObjectBox> matches = null) {

        if (nDetections > 1)
        {
            OntologySystem.instance.RemoveSemanticObject(this);
        }

        if (newDetection != null) {

            foreach (VirtualObjectBox vob in matches)
            {

                SemanticObject so = vob.semanticObject;

                //// Update bounding box
                //position = (nDetections * position + so.nDetections * so.position) / (nDetections + so.nDetections);
                //size = (nDetections * size + so.nDetections * so.size) / (nDetections + so.nDetections);

                //Vector3 eulerRotation = rotation.eulerAngles;
                //eulerRotation = (nDetections * eulerRotation + so.nDetections * so.rotation.eulerAngles) / (nDetections + so.nDetections);
                //rotation = Quaternion.Euler(eulerRotation);

                // Update scores
                foreach (KeyValuePair<string, float> s in so.scores)
                {
                    scores[CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.Key).Replace(" ", "_")] += s.Value;
                }

                nDetections += so.nDetections;
            }

            int historicProportion = Mathf.Min(nDetections, 50);

            // Update bounding box
            //position = (historicProportion * position + newDetection.position) / (historicProportion + 1);
            //size = (historicProportion * size + newDetection.size) / (historicProportion + 1);

            //Vector3 r = rotation.eulerAngles;
            //r = (historicProportion * r + newDetection.rotation.eulerAngles) / (historicProportion + 1);
            //rotation = Quaternion.Euler(r);

            // Update scores
            foreach (KeyValuePair<string, float> s in newDetection.scores)
            {
                scores[CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.Key).Replace(" ", "_")] += s.Value;
            }

            OntologySystem.instance.JoinSemanticObject(this, newDetection);

        }
        else {
            scores["Other"] += 0.4f;
        }

        nDetections++;
        UpdateProperties();

        // Update ontology
        if (nDetections == 2)
        {
            string oldId = id;
            id = "";
            id = OntologySystem.instance.AddNewDetectedObject(this).id;
            OntologySystem.instance.JoinSemanticObject(id, oldId);
        }
        else
        {
            OntologySystem.instance.AddNewDetectedObject(this);
        }
        

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
        SemanticObject newSO = new SemanticObject(scores,corners,fixed_corners);
        newSO.SetId(id);
        return newSO;
    }
}
