using UnityEngine;
using RDFSharp.Semantics;
using RDFSharp.Model;
using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;

public class OntologyManaguer : MonoBehaviour{

    public string _nameFileInput = "OntologyMapir.owl";
    public string _nameFileOutput = "OntologyMapirOutput.owl";
    public string _masterURI = "http://mapir.isa.uma.es/";


    private RDFOntology _ontology;
    private SceneManaguer _sceneManaguer;

    private string _raidID;
    private RDFOntologyFact _raidFact;

    // Start is called before the first frame update
    void Start()
    {
        _sceneManaguer = GetComponent<SceneManaguer>();
        _raidID = GetNewTimeID();
        _ontology = RDFOntology.FromRDFGraph(RDFSharp.Model.RDFGraph.FromFile(RDFModelEnums.RDFFormats.RdfXml, Path.Combine(Application.persistentDataPath, _nameFileInput)));

        _raidFact = new RDFOntologyFact(new RDFResource(NameToURI(_raidID)));
        _ontology.Data.AddFact(_raidFact);
        _ontology.Data.AddClassTypeRelation(_raidFact, new RDFOntologyClass(new RDFResource(NameToURI("Raid"))));
        _ontology.Data.AddAssertionRelation(_raidFact, new RDFOntologyObjectProperty(new RDFResource(NameToURI("Inside"))), new RDFOntologyFact(new RDFResource(NameToURI(GetComponent<SceneManaguer>()._nameMap))));

    }

    public void OnDestroy()
    {
        Debug.Log("Saving....");
        _ontology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData).ToFile(RDFModelEnums.RDFFormats.RdfXml, Path.Combine(Application.persistentDataPath, _nameFileOutput));
    }

    public RDFOntologyFact AddNewDetectedObject(SemanticObject obj) {

        obj._idDetection = GetNewTimeID() + "_" + obj._id;

        var newDetectedObject = new RDFOntologyFact(new RDFResource(NameToURI(obj._idDetection)));
        _ontology.Data.AddFact(newDetectedObject);
        _ontology.Data.AddClassTypeRelation(newDetectedObject, new RDFOntologyClass(new RDFResource(NameToURI(obj._id))));
        _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(new RDFResource(NameToURI("RaidID"))), new RDFOntologyLiteral(new RDFPlainLiteral(NameToURI(_raidID))));
        _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(new RDFResource(NameToURI("Pose"))), new RDFOntologyLiteral(new RDFPlainLiteral(NameToURI(obj._pose.ToString()))));
        _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(new RDFResource(NameToURI("Probability"))) ,new RDFOntologyLiteral(new RDFPlainLiteral(NameToURI(obj._accuracyEstimation.ToString()))));
        _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(new RDFResource(NameToURI("Dimensions"))), new RDFOntologyLiteral(new RDFPlainLiteral(NameToURI(obj._dimensions.ToString()))));
        _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyDatatypeProperty(new RDFResource(NameToURI("Rotation"))), new RDFOntologyLiteral(new RDFPlainLiteral(NameToURI(obj._rotation.eulerAngles.ToString()))));

        if (obj._semanticRoom != null)
            _ontology.Data.AddAssertionRelation(newDetectedObject, new RDFOntologyObjectProperty(new RDFResource(NameToURI("Inside"))), new RDFOntologyFact(new RDFResource(NameToURI(obj._semanticRoom._ID))));

        return newDetectedObject;
    }

    public void JointDetectedObject(SemanticObject obj1, SemanticObject obj2, SemanticObject _newObj) {
        var newObj = AddNewDetectedObject(_newObj);
        _ontology.Data.AddSameAsRelation(newObj, new RDFOntologyFact(new RDFResource(NameToURI(obj1._idDetection))));
        _ontology.Data.AddSameAsRelation(newObj, new RDFOntologyFact(new RDFResource(NameToURI(obj2._idDetection))));
    }

    public void AddNewRooms(List<SemanticRoom> semanticRooms) {
        foreach (SemanticRoom sm in semanticRooms)
        {
            sm._ID = GetNewTimeID() + "_" + sm._ID;
            var newSemanticRoom = new RDFOntologyFact(new RDFResource(NameToURI(sm._ID)));
            _ontology.Data.AddFact(newSemanticRoom);
            _ontology.Data.AddAssertionRelation(newSemanticRoom, new RDFOntologyObjectProperty(new RDFResource(NameToURI("Inside"))), _raidFact);
        }
    }

    public string NameToURI(String name) {
        return _masterURI + CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name).Replace(" ","_");
    }

    public string GetNewTimeID() {
        return System.DateTime.Now.ToString("yyyyMMddHHmmss");
    }

}
