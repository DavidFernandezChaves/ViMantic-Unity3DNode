using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneManaguer : MonoBehaviour {


    public CanvasGroup[] _windows;
    public GameObject _mapPanel;
    public Transform _panelLeftDown;
    public GameObject _gamObjPanelRobot;
    public GameObject[] _pointCloudHouses;
    public GameObject _gamObjRobot;
    public Text _txVersion;
    public InputField _InFiTxNameMap, InFiTfRobot;

    public string _nameMap;
    public List<Transform> _robots;

    private GameObject _geometricMap;
    

	// Use this for initialization
	void Start () {
        _txVersion.text = "Version: " + Application.version + "\nUse the \"A\", \"D\", \"W\", \"S\", \"E\", \"C\" keys to move. Right Mouse Button to Rotate the Camera. Shift to move quickly.";
        _nameMap = PlayerPrefs.GetString("nameMap", "Semantic map 1");
        _InFiTxNameMap.text = _nameMap;
        _robots = new List<Transform>();
    }

    public void NewNameMap(string tx) {
        _nameMap = tx;
        PlayerPrefs.SetString("nameMap", tx);
        PlayerPrefs.Save();
        Debug.Log("save");
    }

    public void NewConnection(Text TxIp) {
        GameObject robot = Instantiate(_gamObjRobot);
        robot.name = "ws://" + TxIp.text;
        var _tfFrameID_temp = GameObject.Find(InFiTfRobot.text);
        if (_tfFrameID_temp == null) {
            _tfFrameID_temp = new GameObject() { name = InFiTfRobot.text };
        }
        robot.transform.parent = _tfFrameID_temp.transform;
        var ros = robot.GetComponent<ROS>();
        ros._ip = TxIp.text;
        ros._enabledPackages = new List<string>() { "Tf_sub", "Semantic_mapping_sub" };
        if (_robots.Count==0)
            ros._enabledPackages.Add("Map_sub");
        ros.Connect();
        _robots.Add(robot.transform);
        var panel = Instantiate(_gamObjPanelRobot, _panelLeftDown);
        panel.GetComponentsInChildren<Text>()[0].text = TxIp.text;
        panel.name = robot.name;
        panel.GetComponentInChildren<Button>().onClick.AddListener(delegate {
            ros.Disconnect();
            _robots.Remove(robot.transform);
            Destroy(robot);
            Destroy(panel);
        });
        SelectWindow(0);
    }

    void MapLoaded(Terrain terrain)
    {
        _geometricMap = terrain.gameObject;
        _mapPanel.SetActive(true);
    }

    public void TurnMap(bool mode) {
        _geometricMap.SetActive(mode);
    }

    public void LoadPointCloudHouse(int id) {
        foreach(GameObject o in _pointCloudHouses) { o.SetActive(false); }
        _pointCloudHouses[id].SetActive(true);
    }

    public void Turn(int id,bool mode) {
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

}
