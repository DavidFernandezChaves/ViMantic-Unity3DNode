using UnityEngine;

[ExecuteInEditMode]
public class ColorView : MonoBehaviour {   

    void Start() {
        GetComponent<Camera>().SetReplacementShader(Shader.Find("Custom/UnlitColorOnly"), "RenderType");
    }

}