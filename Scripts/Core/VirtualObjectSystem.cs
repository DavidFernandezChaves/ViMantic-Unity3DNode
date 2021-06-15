using UnityEngine;
using ROSUnityCore.ROSBridgeLib.ViMantic_msgs;
using ROSUnityCore;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class VirtualObjectSystem : MonoBehaviour {
    
    public static VirtualObjectSystem instance;
    public int verbose;
    
    public float threshold_match = 1f;
    public int minPixelsMask = 1000;

    public GameObject prefDetectedObject;
    public Transform tfFrameForObjects;
    public Camera bbCamera;

    public int nDetections { get; private set; }
    public List<SemanticObject> virtualSemanticMap { get; private set; }
    public Dictionary<Color32, VirtualObjectBox> boxColors { get; private set; }

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
        boxColors = new Dictionary<Color32, VirtualObjectBox>();
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
        bbCamera.targetTexture = renderTextureMask;

        while (Application.isPlaying) {

            if (processingQueue.Count > 0) {
                   
                DetectionArrayMsg _detections = processingQueue.Dequeue();

                //Get view previous detections from bbCamera located in the origin
                bbCamera.transform.position = _detections.origin.GetPositionUnity();
                bbCamera.transform.rotation = _detections.origin.GetRotationUnity() * Quaternion.Euler(0f, 90f, 0f);

                Dictionary<VirtualObjectBox, int> virtualObjectBoxInRange = new Dictionary<VirtualObjectBox, int>();

                while(true)
                {
                    bbCamera.Render();
                    RenderTexture.active = renderTextureMask;

                    image.ReadPixels(rect, 0, 0);
                    image.Apply();

                    var list = image.GetPixels32();
                    var q = from x in list
                            group x by x into g
                            let count = g.Count()
                            //orderby count descending
                            select new { Value = g.Key, Count = count };

                    foreach (var x in q)
                    {
                        VirtualObjectBox vob = GetObjectMatch(x.Value);
                        if (vob != null) {
                            vob.gameObject.SetActive(false);
                            virtualObjectBoxInRange.Add(vob, x.Count);
                            //Debug.Log("Value: " + x.Value + " Count: " + x.Count);
                        }
                    }

                    if (q.Count() == 1) break;

                }

                foreach (VirtualObjectBox vob in virtualObjectBoxInRange.Keys)
                {
                    vob.gameObject.SetActive(true);
                }

                bbCamera.targetTexture = null;
                RenderTexture.active = null; //Clean
                //Destroy(renderTextureMask); //Free memory

                List<VirtualObjectBox> detectedVirtualObjectBox = new List<VirtualObjectBox>();
                foreach (DetectionMsg detection in _detections.detections) {


                    SemanticObject virtualObject = new SemanticObject(detection.GetScores(),
                                                                        detection.GetCorners(),
                                                                        detection.occluded_corners); 

                    //Check the type object is in the ontology
                    if (!OntologySystem.instance.CheckInteresObject(virtualObject.Type)) {
                        Log(virtualObject.Type + " - detected but it is not in the ontology");
                        continue;
                    }

                    //Insertion detection into the ontology
                    virtualObject = OntologySystem.instance.AddNewDetectedObject(virtualObject);

                    //Build Ranking
                    List<SemanticObject.Corner> match_corners_ordered = new List<SemanticObject.Corner>();
                    VirtualObjectBox match = null;
                    float match_distance = -1;
                    foreach (VirtualObjectBox vob in virtualObjectBoxInRange.Keys) {
                        List<SemanticObject.Corner> order = YNN(vob.semanticObject.Corners, virtualObject.Corners);
                        float distance = CalculateCornerDistance(vob.semanticObject.Corners, order, false);
                        Debug.Log(distance);
                        if (distance < (threshold_match)) {
                            match_corners_ordered = order;
                            match_distance = distance;
                            match = vob;
                        }
                    }

                    //Match process
                    if (match != null) {
                        virtualObject.SetNewCorners(match_corners_ordered);
                        match.NewDetection(virtualObject);
                        Debug.Log("Union con Best Distance:" + match_distance);
                    } else {
                        Debug.Log("No Union con Best Distance:" + match_distance);
                        if (verbose > 2) {
                            Log("New object detected: " + virtualObject.ToString());
                        }                        
                        virtualSemanticMap.Add(virtualObject);
                        InstanceNewSemanticObject(virtualObject);
                    }
                    nDetections++;
                }
                detectedVirtualObjectBox.ForEach(dvob => virtualObjectBoxInRange.Remove(dvob));

                foreach (KeyValuePair<VirtualObjectBox, int> o in virtualObjectBoxInRange)
                {
                    if (o.Value > minPixelsMask) o.Key.NewDetection(null);
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private VirtualObjectBox GetObjectMatch(Color color) {
        foreach(KeyValuePair<Color32, VirtualObjectBox> pair in boxColors) {
            if(Mathf.Abs(color.r-pair.Key.r) 
                + Mathf.Abs(color.g - pair.Key.g)
                + Mathf.Abs(color.b - pair.Key.b) < 0.05f) {
                return pair.Value.GetComponent<VirtualObjectBox>();
            }
        }
        return null;
    }

    private void InstanceNewSemanticObject(SemanticObject _obj) {
        Transform obj_inst = Instantiate(prefDetectedObject, _obj.Position, _obj.Rotation).transform;
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

    #region Static Functions
    static List<SemanticObject.Corner> YNN(List<SemanticObject.Corner> reference, List<SemanticObject.Corner> observation) {

        Queue<SemanticObject.Corner> top = new Queue<SemanticObject.Corner>();
        top.Enqueue(observation[2]);
        top.Enqueue(observation[4]);
        top.Enqueue(observation[5]);
        top.Enqueue(observation[7]);
        Queue<SemanticObject.Corner> bottom = new Queue<SemanticObject.Corner>();
        bottom.Enqueue(observation[0]);
        bottom.Enqueue(observation[1]);
        bottom.Enqueue(observation[3]);
        bottom.Enqueue(observation[6]);

        int index = 0;
        float best_distance = Vector3.Distance(reference[2].position, top.ElementAt(0).position) +
                            Vector3.Distance(reference[4].position, top.ElementAt(1).position) +
                            Vector3.Distance(reference[5].position, top.ElementAt(2).position) +
                            Vector3.Distance(reference[7].position, top.ElementAt(3).position);

        for (int i = 1; i < 4; i++) {
            top.Enqueue(top.Dequeue());
            float distance = Vector3.Distance(reference[2].position, top.ElementAt(0).position) +
                                Vector3.Distance(reference[4].position, top.ElementAt(1).position) +
                                Vector3.Distance(reference[5].position, top.ElementAt(2).position) +
                                Vector3.Distance(reference[7].position, top.ElementAt(3).position);

            if (best_distance > distance) {
                index = i;
                best_distance = distance;
            }
        }

        top.Enqueue(top.Dequeue());

        for (int i = 0; i < index; i++) {
            top.Enqueue(top.Dequeue());
            bottom.Enqueue(bottom.Dequeue());
        }

        List<SemanticObject.Corner> result = new List<SemanticObject.Corner> {
            bottom.Dequeue(),
            bottom.Dequeue(),
            top.Dequeue(),
            bottom.Dequeue(),
            top.Dequeue(),
            top.Dequeue(),
            bottom.Dequeue(),
            top.Dequeue()
        };

        return result;
    }

    static float CalculateCornerDistance(List<SemanticObject.Corner> reference, List<SemanticObject.Corner> observation, bool onlyNonOccluded) {
        float distance = 0;
        for (int i = 0; i < reference.Count; i++) {
            if ((!observation[i].occluded && !reference[i].occluded) || !onlyNonOccluded) {
                distance += Vector3.Distance(reference[i].position, observation[i].position);
            }
        }

        if (distance == 0) {
            for (int i = 0; i < reference.Count; i++) {
                distance += Vector3.Distance(reference[i].position, observation[i].position);
            }
        }

        return distance == 0 ? 999 : distance;
    }
    #endregion

}