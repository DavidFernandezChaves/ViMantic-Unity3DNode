using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

public class SemanticObject {
    public Dictionary<string, float> Scores { get; private set; }
    public List<Corner> Corners { get; private set; }

    public float noDetectionProb = 0.4f;
    public float erosion = 0.01f;
    public string Id { get; private set; }
    public string Type { get; private set; }
    public float Score { get; private set; }
    public Vector3 Position { get; private set; }
    public Vector3 Size { get; private set; }
    public Quaternion Rotation { get; private set; }
    public int NDetections { get; private set; }
    public int NNonOccluded { get; private set; }
    public SemanticRoom Room { get; private set; }
    public bool Defined = false;

    private int nOccludedDetection = 0;

    public struct Corner {
        public Vector3 position;
        public bool occluded { get; private set; }
    public Corner(Vector3 position, bool occluded) {
            this.position = position;
            this.occluded = occluded;
        }
        public void SetOccluded(bool mode)
        {
            occluded = mode;
        }
    }
    public SemanticObject(Dictionary<string, float> _scores, List<Vector3> _corners, byte _occluded_corners) {
        Id = "";
        Corners = new List<Corner>();
        for(int i = 0; i < _corners.Count; i++) {
            Corners.Add(new Corner(_corners[i], (_occluded_corners & (1 << i)) > 0));
        }

        NDetections = 1;
        Scores = new Dictionary<string, float>();
        float defaultValue =  (1-Scores.Values.Sum()) / OntologySystem.instance.objectClassInOntology.Count;

        Scores.Add("Other", defaultValue);
        foreach(string objectClass in OntologySystem.instance.objectClassInOntology) {
            Scores.Add(objectClass, defaultValue);
        }

        foreach(KeyValuePair<string,float> s in _scores) {
            Scores[CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.Key).Replace(" ", "_")] = s.Value;
        }

        UpdateProperties();
    }

    public SemanticObject(Dictionary<string, float> _scores, List<Corner> _corners)
    {
        Id = "";
        Scores = _scores;
        Corners = _corners;
        NDetections = 1;

        UpdateProperties();
    }

    public void SetNewCorners(List<Corner> corners) {
        this.Corners = corners;
    }

    public void SetId(string id) {
        if (this.Id == "")
            this.Id = id;
    }

    public void SetRoom(SemanticRoom room) {
        if(room != null && this.Room == null) {
            this.Room = room;
            OntologySystem.instance.ObjectInRoom(Id,room.id);
        }
    }

    public void UpdateProperties() {

        Position = new Vector3(Corners.Average(p => p.position.x), Corners.Average(p => p.position.y), Corners.Average(p => p.position.z));

        float heightAv = (Vector3.Distance(Corners[0].position, Corners[2].position)+
                                    Vector3.Distance(Corners[1].position, Corners[7].position)+
                                    Vector3.Distance(Corners[6].position, Corners[4].position)+
                                    Vector3.Distance(Corners[3].position, Corners[5].position))/4;

        float widthAv = (Vector3.Distance(Corners[0].position, Corners[1].position)+
                                    Vector3.Distance(Corners[3].position, Corners[6].position)+
                                    Vector3.Distance(Corners[2].position, Corners[7].position)+
                                    Vector3.Distance(Corners[4].position, Corners[5].position))/4;

        float deepAv = (Vector3.Distance(Corners[0].position, Corners[3].position)+
                                    Vector3.Distance(Corners[1].position, Corners[6].position)+
                                    Vector3.Distance(Corners[7].position, Corners[4].position)+
                                    Vector3.Distance(Corners[2].position, Corners[5].position))/4;

        Size = new Vector3(deepAv, heightAv,widthAv);

        float angleAv = Mathf.Atan2(Corners[1].position.x - Corners[0].position.x, Corners[1].position.z - Corners[0].position.z) * Mathf.Rad2Deg +
                        Mathf.Atan2(Corners[6].position.x - Corners[3].position.x, Corners[6].position.z - Corners[3].position.z) * Mathf.Rad2Deg +
                        Mathf.Atan2(Corners[7].position.x - Corners[2].position.x, Corners[7].position.z - Corners[2].position.z) * Mathf.Rad2Deg +
                        Mathf.Atan2(Corners[4].position.x - Corners[5].position.x, Corners[4].position.z - Corners[5].position.z) * Mathf.Rad2Deg;


        Rotation = Quaternion.Euler(0, angleAv/4, 0);


        // Update type
        Type = Scores.OrderByDescending(x => x.Value).FirstOrDefault().Key;
        Score = Scores[Type] / NDetections;

        Defined = (!Corners[0].occluded &&
                   !Corners[1].occluded &&
                   (!Corners[7].occluded || !Corners[2].occluded || !Corners[4].occluded || !Corners[5].occluded) &&
                   (!Corners[4].occluded || !Corners[5].occluded || !Corners[6].occluded || !Corners[3].occluded)) || Defined;

        if (Defined)
        {
            Corners.ForEach(c => c.SetOccluded(false));
            NNonOccluded = 8;
        }
        else
        {
            NNonOccluded = Corners.FindAll(c => !c.occluded).Count;
        }

    }

    public void NewDetection(SemanticObject newDetection) {

        if (NDetections > 1)
        {
            OntologySystem.instance.RemoveSemanticObject(this);
        }

        if (newDetection != null)
        {
            if (Defined) {
                if (newDetection.Defined) {
                    nOccludedDetection = 0;
                    float nPreviousDetections = Mathf.Min(NDetections, 20f);
                    for (int i = 0; i < Corners.Count; i++)
                        Corners[i] = new Corner((nPreviousDetections * Corners[i].position + newDetection.Corners[i].position) / (nPreviousDetections + 1f), false);
                } else {
                    nOccludedDetection++;
                    if (nOccludedDetection > 20f) {
                        nOccludedDetection = 0;
                        for (int i = 0; i < Corners.Count; i++)
                            Corners[i] = new Corner(Corners[i].position, true);
                    }
                }
            } else {
                if (newDetection.Defined) {
                    Corners = newDetection.Corners;
                } else {
                    JointBB(newDetection.Corners);  
                }

            }
            // Update scores
            foreach (KeyValuePair<string, float> s in newDetection.Scores)
            {
                Scores[CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.Key).Replace(" ", "_")] += s.Value;
            }

            OntologySystem.instance.JoinSemanticObject(this, newDetection);
            NDetections += newDetection.NDetections;
            UpdateProperties();

            // Update ontology
            if (NDetections == 2)
            {
                string oldId = Id;
                Id = "";
                Id = OntologySystem.instance.AddNewDetectedObject(this).Id;
                OntologySystem.instance.JoinSemanticObject(Id, oldId);
                OntologySystem.instance.JoinSemanticObject(Id, newDetection.Id);
            }
            else
            {
                OntologySystem.instance.AddNewDetectedObject(this);
                OntologySystem.instance.JoinSemanticObject(Id, newDetection.Id);
            }
        }
        else
        {
            Scores["Other"] += noDetectionProb;
            float defaultValue = (1f - noDetectionProb) / (Scores.Count - 1);
            foreach (string key in OntologySystem.instance.objectClassInOntology)
            {   
                Scores[CultureInfo.InvariantCulture.TextInfo.ToTitleCase(key).Replace(" ", "_")] += defaultValue;
            }
            NDetections++;
            UpdateProperties();

            // Update ontology
            OntologySystem.instance.AddNewDetectedObject(this);
        }

    }

    public void JointBB(List<Corner> newBB) {
        float maxY = Mathf.Max(Corners[2].position.y, newBB[2].position.y);
        float minY = Mathf.Min(Corners[0].position.y, newBB[0].position.y);

        //Get Top Corners
        Vector3[] points = new Vector3[8] {
                        Corners[2].position,
                        Corners[4].position,
                        Corners[5].position,
                        Corners[7].position,
                        newBB[2].position,
                        newBB[4].position,
                        newBB[5].position,
                        newBB[7].position
                    };

        //Get best angle with minimun area
        float[] rectangle = CalculateRectangleCorners(points);
        float[] best_rectangle = rectangle;
        float best_area = (rectangle[1] - rectangle[0]) * (rectangle[3] - rectangle[2]);
        float best_angle = 0;
        for (float angle = 1; angle < 90; angle += 1) {

            rectangle = CalculateRectangleCorners(points.Select(r => Quaternion.Euler(0, angle, 0) * r).ToArray());
            float area = (rectangle[1] - rectangle[0]) * (rectangle[3] - rectangle[2]);

            if (area < best_area) {
                best_area = area;
                best_rectangle = rectangle;
                best_angle = angle;
            }
        }        

        //min X, max X, min Y, max Y
        Vector3[] newBox = new Vector3[8] {
                        new Vector3(best_rectangle[1] - erosion, minY + erosion, best_rectangle[2] + erosion),
                        new Vector3(best_rectangle[0] + erosion, minY + erosion, best_rectangle[2] + erosion),
                        new Vector3(best_rectangle[1] - erosion, maxY - erosion, best_rectangle[2] + erosion),
                        new Vector3(best_rectangle[1] - erosion, minY + erosion, best_rectangle[3] - erosion),
                        new Vector3(best_rectangle[0] + erosion, maxY - erosion, best_rectangle[3] - erosion),
                        new Vector3(best_rectangle[1] - erosion, maxY - erosion, best_rectangle[3] - erosion),
                        new Vector3(best_rectangle[0] + erosion, minY + erosion, best_rectangle[3] - erosion),
                        new Vector3(best_rectangle[0] + erosion, maxY - erosion, best_rectangle[2] + erosion)
                    };

        newBox = newBox.Select(r => Quaternion.Euler(0, -best_angle, 0) * r).ToArray();

        for (int i = 0; i < Corners.Count; i++) {
            Corners[i] = new Corner(newBox[i], true);
        }
    }

    public string GetIdRoom() {
        if (Room != null)
            return Room.id;
        else
            return "";
    }

    public SemanticObject GetDeepCopy() {
        SemanticObject newSO = new SemanticObject(Scores, Corners);
        newSO.SetId(Id);
        return newSO;
    }

    public override string ToString() {
        return "SemanticObject [id =" + Id
                        + ", scores=" + Score
                        + ", pose=" + Position.ToString()
                        + ", nDetections=" + NDetections
                        + ", size=" + Size.ToString() + "]";
    }

    static float[] CalculateRectangleCorners(Vector3[] corners) {
        //min X, max X, min Y, max Y

        float[] rectangleCorners = new float[4] { corners[0].x, corners[1].x, corners[2].z, corners[3].z } ;

        for (int i = 1; i < corners.Length; i++) {
            if (corners[i].x < rectangleCorners[0]) rectangleCorners[0] = corners[i].x;
            if (corners[i].x > rectangleCorners[1]) rectangleCorners[1] = corners[i].x;
            if (corners[i].z < rectangleCorners[2]) rectangleCorners[2] = corners[i].z;
            if (corners[i].z > rectangleCorners[3]) rectangleCorners[3] = corners[i].z;
        }

        return rectangleCorners;

    }

}
