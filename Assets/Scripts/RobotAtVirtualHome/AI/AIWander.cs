using ROSUnityCore;
using ROSUnityCore.ROSBridgeLib.geometry_msgs;
using ROSUnityCore.ROSBridgeLib.nav_msgs;
using ROSUnityCore.ROSBridgeLib.std_msgs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

namespace RobotAtVirtualHome {

    [RequireComponent(typeof(NavMeshAgent))]

    public class AIWander : MonoBehaviour {

        public enum StatusMode { Loading, Walking, Turning, Finished }

        public int verbose;

        public bool loop;
        public bool randomSecuence;
        public bool record;
        public float frecuency;
        
        public StatusMode State { get; private set; }
        public List<Vector3> VisitPoints { get; private set; }
        public string currentRoom { get; private set; }

        public bool captureRGB;
        public bool captureDepth;
        public bool captureSemanticMask;

        public bool sendPathToROS;
        public float ROSFrecuency = 1;

        private ROS ros;
        private SmartCamera smartCamera;
        private NavMeshAgent agent;
        private string path;
        private int index = 0;
        private StreamWriter writer;        
        private int index2 = 0;

        #region Unity Functions
        private void Awake() {
            VisitPoints = new List<Vector3>();
            agent = GetComponent<NavMeshAgent>();
            smartCamera = GetComponentInChildren<SmartCamera>();
            if (smartCamera == null) {
                LogWarning("Smart camera not found");
            }
            State = StatusMode.Loading;
        }

        void Start() {
            
            var rooms = FindObjectsOfType<Room>();

            foreach (Room r in rooms) {
                foreach (Light l in r.generalLights) {
                    var point = l.transform.position;
                    point.y = 0;
                    VisitPoints.Add(point);
                }
            }

            if (record) {
                path = FindObjectOfType<VirtualEnvironment>().path;
                string tempPath = Path.Combine(path, "Wandering");
                int i = 0;
                while (Directory.Exists(tempPath)) {
                    i++;
                    tempPath = Path.Combine(path, "Wandering"+i);
                }

                path = tempPath;
                Directory.CreateDirectory(path);           

                Log("The saving path is:" + path);
                writer = new StreamWriter(path + "/Info.csv", true);
                writer.WriteLine("photoID;robotPosition;robotRotation;cameraPosition;cameraRotation;room");
                StartCoroutine("Record");
            }

            agent.SetDestination(VisitPoints[0]);
            agent.isStopped = false;
            State = StatusMode.Walking;

            ros = transform.root.GetComponentInChildren<ROS>();
            if (sendPathToROS && ros != null) {
                Log("Send path to ros: Ok");
                ros.RegisterPublishPackage("Path_pub");
                StartCoroutine(SendPathToROS());
            } else {
                Log("Send path to ros: False");
            }
        }

        void Update() {
            switch (State) {
                case StatusMode.Walking:
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit)) {
                        currentRoom = hit.transform.name;
                    }

                    if (agent.remainingDistance <= agent.stoppingDistance &&
                        agent.velocity.sqrMagnitude == 0f) {
                        Vector3 nextGoal;
                        if (GetNextGoal(out nextGoal)) {
                            agent.SetDestination(nextGoal);
                            agent.isStopped = false;
                            Log("Next goal:" + nextGoal.ToString());
                            State = StatusMode.Loading;
                            StartCoroutine(DoOnGoal());
                        } else {
                            State = StatusMode.Finished;
                            Log("Finish");
                            GetComponent<AudioSource>().Play();
                        }                        
                    }
                    break;               
            }
        }

        private void OnDestroy() {
            if (this.enabled && record) {
                writer.Close();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (Application.isPlaying && this.enabled && verbose>0) {
                Gizmos.color = Color.green;
                foreach (Vector3 point in VisitPoints) {
                    Gizmos.DrawSphere(point, 0.1f);
                }
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(agent.destination, 0.2f);
            }
        }
#endif

        #endregion

        #region Public Functions

        #endregion

        #region Private Functions
        private IEnumerator DoOnGoal() {
            yield return new WaitForSeconds(3);
            State = StatusMode.Walking;
        }

        private IEnumerator SendPathToROS() {
            while (Application.isPlaying) {
                if (ros.IsConnected()) {
                    Vector3[] points = agent.path.corners;
                    PoseStampedMsg[] poses = new PoseStampedMsg[points.Length];
                    HeaderMsg head = new HeaderMsg(0, new TimeMsg(DateTime.Now.Second, 0), "map");
                    Quaternion rotation = transform.rotation;
                    for (int i = 0; i < points.Length; i++) {
                        head.SetSeq(i);
                        if (i > 0) {
                            rotation = Quaternion.FromToRotation(points[i - 1], points[i]);
                        }

                        poses[i] = new PoseStampedMsg(head, new PoseMsg(points[i], rotation, true));
                    }

                    HeaderMsg globalHead = new HeaderMsg(0, new TimeMsg(DateTime.Now.Second, 0), "map");
                    PathMsg pathmsg = new PathMsg(globalHead, poses);
                    ros.Publish(Path_pub.GetMessageTopic(), pathmsg);
                }
                yield return new WaitForSeconds(ROSFrecuency);
            }
        }

        private bool GetNextGoal(out Vector3 result) {
            result = Vector3.zero;
            if (loop) {
                if (randomSecuence) {
                    result = VisitPoints[UnityEngine.Random.Range(0, VisitPoints.Count)];
                } else {
                    index++;
                    if (index >= VisitPoints.Count) {
                        index = 0;
                    }
                    result = VisitPoints[index];
                }
            } else {
                VisitPoints.RemoveAt(index);
                if (VisitPoints.Count == 0) {
                    return false;
                }

                if (randomSecuence) {
                    result = VisitPoints[UnityEngine.Random.Range(0, VisitPoints.Count)];
                } else {
                    result = VisitPoints[index];
                }
            }  
            return true;
        }

        private IEnumerator Record() {
            while (State !=  StatusMode.Finished) {

                yield return new WaitForEndOfFrame();
                agent.isStopped = true;
                yield return new WaitForSeconds(0.1f);
                byte[] itemBGBytes;
                if (captureSemanticMask) {
                    writer.WriteLine(index2.ToString() + "_mask.png;" + transform.position + ";" + transform.rotation.eulerAngles + ";"
                        + smartCamera.transform.position + ";" + smartCamera.transform.rotation.eulerAngles + ";" + currentRoom);
                    itemBGBytes = smartCamera.GetImageMask().EncodeToPNG();
                    File.WriteAllBytes(path + "/" + index2.ToString() + "_mask.png", itemBGBytes);
                }

                if (captureRGB) {
                    writer.WriteLine(index2.ToString() + "_rgb.png;" + transform.position + ";" + transform.rotation.eulerAngles + ";"
                        + smartCamera.transform.position + ";" + smartCamera.transform.rotation.eulerAngles + ";" + currentRoom);
                    itemBGBytes = smartCamera.ImageRGB.EncodeToPNG();
                    File.WriteAllBytes(path + "/" + index2.ToString() + "_rgb.png", itemBGBytes);

                }

                if (captureDepth) {
                    writer.WriteLine(index2.ToString() + "_depth.png;" + transform.position + ";" + transform.rotation.eulerAngles + ";"
                        + smartCamera.transform.position + ";" + smartCamera.transform.rotation.eulerAngles + ";" + currentRoom);
                    itemBGBytes = smartCamera.ImageDepth.EncodeToPNG();
                    File.WriteAllBytes(path + "/" + index2.ToString() + "_depth.png", itemBGBytes);
                }

                agent.isStopped = false;
                index2++;
                if (frecuency != 0) {
                    yield return new WaitForSeconds(frecuency);
                }
                
            }
        }


        private void Log(string _msg) {
            if (verbose > 1)
                Debug.Log("[IAWander]: " + _msg);
        }

        private void LogWarning(string _msg) {
            if (verbose > 0)
                Debug.LogWarning("[IAWander]: " + _msg);
        }
        #endregion
    }
}
