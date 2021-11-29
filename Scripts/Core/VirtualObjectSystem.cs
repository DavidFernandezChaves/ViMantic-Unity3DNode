using UnityEngine;
using ROSUnityCore.ROSBridgeLib.ViMantic_msgs;
using ROSUnityCore;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace ViMantic
{

    public class VirtualObjectSystem : MonoBehaviour
    {
        public static VirtualObjectSystem instance;

        [Header("General")]
        [Tooltip("The log level to use")]
        public LogLevel LogLevel = LogLevel.Normal;

        [Header("Filters")]
        [Tooltip("Indicates the minimum number of pixels that a mask of a previous detection must have to consider that this object should be detected.")]
        public int minPixelsMask = 1000;
        [Tooltip("Sets the depth range in which the detected objects must be.")]
        public Vector2 deepRange;

        [Header("Similarity of objects")]
        [Tooltip("Distance to be taken into account to consider that two observations of the same type belong to the same object.")]
        [Range(0.1f, 10f)]
        public float thresholdMatchSameSype = 1f;

        [Tooltip("Distance to be taken into account to consider that two observations of different types belong to the same object.")]
        [Range(0.1f,10f)]
        public float thresholdMatchDiffType = 2f;

        [Header("Reference objects")]
        [Tooltip("Object detection prefab.")]
        public GameObject prefDetectedObject;
        [Tooltip("Transform where to instantiate the detected objects.")]
        public Transform tfFrameForObjects;
        [Tooltip("Camera to check which objects should be tected.")]
        public Camera bbCamera;

        public int nDetections { get; private set; }
        public List<SemanticObject> m_objectDetected { get; private set; }
        public Dictionary<Color32, VirtualObjectBox> boxColors { get; private set; }

        private Queue<DetectionArrayMsg> processingQueue;

        #region Unity Functions
        private void Awake()
        {
            if (!instance)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            m_objectDetected = new List<SemanticObject>();
            boxColors = new Dictionary<Color32, VirtualObjectBox>();
            processingQueue = new Queue<DetectionArrayMsg>();
            StartCoroutine(ProcessMsgs());
        }
        #endregion

        #region Public Functions
        public void Connected(ROS ros)
        {
            ros.RegisterSubPackage("Vimantic_Detections_sub");
        }

        public void DetectedObject(DetectionArrayMsg _detections, string _ip)
        {
            processingQueue.Enqueue(_detections);
        }


        public Color32 GetColorObject(VirtualObjectBox vob)
        {
            Color32 newColor = new Color32((byte)Random.Range(0f, 255f), (byte)Random.Range(0f, 255f), (byte)Random.Range(0f, 255f), 255);
            while (boxColors.ContainsKey(newColor))
            {
                newColor = new Color32((byte)Random.Range(0f, 255f), (byte)Random.Range(0f, 255f), (byte)Random.Range(0f, 255f), 255);
            }
            boxColors[newColor] = vob;
            return newColor;
        }

        public void UnregisterColor(Color32 color)
        {
            if (boxColors.ContainsKey(color))
            {
                boxColors.Remove(color);
            }
        }
        #endregion

        #region Private Functions
        private IEnumerator ProcessMsgs()
        {

            Rect rect = new Rect(0, 0, 640, 480);
            Texture2D image = new Texture2D(640, 480, TextureFormat.RGB24, false);
            RenderTexture renderTextureMask = new RenderTexture(640, 480, 24);
            while (Application.isPlaying)
            {

                if (processingQueue.Count > 0)
                {

                    DetectionArrayMsg _detections = processingQueue.Dequeue();

                    //Get view previous detections from bbCamera located in the origin
                    bbCamera.transform.position = _detections.origin.GetPositionUnity();
                    bbCamera.transform.rotation = _detections.origin.GetRotationUnity() * Quaternion.Euler(0f, 90f, 0f);



                    Dictionary<VirtualObjectBox, int> virtualObjectBoxInRange = new Dictionary<VirtualObjectBox, int>();
                    List<VirtualObjectBox> visibleVirtualObjectBox = new List<VirtualObjectBox>();

                    //int n = 0;
                    bool newIteration = true;
                    while (newIteration)
                    {
                        newIteration = false;
                        bbCamera.targetTexture = renderTextureMask;
                        bbCamera.Render();
                        RenderTexture.active = renderTextureMask;
                        image.ReadPixels(rect, 0, 0);
                        image.Apply();

                        var q = from x in image.GetPixels()
                                group x by x into g
                                let count = g.Count()
                                orderby count descending
                                select new { Value = g.Key, Count = count };

                        foreach (var xx in q)
                        {

                            if (boxColors.ContainsKey(xx.Value))
                            {
                                var vob = boxColors[xx.Value];
                                vob.gameObject.SetActive(false);
                                visibleVirtualObjectBox.Add(vob);
                                var distance = Vector3.Distance(vob.semanticObject.Position, bbCamera.transform.position);
                                if (deepRange.y >= distance && distance >= deepRange.x)
                                    virtualObjectBoxInRange.Add(vob, xx.Count);
                                newIteration = true;
                                //Debug.Log("Value: " + xx.Value + " Count: " + xx.Count);
                            }
                        }

                        bbCamera.targetTexture = null;
                        RenderTexture.active = null; //Clean
                        //Destroy(renderTextureMask); //Free memory
                        if (q.Count() == 1) break;
                        //n++;
                    }

                    foreach (VirtualObjectBox vob in visibleVirtualObjectBox)
                    {
                        vob.gameObject.SetActive(true);
                    }

                    List<VirtualObjectBox> detectedVirtualObjectBox = new List<VirtualObjectBox>();
                    foreach (DetectionMsg detection in _detections.detections)
                    {                        
                        SemanticObject virtualObject = new SemanticObject(detection.GetScores(),
                                                                            detection.GetCorners(),
                                                                            detection.occluded_corners);

                        //Check the type object is in the ontology
                        if (!OntologySystem.instance.CheckInteresObject(virtualObject.ObjectClass))
                        {
                            Log(virtualObject.ObjectClass + " - detected but it is not in the ontology",LogLevel.Error,true);
                            continue;
                        }

                        //Checks the distance to the object
                        var distance = Vector3.Distance(virtualObject.Position, bbCamera.transform.position);
                        if (distance < deepRange.x || distance > deepRange.y)
                        {
                            Log(virtualObject.ObjectClass + " - detected but it is not in deep range. Distance: " + distance,LogLevel.Normal,true);
                            continue;
                        }

                        //Insertion detection into the ontology
                        virtualObject = OntologySystem.instance.AddNewDetectedObject(virtualObject);

                        //Try to get a match
                        VirtualObjectBox match = Matching(virtualObject, virtualObjectBoxInRange.Keys);

                        //Match process
                        if (match != null)
                        {
                            match.NewDetection(virtualObject);
                            detectedVirtualObjectBox.Add(match);
                        }
                        else
                        {
                            Log("New object detected: " + virtualObject.ToString(),LogLevel.Developer);
                            m_objectDetected.Add(virtualObject);
                            VirtualObjectBox nvob = InstanceNewSemanticObject(virtualObject);
                            detectedVirtualObjectBox.Add(nvob);
                        }
                        nDetections++;
                    }

                    List<VirtualObjectBox> inRange = virtualObjectBoxInRange.Keys.ToList();
                    for (int i = 0; i < inRange.Count - 1; i++)
                    {
                        if (inRange[i] != null)
                        {
                            VirtualObjectBox match = null;
                            match = Matching(inRange[i].semanticObject, inRange.GetRange(i + 1, inRange.Count - i - 1));
                            if (match != null)
                            {
                                match.NewDetection(inRange[i].semanticObject);
                                inRange[i].RemoveVirtualBox();
                            }
                        }
                    }

                    detectedVirtualObjectBox.ForEach(dvob => virtualObjectBoxInRange.Remove(dvob));

                    foreach (KeyValuePair<VirtualObjectBox, int> o in virtualObjectBoxInRange)
                    {
                        if (o.Value > minPixelsMask) o.Key.NewDetection(null);
                    }
                }

                yield return null;
            }
        }

        private VirtualObjectBox Matching(SemanticObject obj1, ICollection<VirtualObjectBox> listToCompare)
        {
            VirtualObjectBox match = null;
            float best_score = 999;
            foreach (VirtualObjectBox vob in listToCompare)
            {

                List<SemanticObject.Corner> order = YNN(vob.semanticObject.Corners, obj1.Corners);
                float score = CalculateMatchingScore(vob.semanticObject.Corners, order);

                if (((score < thresholdMatchSameSype && obj1.ObjectClass.Equals(vob.semanticObject.ObjectClass)) ||
                    (score < thresholdMatchDiffType && !obj1.ObjectClass.Equals(vob.semanticObject.ObjectClass))) && score < best_score) {

                    obj1.SetNewCorners(order);
                    match = vob;
                    best_score = score;
                    //Debug.Log("Union: " + obj1.Id+ " con: " + vob.semanticObject.Id + ", por distancia: " + score);
                }//else { Debug.Log("NO Union: " + obj1.Id+ " con: " + vob.semanticObject.Id + ", por distancia: " + score); }
            }
            return match;
        }

        private VirtualObjectBox InstanceNewSemanticObject(SemanticObject _obj)
        {
            Transform obj_inst = Instantiate(prefDetectedObject, _obj.Position, _obj.Rotation).transform;
            obj_inst.parent = tfFrameForObjects;
            VirtualObjectBox result = obj_inst.GetComponentInChildren<VirtualObjectBox>();
            result.InitializeSemanticObject(_obj);
            return result;
        }
        private void Log(string _msg, LogLevel lvl, bool Warning = false)
        {
            if (LogLevel <= lvl && LogLevel != LogLevel.Nothing)
            {
                if (Warning)
                {
                    Debug.LogWarning("[Object Manager]: " + _msg);
                }
                else
                {
                    Debug.Log("[Object Manager]: " + _msg);
                }
            }
        }
        #endregion

        #region Static Functions
        static public List<SemanticObject.Corner> YNN(List<SemanticObject.Corner> reference, List<SemanticObject.Corner> observation)
        {

            Queue<SemanticObject.Corner> top = new Queue<SemanticObject.Corner>();
            top.Enqueue(observation[2]);
            top.Enqueue(observation[5]);
            top.Enqueue(observation[4]);
            top.Enqueue(observation[7]);
            Queue<SemanticObject.Corner> bottom = new Queue<SemanticObject.Corner>();
            bottom.Enqueue(observation[0]);
            bottom.Enqueue(observation[3]);
            bottom.Enqueue(observation[6]);
            bottom.Enqueue(observation[1]);

            int index = 0;
            float best_distance = Vector3.Distance(reference[2].position, top.ElementAt(0).position) +
                                Vector3.Distance(reference[4].position, top.ElementAt(2).position) +
                                Vector3.Distance(reference[5].position, top.ElementAt(1).position) +
                                Vector3.Distance(reference[7].position, top.ElementAt(3).position);

            for (int i = 1; i < 4; i++)
            {
                top.Enqueue(top.Dequeue());
                float distance = Vector3.Distance(reference[2].position, top.ElementAt(0).position) +
                                    Vector3.Distance(reference[4].position, top.ElementAt(2).position) +
                                    Vector3.Distance(reference[5].position, top.ElementAt(1).position) +
                                    Vector3.Distance(reference[7].position, top.ElementAt(3).position);

                if (best_distance > distance)
                {
                    index = i;
                    best_distance = distance;
                }
            }

            top.Enqueue(top.Dequeue());

            for (int i = 0; i < index; i++)
            {
                top.Enqueue(top.Dequeue());
                bottom.Enqueue(bottom.Dequeue());
            }

            List<SemanticObject.Corner> result = new List<SemanticObject.Corner> {
            bottom.ElementAt(0),
            bottom.ElementAt(3),
            top.ElementAt(0),
            bottom.ElementAt(1),
            top.ElementAt(2),
            top.ElementAt(1),
            bottom.ElementAt(2),
            top.ElementAt(3)
        };

            return result;
        }

        static public float CalculateMatchingScore(List<SemanticObject.Corner> reference, List<SemanticObject.Corner> observation)
        {
            int cornerPairs = 0;
            float score1 = 0, score2 = 0;
            for (int i = 0; i < reference.Count; i++)
            {
                if(!reference[i].occluded && !observation[i].occluded)
                {
                    cornerPairs++;
                    score2 += Vector3.Distance(reference[i].position, observation[i].position);
                }
                score1 += Vector3.Distance(reference[i].position, observation[i].position);
            }

            if(cornerPairs >= 3)
            {
                return score2 / cornerPairs;
            }
            else
            {
                return score1 / 8;
            }            
        }
        #endregion

    }
}