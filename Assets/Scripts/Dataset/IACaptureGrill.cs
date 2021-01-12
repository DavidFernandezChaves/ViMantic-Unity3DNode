using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

namespace RobotAtVirtualHome {

    [RequireComponent(typeof(NavMeshAgent))]

    public class IACaptureGrill : MonoBehaviour {

        public enum StatusMode { Loading, Walking, Turning, Finished }

        public int verbose;
        public StatusMode State { get; private set; }
        public Vector2 minRange;
        public Vector2 maxRange;
        public float size = 0.5f;
        public int photosPerPoint = 10;

        public string room { get; private set; }
        public string path { get; private set; }

        private SmartCamera smartCamera;
        private List<Vector3> grill;
        private NavMeshAgent agent;
        private int index = 0;
        private StreamWriter writer;
        

        #region Unity Functions
        private void Awake() {
            agent = GetComponent<NavMeshAgent>();
            smartCamera = GetComponentInChildren<SmartCamera>();
            if(smartCamera == null) {
                LogWarning("Smart camera not found");
            }
        }

        void Start() {

            if(minRange[0] >= maxRange[0] || minRange[1]>= maxRange[1]) {
                LogWarning("Incorrect ranges");
            }
            path = FindObjectOfType<VirtualEnvironment>().path;
            path = Path.Combine(path, "Grill");

            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            
            Log("The saving path is:" + path);
            writer = new StreamWriter(path + "/InfoGrill.csv", true);
            writer.WriteLine("photoID;robotPosition;robotRotation;cameraPosition;cameraRotation;room");
            grill = new List<Vector3>();
            State = StatusMode.Loading;
            StartCoroutine(CalculateGrill());
        }

        private void Update() {
            switch (State) {
                case StatusMode.Walking:

                    RaycastHit hit;
                    if (Physics.Raycast(transform.position,transform.TransformDirection(Vector3.down), out hit)) {
                        room = hit.transform.name;
                    }

                    if (agent.remainingDistance <= agent.stoppingDistance &&
                        agent.velocity.sqrMagnitude == 0f) {
                        State = StatusMode.Turning;
                        StartCoroutine(Capture());
                        Log("Change state to Capture");
                    }
                    break;
            }
        }

        private void OnDestroy() {
            if (this.enabled) {
                writer.Close();
            }            
        }

#if UNITY_EDITOR 
        private void OnDrawGizmos() {
            if (Application.isPlaying && this.enabled && verbose>0) {
                Gizmos.color = Color.green;
                foreach(Vector3 point in grill) {
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

        private IEnumerator CalculateGrill() {
            NavMeshPath path = new NavMeshPath();
            for (float i = minRange[0]; i <= maxRange[0]; i += size) {
                for (float j = minRange[1]; j <= maxRange[1]; j += size) {
                    Vector3 point = new Vector3(i, transform.position.y, j);
                    agent.CalculatePath(point, path);
                    if (path.status == NavMeshPathStatus.PathComplete) {
                        grill.Add(point);
                    }                    
                }
            }
            yield return new WaitForEndOfFrame();
            agent.SetDestination(grill[index]);
            Log(grill[index].ToString());
            agent.isStopped = false; 
            yield return new WaitForEndOfFrame();
            State = StatusMode.Walking;
            Log("Start");

            yield return null;
        }

        private IEnumerator Capture() {
            transform.rotation = Quaternion.identity;
            yield return new WaitForEndOfFrame();
            for (int i = 1; i <= photosPerPoint; i++) {
                writer.WriteLine(index.ToString() + "_" + i.ToString() + "_rgb.png;" + transform.position + ";" + transform.rotation.eulerAngles + ";"
                    + smartCamera.transform.position + ";" + smartCamera.transform.rotation.eulerAngles + ";" + room);
                byte[] itemBGBytes = smartCamera.ImageRGB.EncodeToPNG();
                File.WriteAllBytes(path + "/"+ index.ToString() + "_" + i.ToString() + "_rgb.png", itemBGBytes);

                writer.WriteLine(index.ToString() + "_" + i.ToString() + "_depth.png;" + transform.position + ";" + transform.rotation.eulerAngles + ";"
                    + smartCamera.transform.position + ";" + smartCamera.transform.rotation.eulerAngles + ";" + room);
                itemBGBytes = smartCamera.ImageDepth.EncodeToPNG();
                File.WriteAllBytes(path + "/" + index.ToString() + "_" + i.ToString() + "depth.png", itemBGBytes);

                writer.WriteLine(index.ToString() + "_" + i.ToString() + "_mask.png;" + transform.position + ";" + transform.rotation.eulerAngles + ";"
                    + smartCamera.transform.position + ";" + smartCamera.transform.rotation.eulerAngles + ";" + room);
                itemBGBytes = smartCamera.GetImageMask().EncodeToPNG();
                File.WriteAllBytes(path + "/" + index.ToString() + "_" + i.ToString() + "_mask.png", itemBGBytes);
                transform.rotation = Quaternion.Euler(0, i * (360 / photosPerPoint), 0);
                yield return new WaitForEndOfFrame();
            }

            index++;
            if (index >= grill.Count) {
                State = StatusMode.Finished;
                Debug.Log("Finished");
            } else {
                agent.SetDestination(grill[index]);
                agent.isStopped = false;
                State = StatusMode.Walking;
                Log(grill[index].ToString());
            }

            yield return null;
        }

        private void Log(string _msg) {
            if (verbose > 1)
                Debug.Log("[Capture Grill]: " + _msg);
        }

        private void LogWarning(string _msg) {
            if (verbose > 0)
                Debug.LogWarning("[Capture Grill]: " + _msg);
        }
        #endregion
    }
}