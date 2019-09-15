using UnityEngine;
using ROSBridgeLib.semantic_mapping;
using ROSBridgeLib.geometry_msgs;
using System.Collections.Generic;
using System;
using System.Linq;

public class SemanticMapping : MonoBehaviour {

    public float _maxDistanceZ = 2;
    public float _joiningDistance = 0.1f;
    public bool _modeDebug = false;
    public GameObject _detectedObjectPrefab;
  

    public List<SemanticObjectComponent> _semantic_map;
    public List<SemanticRoom> _semantic_rooms;
    private Transform _tfFrameID;
    private OntologyManaguer _ontologyManaguer;

    private void Start()
    {
        _ontologyManaguer = GetComponent<OntologyManaguer>();
        _semantic_map = new List<SemanticObjectComponent>();
    }

    public void DetectedObject(SemanticObjectMsg obj,string host) {

        if (_tfFrameID == null)
        {
            var _tfFrameID_temp = GameObject.Find(obj.GetHeader().GetFrameId());
            if (_tfFrameID_temp != null)
                _tfFrameID = _tfFrameID_temp.transform;
            else
                Debug.LogWarning("Detected object but no have a correct Frame ID");
        }

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
                                                            FinSemanticRoom(obj.GetPose().GetPose().GetTranslationUnity()));

                _ontologyManaguer.AddNewDetectedObject(semantic_object);
                InstanceNewSemanticObject(semantic_object);


                CompactMap();
            }
            if (_modeDebug)
            {
                Debug.Log("Detected: " + obj.GetId() + obj.GetAccuracyEstimation() + " -> " + obj.GetScale());
            }
        }
    }

    private void CompactMap() {

        List<SemanticObjectComponent> new_semantic_map = new List<SemanticObjectComponent>();

        SemanticObjectComponent objToCompare = _semantic_map[0];
        while (_semantic_map.Count > 0) {
            objToCompare = _semantic_map[0];
            _semantic_map.Remove(objToCompare);

            SemanticObjectComponent joined_object = null;
            foreach (SemanticObjectComponent o in _semantic_map) {
                if (objToCompare._semanticObject._id.Equals(o._semanticObject._id) &&
                    DistanceBetweenSemanticObjects(objToCompare._semanticObject, o._semanticObject) < _joiningDistance) {

                    var new_go_semantic_obj = InstanceNewSemanticObject(JoinSemanticObject(objToCompare._semanticObject, o._semanticObject));
                    joined_object = o;
                    break;
                }
            }

            if (joined_object == null)
            {
                new_semantic_map.Add(objToCompare);
            }
            else {
                _semantic_map.Remove(joined_object);
                Destroy(joined_object.gameObject);
                Destroy(objToCompare.gameObject);
            }

        }

        _semantic_map = new_semantic_map;

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

    private SemanticObject JoinSemanticObject(SemanticObject obj1, SemanticObject obj2) {

        SemanticObject newSemanticObj;
        float pond1 = (float) (obj1._accuracyEstimation / (obj1._accuracyEstimation + obj2._accuracyEstimation));
        float pond2 = (float) (obj2._accuracyEstimation / (obj1._accuracyEstimation + obj2._accuracyEstimation));

        var pose = ((obj1._pose * pond1) + (obj2._pose * pond2));
        newSemanticObj = new SemanticObject(obj1._id,
                            (obj1._accuracyEstimation * pond1) + (obj2._accuracyEstimation * pond2),
                                pose,
                                (obj1._dimensions * pond1) + (obj2._dimensions * pond2),
                                Quaternion.Euler((obj1._rotation.eulerAngles * pond1) + (obj2._rotation.eulerAngles * pond2)),
                                FinSemanticRoom(pose));

        _ontologyManaguer.JointDetectedObject(obj1, obj2, newSemanticObj);
        return newSemanticObj;
    }

    private GameObject InstanceNewSemanticObject(SemanticObject obj) {

        var obj_inst = Instantiate(_detectedObjectPrefab) as GameObject;
        obj_inst.transform.parent = _tfFrameID;

        var semantic_object_component = obj_inst.GetComponent<SemanticObjectComponent>();
        semantic_object_component._semanticObject = obj;
        semantic_object_component.Load();
        _semantic_map.Add(obj_inst.GetComponent<SemanticObjectComponent>());

        return obj_inst;
    }

    private SemanticRoom FinSemanticRoom(Vector3 center) {
        if (_semantic_map.Count == 0)
        {
            _semantic_rooms = new List<SemanticRoom>(FindObjectsOfType<SemanticRoom>());
            _ontologyManaguer.AddNewRooms(_semantic_rooms);
        }
        return _semantic_rooms.Find(sr => sr.PointInside(center));
    }

}