using UnityEngine;
using ROSBridgeLib.semantic_mapping;
using ROSBridgeLib.geometry_msgs;
using System.Collections.Generic;
using System;
using System.Linq;

[RequireComponent(typeof(OntologyManager))]
[RequireComponent(typeof(SemanticRoomManager))]

public class SemanticMapping : MonoBehaviour {

    public float _maxDistanceZ = 2;
    public float _joiningDistance = 0.1f;
    public bool _modeDebug = false;
    public GameObject _detectedObjectPrefab;
  

    public List<VirtualSemanticObject> _semantic_map;
    private Transform _tfFrameID;
    private OntologyManager _ontologyManager;
    private SemanticRoomManager _semanticRoomManager;

    private void Start()
    {
        _ontologyManager = GetComponent<OntologyManager>();
        _semanticRoomManager = GetComponent<SemanticRoomManager>();
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
            SemanticObjectMsg obj = semanticObjects.GetSemanticObjects()[i];
            if (_modeDebug)
            {
                Debug.Log("Detected: " + obj.GetId() + obj.GetAccuracyEstimation() + " -> " + obj.GetScale());
            }

            if (_ontologyManager.CheckClassObject(obj.GetId()))
            {
                var _robotBase = GameObject.Find(host).transform;

                if (_robotBase == null)
                {
                    Debug.LogWarning("host not found");
                }
                else
                {
                    var distance = Vector2.Distance(_robotBase.position, obj.GetPose().GetPose().GetTranslationUnity());

                    if (distance < _maxDistanceZ)
                    {
                        var semantic_object = new SemanticObject(obj.GetId(),
                                                                    obj.GetAccuracyEstimation(),
                                                                    obj.GetPose().GetPose().GetTranslationUnity(),
                                                                    obj.GetScale().GetVector3Unity(),
                                                                    obj.GetPose().GetPose().GetRotationUnity(1),
                                                                    _semanticRoomManager.FindSemanticRoomOf(obj.GetPose().GetPose().GetTranslationUnity()));

                        _ontologyManager.AddNewDetectedObject(semantic_object);

                        CompactMap(semantic_object);
                        //(i == (semanticObjects.GetSemanticObjects().Length) - 1) &&
                        if (_ontologyManager.CheckInteresObject(semantic_object._id))
                        {
                            _semanticRoomManager.UpdateRoom(semantic_object._semanticRoom);
                        }

                    }
                    else
                    {
                        if (_modeDebug)
                            Debug.Log("Detected far away: " + obj.GetId());
                    }
                }
            }
            else {
                if (_modeDebug)
                    Debug.Log(obj.GetId() + " detected but it is not in the ontology.");
            }
        }
    }



    private void CompactMap(SemanticObject newObj) {
        
        SemanticObject Joined = null;
        for (int i = 0; i < _semantic_map.Count; i++) {
                VirtualSemanticObject obj1 = _semantic_map[i];

            if (obj1._semanticObject._id.Equals(newObj._id) &&
                DistanceBetweenSemanticObjects(obj1._semanticObject, newObj) < _joiningDistance) {
                Joined = JoinSemanticObject(obj1, newObj);
                i = _semantic_map.Count;
            }                
        }
        if (Joined == null)
            InstanceNewSemanticObject(newObj);

    }

    private double DistanceBetweenSemanticObjects(SemanticObject obj1, SemanticObject obj2) {
        var distance_between_centers = Vector3.Distance(obj1._pose, obj2._pose);
        var distance_size1 = (obj1._dimensions / 2).sqrMagnitude;
        var distance_size2 = (obj2._dimensions / 2).sqrMagnitude;

        if (distance_between_centers < (distance_size1 + distance_size2))
            return 0;
        else
            return distance_between_centers - (distance_size1 + distance_size2);

    }

    private SemanticObject JoinSemanticObject(VirtualSemanticObject instanciatedObject, SemanticObject newObject) {     
        
        SemanticObject UnionSemanticObj;
        float pond1 = (float) (instanciatedObject._semanticObject._score / (instanciatedObject._semanticObject._score + newObject._score));
        float pond2 = (float) (newObject._score / (instanciatedObject._semanticObject._score + newObject._score));

        var pose = ((instanciatedObject._semanticObject._pose * pond1) + (newObject._pose * pond2));
        UnionSemanticObj = new SemanticObject(instanciatedObject._semanticObject._id,
                                            (instanciatedObject._semanticObject._score * pond1) + (newObject._score * pond2),
                                            pose,
                                            (instanciatedObject._semanticObject._dimensions * pond1) + (newObject._dimensions * pond2),
                                            Quaternion.Euler((instanciatedObject._semanticObject._rotation.eulerAngles * pond1) + (newObject._rotation.eulerAngles * pond2)),
                                            instanciatedObject._semanticObject._semanticRoom);

        string fatherID;
        if (instanciatedObject._semanticObject._fatherId == null)
        {
            fatherID = instanciatedObject._semanticObject._ontologyId;
        }
        else {
            fatherID = instanciatedObject._semanticObject._fatherId;
        }
        UnionSemanticObj._fatherId = fatherID;
        newObject._fatherId = fatherID;
        _ontologyManager.JointDetectedObject(newObject, UnionSemanticObj);

        instanciatedObject._semanticObject = UnionSemanticObj;
        instanciatedObject.Load();
        return UnionSemanticObj;
    }

    private GameObject InstanceNewSemanticObject(SemanticObject obj) {

        var obj_inst = Instantiate(_detectedObjectPrefab) as GameObject;
        obj_inst.transform.parent = _tfFrameID;

        var semantic_object_component = obj_inst.GetComponent<VirtualSemanticObject>();
        semantic_object_component._semanticObject = obj;
        semantic_object_component.Load();
        _semantic_map.Add(obj_inst.GetComponent<VirtualSemanticObject>());

        return obj_inst;
    }

}