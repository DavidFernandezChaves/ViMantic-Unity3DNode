using ROSUnityCore.ROSBridgeLib.semantic_mapping;
using ROSUnityCore.ROSBridgeLib.std_msgs;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ROSUnityCore;
using System.Collections;

[RequireComponent(typeof(OntologyManager))]

public class SemanticRoomManager : MonoBehaviour
{

    public int verbose;

    //Setting
    public bool recodTimes;
    public int nObservationsToConsider = 5;
    public float frequency = 2;
    public bool sendResultsToROS;
    public Dictionary<string,Dictionary<string, float>> semantic_rooms;
    public string currentRoom{ get; private set; }

    //UI
    public GameObject panel;
    public Text titleRoom;
    public Transform graph;
    public GameObject uiCategoryRoom;

    //Private
    private OntologyManager _ontologyManager;
    public Transform robot;
    private ROS ros;
    private List<Image> _bars;
    private List<Text> _probabilities, _categories;

    #region Unity Functions
    private void Awake() {
        _ontologyManager = GetComponent<OntologyManager>();
    }
    #endregion

    #region Public Functions
    public void OnVirtualEnviromentLoaded() {
        var ids = new List<string>();

        foreach (SemanticRoom room in FindObjectsOfType<SemanticRoom>()) {
            if (!ids.Contains(room.transform.name)) {
                ids.Add(room.transform.name);
                _ontologyManager.AddNewRoom(room.id, room.roomType.ToString());
                Log(room.transform.name + " added");
            }
        }
        var categories = _ontologyManager.GetCategoriesOfRooms();
        semantic_rooms = new Dictionary<string, Dictionary<string, float>>();

        foreach (string id in ids) {

            Dictionary<string, float> probabilities = new Dictionary<string, float>();
            foreach (string category in categories) {
                probabilities.Add(category, 1 / categories.Count);
            }

            semantic_rooms.Add(id, probabilities);
        }

        StartCoroutine("Timer");
    }

    public void UpdateRoom() {
        if(robot != null) {

            GetCurrentRoom();

            List<SemanticObject> detectedObjectsInside = _ontologyManager.GetPreviousDetections(currentRoom);

            if (detectedObjectsInside.Count > nObservationsToConsider) {
                detectedObjectsInside = detectedObjectsInside.GetRange(0, nObservationsToConsider);
            }

            Dictionary<String, float> probabilities = _ontologyManager.GetProbabilityCategories(detectedObjectsInside);
            semantic_rooms[currentRoom] = probabilities;
            if (panel != null) {
                UpdateUI(probabilities);
            }
            if (verbose > 2) {
                PrintResult(currentRoom);
            }
            if (sendResultsToROS) {
                PublishResult(detectedObjectsInside);
            }
            
        } else {
            FindRobot();
        }
    }

    public void PrintResult() {
        foreach (var room in semantic_rooms) {
            PrintResult(room.Key);
        }
    }

    public void PrintResult(string semanticRoom) {

        var categories = semantic_rooms[semanticRoom];

        //Print Result
        string tx = semanticRoom + ":\r\n";

        foreach (KeyValuePair<string, float> c in categories) {
            tx += c.Key + ": " + c.Value + " \r\n";
        }
        Debug.Log(tx);
    }

    #endregion

    #region Private Functions
    private IEnumerator Timer() {
        while (Application.isPlaying) {
            UpdateRoom();
            yield return new WaitForSeconds(frequency);
        }        
    }

    private void FindRobot() {
        ros = FindObjectOfType<ROS>();
        if(ros != null) {            
            robot = ros.transform;

            if (sendResultsToROS) {
                ros.RegisterPublishPackage("RoomScores_pub");
                ros.RegisterPublishPackage("ObjectsInRoom_pub");
            }
        }
                   
    }

    public SemanticRoom GetCurrentRoom() {
        RaycastHit hit;
        if (Physics.Raycast(robot.position, robot.TransformDirection(Vector3.down), out hit)) {
            SemanticRoom room = hit.transform.GetComponent<SemanticRoom>();
            if(room != null) {
                currentRoom = hit.transform.name;
                return room;
            } else {
                currentRoom = "Unknown";
            }            
        }
        return null;
    }

    private void UpdateUI(Dictionary<String, float> probabilities) {

        titleRoom.text = currentRoom;

        if (graph.childCount == 0) {
            _bars = new List<Image>();
            _probabilities = new List<Text>();
            _categories = new List<Text>();
            foreach (var category in probabilities) {
                var UI = Instantiate(uiCategoryRoom, graph);
                _bars.Add(UI.GetComponentInChildren<Image>());
                Text[] txs = UI.GetComponentsInChildren<Text>();
                _probabilities.Add(txs[0]);
                _categories.Add(txs[1]);
                txs[1].text = category.Key;
            }

            panel.SetActive(true);
        }

        foreach (var probability in probabilities) {
            var index = _categories.FindIndex(tx => tx.text.Equals(probability.Key));
            if (index >= 0) {
                _bars[index].fillAmount = probability.Value;
                _probabilities[index].text = probability.Value.ToString("0.00");
            }
        }

    }

    private void PublishResult(List<SemanticObject> detectedObjectsInside) {
        if (ros.IsConnected()) {
            HeaderMsg _head = new HeaderMsg(0, new TimeMsg(ros.epochStart.Second, 0), currentRoom.ToString());
            List<SemanticRoomScoreMsg> probabilities = new List<SemanticRoomScoreMsg>();

            foreach (KeyValuePair<string, float> result in semantic_rooms[currentRoom]) {
                probabilities.Add(new SemanticRoomScoreMsg(result.Key, result.Value));
            }

            SemanticRoomMsg msg = new SemanticRoomMsg(_head, currentRoom, probabilities.ToArray());
            ros.Publish(RoomScores_pub.GetMessageTopic(), msg);

            List<SemanticObjectMsg> obj_msg = new List<SemanticObjectMsg>();
            foreach (var obj in detectedObjectsInside) {
                obj_msg.Add(new SemanticObjectMsg(obj));
            }

            SemanticObjectsMsg msg2 = new SemanticObjectsMsg(_head, obj_msg.ToArray());
            ros.Publish(ObjectsInRoom_pub.GetMessageTopic(), msg2);
        }
    }

    private void Log(string _msg) {
    if (verbose > 1)
        Debug.Log("[Semantic Room Manager]: " + _msg);
    }

    private void LogWarning(string _msg) {
        if (verbose > 0)
            Debug.LogWarning("[Semantic Room Manager]: " + _msg);
    }
    #endregion

}
