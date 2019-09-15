using SimpleJSON;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.geometry_msgs;
using System.Collections.Generic;

namespace ROSBridgeLib
{
    namespace nav_msgs
    {
        public class PathMsg : ROSBridgeMsg
        {
            private HeaderMsg _header;
            private PoseStampedMsg[] _poses;

            public PathMsg(JSONNode msg)
            {
                _header = new HeaderMsg(msg["header"]);
                _poses = new PoseStampedMsg[msg["poses"].Count];
                for (int i = 0; i < _poses.Length; i++)
                {
                    _poses[i] = new PoseStampedMsg(msg["poses"][i]);
                }
            }

            public PathMsg(HeaderMsg header, PoseStampedMsg[] poses)
            {
                _header = header;
                _poses = poses;
            }

            public PathMsg(HeaderMsg header, List<PoseStampedMsg> poses)
            {
                _header = header;
                _poses = poses.ToArray();
            }

            public static string GetMessageType()
            {
                return "nav_msgs/Path";
            }

            public HeaderMsg GetHeader()
            {
                return _header;
            }

            public PoseStampedMsg[] GetPoses()
            {
                return _poses;
            }

            public override string ToString()
            {
                string array = "[";
                for (int i = 0; i < _poses.Length; i++)
                {
                    array = array + _poses[i];
                    if (i < (_poses.Length - 1))
                        array += ",";
                }
                array += "]";
                return "Path [header=" + _header.ToString() + ",  poses=" + array + "]";
            }

            public override string ToYAMLString()
            {
                string array = "[";
                for (int i = 0; i < _poses.Length; i++)
                {
                    array = array + _poses[i].ToYAMLString();
                    if (i<(_poses.Length - 1))
                        array += ",";
                }
                array += "]";
                return "{\"header\" : " + _header.ToYAMLString() + ", \"poses\" : " + array + "}";
            }
        }
    }
}
