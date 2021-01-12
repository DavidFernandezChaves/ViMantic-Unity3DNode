using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ROSUnityCore;


namespace Vimantic {

    [RequireComponent(typeof(ObjectManager))]
    [RequireComponent(typeof(OntologyManager))]

    public class GeneralManager : MonoBehaviour {


        public CanvasGroup[] _windows;
        public GameObject _mapPanel;
        public Transform _panelLeftDown;
        public GameObject _gamObjPanelRobot;
        public GameObject[] _pointCloudHouses;
        public GameObject _gamObjRobot;
        public Text _txVersion, _txPath;
        public InputField _InFiTxNameMap, _InFiTfRobot, _InFiSetting0, _InFiSetting1, _InFiSetting2, _InFiSetting3;

        public string _nameMap;
        public List<Transform> _robots;

        private GameObject _geometricMap;
        private ObjectManager _semanticMapping;
        private OntologyManager _ontologyManager;
        private SemanticRoomManager _semanticRoomManager;

        // Use this for initialization
        void Start() {
            Application.targetFrameRate = -1;
            _txVersion.text = "Version: " + Application.version + "\nUse the \"A\", \"D\", \"W\", \"S\", \"E\", \"C\" keys to move. Right Mouse Button to Rotate the Camera. Shift to move quickly.";
            _txPath.text = Application.dataPath;
            _nameMap = PlayerPrefs.GetString("nameMap", "Semantic map 1");
            _InFiTxNameMap.text = _nameMap;
            _robots = new List<Transform>();
            _semanticMapping = GetComponent<ObjectManager>();
            _semanticMapping.maxDistance = PlayerPrefs.GetFloat("maxDistanceZ", 2f);
            _semanticMapping.minimunConfidenceScore = PlayerPrefs.GetFloat("minimunConfidenceScore", 0.5f);
            _InFiSetting0.text = _semanticMapping.minimunConfidenceScore.ToString();
            _InFiSetting2.text = _semanticMapping.maxDistance.ToString();
            _ontologyManager = GetComponent<OntologyManager>();
            _ontologyManager._pathToSave = PlayerPrefs.GetString("pathToSave", Application.dataPath);
            _InFiSetting3.text = _ontologyManager._pathToSave;
            _semanticRoomManager = GetComponent<SemanticRoomManager>();
            _ontologyManager.LoadOntology(_nameMap);
        }

        public void NewNameMap(string tx) {
            _nameMap = tx;
            PlayerPrefs.SetString("nameMap", tx);
            PlayerPrefs.Save();
            Debug.Log("save");
            _ontologyManager.LoadOntology(_nameMap);
        }

        public void NewConnection(Text TxIp) {
            GameObject robot = Instantiate(_gamObjRobot);
            robot.name = "ws://" + TxIp.text;
            var ros = robot.GetComponent<ROS>();
            ros.SetIP(TxIp.text);
            ros.pubPackages = new List<string>() { "RoomScores_pub", "ObjectsInRoom_pub", "CameraRGB_pub", "CameraDepth_pub" };
            ros.subPackages = new List<string>() { "Tf_sub", "Semantic_mapping_sub" };
            //if (_robots.Count==0)
            //    ros._subPackages.Add("Map_sub");


            var _tfFrameID_temp = GameObject.Find(_InFiTfRobot.text + "_" + ros.ip);
            if (_tfFrameID_temp == null) {
                _tfFrameID_temp = new GameObject() { name = _InFiTfRobot.text + "_" + ros.ip };
            }
            robot.transform.parent = _tfFrameID_temp.transform;

            ros.Connect();
            _robots.Add(robot.transform);

            //Add connectio to Menu
            var panel = Instantiate(_gamObjPanelRobot, _panelLeftDown);
            panel.GetComponentsInChildren<Text>()[0].text = TxIp.text;
            panel.name = robot.name;
            panel.GetComponentInChildren<Button>().onClick.AddListener(delegate {
                ros.Disconnect();
                _robots.Remove(robot.transform);
                try {
                    Destroy(_tfFrameID_temp);
                    Destroy(panel);
                } catch { Debug.LogWarning("Tf not found"); }
            });
            SelectWindow(0);
        }

        void MapLoaded(Terrain terrain) {
            _geometricMap = terrain.gameObject;
            _mapPanel.SetActive(true);
        }

        public void TurnMap(bool mode) {
            _geometricMap.SetActive(mode);
        }

        public void LoadPointCloudHouse(int id) {
            foreach (GameObject o in _pointCloudHouses) { o.SetActive(false); }
            _pointCloudHouses[id].SetActive(true);
        }

        public void Turn(int id, bool mode) {
            _windows[id].interactable = mode;
            _windows[id].blocksRaycasts = mode;
            _windows[id].alpha = mode ? 1 : 0;
        }

        public void SelectWindow(int id) {
            for (int i = 0; i < _windows.Length; i++) {
                Turn(i, false);
            }
            Turn(id, true);
        }

        public void SaveSetting() {
            _semanticMapping.maxDistance = float.Parse(_InFiSetting2.text);
            _semanticMapping.minimunConfidenceScore = float.Parse(_InFiSetting0.text);
            _ontologyManager._pathToSave = _InFiSetting3.text;
            PlayerPrefs.SetFloat("maxDistanceZ", _semanticMapping.maxDistance);
            PlayerPrefs.SetFloat("minimunConfidenceScore", _semanticMapping.minimunConfidenceScore);
            PlayerPrefs.SetString("pathToSave", _InFiSetting3.text);
            PlayerPrefs.Save();
            _ontologyManager.LoadOntology(_nameMap);
            SelectWindow(0);
        }

    }

}
