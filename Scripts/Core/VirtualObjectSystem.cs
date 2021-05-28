using UnityEngine;
using ROSUnityCore.ROSBridgeLib.ViMantic_msgs;
using ROSUnityCore.ROSBridgeLib.geometry_msgs;
using System.Collections.Generic;
using ROSUnityCore;

public class VirtualObjectSystem : MonoBehaviour {
    
    public static VirtualObjectSystem instance;
    public int verbose;

    public float minSize = 0.05f;
    public float minimunConfidenceScore = 0.5f;

    public GameObject prefDetectedObject;
    public Transform tfFrameForObjects;
    public Camera bbCamera;

    public int nDetections { get; private set; }
    public List<SemanticObject> virtualSemanticMap { get; private set; }
    public Dictionary<Color,Transform> boxColors { get; private set; }

    #region Unity Functions
    private void Awake() {
        if (!instance) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }

        virtualSemanticMap = new List<SemanticObject>();
        boxColors = new Dictionary<Color, Transform>();
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
            SemanticObject virtualObject = new SemanticObject(  detection.GetScores(),
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

            //Creamos ranking
            KeyValuePair<Transform, float> high_match = new KeyValuePair<Transform, float>(transform, 0);
            colors.Remove(Color.black);
            foreach (Color c in colors) {

                SemanticObject previous_object = boxColors[c].GetComponent<VirtualObjectBox>().semanticObject;

                float distance = Vector3.Distance(previous_object.position, virtualObject.position);
                Vector3 diff_sizes = (previous_object.size - virtualObject.size);
                float sizes = Mathf.Abs(diff_sizes.x) + Mathf.Abs(diff_sizes.y) + Mathf.Abs(diff_sizes.z);
                float score = distance + sizes;

                if (high_match.Value < score) {
                    high_match = new KeyValuePair<Transform, float>(boxColors[c], score);
                }
            }

            if (high_match.Value != 0) {
                Destroy(high_match.Key.gameObject);
            }


            //Si no hay match

            if (verbose > 2) {
                Log("New object detected: " + virtualObject.ToString());
            }

            //Insertion detection into the ontology
            virtualObject = OntologySystem.instance.AddNewDetectedObject(virtualObject);
            nDetections++;
            virtualSemanticMap.Add(virtualObject);
            InstanceNewSemanticObject(virtualObject);

            //Si hay match
            //Union
        }

        //Si algun objeto no se ha visto, se le mete un penalizador

    }

    public Color GetColorObject(Transform vob) {
        Color newColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 0.5f);
        while (boxColors.ContainsKey(newColor)) {
            newColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 0.5f);
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