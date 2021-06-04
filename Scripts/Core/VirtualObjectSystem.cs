using UnityEngine;
using ROSUnityCore.ROSBridgeLib.ViMantic_msgs;
using ROSUnityCore.ROSBridgeLib.geometry_msgs;
using System.Collections.Generic;
using ROSUnityCore;
using System.IO;
using System.Collections;

public class VirtualObjectSystem : MonoBehaviour {
    
    public static VirtualObjectSystem instance;
    public int verbose;

    public float threshold_match = 0.8f;
    private float wDistance = 0.35f;
    private float wSize = 0.65f;

    public GameObject prefDetectedObject;
    public Transform tfFrameForObjects;
    public Camera bbCamera;

    public int nDetections { get; private set; }
    public List<SemanticObject> virtualSemanticMap { get; private set; }
    public Dictionary<Color, VirtualObjectBox> boxColors { get; private set; }

    private Queue<DetectionArrayMsg> processingQueue;

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
        processingQueue = new Queue<DetectionArrayMsg>();
        StartCoroutine(ProcessMsgs());
    }
    #endregion

    #region Public Functions
    public void Connected(ROS ros) {
        ros.RegisterSubPackage("Vimantic_Detections_sub");
    }

    public void DetectedObject(DetectionArrayMsg _detections, string _ip) {
        processingQueue.Enqueue(_detections);
    }
        

    public Color GetColorObject(VirtualObjectBox vob) {
        Color newColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1f);
        while (boxColors.ContainsKey(newColor)) {
            newColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1f);
        }
        boxColors[newColor]=vob;
        return newColor;
    }

    public void UnregisterColor(Color color) {
        if (boxColors.ContainsKey(color)) {
            boxColors.Remove(color);
        }
    }

    #endregion

    #region Private Functions
    private IEnumerator ProcessMsgs() {

        Rect rect = new Rect(0, 0, bbCamera.pixelWidth, bbCamera.pixelHeight);
        RenderTexture renderTextureMask = new RenderTexture(bbCamera.pixelWidth, bbCamera.pixelHeight, 24);
        Texture2D image = new Texture2D(bbCamera.pixelWidth, bbCamera.pixelHeight, TextureFormat.RGB24, false);

        while (Application.isPlaying) {

            if (processingQueue.Count > 0) {
                   
                DetectionArrayMsg _detections = processingQueue.Dequeue();

                //Get view previous detections from bbCamera located in the origin
                bbCamera.transform.position = _detections._origin.GetPositionUnity();
                bbCamera.transform.rotation = _detections._origin.GetRotationUnity() * Quaternion.Euler(0f, 90f, 0f);

                bbCamera.targetTexture = renderTextureMask;

                HashSet<Color> colors = new HashSet<Color>();
                List<VirtualObjectBox> virtualObjectBoxInRange = new List<VirtualObjectBox>();

                do
                {
                    bbCamera.Render();
                    RenderTexture.active = renderTextureMask;

                    image.ReadPixels(rect, 0, 0);
                    image.Apply();

                    colors = new HashSet<Color>(image.GetPixels());
                    colors.Remove(new Color(0, 0, 0, 1f));
                    
                    foreach (Color c in colors)
                    {
                        VirtualObjectBox vob = GetObjectMatch(c);
                        virtualObjectBoxInRange.Add(vob);
                        vob.gameObject.SetActive(false);
                    }

                } while (colors.Count > 0);

                foreach (VirtualObjectBox vob in virtualObjectBoxInRange)
                {
                    vob.gameObject.SetActive(true);
                }

                bbCamera.targetTexture = null;
                RenderTexture.active = null; //Clean
                //Destroy(renderTextureMask); //Free memory

                List<VirtualObjectBox> detectedVirtualObjectBox = new List<VirtualObjectBox>();
                foreach (DetectionMsg detection in _detections._detections) {

                    SemanticObject virtualObject = new SemanticObject(detection.GetScores(),
                                                                        detection._pose.GetPositionUnity(),
                                                                        detection._pose.GetRotationUnity(),
                                                                        detection._size.GetVector3());

                    //Check the type object is in the ontology
                    if (!OntologySystem.instance.CheckInteresObject(virtualObject.type)) {
                        Log(virtualObject.type + " - detected but it is not in the ontology");
                        continue;
                    }

                    //Insertion detection into the ontology
                    virtualObject = OntologySystem.instance.AddNewDetectedObject(virtualObject);

                    //Build Ranking
                    List<VirtualObjectBox> matches = new List<VirtualObjectBox>();
                    foreach (VirtualObjectBox vob in virtualObjectBoxInRange) {
                        //"(1.2, 0.4, -7.3)"  "(0.4, 0.5, -8.5)"
                        float distance = Mathf.Max(1 - Vector3.Distance(vob.semanticObject.position, virtualObject.position), 0);

                        if (distance == 0) { continue; }

                        float scoreSize = Mathf.Min(vob.semanticObject.size.x, virtualObject.size.x) / Mathf.Max(vob.semanticObject.size.x, virtualObject.size.x);
                        scoreSize += Mathf.Min(vob.semanticObject.size.y, virtualObject.size.y) / Mathf.Max(vob.semanticObject.size.y, virtualObject.size.y);
                        scoreSize += Mathf.Min(vob.semanticObject.size.z, virtualObject.size.z) / Mathf.Max(vob.semanticObject.size.z, virtualObject.size.z);
                        scoreSize /= 3;

                        float score = (wDistance * distance + wSize * scoreSize);

                        if (score >= threshold_match) {
                            matches.Add(vob);
                        }
                    }

                    //Match process
                    if (matches.Count > 0) {
                        VirtualObjectBox vob = matches[0];
                        matches.RemoveAt(0);
                        //matches.Remove(vob);
                        matches.ForEach(m => virtualObjectBoxInRange.Remove(m));
                        detectedVirtualObjectBox.Add(vob);
                        vob.NewDetection(virtualObject, matches);
                    } else {
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

            yield return new WaitForEndOfFrame();
        }
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