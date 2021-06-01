using UnityEngine;
using ROSUnityCore.ROSBridgeLib.ViMantic_msgs;
using ROSUnityCore.ROSBridgeLib.geometry_msgs;
using System.Collections.Generic;
using ROSUnityCore;
using System.IO;

public class VirtualObjectSystem : MonoBehaviour {
    
    public static VirtualObjectSystem instance;
    public int verbose;

    public float threshold_match = 0.8f;
    public float minSize = 0.05f;
    public float minimunConfidenceScore = 0.5f;

    public GameObject prefDetectedObject;
    public Transform tfFrameForObjects;
    public Camera bbCamera;

    public int nDetections { get; private set; }
    public List<SemanticObject> virtualSemanticMap { get; private set; }
    public Dictionary<Color, VirtualObjectBox> boxColors { get; private set; }

    #region Unity Functions
    private void Awake() {
        if (!instance) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }

        virtualSemanticMap = new List<SemanticObject>();
        boxColors = new Dictionary<Color, VirtualObjectBox>();
    }
    #endregion

    #region Public Functions
    public void Connected(ROS ros) {
        ros.RegisterSubPackage("Vimantic_Detections_sub");
    }

    public void DetectedObject(DetectionArrayMsg _detections, string _ip) {

        //Get view previous detections from bbCamera located in the origin
        bbCamera.transform.position = _detections._origin.GetPositionUnity();
        bbCamera.transform.rotation = _detections._origin.GetRotationUnity() * Quaternion.Euler(0f, 90f, 0f);

        RenderTexture renderTextureMask = new RenderTexture(bbCamera.pixelWidth, bbCamera.pixelHeight, 24);
        bbCamera.targetTexture = renderTextureMask;
        bbCamera.Render();
        RenderTexture.active = renderTextureMask;

        Texture2D image = new Texture2D(bbCamera.targetTexture.width, bbCamera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, bbCamera.targetTexture.width, bbCamera.targetTexture.height), 0, 0);
        image.Apply();

        //var itemBGBytes = image.EncodeToPNG();
        //File.WriteAllBytes("D:/foto"+_detections._header.GetTimeMsg().ToString()+".png", itemBGBytes);

        HashSet<Color> colors = new HashSet<Color>(image.GetPixels());

        bbCamera.targetTexture = null;
        RenderTexture.active = null; //Clean
        Destroy(renderTextureMask); //Free memory

        foreach (DetectionMsg detection in _detections._detections) {

            //Check if its an interesting object
            Vector3 detectionSize = detection._size.GetVector3();
            if (detectionSize.x < minSize || detectionSize.y < minSize || detectionSize.z < minSize) {
                Log("Object detected but does not meet the minimum features: size[" + detectionSize.x + ";" + detectionSize.y + ";" + detectionSize.z + "]");
                continue;
            }

            Vector3 detectionPosition = detection._pose.GetPositionUnity();
            SemanticObject virtualObject = new SemanticObject(detection.GetScores(),
                                                                detectionPosition,
                                                                detection._pose.GetRotationUnity(),
                                                                detectionSize);

            //Check the type object is in the ontology
            if (!OntologySystem.instance.CheckInteresObject(virtualObject.type)) {
                Log(virtualObject.type + " - detected but it is not in the ontology");
                continue;
            }

            //Check minimun Condifence Score
            if (virtualObject.score < minimunConfidenceScore) {
                Log(virtualObject.type + " - detected but it has low score: " + virtualObject.score + "/" + minimunConfidenceScore);
                continue;
            }

            //Build Ranking
            KeyValuePair<VirtualObjectBox, float> high_match = new KeyValuePair<VirtualObjectBox, float>(null, 0);
            colors.Remove(new Color(0, 0, 0, 0));

            foreach (Color c in colors) {

                VirtualObjectBox previous_object = GetObjectMatch(c);

                if (previous_object != null) {
                    
                    float distance = Mathf.Max(1 - Vector3.Distance(previous_object.semanticObject.position, virtualObject.position), 0);

                    Vector3 diff_sizes = (previous_object.semanticObject.size - virtualObject.size);
                    float sizes = Mathf.Max(1 - (Mathf.Abs(diff_sizes.x) + Mathf.Abs(diff_sizes.y) + Mathf.Abs(diff_sizes.z))/3, 0);

                    float score = (distance + sizes) / 2;

                    if (high_match.Value < score) {
                        high_match = new KeyValuePair<VirtualObjectBox, float>(previous_object, score);
                    }
                } else {
                    LogWarning("Color "+c.ToString()+"detected, but it is not registered.");
                    continue;
                }

            }

            //Match process
            if (high_match.Value >= threshold_match) {
                high_match.Key.NewDetection(virtualObject);
            } else {
                if (verbose > 2) {
                    Log("New object detected: " + virtualObject.ToString());
                }

                //Insertion detection into the ontology
                virtualObject = OntologySystem.instance.AddNewDetectedObject(virtualObject);
                nDetections++;
                virtualSemanticMap.Add(virtualObject);
                InstanceNewSemanticObject(virtualObject);
            }

        }

        //Si algun objeto no se ha visto, se le mete un penalizador

    }

    public Color GetColorObject(VirtualObjectBox vob) {
        Color newColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1f);
        while (boxColors.ContainsKey(newColor)) {
            newColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1f);
        }
        boxColors[newColor]=vob;
        return newColor;
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

    private VirtualObjectBox GetObjectMatch(Color color) {
        foreach(KeyValuePair<Color, VirtualObjectBox> pair in boxColors) {
            if(Mathf.Abs(color.r-pair.Key.r) 
                + Mathf.Abs(color.g - pair.Key.g)
                + Mathf.Abs(color.b - pair.Key.b) < 0.05f) {
                return pair.Value.GetComponent<VirtualObjectBox>();
            }
        }
        return null;
    }

    private void InstanceNewSemanticObject(SemanticObject _obj) {
        Transform obj_inst = Instantiate(prefDetectedObject, _obj.position, _obj.rotation).transform;
        obj_inst.parent = tfFrameForObjects;
        obj_inst.GetComponentInChildren<VirtualObjectBox>().InitializeSemanticObject(_obj);
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