using UnityEngine;
using ROSUnityCore.ROSBridgeLib.ViMantic_msgs;
using ROSUnityCore.ROSBridgeLib.geometry_msgs;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using ROSUnityCore;

[RequireComponent(typeof(OntologyManager))]

public class ObjectManager : MonoBehaviour {
    
    public static ObjectManager instance;
    public int verbose;

    public float maxDistance = 2;
    public float minSize = 0.05f;
    public float maxSizeZ = 1;
    public float minimunConfidenceScore = 0.5f;

    public GameObject prefDetectedObject;
    public Transform tfFrameForObjects;

    public int detecciones { get; private set; }
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
        ros.RegisterSubPackage("Vimantic_SemanticObjects_sub");
    }

    public void DetectedObject(SemanticObjectArrayMsg _semanticObjects, string _host) {

        for (int i = 0; i < _semanticObjects.GetSemanticObjects().Length; i++) {

            SemanticObjectMsg _obj = _semanticObjects.GetSemanticObjects()[i];

            //Check if its an interesting object
            Vector3 objSize = _obj._size.GetVector3Unity();
            if (objSize.x > minSize && objSize.y > minSize && objSize.z > minSize && objSize.z < maxSizeZ) {

                Vector3 objPosition = _obj._pose.GetPose().GetPositionUnity();
                SemanticObject virtualObject = new SemanticObject("",
                                                                  _obj.GetScores(),
                                                                  objPosition,                                                                  
                                                                  _obj._pose.GetPose().GetRotationUnity(),
                                                                  objSize,
                                                                  null);

                if (verbose > 2)
                    Log("New object detected: " + virtualObject.ToString());

                virtualObject = OntologyManager.instance.AddNewDetectedObject(virtualObject);

                if (virtualObject.score > minimunConfidenceScore) {

                    Transform host = FindClient(_host);
                    float distance = Vector2.Distance(new Vector2(objPosition.x, objPosition.z), new Vector2(host.position.x, host.position.z));
                    if (maxDistance > distance) {
                        detecciones++;
                        virtualSemanticMap.Add(virtualObject);
                        InstanceNewSemanticObject(virtualObject, host);
                    } else {
                        Log(virtualObject.type + " - detected far away: " + distance + "/" + maxDistance);
                    }
                } else {
                    Log(virtualObject.type + " - detected but it have low score: " + virtualObject.score + "/" + minimunConfidenceScore);
                }
            } else {
                Log("Object detected, but does not meet the minimum features: size[" + objSize.x + "," + objSize.y + "," + objSize.z + "]/[>" + minSize + ",>" + minSize + ",<" + maxSizeZ + "]");
            }
        }
    }

    #endregion

    #region Private Functions
    private Transform FindClient(string _ip) {
        return GameObject.Find(_ip).GetComponent<ROS>().transform;
    }

    private void InstanceNewSemanticObject(SemanticObject _obj, Transform host) {
        Transform obj_inst = Instantiate(prefDetectedObject, _obj.pose, _obj.rotation).transform;
        obj_inst.parent = tfFrameForObjects;
        obj_inst.GetComponentInChildren<VirtualObjectBox>().InitializeObject(_obj, host);
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