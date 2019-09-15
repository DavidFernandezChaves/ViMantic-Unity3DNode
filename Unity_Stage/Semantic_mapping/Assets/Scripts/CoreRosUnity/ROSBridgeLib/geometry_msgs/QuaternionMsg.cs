using SimpleJSON;
using UnityEngine;

namespace ROSBridgeLib
{
    namespace geometry_msgs
    {
        public class QuaternionMsg : ROSBridgeMsg
        {
            private float _x;
            private float _y;
            private float _z;
            private float _w;

            public QuaternionMsg(JSONNode msg)
            {
                _x = System.Convert.ToSingle(double.Parse(msg["x"], System.Globalization.CultureInfo.InvariantCulture));
                _y = System.Convert.ToSingle(double.Parse(msg["y"], System.Globalization.CultureInfo.InvariantCulture));
                _z = System.Convert.ToSingle(double.Parse(msg["z"], System.Globalization.CultureInfo.InvariantCulture));
                _w = System.Convert.ToSingle(double.Parse(msg["w"], System.Globalization.CultureInfo.InvariantCulture));
            }

            public QuaternionMsg(float x, float y, float z, float w)
            {
                _x = x;
                _y = y;
                _z = z;
                _w = w;
            }

            public QuaternionMsg(float yaw, float pitch, float roll)
            {
                float halfYaw = yaw * 0.5f;
                float halfPitch = pitch * 0.5f;
                float halfRoll = roll * 0.5f;
                float cosYaw = Mathf.Cos(halfYaw);
                float sinYaw = Mathf.Sin(halfYaw);
                float cosPitch = Mathf.Cos(halfPitch);
                float sinPitch = Mathf.Sin(halfPitch);
                float cosRoll = Mathf.Cos(halfRoll);
                float sinRoll = Mathf.Sin(halfRoll);

                _x = sinRoll * cosPitch * cosYaw - cosRoll * sinPitch * sinYaw;
                _y = cosRoll * sinPitch * cosYaw + sinRoll * cosPitch * sinYaw;
                _z = cosRoll * cosPitch * sinYaw - sinRoll * sinPitch * cosYaw;
                _w = cosRoll * cosPitch * cosYaw + sinRoll * sinPitch * sinYaw;
            }

            public QuaternionMsg(Quaternion q) : this(-(q.eulerAngles * Mathf.Deg2Rad).y,
                                                    (q.eulerAngles * Mathf.Deg2Rad).x,
                                                    (q.eulerAngles * Mathf.Deg2Rad).z) { }

            public static string GetMessageType()
            {
                return "geometry_msgs/Quaternion";
            }


            public Vector3 GetRotationEulerROS(int solution) {           
                return TfUtils.GetEulerYPR(new Quaternion(_x, _y, _z, _w), solution);
            }

            public Vector3 GetRotationEulerUnity(int solution) {
                Vector3 rotationEuler =  TfUtils.GetEulerYPR(new Quaternion(_x, _y, _z, _w), solution) * Mathf.Rad2Deg;
                return new Vector3(rotationEuler.z, -rotationEuler.x, -rotationEuler.y);
            }

            public Quaternion GetQuaternionUnity(int solution) {
                //return Quaternion.Euler(GetRotationEulerUnity(solution));
                return Quaternion.Euler(GetRotationEulerUnity(solution));
            }

            public float GetX()
            {
                return _x;
            }

            public float GetY()
            {
                return _y;
            }

            public float GetZ()
            {
                return _z;
            }

            public float GetW() {
                return _w;
            }

            public override string ToString()
            {
                return "Quaternion [x=" + _x.ToString("F", System.Globalization.CultureInfo.InvariantCulture) + ",  y=" + _y.ToString("F", System.Globalization.CultureInfo.InvariantCulture) + ",  z=" + _z.ToString("F", System.Globalization.CultureInfo.InvariantCulture) + ",  w=" + _w.ToString("F", System.Globalization.CultureInfo.InvariantCulture) + "]";
            }

            public override string ToYAMLString()
            {
                return "{\"x\" : " + _x.ToString("F", System.Globalization.CultureInfo.InvariantCulture) + ", \"y\" : " + _y.ToString("F", System.Globalization.CultureInfo.InvariantCulture) + ", \"z\" : " + _z.ToString("F", System.Globalization.CultureInfo.InvariantCulture) + ", \"w\" : " + _w.ToString("F", System.Globalization.CultureInfo.InvariantCulture) + "}";
            }
        }
    }
}