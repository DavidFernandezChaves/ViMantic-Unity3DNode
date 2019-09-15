using SimpleJSON;
using UnityEngine;

namespace ROSBridgeLib
{
    namespace geometry_msgs
    {
        public class Point32Msg : ROSBridgeMsg
        {
            private float _x;
            private float _y;
            private float _z;

            public Point32Msg(JSONNode msg)
            {
                string d = msg["x"];
                _x = float.Parse(msg["x"], System.Globalization.CultureInfo.InvariantCulture);
                _y = float.Parse(msg["y"], System.Globalization.CultureInfo.InvariantCulture);
                _z = float.Parse(msg["z"], System.Globalization.CultureInfo.InvariantCulture);
            }

            public Point32Msg(float x, float y, float z)
            {
                _x = x;
                _y = y;
                _z = z;
            }

            public Point32Msg(Vector3 point)
            {
                _x = point.x;
                _y = point.y;
                _z = point.z;
            }

            public static string GetMessageType()
            {
                return "geometry_msgs/Point32";
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

            public Vector3 GetPoint()
            {
                return new Vector3((float)_x, (float)_y, (float)_z);
            }

            public override string ToString()
            {
                return "Point32 [x=" + _x.ToString("N", System.Globalization.CultureInfo.InvariantCulture) + ",  y=" + _y.ToString("N", System.Globalization.CultureInfo.InvariantCulture) + ",  z=" + _z.ToString("N", System.Globalization.CultureInfo.InvariantCulture) + "]";
            }

            public override string ToYAMLString()
            {
                return "{\"x\" : " + _x.ToString("N", System.Globalization.CultureInfo.InvariantCulture) + ", \"y\" : " + _y.ToString("N", System.Globalization.CultureInfo.InvariantCulture) + ", \"z\" : " + _z.ToString("N", System.Globalization.CultureInfo.InvariantCulture) + "}";
            }
        }
    }
}