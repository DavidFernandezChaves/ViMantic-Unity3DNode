using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]

public class LinesManager : MonoBehaviour {

    public Material _lineMaterial;

    private LineRenderer line;

    private int _segments = 2;

    private void Start() {
        line = GetComponent<LineRenderer>();
    }

    public void CanvasLine(Vector3 poseCanvas,Vector3 poseObject) {
        line.SetPosition(0, poseCanvas - new Vector3(0, 0.2f, 0));        
        line.SetPosition(1, poseObject);
        line.enabled = true;
    }

    public void AddGameobjectLine(Vector3 position)
    {
        _segments++;
        line.positionCount = _segments;
        line.SetPosition(_segments-1, position);
    }

    public void SetOff() {
        line.enabled = false;
    }



}
