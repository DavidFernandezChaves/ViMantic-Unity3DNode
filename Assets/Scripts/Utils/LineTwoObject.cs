using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]

public class LineTwoObject : MonoBehaviour {

    public GameObject gameObject1;          // Reference to the first GameObject
    public GameObject gameObject2;          // Reference to the second GameObject

    private LineRenderer line;                           // Line Renderer

    // Use this for initialization
    void Start()
    {
        // Add a Line Renderer to the GameObject
        line = GetComponent<LineRenderer>();
        UpdateLine();
    }

    private void Update()
    {
        UpdateLine();
    }

    public void UpdateLine() {
        // Check if the GameObjects are not null
        if (gameObject1 != null && gameObject2 != null)
        {
            // Update position of the two vertex of the Line Renderer
            line.SetPosition(0, gameObject1.transform.position-new Vector3(0,0.2f,0));
            line.SetPosition(1, gameObject2.transform.position);
        }
    }

}
