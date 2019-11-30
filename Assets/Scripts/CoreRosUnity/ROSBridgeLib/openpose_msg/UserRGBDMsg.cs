using SimpleJSON;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.geometry_msgs;
using System;

namespace ROSBridgeLib
{
    namespace openpose_msgs
    {
        public class UserRGBDMsg : ROSBridgeMsg
        {
            private String _name;
            private float _u;
            private float _v;
            private float _w;
            private float _h;

            private PoseMsg _pose_3D;
            private float _certainty;

            private BodyPart3DMsg[] _body_part_3d;


            public UserRGBDMsg(JSONNode msg)
            {
                _name = msg["name"];
                _u = float.Parse(msg["u"], System.Globalization.CultureInfo.InvariantCulture);
                _v = float.Parse(msg["v"], System.Globalization.CultureInfo.InvariantCulture);
                _w = float.Parse(msg["w"], System.Globalization.CultureInfo.InvariantCulture);
                _h = float.Parse(msg["h"], System.Globalization.CultureInfo.InvariantCulture);
                _pose_3D = new PoseMsg(msg["pose_3D"]);
                _certainty = float.Parse(msg["certainty"], System.Globalization.CultureInfo.InvariantCulture);

                _body_part_3d = new BodyPart3DMsg[msg["body_part_3d"].Count];
                for (int i = 0; i < _body_part_3d.Length; i++)
                {
                    _body_part_3d[i] = new BodyPart3DMsg(msg["body_part_3d"][i]);
                }
            }

            public UserRGBDMsg(string name, float u, float v, float w, float h, PoseMsg pose_3d, float certainty)
            {
                _name = name;
                _u = u;
                _v = v;
                _w = w;
                _h = h;
                _pose_3D = pose_3d;
                _certainty = certainty;
            }

            public static string GetMessageType()
            {
                return "UserRGBD";
            }

            public string GetName()
            {
                return _name;
            }

            public PoseMsg GetPose()
            {
                return _pose_3D;
            }

            public float GetCertainty()
            {
                return _certainty;
            }

            public BodyPart3DMsg[] GetBodyParts()
            {
                return _body_part_3d;
            }

            public override string ToString()
            {
                string array = "[";
                for (int i = 0; i < _body_part_3d.Length; i++)
                {
                    array = array + _body_part_3d[i].ToString();
                    if (_body_part_3d.Length - i <= 1)
                        array += ",";
                }
                array += "]";

                return "User [name=" + _name.ToString() + ", u="+ _u.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                                                        + ", v="+ _v.ToString("F", System.Globalization.CultureInfo.InvariantCulture) 
                                                        + ", w="+ _w.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                                                        + ", h="+ _h.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                                                        + ",  pose_3D=" + _pose_3D.ToString()
                                                        + ",  certainty=" + _certainty.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                                                        + ", body_part_3d=" + array
                                                        + "]";
            }

            public override string ToYAMLString()
            {
                string array = "[";
                for (int i = 0; i < _body_part_3d.Length; i++)
                {
                    array = array + _body_part_3d[i].ToYAMLString();
                    if (_body_part_3d.Length - i <= 1)
                        array += ",";
                }
                array += "]";
                return "{\"name\" : " + _name 
                    + ", \"u\" : " + _u.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", \"v\" : " + _v.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", \"w\" : " + _v.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", \"h\" : " + _v.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", \"pose_3D\" : " + _pose_3D.ToYAMLString()
                    + ", \"certainty\" : " + _certainty.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
                    + ", \"body_part_3d\" : " + array
                    + "}";
            }
        }
    }
}
