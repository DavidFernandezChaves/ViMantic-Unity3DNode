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

        Rect rect = new Rect(0, 0, bbCamera.pixelWidth, bbCamera.pixelHeight);
        RenderTexture renderTextureMask = new RenderTexture(bbCamera.pixelWidth, bbCamera.pixelHeight, 24);

        bbCamera.targetTexture = renderTextureMask;
        bbCamera.Render();
        RenderTexture.active = renderTextureMask;

        Texture2D image = new Texture2D(bbCamera.pixelWidth, bbCamera.pixelHeight, TextureFormat.RGB24, false);
        image.ReadPixels(rect, 0, 0);
        image.Apply();

        //var itemBGBytes = image.EncodeToPNG();
        //File.WriteAllBytes("D:/foto"+_detections._header.GetTimeMsg().ToString()+".png", itemBGBytes);

        HashSet<Color> colors = new HashSet<Color>(image.GetPixels());
        colors.Remove(new Color(0, 0, 0, 1f));
        List<VirtualObjectBox> virtualObjectBoxInRange = new List<VirtualObjectBox>();
        foreach(Color c in colors) {
            virtualObjectBoxInRange.Add(GetObjectMatch(c));
        }


        bbCamera.targetTexture = null;
        RenderTexture.active = null; //Clean
        Destroy(renderTextureMask); //Free memory

        List<VirtualObjectBox> detectedVirtualObjectBox = new List<VirtualObjectBox>();
        foreach (DetectionMsg detection in _detections._detections) {

            //Check if its an interesting object
            Vector3 detectionSize = detection._size.GetVector3();
            if (detectionSize.x < minSize || detectionSize.y < minSize || detectionSize.z < minSize) {
                Log("Object detected but does not meet the minimum features: size[" + detectionSize.x + ";" + detectionSize.y + ";" + detectionSize.z + "]");
                continue;
            }

            SemanticObject virtualObject = new SemanticObject(detection.GetScores(),
                                                                detection._pose.GetPositionUnity(),
                                                                detection._pose.GetRotationUnity(),
                                                                detectionSize);

            //Check the type object is in the ontology
            if (!OntologySystem.instance.CheckInteresObject(virtualObject.type)) {
                Log(virtualObject.type + " - detected but it is not in the ontology");
                continue;
            }

            //Build Ranking
            VirtualObjectBox match = null;
            float best_score = 0;
            foreach (VirtualObjectBox vob in virtualObjectBoxInRange) {

                float distance = Mathf.Max(1 - Vector3.Distance(vob.semanticObject.position, virtualObject.position), 0);

                float scoreSize = Mathf.Min(vob.semanticObject.size.x, virtualObject.size.x) / Mathf.Max(vob.semanticObject.size.x, virtualObject.size.x);
                scoreSize += Mathf.Min(vob.semanticObject.size.y, virtualObject.size.y) / Mathf.Max(vob.semanticObject.size.y, virtualObject.size.y);
                scoreSize += Mathf.Min(vob.semanticObject.size.z, virtualObject.size.z) / Mathf.Max(vob.semanticObject.size.z, virtualObject.size.z);
                scoreSize /= 3;

                float score = (distance + scoreSize) / 2;

                if (best_score < score) {
                    match = vob;
                    best_score = score;
                }
            }

            //Insertion detection into the ontology
            virtualObject = OntologySystem.instance.AddNewDetectedObject(virtualObject);

            //Match process
            if (best_score >= threshold_match) {
                match.NewDetection(virtualObject);
                detectedVirtualObjectBox.Add(match);
                //Destroy(high_match.Key.transform.parent.gameObject);
            }else {
                if (verbose > 2) {
                    Log("New object detected: " + virtualObject.ToString());
                }
                nDetections++;
                virtualSemanticMap.Add(virtualObject);
                InstanceNewSemanticObject(virtualObject);
            }

        }
        detectedVirtualObjectBox.ForEach(dvob => virtualObjectBoxInRange.Remove(dvob));
        virtualObjectBoxInRange.ForEach(vob => vob.NewDetection(null));      

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