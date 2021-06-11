using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using g3;

public class SemanticObject {
    public Dictionary<string, float> Scores { get; private set; }
    public List<Corner> Corners { get; private set; }

    public string Id { get; private set; }
    public string Type { get; private set; }
    public float Score { get; private set; }
    public Vector3 Position { get; private set; }
    public Vector3 Size { get; private set; }
    public Quaternion Rotation { get; private set; }
    public int NDetections { get; private set; }
    public int NNonOccluded { get; private set; }
    public SemanticRoom Room { get; private set; }

    public struct Corner {
        public Vector3 position;
        public bool occluded;
        public Corner(Vector3 position, bool occluded) {
            this.position = position;
            this.occluded = occluded;
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
        // Update bounding box
        Position = new Vector3(Corners.Average(p => p.position.x), Corners.Average(p => p.position.y), Corners.Average(p => p.position.z));
        Size = new Vector3(Mathf.Abs(Vector3.Distance(Corners[0].position, Corners[3].position)),
                           Mathf.Abs(Vector3.Distance(Corners[0].position, Corners[2].position)),
                           Mathf.Abs(Vector3.Distance(Corners[0].position, Corners[1].position)));
        Rotation = Quaternion.Euler(0, Mathf.Atan2(Corners[0].position.x - Corners[1].position.x, Corners[0].position.z - Corners[1].position.z) * Mathf.Rad2Deg, 0);

        // Update type
        Type = Scores.OrderByDescending(x => x.Value).FirstOrDefault().Key;
        Score = Scores[Type] / NDetections;

        NNonOccluded = 0;
        foreach (Corner c in Corners)
        {
            if (c.occluded) NNonOccluded++;
        }
    }

    public void NewDetection(SemanticObject newDetection) {

        //if (NDetections > 1)
        //{
        //    OntologySystem.instance.RemoveSemanticObject(this);
        //}

        if (newDetection != null)
        {

           
            // Update corners
            for (int i = 0; i < Corners.Count; i++)
            {

                if (DefinedObject()) {
                    if (newDetection.DefinedObject()) {
                        Corners[i] = new Corner((Corners[i].position + newDetection.Corners[i].position) / 2, Corners[i].occluded && newDetection.Corners[i].occluded);
                    }
                }else {
                    if (newDetection.DefinedObject())
                    {
                        Corners = newDetection.Corners;
                    }
                    else
                    {
                        Vector3d[] points = new Vector3d[16];

                        float maxZ = Corners[0].position.y;
                        float minZ = Corners[0].position.y;

                        for (int k = 0; k < Corners.Count; k++)
                        {
                            if (Corners[k].position.y > maxZ) maxZ = Corners[k].position.y;
                            if (Corners[k].position.y < minZ) minZ = Corners[k].position.y;
                            if (newDetection.Corners[k].position.y > maxZ) maxZ = newDetection.Corners[k].position.y;
                            if (newDetection.Corners[k].position.y < minZ) minZ = newDetection.Corners[k].position.y;
                        }

                        float middle = (maxZ - minZ) / 2.0f;
                        for (int j = 0; j < Corners.Count; j++)
                        {
                            if (Corners[j].position.y > middle)
                            {
                                points[j] = new Vector3d(Corners[j].position.x, maxZ, Corners[j].position.z);
                            }
                            else
                            {
                                points[j] = new Vector3d(Corners[j].position.x, minZ, Corners[j].position.z);
                            }

                            if (newDetection.Corners[j].position.y > middle)
                            {
                                points[j + Corners.Count] = new Vector3d(newDetection.Corners[j].position.x, maxZ, newDetection.Corners[j].position.z);
                            }
                            else
                            {
                                points[j + Corners.Count] = new Vector3d(newDetection.Corners[j].position.x, minZ, newDetection.Corners[j].position.z);
                            }

                        }

                        var orientedBB = new ContOrientedBox3(points);

                        Vector3d[] newCorners = orientedBB.Box.ComputeVertices();

                        Debug.Log("Axis X");
                        Debug.Log(orientedBB.Box.AxisX);
                        Debug.Log("Axis Y");
                        Debug.Log(orientedBB.Box.AxisY);
                        Debug.Log("Axis Z");
                        Debug.Log(orientedBB.Box.AxisZ);

                        for (int l = 0; l < 8; l++)
                        {
                            Debug.Log(newCorners[l]);
                        }


                        Corners[i] = new Corner((Corners[i].position + newDetection.Corners[i].position) / 2, Corners[i].occluded);
                    }
                    
            }



        }

            // Update scores
            foreach (KeyValuePair<string, float> s in newDetection.Scores)
            {
                Scores[CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.Key).Replace(" ", "_")] += s.Value;
            }

            OntologySystem.instance.JoinSemanticObject(this, newDetection);

        }
        else
        {
            Scores["Other"] += 0.4f;
        }

        NDetections++;
        UpdateProperties();

        // Update ontology
        //if (NDetections == 2)
        //{
        //    string oldId = Id;
        //    Id = "";
        //    Id = OntologySystem.instance.AddNewDetectedObject(this).Id;
        //    OntologySystem.instance.JoinSemanticObject(Id, oldId);
        //}
        //else
        //{
        //    OntologySystem.instance.AddNewDetectedObject(this);
        //}

    }

    public bool DefinedObject() {
        return (!Corners[0].occluded && !Corners[1].occluded &&
            (!Corners[7].occluded || !Corners[2].occluded || !Corners[4].occluded || !Corners[5].occluded) &&
            (!Corners[4].occluded || !Corners[5].occluded || !Corners[6].occluded || !Corners[3].occluded)) ;
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

}
