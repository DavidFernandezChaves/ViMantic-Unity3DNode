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

    public string _prefix = "MAPIR";
    public string _masterURI = "http://mapir.isa.uma.es/";
    public string _pathToSave = @"D:\";

    private RDFNamespace _nameSpace;
    private RDFOntology _ontology;
    private string _nameFile = "OntologyMapir.owl";
    private string _raidID;
    private RDFOntologyFact _raidFact, _houseFact;
    private List<string> _objectClassInOntology;
    private List<string> _interestClass;
    public List<string> _cateogiesOfRooms;
    private Dictionary<string, Dictionary<string, float>> _probabilityRoomByClass;
    private SemanticRoomManager _semanticRoomManager;
    private ObjectManager _semanticMapping;

    private void Start()
    {
        _semanticRoomManager = GetComponent<SemanticRoomManager>();
        _semanticMapping = GetComponent<ObjectManager>();
    }

    private void UpdateListOfObjectsClassInOntology()
    {
        RDFVariable c = new RDFVariable("CLASS");
        RDFVariable tc = new RDFVariable("TCLASS");
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(c, RDFVocabulary.RDFS.SUB_CLASS_OF, tc))
                .AddPattern(new RDFPattern(tc, RDFVocabulary.RDFS.SUB_CLASS_OF, GetClassResource("Object"))))
            .AddProjectionVariable(c);

        RDFSelectQueryResult resultDetectedObject = query.ApplyToGraph(_ontology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData));
        _objectClassInOntology = (from r in resultDetectedObject.SelectResults.AsEnumerable() select r["?CLASS"].ToString()).Distinct().ToList();
    }
   
    public void SaveOntology()
    {
        if (_ontology != null)
        {
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

            _ontology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData).ToFile(RDFModelEnums.RDFFormats.RdfXml, Path.Combine(_pathToSave, _nameFile));
            Debug.Log("Ontology saved");
        }
    }

    public void LoadOntology(string name)
    {
        // CREATE NAMESPACE
        _nameSpace = new RDFNamespace(_prefix, _masterURI);

        _nameFile = name + ".owl";
        _raidID = GetNewTimeID();
        var time = Time.realtimeSinceStartup;
        _ontology = RDFOntology.FromRDFGraph(RDFSharp.Model.RDFGraph.FromFile(RDFModelEnums.RDFFormats.RdfXml, Path.Combine(_pathToSave, _nameFile)));

        _raidFact = new RDFOntologyFact(GetClassResource(_raidID));
        _ontology.Data.AddFact(_raidFact);
        _ontology.Data.AddClassTypeRelation(_raidFact, new RDFOntologyClass(GetClassResource("Raid")));
        UpdateListOfObjectsClassInOntology();
        GetProbabilityRoomByClass();
        _cateogiesOfRooms = GetCategoriesOfRooms();
        Debug.Log("Ontology loading time: " + (Time.realtimeSinceStartup - time).ToString());
    }


    public void AddNewHouse(string name)
    {
        _houseFact = new RDFOntologyFact(GetClassResource(name));
        _ontology.Data.AddClassTypeRelation(_houseFact, new RDFOntologyClass(GetClassResource("House")));
        _ontology.Data.AddAssertionRelation(_raidFact, new RDFOntologyObjectProperty(GetResource("recordedIn")), _houseFact);
    }

    public SemanticObject AddNewDetectedObject(SemanticObject obj)
    {
        if (obj._ontologyID.Equals(""))
        {
            obj._ontologyID = GetNewTimeID() + "_" + obj._type;
        }
        var newDetectedObject = new RDFOntologyFact(GetClassResource(obj._ontologyID));
        _ontology.Data.AddFact(newDetectedObject);
        _ontology.Data.AddClassTypeRelation(newDetectedObject, new RDFOntologyClass(GetClassResource(obj._type)));
        _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(GetResource("position")), new RDFOntologyLiteral(new RDFPlainLiteral(obj._pose.ToString())));
        _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(GetResource("rotation")), new RDFOntologyLiteral(new RDFPlainLiteral(obj._rotation.eulerAngles.ToString())));
        _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(GetResource("score")), new RDFOntologyLiteral(new RDFPlainLiteral(obj._confidenceScore.ToString())));
        _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(GetResource("nDetections")), new RDFOntologyLiteral(new RDFPlainLiteral(obj._nDetections.ToString())));
        _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(GetResource("size")), new RDFOntologyLiteral(new RDFPlainLiteral(obj._size.ToString())));
        _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyObjectProperty(GetResource("recordedIn")), _raidFact);
        
        return obj;
    }

    public void RemoveSemanticObject(SemanticObject obj) {
        var objectFact = _ontology.Data.SelectFact(GetNameWithURI(obj._ontologyID));
        _ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("position")), new RDFOntologyLiteral(new RDFPlainLiteral(obj._pose.ToString())));
        _ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("rotation")), new RDFOntologyLiteral(new RDFPlainLiteral(obj._rotation.eulerAngles.ToString())));
        _ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("score")), new RDFOntologyLiteral(new RDFPlainLiteral(obj._confidenceScore.ToString())));
        _ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("nDetections")), new RDFOntologyLiteral(new RDFPlainLiteral(obj._nDetections.ToString())));
        _ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("size")), new RDFOntologyLiteral(new RDFPlainLiteral(obj._size.ToString())));
        _ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyObjectProperty(GetResource("recordedIn")), _raidFact);
        if (obj._semanticRoom != null)
            _ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("isIn")), new RDFOntologyLiteral(new RDFPlainLiteral(obj._semanticRoom.ToString())));
        _ontology.Data.RemoveFact(objectFact);       
    }

    public void JoinSemanticObject(SemanticObject father, SemanticObject child)
    {
        _ontology.Data.AddAssertionRelation(new RDFOntologyFact(GetClassResource(child._ontologyID)), 
                                            new RDFOntologyObjectProperty(GetResource("isPartOf")), 
                                            new RDFOntologyFact(GetClassResource(father._ontologyID)));
    }

    public void RemoveSemanticObjectUnion(SemanticObject father, SemanticObject child) {
        _ontology.Data.RemoveAssertionRelation(new RDFOntologyFact(GetClassResource(child._ontologyID)), 
                                                new RDFOntologyObjectProperty(GetResource("isPartOf")), 
                                                new RDFOntologyFact(GetClassResource(father._ontologyID)));
    }

    public SemanticObject UpdateNDetections(SemanticObject obj, int nDetection) {
        var objectFact = new RDFOntologyFact(GetClassResource(obj._ontologyID));
        _ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("nDetections")),
                                        new RDFOntologyLiteral(new RDFPlainLiteral(obj._nDetections.ToString())));

        _ontology.Data.AddAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("nDetections")),
                                            new RDFOntologyLiteral(new RDFPlainLiteral(nDetection.ToString())));
        obj._nDetections = nDetection;
        return obj;
    }
    

    public SemanticObject UpdateObject(SemanticObject obj, Vector3 pose, Vector3 rotation, Vector3 size, double score,  int nDetection) {
        var objectFact = new RDFOntologyFact(GetClassResource(obj._ontologyID));

        //Update pose
        _ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("position")),
                                                new RDFOntologyLiteral(new RDFPlainLiteral(obj._pose.ToString())));

        _ontology.Data.AddAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("position")), 
                                            new RDFOntologyLiteral(new RDFPlainLiteral(pose.ToString())));

        obj._pose = pose;

        //Update rotation

        _ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("rotation")),
                                                new RDFOntologyLiteral(new RDFPlainLiteral(obj._rotation.eulerAngles.ToString())));

        _ontology.Data.AddAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("rotation")), 
                                            new RDFOntologyLiteral(new RDFPlainLiteral(rotation.ToString())));

        obj._rotation = Quaternion.Euler(rotation);

        //Update size

        _ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("size")),
                                        new RDFOntologyLiteral(new RDFPlainLiteral(obj._size.ToString())));

        _ontology.Data.AddAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("size")),
                                            new RDFOntologyLiteral(new RDFPlainLiteral(size.ToString())));

        obj._size = size;

        //Update score

        _ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("score")),
                                new RDFOntologyLiteral(new RDFPlainLiteral(obj._confidenceScore.ToString())));

        _ontology.Data.AddAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("score")),
                                            new RDFOntologyLiteral(new RDFPlainLiteral(score.ToString())));

        obj._confidenceScore = score;

        //Update nDetections

        _ontology.Data.RemoveAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("nDetections")),
                                                new RDFOntologyLiteral(new RDFPlainLiteral(obj._nDetections.ToString())));

        _ontology.Data.AddAssertionRelation(objectFact, new RDFOntologyDatatypeProperty(GetResource("nDetections")),
                                            new RDFOntologyLiteral(new RDFPlainLiteral(nDetection.ToString())));

        obj._nDetections = nDetection;

        return obj;
    }

    public bool CheckClassObject(string class_object) {
        return _objectClassInOntology.Contains(GetNameWithURI(class_object));
    }

    // <------- Utils

    public string GetNameWithURI(string name) {
        return _nameSpace + CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name).Replace(" ", "_");
    }

    public string GetNameWithoutURI(string name)
    {
        return name.Substring(_nameSpace.ToString().Length);
    }

    public RDFResource GetClassResource(String name)
    {
        return new RDFResource(GetNameWithURI(name));
    }

    public RDFResource GetResource(String name)
    {
        return new RDFResource(_nameSpace + name);
    }

    public string GetNewTimeID()
    {
        return System.DateTime.Now.ToString("yyyyMMddHHmmssfff").ToString();
    }

    public static Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
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

    public static Quaternion StringToQuaternion(string sQuaternion)
    {
        // Remove the parentheses
        if (sQuaternion.StartsWith("(") && sQuaternion.EndsWith(")"))
        {
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

    // <------- Semantic Rooms Code


    public void AddNewRoom(string id, string typeRoom)
    {
        var newSemanticRoom = new RDFOntologyFact(GetClassResource(id));
        _ontology.Data.AddFact(newSemanticRoom);
        _ontology.Data.AddClassTypeRelation(newSemanticRoom, new RDFOntologyClass(GetClassResource(typeRoom)));
        _ontology.Data.AddAssertionRelation(newSemanticRoom, new RDFOntologyObjectProperty(GetResource("recordedIn")), _raidFact);
        _ontology.Data.AddAssertionRelation(newSemanticRoom, new RDFOntologyObjectProperty(GetResource("isPartOf")), _houseFact);
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

        RDFSelectQueryResult resultDetectedObject = query.ApplyToGraph(_ontology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData));
        _interestClass = (from r in resultDetectedObject.SelectResults.AsEnumerable() select r["?OBJECTCLASS"].ToString()).Distinct().ToList();

        _probabilityRoomByClass = new Dictionary<string, Dictionary<string, float>>();

        var _roomCategoriesInOntology = GetCategoriesOfRooms();

        foreach (string category in _roomCategoriesInOntology) {
            Dictionary<string, float> probabilityRoom = new Dictionary<string, float>();
            foreach (string objClass in _interestClass) {
                List<string> posibilities = (from r in resultDetectedObject.SelectResults.AsEnumerable().Where(r => r.Field<string>("?OBJECTCLASS") == objClass) select GetNameWithoutURI(r["?CATEGORYROOM"].ToString())).ToList();

                if (posibilities.Contains(category)) {
                    probabilityRoom.Add(objClass, (float)0.9 / posibilities.Count);
                }
                else
                {
                    probabilityRoom.Add(objClass, (float)0.1 / (_roomCategoriesInOntology.Count - posibilities.Count));
                }

            }
            _probabilityRoomByClass.Add(category, probabilityRoom);
        }

    }

    public bool CheckInteresObject(string typeObject) {
        return _interestClass.Contains(GetNameWithURI(typeObject));
    }

    public void ObjectInRoom(SemanticObject semanticObject)
    {
        var obj = new RDFOntologyFact(GetClassResource(semanticObject._ontologyID));
        _ontology.Data.AddAssertionRelation(obj, new RDFOntologyObjectProperty(GetResource("isIn")), new RDFOntologyFact(GetClassResource(semanticObject._semanticRoom._id)));
    }

    public List<String> GetCategoriesOfRooms() {
        List<String> categoreies = new List<string>();
        RDFVariable typesRooms = new RDFVariable("TYPESROOM");
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(typesRooms, RDFVocabulary.RDFS.SUB_CLASS_OF, GetClassResource("Room"))))
            .AddProjectionVariable(typesRooms);

        RDFSelectQueryResult resultDetectedObject = query.ApplyToGraph(_ontology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData));
        categoreies = (from r in resultDetectedObject.SelectResults.AsEnumerable() select GetNameWithoutURI(r["?TYPESROOM"].ToString())).Distinct().ToList();
        return categoreies;
    }

    public Dictionary<string, float> GetProbabilityCategories(List<SemanticObject> detectedClasses) {

        Dictionary<string, float> rooms = new Dictionary<string, float>();

        float total = 0;
        foreach (string categoryRoom in _cateogiesOfRooms)
        {
            var probability = GetProbabilityCategory(categoryRoom, new List<SemanticObject>(detectedClasses), new List<SemanticObject>());
            rooms.Add(categoryRoom, probability);
            total += probability;
        }
        if (total != 0)
        {
            foreach (string category in _cateogiesOfRooms)
            {
                rooms[category] /= total;
            }
        }

        
        return rooms;
    }

    public float GetProbabilityCategory(string categoryRoom, List<SemanticObject> detectedClasses, List<SemanticObject> previousClass)
    {
        if (detectedClasses.Count == 0)
            return 0;

        float probability = 0;
        SemanticObject detection = detectedClasses[0];
        detectedClasses.Remove(detection);
        previousClass.Add(detection);

        foreach (string classObject in _interestClass)
        {
            var total = (_interestClass.Count - 1) * 0.1f;
            float P1 = 0.1f / total;

            if (classObject.Equals(detection._type))
            {
                P1 = (float)detection._confidenceScore / total;
            }

            float P2 = 0f;

            if (detectedClasses.Count == 0)
            {
                P2 = GetProbabilityCategory(categoryRoom, previousClass);
            }
            else
            {
                P2 = GetProbabilityCategory(categoryRoom, detectedClasses, previousClass);
            }

            probability += P1 * P2;
        }

        return probability;
    }

    public float GetProbabilityCategory(string categoryRoom, List<SemanticObject> previousClass)
    {
        float probability = 1;
        float probabilitytotal = 0;
        foreach (KeyValuePair<string, Dictionary<string, float>> category in _probabilityRoomByClass) {
            var probabilityByCategory = 1f;
            foreach (SemanticObject detection in previousClass) {
                probabilityByCategory *= category.Value[GetNameWithURI(detection._type)];
            }
            probabilitytotal += probabilityByCategory;
            if (category.Key.Equals(categoryRoom))
                probability = probabilityByCategory;
        }
        return probability / probabilitytotal;
    }

    public List<SemanticObject> GetPreviousDetections(string room) {

        List<SemanticObject> result = _semanticMapping._virtualSemanticMap.FindAll(obj => obj.GetIdRoom().Equals(room) && CheckInteresObject(obj._type));

        result.Sort(
            delegate (SemanticObject p1, SemanticObject p2)
            {
                int compareDate = p2._nDetections.CompareTo(p1._nDetections);
                if (compareDate == 0)
                {
                    return p2._confidenceScore.CompareTo(p1._confidenceScore);
                }
                return compareDate;
            }
        );

        return result;
    }

    public List<SemanticObject> GetSemanticObjectsInTheRoom(string room) {

        RDFVariable id = new RDFVariable("ID");
        RDFVariable type = new RDFVariable("TYPE");
        RDFVariable score = new RDFVariable("SCORE");
        RDFVariable detections = new RDFVariable("DETECTIONS");
        RDFVariable position = new RDFVariable("POSITION");
        RDFVariable rotation = new RDFVariable("ROTATION");
        RDFVariable scale = new RDFVariable("SCALE");
        RDFVariable father = new RDFVariable("FATHER");

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(id, RDFVocabulary.RDF.TYPE, type))
                .AddPattern(new RDFPattern(id, GetResource("position"), position))
                .AddPattern(new RDFPattern(id, GetResource("rotation"), rotation))
                .AddPattern(new RDFPattern(id, GetResource("score"), score))
                .AddPattern(new RDFPattern(id, GetResource("detections"), detections))
                .AddPattern(new RDFPattern(id, GetResource("size"), scale))
                .AddPattern(new RDFPattern(id, GetResource("isIn"), GetClassResource(room)))
                .AddPattern(new RDFPattern(type, RDFVocabulary.RDFS.SUB_CLASS_OF, GetClassResource("Furniture")).UnionWithNext())
                .AddPattern(new RDFPattern(type, RDFVocabulary.RDFS.SUB_CLASS_OF, GetClassResource("Appliance")))
                .AddPattern(new RDFPattern(id, GetResource("isPartOf"), father).Optional()))
            .AddProjectionVariable(id)
            .AddProjectionVariable(type)
            .AddProjectionVariable(score)
            .AddProjectionVariable(detections)
            .AddProjectionVariable(position)
            .AddProjectionVariable(rotation)
            .AddProjectionVariable(scale)
            .AddProjectionVariable(father);

        query.AddModifier(new RDFDistinctModifier());

        RDFSelectQueryResult resultDetectedObject = query.ApplyToGraph(_ontology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData));

        List<SemanticObject> result = new List<SemanticObject>();
        foreach (DataRow row in resultDetectedObject.SelectResults.AsEnumerable())
        {
            //if (row.Field<string>("?FATHER") == null)
            //{
            //    result.Add(new SemanticObject(row.Field<string>("?ID"),
            //                                    row.Field<string>("?TYPE"),
            //                                    float.Parse(row.Field<string>("?SCORE")),
            //                                    int.Parse(row.Field<string>("?DETECTIONS")),
            //                                    StringToVector3(row.Field<string>("?POSITION")),
            //                                    StringToVector3(row.Field<string>("?SCALE")),
            //                                    StringToQuaternion(row.Field<string>("?ROTATION")),
            //                                    _semanticRoomManager._semantic_rooms.Find(r => r._id.Equals(room))
            //                                    ));
            //}
        }

        return result;
    }


}
