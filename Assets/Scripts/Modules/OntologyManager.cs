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


    public struct Detection {
        public string _id;
        public float _score;
        public string _class;
        public int _nDetections;

        public Detection(string id, float score, string _class, int nDetections)
        {
            _id = id;
            _score = score;
            this._class = _class;
            _nDetections = nDetections;
        }
    }

    private void UpdateListOfClassAvailable()
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

    private void UpdateProbabilityRoomByClass() {
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
        var _roomCategoriesInOntology = (from r in resultDetectedObject.SelectResults.AsEnumerable() select r["?CATEGORYROOM"].ToString()).Distinct().ToList();
        _interestClass = (from r in resultDetectedObject.SelectResults.AsEnumerable() select r["?OBJECTCLASS"].ToString()).Distinct().ToList();

        _probabilityRoomByClass = new Dictionary<string, Dictionary<string, float>>();

        foreach (string category in _roomCategoriesInOntology) {
            Dictionary<string, float> probabilityRoom = new Dictionary<string, float>();
            foreach (string objClass in _interestClass) {
                List<string> posibilities = (from r in resultDetectedObject.SelectResults.AsEnumerable().Where(r => r.Field<string>("?OBJECTCLASS") == objClass) select r["?CATEGORYROOM"].ToString()).ToList();

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

    private void SaveOntology()
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

            //_ontology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData).ToFile(RDFModelEnums.RDFFormats.RdfXml, Path.Combine(_pathToSave, _nameFile));
            Debug.Log("Ontology saved");
        }
    }

    private void OnDestroy()
    {
        SaveOntology();
    }

    public void LoadOntology(string name)
    {
        SaveOntology();

        // CREATE NAMESPACE
        _nameSpace = new RDFNamespace(_prefix, _masterURI);

        _nameFile = name + ".owl";
        _raidID = GetNewTimeID();
        var time = Time.realtimeSinceStartup;
        _ontology = RDFOntology.FromRDFGraph(RDFSharp.Model.RDFGraph.FromFile(RDFModelEnums.RDFFormats.RdfXml, Path.Combine(_pathToSave, _nameFile)));

        _raidFact = new RDFOntologyFact(GetClassResource(_raidID));
        _ontology.Data.AddFact(_raidFact);
        _ontology.Data.AddClassTypeRelation(_raidFact, new RDFOntologyClass(GetClassResource("Raid")));
        UpdateListOfClassAvailable();
        UpdateProbabilityRoomByClass();
        _cateogiesOfRooms = GetCategoriesOfRooms();
        Debug.Log("Ontology loading time: " + (Time.realtimeSinceStartup - time).ToString());
    }


    public void RecordtHouse(string name)
    {
        _houseFact = new RDFOntologyFact(GetClassResource(name));
        _ontology.Data.AddClassTypeRelation(_houseFact, new RDFOntologyClass(GetClassResource("House")));
        _ontology.Data.AddAssertionRelation(_raidFact, new RDFOntologyObjectProperty(GetResource("recordedIn")), _houseFact);
    }

    public RDFOntologyFact AddNewDetectedObject(SemanticObject obj)
    {

        obj._ontologyId = GetNewTimeID() + "_" + obj._id;
        var newDetectedObject = new RDFOntologyFact(GetClassResource(obj._ontologyId));
        _ontology.Data.AddFact(newDetectedObject);
        _ontology.Data.AddClassTypeRelation(newDetectedObject, new RDFOntologyClass(GetClassResource(obj._id)));
        _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(GetResource("position")), new RDFOntologyLiteral(new RDFPlainLiteral(obj._pose.ToString())));
        _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(GetResource("rotation")), new RDFOntologyLiteral(new RDFPlainLiteral(obj._rotation.eulerAngles.ToString())));
        _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(GetResource("score")), new RDFOntologyLiteral(new RDFPlainLiteral(obj._score.ToString())));
        _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(GetResource("size")), new RDFOntologyLiteral(new RDFPlainLiteral(obj._dimensions.ToString())));
        _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyObjectProperty(GetResource("recordedIn")), _raidFact);
        if (obj._semanticRoom != null)
            _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyObjectProperty(GetResource("isIn")), new RDFOntologyFact(GetClassResource(obj._semanticRoom._id)));
        if (obj._fatherId != null)
            _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyObjectProperty(GetResource("isPartOf")), new RDFOntologyFact(GetClassResource(obj._fatherId)));


        return newDetectedObject;
    }

    public void JointDetectedObject(SemanticObject obj2, SemanticObject _newObj)
    {
        AddNewDetectedObject(_newObj);
        _ontology.Data.AddAssertionRelation(new RDFOntologyFact(GetClassResource(obj2._ontologyId)), new RDFOntologyObjectProperty(GetResource("isPartOf")), new RDFOntologyFact(GetClassResource(obj2._fatherId)));
    }

    public void AddNewRooms(List<SemanticRoom> semanticRooms)
    {
        foreach (SemanticRoom sm in semanticRooms)
        {
            sm._id = GetNewTimeID() + "_" + sm._id;
            var newSemanticRoom = new RDFOntologyFact(GetClassResource(sm._id));
            _ontology.Data.AddFact(newSemanticRoom);
            _ontology.Data.AddClassTypeRelation(newSemanticRoom, new RDFOntologyClass(GetClassResource(sm._typeRoom)));
            _ontology.Data.AddAssertionRelation(newSemanticRoom, new RDFOntologyObjectProperty(GetResource("recordedIn")), _raidFact);
            _ontology.Data.AddAssertionRelation(newSemanticRoom, new RDFOntologyObjectProperty(GetResource("isPartOf")), _houseFact);
        }
    }

    public bool CheckClassObject(string class_object) {
        return _objectClassInOntology.Contains(GetNameWithURI(class_object));
    }

    public bool CheckInteresObject(string class_object) {
        return _interestClass.Contains(GetNameWithURI(class_object));
    }

    public List<String> GetCategoriesOfRooms() {
        List<String> categoreies = new List<string>();
        RDFVariable typesRooms = new RDFVariable("TYPESROOM");
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(typesRooms, RDFVocabulary.RDFS.SUB_CLASS_OF, GetClassResource("Room"))))
            .AddProjectionVariable(typesRooms);

        RDFSelectQueryResult resultDetectedObject = query.ApplyToGraph(_ontology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData));
        categoreies = (from r in resultDetectedObject.SelectResults.AsEnumerable() select r["?TYPESROOM"].ToString()).Distinct().ToList();
        return categoreies;
    }

    public Dictionary<string, float> GetCategoryProbability(string room, int n) {


        List<Detection> classDetections = GetPreviousDetections(room, n);

        Dictionary<string, float> categories = new Dictionary<string, float>();
        if (classDetections.Count > 0)
        {
            float total = 0;
            foreach (string category in _cateogiesOfRooms)
            {
                categories.Add(category, GetCategoryProbability(category, classDetections, new List<Detection>()));
                total += categories[category];
            }
            foreach (string category in _cateogiesOfRooms)
            {
                categories[category] /= total;
            }
        }
        return categories;
    }

    public float GetCategoryProbability(string category, List<Detection> classDetections, List<Detection> previousClass)
    {
        float probability = 0;
        List<Detection> _classDetections = classDetections.ToList();
        List<Detection> _previousClass = previousClass.ToList();
        Detection detection = _classDetections[0];
        _classDetections.Remove(detection);
        _previousClass.Add(detection);

        foreach (string classObject in _interestClass)
        {
            var total = (_interestClass.Count - 1) * 0.1f;
            float P1 = 0.1f / total;

            if (classObject.Equals(detection._class))
            {
                P1 = (float)detection._score / total;
            }

            float P2 = 0f;

            if (_classDetections.Count == 0)
            {
                P2 = GetCategoryProbability(category, _previousClass);
            }
            else
            {
                P2 = GetCategoryProbability(category, _classDetections, _previousClass);
            }

            probability += P1 * P2;
        }

        return probability;
    }

    public float GetCategoryProbability(string _category, List<Detection> previousClass)
    {
        float probability = 1;
        float probabilitytotal = 0;
        foreach (KeyValuePair<string, Dictionary<string, float>> category in _probabilityRoomByClass) {
            var probabilityByCategory = 1f;
            foreach (Detection detection in previousClass) {
                probabilityByCategory *= category.Value[detection._class];
            }
            probabilitytotal += probabilityByCategory;
            if (category.Key.Equals(_category))
                probability = probabilityByCategory;
        }
        return probability / probabilitytotal;
    }


    public List<Detection> GetPreviousDetections(string room, int n) {

        RDFVariable id = new RDFVariable("ID");
        RDFVariable father = new RDFVariable("FATHER");
        RDFVariable score = new RDFVariable("SCORE");
        RDFVariable objectClass = new RDFVariable("CLASS");

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(id, RDFVocabulary.RDF.TYPE, objectClass))
                .AddPattern(new RDFPattern(id, GetResource("score"), score))
                .AddPattern(new RDFPattern(id, GetResource("isIn"), GetClassResource(room)))
                .AddPattern(new RDFPattern(objectClass, RDFVocabulary.RDFS.SUB_CLASS_OF, GetClassResource("Furniture")).UnionWithNext())
                .AddPattern(new RDFPattern(objectClass, RDFVocabulary.RDFS.SUB_CLASS_OF, GetClassResource("Appliance")))
                .AddPattern(new RDFPattern(id, GetResource("isPartOf"), father).Optional()))
            .AddProjectionVariable(id)
            .AddProjectionVariable(objectClass)
            .AddProjectionVariable(father)
            .AddProjectionVariable(score);

        RDFSelectQueryResult resultDetectedObject = query.ApplyToGraph(_ontology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData));

        List<Detection> result = new List<Detection>();
        //resultDetectedObject.ToSparqlXmlResult(Path.Combine(_pathToSave, "select_results.srq"));

        foreach (DataRow row in resultDetectedObject.SelectResults.AsEnumerable()) {
            string superDetectionID;
            if (row["?FATHER"].ToString() == "")
            {
                superDetectionID = row.Field<string>("?ID");
            }
            else
            {
                superDetectionID = row.Field<string>("?FATHER");
            }

            var index = result.FindIndex(d => d._id.Equals(superDetectionID));
            if (index < 0) 
            {
                result.Add(new Detection(superDetectionID, float.Parse(row.Field<string>("?SCORE")),row.Field<string>("?CLASS"), 1));
            }
            else
            {                
                Detection detection = result[index];
                detection._nDetections += 1;
                detection._score = float.Parse(row.Field<string>("?SCORE"));
                result[index] = detection;
            }
        }   

        result.Sort(
            delegate (Detection p1, Detection p2)
            {
                int compareDate = p2._nDetections.CompareTo(p1._nDetections);
                if (compareDate == 0)
                {
                    return p2._score.CompareTo(p1._score);
                }
                return compareDate;
            }
        );
        if (result.Count > n)
            result = result.GetRange(0, n);

        return result;
    }

    public string GetNameWithURI(string name) {
        return _nameSpace + CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name).Replace(" ", "_");
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

}
