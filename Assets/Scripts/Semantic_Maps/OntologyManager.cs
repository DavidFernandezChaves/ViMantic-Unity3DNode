using UnityEngine;
using RDFSharp.Semantics;
using RDFSharp.Model;
using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using RDFSharp.Semantics.Reasoner;
using RDFSharp.Query;
using System.Data;
using System.Linq;



public class OntologyManager : MonoBehaviour
{
    public int verbose;
    public bool saveOntology;
    public string prefix = "MAPIR";
    public string masterURI = "http://mapir.isa.uma.es/";
    public string path = @"D:\SemanticMap";

    private RDFNamespace nameSpace;
    private RDFOntology ontology;
    private string raidID;
    private RDFOntologyFact raidFact, houseFact;
    private List<string> objectClassInOntology;
    private List<string> interestClass;
    public List<string> cateogiesOfRooms;
    private Dictionary<string, Dictionary<string, float>> probabilityRoomByClass;
    private SemanticRoomManager semanticRoomManager;
    private ObjectManager semanticMapping;

    #region Unity Functions
    private void Start() {
        semanticRoomManager = GetComponent<SemanticRoomManager>();
        semanticMapping = GetComponent<ObjectManager>();
    }

    private void OnDestroy() {
        if (saveOntology) {
            SaveOntology();
        }
    }
    #endregion

    #region Public Functions

    public void LoadOntology() {
        // CREATE NAMESPACE
        nameSpace = new RDFNamespace(prefix, masterURI);
        raidID = GetNewTimeID();
        var time = Time.realtimeSinceStartup;
        ontology = RDFOntology.FromRDFGraph(RDFSharp.Model.RDFGraph.FromFile(RDFModelEnums.RDFFormats.RdfXml, path));

        raidFact = new RDFOntologyFact(GetClassResource(raidID));
        ontology.Data.AddFact(raidFact);
        ontology.Data.AddClassTypeRelation(raidFact, new RDFOntologyClass(GetClassResource("Raid")));
        UpdateListOfObjectsClassInOntology();
        GetProbabilityRoomByClass();
        cateogiesOfRooms = GetCategoriesOfRooms();
        Log("Ontology loading time: " + (Time.realtimeSinceStartup - time).ToString());
    }

    public void SaveOntology() {
        if (ontology != null) {
            //// CREATE A REASONER AND APPLY IT ON ONTOLOGY
            //var rep = RDFOntologyReasoner
            //            .CreateNew()
            //                .WithRule(RDFOntologyReasonerRuleset.SubClassTransitivity)
            //                .WithRule(RDFOntologyReasonerRuleset.SubPropertyTransitivity)
            //                .WithRule(RDFOntologyReasonerRuleset.DomainEntailment)
            //                .WithRule(RDFOntologyReasonerRuleset.RangeEntailment)
            //                .WithRule(RDFOntologyReasonerRuleset.ClassTypeEntailment)
            //                .WithRule(RDFOntologyReasonerRuleset.PropertyEntailment)
            //                .WithRule(RDFOntologyReasonerRuleset.DifferentFromEntailment)
            //                .WithRule(RDFOntologyReasonerRuleset.DisjointWithEntailment)
            //                .WithRule(RDFOntologyReasonerRuleset.EquivalentClassTransitivity)
            //                .WithRule(RDFOntologyReasonerRuleset.EquivalentPropertyTransitivity)
            //                .WithRule(RDFOntologyReasonerRuleset.InverseOfEntailment)
            //                .WithRule(RDFOntologyReasonerRuleset.SameAsEntailment)
            //                .WithRule(RDFOntologyReasonerRuleset.SameAsTransitivity)
            //                .WithRule(RDFOntologyReasonerRuleset.SymmetricPropertyEntailment)
            //                .WithRule(RDFOntologyReasonerRuleset.TransitivePropertyEntailment)
            //                .ApplyToOntology(ref _ontology); //ontology must be passed by reference
            //                                                 // ITERATE OVER THE MATERIALIZED INFERENCES
            //foreach (var ev in rep)
            //{
            //    Debug.Log(ev.EvidenceProvenance + ";" + ev.EvidenceCategory + ";" + ev.EvidenceContent);
            //}

            ontology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData).ToFile(RDFModelEnums.RDFFormats.RdfXml, path);
            Log("Ontology saved");
        }
    }

    public void AddNewHouse(string name) {
        houseFact = new RDFOntologyFact(GetClassResource(name));
        ontology.Data.AddClassTypeRelation(houseFact, new RDFOntologyClass(GetClassResource("House")));
        ontology.Data.AddAssertionRelation(raidFact, new RDFOntologyObjectProperty(GetResource("recordedIn")), houseFact);
    }

    public SemanticObject AddNewDetectedObject(SemanticObject obj) {
        if (obj.ontologyID.Equals("")) {
            obj.ontologyID = GetNewTimeID() + "_" + obj.type;
        }
        var newDetectedObject = new RDFOntologyFact(GetClassResource(obj.ontologyID));
        ontology.Data.AddFact(newDetectedObject);
        ontology.Data.AddClassTypeRelation(newDetectedObject, new RDFOntologyClass(GetClassResource(obj.type)));
        ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(GetResource("position")), new RDFOntologyLiteral(new RDFPlainLiteral(obj.pose.ToString())));
        ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(GetResource("rotation")), new RDFOntologyLiteral(new RDFPlainLiteral(obj.rotation.eulerAngles.ToString())));
        ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(GetResource("score")), new RDFOntologyLiteral(new RDFPlainLiteral(obj.confidenceScore.ToString())));
        ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(GetResource("nDetections")), new RDFOntologyLiteral(new RDFPlainLiteral(obj.nDetections.ToString())));
        ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(GetResource("size")), new RDFOntologyLiteral(new RDFPlainLiteral(obj.size.ToString())));
        ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyObjectProperty(GetResource("recordedIn")), raidFact);

        return obj;
    }

    public void RemoveSemanticObject(SemanticObject obj) {
        var objectFact = ontology.Data.SelectFact(GetNameWithURI(obj.ontologyID));
        ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("position")), new RDFOntologyLiteral(new RDFPlainLiteral(obj.pose.ToString())));
        ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("rotation")), new RDFOntologyLiteral(new RDFPlainLiteral(obj.rotation.eulerAngles.ToString())));
        ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("score")), new RDFOntologyLiteral(new RDFPlainLiteral(obj.confidenceScore.ToString())));
        ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("nDetections")), new RDFOntologyLiteral(new RDFPlainLiteral(obj.nDetections.ToString())));
        ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("size")), new RDFOntologyLiteral(new RDFPlainLiteral(obj.size.ToString())));
        ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyObjectProperty(GetResource("recordedIn")), raidFact);
        if (obj.semanticRoom != null)
            ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("isIn")), new RDFOntologyLiteral(new RDFPlainLiteral(obj.semanticRoom.ToString())));
        ontology.Data.RemoveFact(objectFact);
    }

    public void JoinSemanticObject(SemanticObject father, SemanticObject child) {
        ontology.Data.AddAssertionRelation(new RDFOntologyFact(GetClassResource(child.ontologyID)),
                                            new RDFOntologyObjectProperty(GetResource("isPartOf")),
                                            new RDFOntologyFact(GetClassResource(father.ontologyID)));
    }

    public void RemoveSemanticObjectUnion(SemanticObject father, SemanticObject child) {
        ontology.Data.RemoveAssertionRelation(new RDFOntologyFact(GetClassResource(child.ontologyID)),
                                                new RDFOntologyObjectProperty(GetResource("isPartOf")),
                                                new RDFOntologyFact(GetClassResource(father.ontologyID)));
    }

    public SemanticObject UpdateNDetections(SemanticObject obj, int nDetection) {
        var objectFact = new RDFOntologyFact(GetClassResource(obj.ontologyID));
        ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("nDetections")),
                                        new RDFOntologyLiteral(new RDFPlainLiteral(obj.nDetections.ToString())));

        ontology.Data.AddAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("nDetections")),
                                            new RDFOntologyLiteral(new RDFPlainLiteral(nDetection.ToString())));
        obj.nDetections = nDetection;
        return obj;
    }


    public SemanticObject UpdateObject(SemanticObject obj, Vector3 pose, Quaternion rotation, Vector3 size, double score, int nDetection) {
        var objectFact = new RDFOntologyFact(GetClassResource(obj.ontologyID));

        //Update pose
        ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("position")),
                                                new RDFOntologyLiteral(new RDFPlainLiteral(obj.pose.ToString())));

        ontology.Data.AddAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("position")),
                                            new RDFOntologyLiteral(new RDFPlainLiteral(pose.ToString())));

        obj.pose = pose;

        //Update rotation

        ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("rotation")),
                                                new RDFOntologyLiteral(new RDFPlainLiteral(obj.rotation.eulerAngles.ToString())));

        ontology.Data.AddAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("rotation")),
                                            new RDFOntologyLiteral(new RDFPlainLiteral(rotation.eulerAngles.ToString())));

        obj.rotation = rotation;

        //Update size

        ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("size")),
                                        new RDFOntologyLiteral(new RDFPlainLiteral(obj.size.ToString())));

        ontology.Data.AddAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("size")),
                                            new RDFOntologyLiteral(new RDFPlainLiteral(size.ToString())));

        obj.size = size;

        //Update score

        ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("score")),
                                new RDFOntologyLiteral(new RDFPlainLiteral(obj.confidenceScore.ToString())));

        ontology.Data.AddAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("score")),
                                            new RDFOntologyLiteral(new RDFPlainLiteral(score.ToString())));

        obj.confidenceScore = score;

        //Update nDetections

        ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("nDetections")),
                                                new RDFOntologyLiteral(new RDFPlainLiteral(obj.nDetections.ToString())));

        ontology.Data.AddAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("nDetections")),
                                            new RDFOntologyLiteral(new RDFPlainLiteral(nDetection.ToString())));

        obj.nDetections = nDetection;

        return obj;
    }

    public bool CheckClassObject(string class_object) {
        return objectClassInOntology.Contains(GetNameWithURI(class_object));
    }

    public void AddNewRoom(string id, string typeRoom) {
        var newSemanticRoom = new RDFOntologyFact(GetClassResource(id));
        ontology.Data.AddFact(newSemanticRoom);
        ontology.Data.AddClassTypeRelation(newSemanticRoom, new RDFOntologyClass(GetClassResource(typeRoom)));
        ontology.Data.AddAssertionRelation(newSemanticRoom, new RDFOntologyObjectProperty(GetResource("recordedIn")), raidFact);
        ontology.Data.AddAssertionRelation(newSemanticRoom, new RDFOntologyObjectProperty(GetResource("isPartOf")), houseFact);
    }

    // <------- Utils
    public string GetNameWithURI(string name) {
        return nameSpace + CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name).Replace(" ", "_");
    }

    public string GetNameWithoutURI(string name) {
        return name.Substring(nameSpace.ToString().Length);
    }

    public RDFResource GetClassResource(String name) {
        return new RDFResource(GetNameWithURI(name));
    }

    public RDFResource GetResource(String name) {
        return new RDFResource(nameSpace + name);
    }

    public string GetNewTimeID() {
        return System.DateTime.Now.ToString("yyyyMMddHHmmssfff").ToString();
    }

    public static Vector3 StringToVector3(string sVector) {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")")) {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }

    public static Quaternion StringToQuaternion(string sQuaternion) {
        // Remove the parentheses
        if (sQuaternion.StartsWith("(") && sQuaternion.EndsWith(")")) {
            sQuaternion = sQuaternion.Substring(1, sQuaternion.Length - 2);
        }

        // split the items
        string[] sArray = sQuaternion.Split(',');

        // store as a Quaternion
        Quaternion result = Quaternion.Euler(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }

    public bool CheckInteresObject(string typeObject) {
        return interestClass.Contains(GetNameWithURI(typeObject));
    }

    public void ObjectInRoom(SemanticObject semanticObject) {
        var obj = new RDFOntologyFact(GetClassResource(semanticObject.ontologyID));
        ontology.Data.AddAssertionRelation(obj, new RDFOntologyObjectProperty(GetResource("isIn")), new RDFOntologyFact(GetClassResource(semanticObject.semanticRoom.id)));
    }

    public List<String> GetCategoriesOfRooms() {
        List<String> categoreies = new List<string>();
        RDFVariable typesRooms = new RDFVariable("TYPESROOM");
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(typesRooms, RDFVocabulary.RDFS.SUB_CLASS_OF, GetClassResource("Room"))))
            .AddProjectionVariable(typesRooms);

        RDFSelectQueryResult resultDetectedObject = query.ApplyToGraph(ontology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData));
        categoreies = (from r in resultDetectedObject.SelectResults.AsEnumerable() select GetNameWithoutURI(r["?TYPESROOM"].ToString())).Distinct().ToList();
        return categoreies;
    }

    public Dictionary<string, float> GetProbabilityCategories(List<SemanticObject> detectedClasses) {

        Dictionary<string, float> rooms = new Dictionary<string, float>();

        float total = 0;
        foreach (string categoryRoom in cateogiesOfRooms) {
            var probability = GetProbabilityCategory(categoryRoom, new List<SemanticObject>(detectedClasses), new List<SemanticObject>());
            rooms.Add(categoryRoom, probability);
            total += probability;
        }
        if (total != 0) {
            foreach (string category in cateogiesOfRooms) {
                rooms[category] /= total;
            }
        }


        return rooms;
    }

    public float GetProbabilityCategory(string categoryRoom, List<SemanticObject> detectedClasses, List<SemanticObject> previousClass) {
        if (detectedClasses.Count == 0)
            return 0;

        float probability = 0;
        SemanticObject detection = detectedClasses[0];
        detectedClasses.Remove(detection);
        previousClass.Add(detection);

        foreach (string classObject in interestClass) {
            var total = (interestClass.Count - 1) * 0.1f;
            float P1 = 0.1f / total;

            if (classObject.Equals(detection.type)) {
                P1 = (float)detection.confidenceScore / total;
            }

            float P2 = 0f;

            if (detectedClasses.Count == 0) {
                P2 = GetProbabilityCategory(categoryRoom, previousClass);
            } else {
                P2 = GetProbabilityCategory(categoryRoom, detectedClasses, previousClass);
            }

            probability += P1 * P2;
        }

        return probability;
    }

    public float GetProbabilityCategory(string categoryRoom, List<SemanticObject> previousClass) {
        float probability = 1;
        float probabilitytotal = 0;
        foreach (KeyValuePair<string, Dictionary<string, float>> category in probabilityRoomByClass) {
            var probabilityByCategory = 1f;
            foreach (SemanticObject detection in previousClass) {
                probabilityByCategory *= category.Value[GetNameWithURI(detection.type)];
            }
            probabilitytotal += probabilityByCategory;
            if (category.Key.Equals(categoryRoom))
                probability = probabilityByCategory;
        }
        return probability / probabilitytotal;
    }

    public List<SemanticObject> GetPreviousDetections(string room) {

        List<SemanticObject> result = semanticMapping.virtualSemanticMap.FindAll(obj => obj.GetIdRoom().Equals(room) && CheckInteresObject(obj.type));

        result.Sort(
            delegate (SemanticObject p1, SemanticObject p2) {
                int compareDate = p2.nDetections.CompareTo(p1.nDetections);
                if (compareDate == 0) {
                    return p2.confidenceScore.CompareTo(p1.confidenceScore);
                }
                return compareDate;
            }
        );

        return result;
    }

    //public List<SemanticObject> GetSemanticObjectsInTheRoom(string room) {

    //    RDFVariable id = new RDFVariable("ID");
    //    RDFVariable type = new RDFVariable("TYPE");
    //    RDFVariable score = new RDFVariable("SCORE");
    //    RDFVariable detections = new RDFVariable("DETECTIONS");
    //    RDFVariable position = new RDFVariable("POSITION");
    //    RDFVariable rotation = new RDFVariable("ROTATION");
    //    RDFVariable scale = new RDFVariable("SCALE");
    //    RDFVariable father = new RDFVariable("FATHER");

    //    RDFSelectQuery query = new RDFSelectQuery()
    //        .AddPatternGroup(new RDFPatternGroup("PG1")
    //            .AddPattern(new RDFPattern(id, RDFVocabulary.RDF.TYPE, type))
    //            .AddPattern(new RDFPattern(id, GetResource("position"), position))
    //            .AddPattern(new RDFPattern(id, GetResource("rotation"), rotation))
    //            .AddPattern(new RDFPattern(id, GetResource("score"), score))
    //            .AddPattern(new RDFPattern(id, GetResource("detections"), detections))
    //            .AddPattern(new RDFPattern(id, GetResource("size"), scale))
    //            .AddPattern(new RDFPattern(id, GetResource("isIn"), GetClassResource(room)))
    //            .AddPattern(new RDFPattern(type, RDFVocabulary.RDFS.SUB_CLASS_OF, GetClassResource("Furniture")).UnionWithNext())
    //            .AddPattern(new RDFPattern(type, RDFVocabulary.RDFS.SUB_CLASS_OF, GetClassResource("Appliance")))
    //            .AddPattern(new RDFPattern(id, GetResource("isPartOf"), father).Optional()))
    //        .AddProjectionVariable(id)
    //        .AddProjectionVariable(type)
    //        .AddProjectionVariable(score)
    //        .AddProjectionVariable(detections)
    //        .AddProjectionVariable(position)
    //        .AddProjectionVariable(rotation)
    //        .AddProjectionVariable(scale)
    //        .AddProjectionVariable(father);

    //    query.AddModifier(new RDFDistinctModifier());

    //    RDFSelectQueryResult resultDetectedObject = query.ApplyToGraph(_ontology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData));

    //    List<SemanticObject> result = new List<SemanticObject>();
    //    foreach (DataRow row in resultDetectedObject.SelectResults.AsEnumerable()) {
    //        //if (row.Field<string>("?FATHER") == null)
    //        //{
    //        //    result.Add(new SemanticObject(row.Field<string>("?ID"),
    //        //                                    row.Field<string>("?TYPE"),
    //        //                                    float.Parse(row.Field<string>("?SCORE")),
    //        //                                    int.Parse(row.Field<string>("?DETECTIONS")),
    //        //                                    StringToVector3(row.Field<string>("?POSITION")),
    //        //                                    StringToVector3(row.Field<string>("?SCALE")),
    //        //                                    StringToQuaternion(row.Field<string>("?ROTATION")),
    //        //                                    _semanticRoomManager._semantic_rooms.Find(r => r._id.Equals(room))
    //        //                                    ));
    //        //}
    //    }

    //    return result;
    //}
    #endregion

    #region Private Functions
    private void UpdateListOfObjectsClassInOntology() {
        RDFVariable c = new RDFVariable("CLASS");
        RDFVariable tc = new RDFVariable("TCLASS");
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(c, RDFVocabulary.RDFS.SUB_CLASS_OF, tc))
                .AddPattern(new RDFPattern(tc, RDFVocabulary.RDFS.SUB_CLASS_OF, GetClassResource("Object"))))
            .AddProjectionVariable(c);

        RDFSelectQueryResult resultDetectedObject = query.ApplyToGraph(ontology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData));
        objectClassInOntology = (from r in resultDetectedObject.SelectResults.AsEnumerable() select r["?CLASS"].ToString()).Distinct().ToList();
    }

    private void GetProbabilityRoomByClass() {
        RDFVariable objectClass = new RDFVariable("OBJECTCLASS");
        RDFVariable categoryRoom = new RDFVariable("CATEGORYROOM");
        RDFVariable Bnode = new RDFVariable("bnode");

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(objectClass, RDFVocabulary.RDFS.SUB_CLASS_OF, Bnode))
                .AddPattern(new RDFPattern(objectClass, RDFVocabulary.RDFS.SUB_CLASS_OF, GetClassResource("Furniture")).UnionWithNext())
                .AddPattern(new RDFPattern(objectClass, RDFVocabulary.RDFS.SUB_CLASS_OF, GetClassResource("Appliance")))
                .AddPattern(new RDFPattern(Bnode, RDFVocabulary.OWL.ON_PROPERTY, GetResource("isIn")))
                .AddPattern(new RDFPattern(Bnode, RDFVocabulary.OWL.SOME_VALUES_FROM, categoryRoom)))
            .AddProjectionVariable(objectClass)
            .AddProjectionVariable(categoryRoom);

        RDFSelectQueryResult resultDetectedObject = query.ApplyToGraph(ontology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData));
        interestClass = (from r in resultDetectedObject.SelectResults.AsEnumerable() select r["?OBJECTCLASS"].ToString()).Distinct().ToList();

        probabilityRoomByClass = new Dictionary<string, Dictionary<string, float>>();

        var _roomCategoriesInOntology = GetCategoriesOfRooms();

        foreach (string category in _roomCategoriesInOntology) {
            Dictionary<string, float> probabilityRoom = new Dictionary<string, float>();
            foreach (string objClass in interestClass) {
                List<string> posibilities = (from r in resultDetectedObject.SelectResults.AsEnumerable().Where(r => r.Field<string>("?OBJECTCLASS") == objClass) select GetNameWithoutURI(r["?CATEGORYROOM"].ToString())).ToList();

                if (posibilities.Contains(category)) {
                    probabilityRoom.Add(objClass, (float)0.9 / posibilities.Count);
                } else {
                    probabilityRoom.Add(objClass, (float)0.1 / (_roomCategoriesInOntology.Count - posibilities.Count));
                }

            }
            probabilityRoomByClass.Add(category, probabilityRoom);
        }

    }

    private void Log(string _msg) {
        if (verbose > 1)
            Debug.Log("[Ontology Manager]: " + _msg);
    }

    private void LogWarning(string _msg) {
        if (verbose > 0)
            Debug.LogWarning("[Ontology Manager]: " + _msg);
    }
    #endregion

}
