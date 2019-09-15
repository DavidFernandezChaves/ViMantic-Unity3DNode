using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SemanticObjectComponent : MonoBehaviour
{
    public Vector2 _heightCanvas;
    public Transform _canvas;
    public Transform _cube;
    public SemanticObject _semanticObject;

    public Transform user;
   
    private void Start()
    {
        var _position_canvas = _canvas.position;
        _position_canvas.y = Random.Range(_heightCanvas.x, _heightCanvas.y);
        _canvas.transform.position = _position_canvas;
        user = GameObject.Find("User").transform;
    }

    private void Update()
    {
        //_canvas.transform.LookAt(user);
        _canvas.rotation = Quaternion.LookRotation(_canvas.position - user.transform.position);
    }

    public void Load()
    {
        transform.name = _semanticObject._id;
        transform.position = _semanticObject._pose;

        gameObject.GetComponentInChildren<Text>().text ="  "+ name.ToUpper() + " (" + (_semanticObject._accuracyEstimation).ToString("0.00") + ")  ";
        if (_cube == null)
            _cube = this.transform.Find("Cube").transform;

        _cube.localScale = _semanticObject._dimensions;
        _cube.rotation = _semanticObject._rotation;
    }
}
