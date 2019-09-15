using UnityEngine;
using ROSBridgeLib;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

public class ROS : MonoBehaviour  {

    //public enum Packages {Trajectory,BatteryState,UserDetected };

    public string _ip = "localhost";
	public int _pot = 9090;
    public bool viewfinder = false;
    public bool _debug = false;
    public List<string> _enabledPackages;
    

    private ROSBridgeWebSocketConnection _ros = null;
	private DateTime epochStart;


    public void Connect() {
        epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _ros = new ROSBridgeWebSocketConnection("ws://"+ _ip, _pot);
        _ros.SetDebug(_debug);
        if(_debug)
            Debug.Log("Connecting IP:" + _ip);
        try
        {
            _ros.Connect();
        }
        catch {
            Debug.LogWarning("Fault when connecting to "+ _ip);
            Destroy(this.gameObject);
        }
            
        Invoke("InitialPackage", 0.5f);
    }

    public void Disconnect() {
        if (_ros != null)
        {
            _ros.Disconnect();
        }
    }

    void InitialPackage() {
        foreach (string type in _enabledPackages) {
            _ros.AddSubscriberOnline(Type.GetType(type));
        }

        gameObject.SendMessage("Connected",SendMessageOptions.DontRequireReceiver);
    }

    public void Subcribe(Type type,int frecuency) {
        _ros.AddSubscriberOnline(type, frecuency);
    }

    public void UnSubcribe(Type unsubcribe) {
        _ros.UnSubcribe(unsubcribe);
    }

    public void Publish(String topic,ROSBridgeMsg msg)
    {
        if(!viewfinder)
            _ros.Publish(topic, msg);
    }

    // extremely important to disconnect from ROS. OTherwise packets continue to flow
    void OnApplicationQuit() {
        if (_ros != null) {
            _ros.Disconnect();
        }			
	}

	void Update () {
        if(_ros!=null)
            _ros.Render ();
	}

    public DateTime GetepochStart() { return epochStart; }
    public ROSBridgeWebSocketConnection GetCore() { return _ros; }

}