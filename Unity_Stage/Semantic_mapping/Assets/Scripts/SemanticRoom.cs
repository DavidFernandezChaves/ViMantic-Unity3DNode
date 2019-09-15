using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SemanticRoom : MonoBehaviour
{
    public List<Transform> _trigger;
    public string _ID;
    public string _typeRoom = "Unknow";

    public bool PointInside(Vector3 point) {
        bool result = false;
        foreach (Transform _tramsform in _trigger) {

            if ((_tramsform.position.y + _tramsform.localScale.y / 2) > point.y && (_tramsform.position.y - _tramsform.localScale.y / 2) < point.y) {
                var angle = -_tramsform.rotation.eulerAngles.y * Mathf.Deg2Rad;

                var m1 = Mathf.Tan(angle);
                var m2 = Mathf.Tan(angle + Mathf.PI / 2);

                //Line1
                var px1 = _tramsform.position.x + (_tramsform.lossyScale.z / 2) * Mathf.Cos(Mathf.PI / 2 + angle);
                var py1 = _tramsform.position.z + (_tramsform.lossyScale.z / 2) * Mathf.Sin(Mathf.PI / 2 + angle);                

                var C1 = -m1 * px1 + py1;

                //Line2
                var px2 = _tramsform.position.x + (-_tramsform.lossyScale.z / 2) * Mathf.Cos(Mathf.PI / 2 + angle);
                var py2 = _tramsform.position.z + (-_tramsform.lossyScale.z / 2) * Mathf.Sin(Mathf.PI / 2 + angle);

                var C2 = -m1 * px2 + py2;

                //Line3
                var px3 = _tramsform.position.x + (_tramsform.lossyScale.x / 2) * Mathf.Cos(angle);
                var py3 = _tramsform.position.z + (_tramsform.lossyScale.x / 2) * Mathf.Sin(angle);                

                var C3 = -m2 * px3 + py3;

                //Line4
                var px4 = _tramsform.position.x + (-_tramsform.lossyScale.x / 2) * Mathf.Cos(angle);
                var py4 = _tramsform.position.z + (-_tramsform.lossyScale.x / 2) * Mathf.Sin(angle);

                var C4 = -m2 * px4 + py4;

                var distancia1 = Mathf.Abs(m1 * point.x - point.z + C1) / Mathf.Sqrt(Mathf.Pow(m1, 2) + 1);
                var distancia2 = Mathf.Abs(m1 * point.x - point.z + C2) / Mathf.Sqrt(Mathf.Pow(m1, 2) + 1);
                var distancia3 = Mathf.Abs(m2 * point.x - point.z + C3) / Mathf.Sqrt(Mathf.Pow(m2, 2) + 1);
                var distancia4 = Mathf.Abs(m2 * point.x - point.z + C4) / Mathf.Sqrt(Mathf.Pow(m2, 2) + 1);

                //Debug.Log(point + "/" + _tramsform.position + "/" + _tramsform.lossyScale + _tramsform.rotation.eulerAngles.y);
                //Debug.Log(distancia3 + "/" + distancia4 + "/" + (distancia3 + distancia4) + "/" + _tramsform.lossyScale.x);
                //Debug.Log(distancia1 + "/" + distancia2 + "/" + (distancia2 + distancia1) + "/" + _tramsform.lossyScale.z);
                //Debug.Log((distancia2 + distancia1 - _tramsform.lossyScale.z)+"/"+(distancia3 + distancia4 - _tramsform.lossyScale.x));
                if (Mathf.Abs(distancia2 + distancia1 - _tramsform.lossyScale.z) <= 0.001 && Mathf.Abs(distancia3 + distancia4 - _tramsform.lossyScale.x) <= 0.001)
                {
                    result = true;
                    break;
                }
            }
        }

        return result;
    }


}
