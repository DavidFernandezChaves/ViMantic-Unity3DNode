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
        // Update bounding box
        position = new Vector3(corners.Average(p => p.x), corners.Average(p => p.y), corners.Average(p => p.z));
        size = new Vector3(Mathf.Abs(Vector3.Distance(corners[1], corners[0])),
                           Mathf.Abs(Vector3.Distance(corners[1], corners[5])),
                           Mathf.Abs(Vector3.Distance(corners[1], corners[2])));
        rotation = Quaternion.Euler(0, Mathf.Atan2(corners[1].x - corners[2].x, corners[1].z - corners[2].z) * Mathf.Rad2Deg, 0);

        // Update type
        type = scores.OrderByDescending(x => x.Value).FirstOrDefault().Key;
        score = scores[type] / nDetections;
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

                // Update corners
                for (int i = 0; i < 8; i++)
                {

                    bool cornerFixed1 = (fixed_corners & (1 << i)) > 0;
                    bool cornerFixed2 = (so.fixed_corners & (1 << i)) > 0;

                    if (cornerFixed1 && cornerFixed2 || !cornerFixed1 && !cornerFixed2)
                    {
                        corners[i] = (corners[i] + so.corners[i]) / 2;
                    }
                    else if (!cornerFixed1 && cornerFixed2)
                    {
                        corners[i] = so.corners[i];
                    }

                }

                // Update scores
                foreach (KeyValuePair<string, float> s in so.scores)
                {
                    scores[CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.Key).Replace(" ", "_")] += s.Value;
                }

                nDetections += so.nDetections;
            }

            // Update corners
            for (int i = 0; i < 8; i++)
            {

                bool cornerFixed1 = (fixed_corners & (1 << i)) > 0;
                bool cornerFixed2 = (newDetection.fixed_corners & (1 << i)) > 0;

                if (cornerFixed1 && cornerFixed2 || !cornerFixed1 && !cornerFixed2)
                {
                    corners[i] = (corners[i] + newDetection.corners[i]) / 2;
                }
                else if (!cornerFixed1 && cornerFixed2)
                {
                    corners[i] = newDetection.corners[i];
                }

            }

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

    public string GetIdRoom() {
        if (room != null)
            return room.id;
        else
            return "";
    }

    public SemanticObject GetDeepCopy() {
        SemanticObject newSO = new SemanticObject(scores, corners, fixed_corners);
        newSO.SetId(id);
        return newSO;
    }

    public override string ToString() {
        return "SemanticObject [id =" + id
                        + ", scores=" + score
                        + ", pose=" + position.ToString()
                        + ", nDetections=" + nDetections
                        + ", size=" + size.ToString() + "]";
    }

}
