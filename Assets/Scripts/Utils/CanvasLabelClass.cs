using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasLabelClass : MonoBehaviour
{

    private Dictionary<string, Color> _colors;
    private Transform user;
    public Image _panel;
    public Text _label;

    // Start is called before the first frame update
    void Start()
    {
        try {
            user = GameObject.Find("User").transform;
        } catch { }
        
        Colors();
    }

    // Update is called once per frame
    void Update()
    {
        if (user != null && Time.frameCount % 2 == 0)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - user.transform.position);
        }
    }

    public void LoadLabel(string label, double confidenceScorescore) {

        if (_colors == null)
            Colors();

        if (_colors.ContainsKey(label))
        {
            Color32 c = _colors[label];
            c.a = (byte)(confidenceScorescore * 255);
            _panel.color = c;
        }
        else
        {
            _panel.color = new Color32(41, 41, 41, (byte)(confidenceScorescore * 255));
        }

        transform.localScale = Vector3.one * (0.015f * (float) confidenceScorescore);

        _label.text = "  " + label.ToUpper() + " (" + (confidenceScorescore).ToString("0.00") + ")  ";
    }

    public void RemoveThisSemanticObject()
    {
        transform.parent.GetChild(0).GetComponent<VirtualSemanticObject>().RemoveSemanticObject();
    }

    private void Colors()
    {
        _colors = new Dictionary<string, Color>
        {
            { "tv", new Color32(94, 126, 127, 255) },
            { "oven", new Color32(164, 126, 127, 255) },
            { "microwave", new Color32(183, 126, 127, 255) },
            { "toaster", new Color32(210, 126, 127, 255) },
            { "toilet", new Color32(0, 143, 255, 255) },
            { "bed", new Color32(0, 140, 80, 255) },
            { "bench", new Color32(128, 107, 85, 255) },
            { "couch", new Color32(178, 107, 0, 255) },
            { "dining table", new Color32(130, 25, 0, 255) },
            { "sink", new Color32(0, 189, 179, 255) },
            { "chair", new Color32(78, 25, 0, 255) },
            { "potted plant", new Color32(95, 166, 97, 255) },
            { "backpack", new Color32(20, 46, 61, 255) },
            { "vase", new Color32(224, 212, 150, 255) },
            { "handbag", new Color32(83, 62, 31, 255) },
            { "bowl", new Color32(196, 225, 31, 255) },
            { "bottle", new Color32(58, 42, 41, 255) },
            { "cell phone", new Color32(15, 29, 8, 255) },
            { "book", new Color32(185, 132, 47, 255) },
            { "teddy_bear", new Color32(172, 63, 98, 255) },
            { "clock", new Color32(44, 63, 98, 255) }
        };
    }
}
