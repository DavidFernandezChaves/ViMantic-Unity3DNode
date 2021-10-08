using UnityEngine;
using System.Collections;

public class FlyCamera : MonoBehaviour
{
    [TextArea]
    public string MyTextArea ="Instrucciones: \n A-Left \n D -Right";
    public float moveSpeed = 1;
    public float rotationSpeed = 20;
    
    void Update()
    {
        // Keyboard commands.
        Vector3 traslation = getDirection();
        traslation *= Time.deltaTime * moveSpeed;
        transform.Translate(traslation);

        Vector3 eulerAngles = transform.rotation.eulerAngles + getRotation()* rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(eulerAngles);

        Vector3 newPosition = transform.position;
        if (Input.GetKey(KeyCode.V))
        { //If player wants to move on X and Z axis only
            transform.Translate(traslation);
            newPosition.x = transform.position.x;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
        else
        {
            transform.Translate(traslation);
        }

    }

    private Vector3 getDirection()
    {
        Vector3 direction = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            direction += new Vector3(1, 0, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.C))
        {
            direction += new Vector3(0, -1, 0);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            direction += new Vector3(0, 1, 0);
        }
        return direction;
    }

    private Vector3 getRotation()
    {
        Vector3 rotation = new Vector2();
        if (Input.GetKey(KeyCode.Q))
        {
            rotation += new Vector3(0,-1,0);
        }
        if (Input.GetKey(KeyCode.E))
        {
            rotation += new Vector3(0, 1, 0);
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            rotation += new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            rotation += new Vector3(0, 0, 1);
        }

        return rotation;
    }
}
