using SimpleJSON;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.geometry_msgs;
using System;

namespace ROSBridgeLib
{
    namespace openpose_msgs
    {
        public class BodyPartMsg : ROSBridgeMsg
        {
            private uint _idx;
            private float _score;
            private float _x_percent;
            private float _y_percent;

            public BodyPartMsg(JSONNode msg)
            {
                _idx = uint.Parse(msg["idx"], System.Globalization.CultureInfo.InvariantCulture);
                _score = float.Parse(msg["score"], System.Globalization.CultureInfo.InvariantCulture);
                _x_percent = float.Parse(msg["x_percent"], System.Globalization.CultureInfo.InvariantCulture);
                _y_percent = float.Parse(msg["y_percent"], System.Globalization.CultureInfo.InvariantCulture);                
            }

            public BodyPartMsg(uint idx, float score, float x_percent, float y_percent)
            {
                _idx = idx;
                _score = score;
                _x_percent = x_percent;
                _y_percent = y_percent;
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

            public float Get_x_percent()
            {
                return _x_percent;
            }

            public float Get_y_percent()
            {
                return _y_percent;
            }

            public override string ToString()
            {

                return "User [idx=" + _idx.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", score=" + _score.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", x_percent=" + _x_percent.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", y_percent=" + _y_percent.ToString("F", System.Globalization.CultureInfo.InvariantCulture)+ "]";
            }

            public override string ToYAMLString()
            {
                return "{\"idx\" : " + _idx.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", \"score\" : " + _score.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", \"x_percent\" : " + _x_percent.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", \"y_percent\" : " + _y_percent.ToString("F", System.Globalization.CultureInfo.InvariantCulture) + "}";
            }
        }
    }
}
