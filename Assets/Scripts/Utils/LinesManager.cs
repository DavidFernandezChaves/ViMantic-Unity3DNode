using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]

public class LinesManager : MonoBehaviour {

    public Material _lineMaterial;

    public LineRenderer _line;

    private int _segments = 2;

    public void CanvasLine(Vector3 poseCanvas,Vector3 poseObject) {
        _line.SetPosition(0, poseCanvas - new Vector3(0, 0.2f, 0));        
        _line.SetPosition(1, poseObject);
        _line.enabled = true;
    }

    public void AddGameobjectLine(Vector3 position)
    {
        _segments++;
        _line.positionCount = _segments;
        _line.SetPosition(_segments-1, position);
    }

    public void SetOff() {
        _line.enabled = false;
    }



}
