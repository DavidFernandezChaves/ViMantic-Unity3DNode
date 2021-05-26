using UnityEngine;
using ROSUnityCore.ROSBridgeLib.ViMantic_msgs;
using ROSUnityCore.ROSBridgeLib.geometry_msgs;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using ROSUnityCore;

public class ObjectManagerSystem : MonoBehaviour {
    
    public static ObjectManagerSystem instance;
    public int verbose;

    public float minSize = 0.05f;
    public float minimunConfidenceScore = 0.5f;

    public GameObject prefDetectedObject;
    public Transform tfFrameForObjects;

    public int nDetections { get; private set; }
    public List<SemanticObject> virtualSemanticMap { get; private set; }

    #region Unity Functions
    private void Awake() {
        if (!instance) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }

        virtualSemanticMap = new List<SemanticObject>();
    }
    #endregion

    #region Public Functions
    public void Connected(ROS ros) {
        ros.RegisterSubPackage("Vimantic_Detections_sub");
    }

    public void DetectedObject(DetectionArrayMsg _detections, string _ip) {


        foreach(DetectionMsg detection in _detections.GetDetections()) { 

            //Check if its an interesting object
            Vector3 detectionSize = detection._size.GetVector3Unity();
            if (detectionSize.x < minSize || detectionSize.y < minSize || detectionSize.z < minSize) {
                Log("Object detected but does not meet the minimum features: size[" + detectionSize.x + "," + detectionSize.y + "," + detectionSize.z + "]");
                continue;
            }

            Vector3 detectionPosition = detection._pose.GetPositionUnity();
            SemanticObject virtualObject = new SemanticObject(  detection.GetScores(),
                                                                detectionPosition,
                                                                detection._pose.GetRotationUnity(),
                                                                detectionSize);

            //Check minimun Condifence Score
            if (virtualObject.score < minimunConfidenceScore) {
                Log(virtualObject.type + " - detected but it have low score: " + virtualObject.score + "/" + minimunConfidenceScore);
                continue;
            }

            if (verbose > 2) {
                Log("New object detected: " + virtualObject.ToString());
            }

            //Insertion detection into the ontology
            virtualObject = OntologySystem.instance.AddNewDetectedObject(virtualObject);
            nDetections++;
            //Procesamiento que tendremos que hacer....
            virtualSemanticMap.Add(virtualObject);
            InstanceNewSemanticObject(virtualObject);
        }
        
    }

    #endregion

    #region Private Functions
    private Transform FindClient(string _ip) {
        var agents = GameObject.FindObjectsOfType<ROS>();
        foreach(ROS a in agents) {
            if (a.ip == _ip) {
                return a.transform;
            }
        }
        return null;
    }

    private void InstanceNewSemanticObject(SemanticObject _obj) {
        Transform obj_inst = Instantiate(prefDetectedObject, _obj.pose, _obj.rotation).transform;
        obj_inst.parent = tfFrameForObjects;
        obj_inst.GetComponentInChildren<VirtualObjectBox>().InitializeObject(_obj);
    }

    private void Log(string _msg) {
        if (verbose > 1)
            Debug.Log("[Object Manager]: " + _msg);
    }

    private void LogWarning(string _msg) {
        if (verbose > 0)
            Debug.LogWarning("[Object Manager]: " + _msg);
    }
    #endregion

}