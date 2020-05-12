using UnityEngine;
using ROSBridgeLib.semantic_mapping;
using ROSBridgeLib.geometry_msgs;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;

[RequireComponent(typeof(OntologyManager))]
[RequireComponent(typeof(SemanticRoomManager))]

public class ObjectManager : MonoBehaviour {

    public float _maxDistanceZ = 2;
    public float _joiningDistance = 0.1f;
    public float _minimunConfidenceScore = 0.5f;
    public bool _modeDebug = false;
    public GameObject _detectedObjectPrefab;

    public List<SemanticObject> _virtualSemanticMap;

    public Transform _tfFrameID;
    private OntologyManager _ontologyManager;
    private List<long> _listTimes;
    private List<int> _listTimesUnionObject;
    private List<long> _listTimesOntology;


    private void Start()
    {
        _ontologyManager = GetComponent<OntologyManager>();
        _virtualSemanticMap = new List<SemanticObject>();
        _listTimes = new List<long>();
        _listTimesUnionObject = new List<int>();
        _listTimesOntology = new List<long>();
    }

    public void DetectedObject(SemanticObjectsMsg semanticObjects, string host) {
        
        if (_tfFrameID == null)
        {
            var _tfFrameID_temp = GameObject.Find(semanticObjects.GetHeader().GetFrameId());
            if (_tfFrameID_temp != null)
                _tfFrameID = _tfFrameID_temp.transform;
            else
                Debug.LogWarning("Detected object but no have a correct Frame ID");
        }

        for (int i = 0; i < semanticObjects.GetSemanticObjects().Length; i++)
        {
            var time = DateTime.Now.Ticks;
            SemanticObjectMsg obj = semanticObjects.GetSemanticObjects()[i];

            if (_ontologyManager.CheckClassObject(obj.GetTypeObject()))
            {               

                SemanticObject semantic_object = new SemanticObject(obj.GetTypeObject(),
                                            obj.GetConfidenceScore(),
                                            obj.GetPose().GetTranslationUnity(),
                                            obj.GetScale().GetVector3Unity(),
                                            obj.GetPose().GetRotationUnity(1));

                var ontologyTime = DateTime.Now.Ticks;
                semantic_object = _ontologyManager.AddNewDetectedObject(semantic_object);
                _listTimesOntology.Add((DateTime.Now.Ticks - ontologyTime)/TimeSpan.TicksPerMillisecond);

                if (_modeDebug)
                {
                    Debug.Log("Detected: " + obj.GetTypeObject() + obj.GetConfidenceScore() + " -> " + obj.GetScale());
                }

                if (obj.GetConfidenceScore() > _minimunConfidenceScore)
                {
                    var _robotBase = GameObject.Find(host).transform;

                    if (_robotBase == null)
                    {
                        Debug.LogWarning("robot not found");
                    }
                    else
                    {
                        var distance = Vector2.Distance(_robotBase.position, obj.GetPose().GetTranslationUnity());

                        if (distance < _maxDistanceZ)
                        {
                            _virtualSemanticMap.Add(semantic_object);
                            InstanceNewSemanticObject(semantic_object);

                            _listTimes.Add((DateTime.Now.Ticks - time) / TimeSpan.TicksPerMillisecond);

                        }
                        else
                        {
                            if (_modeDebug)
                                Debug.Log(obj.GetId() + " - Detected far away");
                        }
                    }
                }
            }
            else
            {
                if (_modeDebug)
                    Debug.Log(obj.GetId() + " detected but it is not in the ontology.");
            }
        }
    }

    private GameObject InstanceNewSemanticObject(SemanticObject obj) {

        var obj_inst = Instantiate(_detectedObjectPrefab) as GameObject;
        obj_inst.transform.parent = _tfFrameID;
        obj_inst.GetComponent<VirtualSemanticObject>().Load(obj, _tfFrameID, _joiningDistance);

        return obj_inst;
    }

    public void AddTimeUnion(int time) {
        _listTimesUnionObject.Add(time);
    }

    private void OnApplicationQuit()
    {
        string csv1 = String.Join(" ", _listTimes.Select(x => x.ToString()).ToArray());
        string csv2 = String.Join(" ", _listTimesUnionObject.Select(x => x.ToString()).ToArray());
        string csv3 = String.Join(" ", _listTimesOntology.Select(x => x.ToString()).ToArray());
        StreamWriter writer = new StreamWriter(PlayerPrefs.GetString("pathToSave")+"ObjectManagerTimes.csv");
        writer.Write(csv1);
        writer.Close();
        writer = new StreamWriter(PlayerPrefs.GetString("pathToSave") + "UnionTimes.csv");
        writer.Write(csv2);
        writer.Close();
        writer = new StreamWriter(PlayerPrefs.GetString("pathToSave") + "InsertionOntologyTimes.csv");
        writer.Write(csv2);
        writer.Close();
    }

}