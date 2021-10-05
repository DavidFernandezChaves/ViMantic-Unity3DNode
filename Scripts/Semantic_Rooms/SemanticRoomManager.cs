using ROSUnityCore.ROSBridgeLib.ViMantic_msgs;
using ROSUnityCore.ROSBridgeLib.std_msgs;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ROSUnityCore;
using System.Collections;
using ROSUnityCore.ROSBridgeLib.geometry_msgs;

namespace ViMantic
{

    [RequireComponent(typeof(OntologySystem))]

    public class SemanticRoomManager : MonoBehaviour
    {
        public static SemanticRoomManager instance;

        [Header("General")]
        [Tooltip("The log level to use")]
        public LogLevel LogLevel = LogLevel.Normal;

        public List<ROS> clients { get; private set; }

        //Setting
        public int nObservationsToConsider = 5;
        public float rate = 2;
        public bool sendResultsToROS;
        public Dictionary<string, Dictionary<string, float>> semantic_rooms;
        public string currentRoom { get; private set; }

        //UI
        public GameObject panel;
        public Text titleRoom;
        public Transform graph;
        public GameObject uiCategoryRoom;

        //Private
        public Transform robot;
        private List<Image> _bars;
        private List<Text> _probabilities, _categories;

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
            clients = new List<ROS>();
        }
        #endregion

        #region Public Functions
        public void VirtualEnviromentLoaded()
        {
            if (this.enabled)
            {
                var ids = new List<string>();
                foreach (SemanticRoom room in FindObjectsOfType<SemanticRoom>())
                {
                    if (!ids.Contains(room.id))
                    {
                        ids.Add(room.id);
                        OntologySystem.instance.AddNewRoom(room.id, room.roomType.ToString());
                        Log(room.id + " added", LogLevel.Developer);
                    }
                }
                var categories = OntologySystem.instance.GetCategoriesOfRooms();
                semantic_rooms = new Dictionary<string, Dictionary<string, float>>();

                foreach (string id in ids)
                {

                    Dictionary<string, float> probabilities = new Dictionary<string, float>();
                    foreach (string category in categories)
                    {
                        probabilities.Add(category, 1 / categories.Count);
                    }

                    semantic_rooms.Add(id, probabilities);
                }

                StartCoroutine(Timer());
            }
        }

        public void Connected(ROS ros)
        {
            if (robot == null) robot = ros.transform;
            clients.Add(ros);
            ros.RegisterPubPackage("RoomScores_pub");
            ros.RegisterPubPackage("ObjectsInRoom_pub");
        }


        public void Disconnected(ROS ros)
        {
            clients.Remove(ros);
        }

        public void UpdateRoom()
        {
            if (robot != null)
            {

                GetCurrentRoom();

                List<SemanticObject> detectedObjectsInside = OntologySystem.instance.GetPreviousDetections(currentRoom);

                if (detectedObjectsInside.Count > nObservationsToConsider)
                {
                    detectedObjectsInside = detectedObjectsInside.GetRange(0, nObservationsToConsider);
                }

                Dictionary<String, float> probabilities = OntologySystem.instance.GetProbabilityCategories(detectedObjectsInside);
                semantic_rooms[currentRoom] = probabilities;

                Log("Rooms probabilities updated",LogLevel.Developer);

                if (panel != null)
                {
                    UpdateUI(probabilities);
                }
                if (LogLevel == LogLevel.Developer)
                {
                    PrintResult(currentRoom);
                }
                if (sendResultsToROS)
                {
                    PublishResult(detectedObjectsInside);
                }

            }
            else
            {
                //FindRobot();
            }
        }

        public void PrintResult()
        {
            foreach (var room in semantic_rooms)
            {
                PrintResult(room.Key);
            }
        }

        public void PrintResult(string semanticRoom)
        {

            var categories = semantic_rooms[semanticRoom];

            //Print Result
            string tx = semanticRoom + ":\r\n";

            foreach (KeyValuePair<string, float> c in categories)
            {
                tx += c.Key + ": " + c.Value + " \r\n";
            }
            Debug.Log(tx);
        }

        #endregion

        #region Private Functions
        private IEnumerator Timer()
        {
            while (Application.isPlaying)
            {
                UpdateRoom();
                yield return new WaitForSeconds(rate);
            }
        }

        //private void FindRobot()
        //{
        //    ros = FindObjectOfType<ROS>();
        //    if (ros != null)
        //    {
        //        robot = ros.transform;

        //        if (sendResultsToROS)
        //        {
        //            ros.RegisterPubPackage("RoomScores_pub");
        //            ros.RegisterPubPackage("ObjectsInRoom_pub");
        //        }
        //    }

        //}

        public SemanticRoom GetCurrentRoom()
        {
            RaycastHit hit;
            Vector3 position = robot.position;
            position.y = -100;
            if (Physics.Raycast(position, robot.TransformDirection(Vector3.up), out hit))
            {
                SemanticRoom room = hit.transform.GetComponent<SemanticRoom>();
                if (room != null)
                {
                    currentRoom = room.id;
                    return room;
                }
                else
                {
                    currentRoom = "Unknown";
                }
            }
            return null;
        }

        private void UpdateUI(Dictionary<String, float> probabilities)
        {

            titleRoom.text = currentRoom;

            if (graph.childCount == 0)
            {
                _bars = new List<Image>();
                _probabilities = new List<Text>();
                _categories = new List<Text>();
                foreach (var category in probabilities)
                {
                    var UI = Instantiate(uiCategoryRoom, graph);
                    _bars.Add(UI.GetComponentInChildren<Image>());
                    Text[] txs = UI.GetComponentsInChildren<Text>();
                    _probabilities.Add(txs[0]);
                    _categories.Add(txs[1]);
                    txs[1].text = category.Key;
                }

                panel.SetActive(true);
            }

            foreach (var probability in probabilities)
            {
                var index = _categories.FindIndex(tx => tx.text.Equals(probability.Key));
                if (index >= 0)
                {
                    _bars[index].fillAmount = probability.Value;
                    _probabilities[index].text = probability.Value.ToString("0.00");
                }
            }

        }

        private void PublishResult(List<SemanticObject> detectedObjectsInside)
        {
            foreach (ROS client in clients)
            {
                HeaderMsg _head = new HeaderMsg(0, new TimeMsg(DateTime.Now.Second, 0), currentRoom.ToString());
                List<SemanticRoomScoreMsg> probabilities = new List<SemanticRoomScoreMsg>();

                foreach (KeyValuePair<string, float> result in semantic_rooms[currentRoom])
                {
                    probabilities.Add(new SemanticRoomScoreMsg(result.Key, result.Value));
                }

                SemanticRoomMsg msg = new SemanticRoomMsg(_head, currentRoom, probabilities.ToArray());
                client.Publish(RoomScores_pub.GetMessageTopic(), msg);

                if (detectedObjectsInside.Count > 0)
                {
                    List<SemanticObjectMsg> obj_msg = new List<SemanticObjectMsg>();
                    foreach (var obj in detectedObjectsInside)
                    {

                        var _scores = new ObjectHypothesisMsg[obj.Scores.Count];
                        int i = 0;
                        foreach (KeyValuePair<string, float> score in obj.Scores)
                        {
                            _scores[i] = new ObjectHypothesisMsg(score.Key, score.Value);
                            i++;
                        }

                        SemanticObjectMsg semanticObject = new SemanticObjectMsg(
                            obj.Id,
                            _scores, 
                            new PoseMsg(obj.Position, obj.Rotation),
                            obj.NDetections,
                            obj.GetIdRoom(),
                            obj.Room.roomType,
                            new Vector3Msg(obj.Size));

                        obj_msg.Add(semanticObject);
                    }

                    SemanticObjectArrayMsg msg2 = new SemanticObjectArrayMsg(_head, obj_msg.ToArray());
                    client.Publish(ObjectsInRoom_pub.GetMessageTopic(), msg2);
                }
            }
        }

        private void Log(string _msg, LogLevel lvl, bool Warning = false)
        {
            if (LogLevel <= lvl && LogLevel != LogLevel.Nothing)
            {
                if (Warning)
                {
                    Debug.LogWarning("[Semantic Room Manager]: " + _msg);
                }
                else
                {
                    Debug.Log("[Semantic Room Manager]: " + _msg);
                }
            }
        }
        #endregion

    }
}