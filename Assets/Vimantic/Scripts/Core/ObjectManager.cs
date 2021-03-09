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
    public bool recordTimes = false;

    public float maxDistance = 2;
    public float minSize = 0.05f;
    public float maxSizeZ = 1;
    public float minimunConfidenceScore = 0.5f;

    public GameObject prefDetectedObject;
    public Transform tfFrameForObjects;

    public int nDetections = 0;

    public List<SemanticObject> virtualSemanticMap { get; private set; }

    private List<long> listTimes;
    private List<int> listTimesUnionObject;
    private List<long> listTimesOntology;

    #region Unity Functions
    private void Awake() {
        if (!instance) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }

        virtualSemanticMap = new List<SemanticObject>();

        listTimes = new List<long>();
        listTimesUnionObject = new List<int>();
        listTimesOntology = new List<long>();
    }

    private void OnApplicationQuit() {
        if (recordTimes) {
            string csv1 = String.Join(" ", listTimes.Select(x => x.ToString()).ToArray());
            string csv2 = String.Join(" ", listTimesUnionObject.Select(x => x.ToString()).ToArray());
            string csv3 = String.Join(" ", listTimesOntology.Select(x => x.ToString()).ToArray());
            StreamWriter writer = new StreamWriter(PlayerPrefs.GetString("pathToSave") + "ObjectManagerTimes.csv");
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
    #endregion

    #region Public Functions
    public void DetectedObject(SemanticObjectArrayMsg _semanticObjects, string _host) {

        for (int i = 0; i < _semanticObjects.GetSemanticObjects().Length; i++) {

            var time = DateTime.Now.Ticks;

            SemanticObjectMsg _obj = _semanticObjects.GetSemanticObjects()[i];

            //Check if its an interesting object
            Vector3 objSize = _obj._size.GetVector3Unity();
            if (OntologyManager.instance.CheckClassObject(_obj._objectType)){

                nDetections++;
                
                if (objSize.x > minSize && objSize.y > minSize && objSize.z > minSize && objSize.z < maxSizeZ) {

                    Log("New object detected: " + _obj.ToString());

                    SemanticObject virtualObject = new SemanticObject(_obj._objectType,
                                                    _obj._object._score,
                                                    _obj._object._pose.GetPose().GetPositionUnity(),
                                                    objSize,
                                                    _obj._object._pose.GetPose().GetRotationUnity());

                    var ontologyTime = DateTime.Now.Ticks;
                    virtualObject = OntologyManager.instance.AddNewDetectedObject(virtualObject);
                    listTimesOntology.Add((DateTime.Now.Ticks - ontologyTime) / TimeSpan.TicksPerMillisecond);


                    if (_obj._object._score > minimunConfidenceScore) {

                        Transform host = FindClient(_host);
                        Vector3 objPosition = _obj._object._pose.GetPose().GetPositionUnity();

                        if (maxDistance > Vector2.Distance(new Vector2(objPosition.x, objPosition.z), new Vector2(host.position.x, host.position.z))) {
                            virtualSemanticMap.Add(virtualObject);
                            InstanceNewSemanticObject(virtualObject, host);
                            listTimes.Add((DateTime.Now.Ticks - time) / TimeSpan.TicksPerMillisecond);
                        } else {
                            Log(_obj._objectType + " - detected far away");
                        }
                    } else {
                        Log(_obj._objectType + " - detected but it have low score.");
                    }
                } else {
                    Log(_obj._objectType + " - detected, but does not meet the minimum features: size["+ objSize.x+","+ objSize.y+","+ objSize.z+"]");
                }
            } else {
                Log(_obj._objectType + " detected but not identified in the ontology.");
            }
        }
    }

    public void AddTimeUnion(int time) {
        listTimesUnionObject.Add(time);
    }
    #endregion

    #region Private Functions
    private Transform FindClient(string _ip) {
        return GameObject.Find(_ip).GetComponent<ROS>().transform;
    }

    private void InstanceNewSemanticObject(SemanticObject _obj, Transform host) {
        Transform obj_inst = Instantiate(prefDetectedObject, _obj.pose, _obj.rotation).transform;
        obj_inst.parent = tfFrameForObjects;
        obj_inst.GetComponentInChildren<VirtualSemanticObject>().InitializeObject(_obj, host);
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