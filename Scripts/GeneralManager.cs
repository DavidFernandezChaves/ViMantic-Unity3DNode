using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ROSUnityCore;


namespace Vimantic {

    [RequireComponent(typeof(VirtualObjectSystem))]
    [RequireComponent(typeof(OntologySystem))]

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
        private VirtualObjectSystem _semanticMapping;
        private OntologySystem _ontologyManager;
        private SemanticRoomManager _semanticRoomManager;

        // Use this for initialization
        void Start() {
            Application.targetFrameRate = -1;
            _txVersion.text = "Version: " + Application.version + "\nUse the \"A\", \"D\", \"W\", \"S\", \"E\", \"C\" keys to move. Right Mouse Button to Rotate the Camera. Shift to move quickly.";
            _txPath.text = Application.dataPath;
            _nameMap = PlayerPrefs.GetString("nameMap", "SemanticMap");
            _InFiTxNameMap.text = _nameMap;
            _robots = new List<Transform>();
            _semanticMapping = GetComponent<VirtualObjectSystem>();
            //_semanticMapping.minimunConfidenceScore = PlayerPrefs.GetFloat("minimunConfidenceScore", 0.5f);
            //_InFiSetting0.text = _semanticMapping.minimunConfidenceScore.ToString();
            _ontologyManager = GetComponent<OntologySystem>();
            //_ontologyManager.path = PlayerPrefs.GetString("pathToSave", Application.dataPath);
            _InFiSetting3.text = _ontologyManager.path;
            _semanticRoomManager = GetComponent<SemanticRoomManager>();
            _ontologyManager.LoadOntology();
        }

        public void NewNameMap(string tx) {
            _nameMap = tx;
            PlayerPrefs.SetString("nameMap", tx);
            PlayerPrefs.Save();
            Debug.Log("save");
            _ontologyManager.LoadOntology();
        }

        public void NewConnection(Text TxIp) {
            var _tfRobotParent = GameObject.Find(_InFiTfRobot.text);
            if (_tfRobotParent == null) {
                _tfRobotParent = new GameObject() { name = _InFiTfRobot.text};
            }

            GameObject robot = Instantiate(_gamObjRobot,_tfRobotParent.transform);
            ROS ros = robot.GetComponent<ROS>();
            ros.Connect(TxIp.text);
            _robots.Add(robot.transform);

            //Add connectio to Menu
            var panel = Instantiate(_gamObjPanelRobot, _panelLeftDown);
            panel.GetComponentsInChildren<Text>()[0].text = TxIp.text;
            panel.name = robot.name;
            panel.GetComponentInChildren<Button>().onClick.AddListener(delegate {
                ros.Disconnect();
                _robots.Remove(robot.transform);
                try {
                    Destroy(robot.gameObject);
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
            //_semanticMapping.minimunConfidenceScore = float.Parse(_InFiSetting0.text);
            //_ontologyManager.path = _InFiSetting3.text;
            //PlayerPrefs.SetFloat("minimunConfidenceScore", _semanticMapping.minimunConfidenceScore);
            PlayerPrefs.SetString("pathToSave", _InFiSetting3.text);
            PlayerPrefs.Save();
            _ontologyManager.LoadOntology();
            SelectWindow(0);
        }

    }

}
