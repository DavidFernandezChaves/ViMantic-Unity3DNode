using ROSBridgeLib.semantic_mapping;
using ROSBridgeLib.std_msgs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(OntologyManager))]

public class SemanticRoomManager : MonoBehaviour
{

    //Setting
    public int _nObservationsToConsider = 5;
    public float _frequency = 2;
    public Dictionary<string,Dictionary<string, float>> _semantic_rooms;

    //UI
    public GameObject _panel;
    public Text _titleRoom;
    public Transform _graph;
    public GameObject _uiCategoryRoom;

    //Private
    private OntologyManager _ontologyManager;
    private SemanticRoom _robotLocation;
    private List<Image> _bars;
    private List<Text> _probabilities, _categories;

    void Start()
    {
        _ontologyManager = GetComponent<OntologyManager>();
        Timer();
    }

    public void SetRobotIn(SemanticRoom semanticRoom) {
        _robotLocation = semanticRoom;
    }

    public void SeekRooms()
    {
        var ids = new List<string>();

        foreach (SemanticRoom room in FindObjectsOfType<SemanticRoom>())
        {
            if (!ids.Contains(room._id))
            {
                ids.Add(room._id);
                _ontologyManager.AddNewRoom(room._id, room._typeRoom);
            }
        }
        var categories = _ontologyManager.GetCategoriesOfRooms();
        _semantic_rooms = new Dictionary<string, Dictionary<string, float>>();

        foreach (string id in ids)
        {

            Dictionary<string, float> probabilities = new Dictionary<string, float>();
            foreach (string category in categories)
            {
                probabilities.Add(category, 1 / categories.Count);
            }

            _semantic_rooms.Add(id, probabilities);
        }
    }

    private void Timer() {
        UpdateRoom();
        Invoke("Timer", _frequency);
    }

    public void UpdateRoom() {

        if (_robotLocation != null)
        {

            List<SemanticObject> detectedObjectsInside = _ontologyManager.GetPreviousDetections(_robotLocation._id);
            
            if (detectedObjectsInside.Count > _nObservationsToConsider)
            {
                detectedObjectsInside = detectedObjectsInside.GetRange(0, _nObservationsToConsider);
            }

            Dictionary<String, float> probabilities = _ontologyManager.GetProbabilityCategories(detectedObjectsInside);
            _semantic_rooms[_robotLocation._id] = probabilities;
            UpdateUI(probabilities);
            PublishResult(detectedObjectsInside);
        }
    }

    private void UpdateUI(Dictionary<String, float> probabilities) {
        String[] strlist = _robotLocation._id.Split('_');
        _titleRoom.text = _robotLocation._id;

        if (_graph.childCount == 0)
        {
            _bars = new List<Image>();
            _probabilities = new List<Text>();
            _categories = new List<Text>();
            foreach (var category in probabilities)
            {
                var UI = Instantiate(_uiCategoryRoom, _graph);
                _bars.Add(UI.GetComponentInChildren<Image>());
                Text[] txs = UI.GetComponentsInChildren<Text>();
                _probabilities.Add(txs[0]);
                _categories.Add(txs[1]);
                txs[1].text = category.Key;
            }

            _panel.SetActive(true);
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

    //Mal para multirobot
    private void PublishResult(List<SemanticObject> detectedObjectsInside) {
        foreach (var robot in FindObjectsOfType<ROS>())
        {
            HeaderMsg _head = new HeaderMsg(0, new TimeMsg(robot.GetepochStart().Second, 0), _robotLocation._id.ToString());
            List<SemanticRoomScoreMsg> probabilities = new List<SemanticRoomScoreMsg>();

            foreach (KeyValuePair<string, float> result in _semantic_rooms[_robotLocation._id])
            {
                probabilities.Add(new SemanticRoomScoreMsg(result.Key, result.Value));
            }

            SemanticRoomMsg msg = new SemanticRoomMsg(_head, _robotLocation._id, probabilities.ToArray());
            robot.Publish(RoomScores_pub.GetMessageTopic(), msg);

            List<SemanticObjectMsg> obj_msg = new List<SemanticObjectMsg>();
            foreach (var obj in detectedObjectsInside)
            {
                obj_msg.Add(new SemanticObjectMsg(obj));
            }

            SemanticObjectsMsg msg2 = new SemanticObjectsMsg(_head, obj_msg.ToArray());
            robot.Publish(ObjectsInRoom_pub.GetMessageTopic(), msg2);
           
        }
    }

    public void PrintResult() {
        foreach (var room in _semantic_rooms)
        {
            PrintResult(room.Key);
        }
    }

    public void PrintResult(string semanticRoom) {

        var categories = _semantic_rooms[semanticRoom];

        //Print Result
        string tx = semanticRoom + ":\r\n";

        foreach (KeyValuePair<string, float> c in categories)
        {
            tx += c.Key + ": " + c.Value + " \r\n";
        }
        Debug.Log(tx);
    }

}
