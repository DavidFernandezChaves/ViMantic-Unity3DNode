using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

namespace RobotAtVirtualHome {

    [RequireComponent(typeof(NavMeshAgent))]

    public class IAWander : MonoBehaviour {

        public enum StatusMode { Loading, Walking, Turning, Finished }

        public int verbose;

        public bool loop;
        public bool randomRoom;
        public bool record;
        public float frecuency;
        
        public StatusMode State { get; private set; }
        public List<Vector3> VisitPoints { get; private set; }
        public string currentRoom { get; private set; }

        public bool RGBRecord;
        public bool DepthRecord;
        public bool MaskRecord;

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
                path = Path.Combine(path, "Wandering");

                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }

                Log("The saving path is:" + path);
                writer = new StreamWriter(path + "/Info.csv", true);
                writer.WriteLine("photoID;robotPosition;robotRotation;cameraPosition;cameraRotation;room");
                StartCoroutine("Record");
            }

            agent.SetDestination(VisitPoints[0]);
            agent.isStopped = false;
            State = StatusMode.Walking;
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
                        } else {
                            State = StatusMode.Finished;
                            Log("Finish");
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
        private bool GetNextGoal(out Vector3 result) {
            result = Vector3.zero;
            if (!loop) {
                VisitPoints.RemoveAt(index);
                if (VisitPoints.Count == 0) {
                    return false;
                }
                result = VisitPoints[Random.Range(0, VisitPoints.Count)];
            } else {
                if (randomRoom) {
                    result = VisitPoints[Random.Range(0, VisitPoints.Count)];
                } else {
                    index++;
                    if(index >= VisitPoints.Count) {
                        index = 0;
                    }
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
                if (MaskRecord) {
                    writer.WriteLine(index2.ToString() + "_mask.png;" + transform.position + ";" + transform.rotation.eulerAngles + ";"
                        + smartCamera.transform.position + ";" + smartCamera.transform.rotation.eulerAngles + ";" + currentRoom);
                    itemBGBytes = smartCamera.GetImageMask().EncodeToPNG();
                    File.WriteAllBytes(path + "/" + index2.ToString() + "_mask.png", itemBGBytes);
                }

                if (RGBRecord) {
                    writer.WriteLine(index2.ToString() + "_rgb.png;" + transform.position + ";" + transform.rotation.eulerAngles + ";"
                        + smartCamera.transform.position + ";" + smartCamera.transform.rotation.eulerAngles + ";" + currentRoom);
                    itemBGBytes = smartCamera.ImageRGB.EncodeToPNG();
                    File.WriteAllBytes(path + "/" + index2.ToString() + "_rgb.png", itemBGBytes);

                }

                if (DepthRecord) {
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
