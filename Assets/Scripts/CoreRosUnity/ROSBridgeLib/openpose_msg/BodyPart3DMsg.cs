using SimpleJSON;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.geometry_msgs;
using System;

namespace ROSBridgeLib
{
    namespace openpose_msgs
    {
        public class BodyPart3DMsg : ROSBridgeMsg
        {
            private uint _idx;
            private float _score;
            private float _x_3d;
            private float _y_3d;
            private float _z_3d;

            public BodyPart3DMsg(JSONNode msg)
            {
                _idx = uint.Parse(msg["idx"], System.Globalization.CultureInfo.InvariantCulture);
                _score = float.Parse(msg["score"], System.Globalization.CultureInfo.InvariantCulture);
                _x_3d = float.Parse(msg["x_3d"], System.Globalization.CultureInfo.InvariantCulture);
                _y_3d = float.Parse(msg["y_3d"], System.Globalization.CultureInfo.InvariantCulture);
                _z_3d = float.Parse(msg["z_3d"], System.Globalization.CultureInfo.InvariantCulture);
            }

            public BodyPart3DMsg(uint idx, float score, float x_3d, float y_3d, float z_3d)
            {
                _idx = idx;
                _score = score;
                _x_3d = x_3d;
                _y_3d = y_3d;
                _z_3d = z_3d;
            }

            public static string GetMessageType()
            {
                return "BodyPart";
            }

            public uint GetIdx()
            {
                return _idx;
            }

            public float GetScore()
            {
                return _score;
            }

            public float Get_x_3d()
            {
                return _x_3d;
            }

            public float Get_y_3d()
            {
                return _y_3d;
            }

            public float Get_z_3d()
            {
                return _z_3d;
            }

            public override string ToString()
            {

                return "User [idx=" + _idx.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", score=" + _score.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", x_percent=" + _x_3d.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", y_percent=" + _y_3d.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", z_percent=" + _z_3d.ToString("F", System.Globalization.CultureInfo.InvariantCulture) + "]";
            }

            public override string ToYAMLString()
            {
                return "{\"idx\" : " + _idx.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", \"score\" : " + _score.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", \"x_percent\" : " + _x_3d.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", \"y_percent\" : " + _y_3d.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", \"z_percent\" : " + _z_3d.ToString("F", System.Globalization.CultureInfo.InvariantCulture) + "}";
            }
        }
    }
}
