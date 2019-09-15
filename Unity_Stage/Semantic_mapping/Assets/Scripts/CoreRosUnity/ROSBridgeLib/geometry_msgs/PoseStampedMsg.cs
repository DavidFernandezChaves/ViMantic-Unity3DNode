using SimpleJSON;
using ROSBridgeLib.std_msgs;
using UnityEngine;

namespace ROSBridgeLib
{
    namespace geometry_msgs
    {
        public class PoseStampedMsg : ROSBridgeMsg
        {
            private HeaderMsg _header;
            private PoseMsg _pose;

            public PoseStampedMsg(JSONNode msg)
            {
                _header = new HeaderMsg(msg["header"]);
                _pose = new PoseMsg(msg["pose"]);
            }

            public PoseStampedMsg(HeaderMsg header, PoseMsg pose)
            {
                _header = header;
                _pose = pose;
            }

            public static string GetMessageType()
            {
                return "geometry_msgs/PoseStamped";
            }

            public HeaderMsg GetHeader()
            {
                return _header;
            }

            public PoseMsg GetPose()
            {
                return _pose;
            }

            public bool Equals(PoseStampedMsg other)
            {
                if (other == null) return false;
                return (this._header.GetSeq().Equals(other._header.GetSeq()));
            }

            public override string ToString()
            {
                return "PoseStamped [header=" + _header.ToString() + ",  pose=" + _pose.ToString() + "]";
            }

            public override string ToYAMLString()
            {
                return "{\"header\" : " + _header.ToYAMLString() + ", \"pose\" : " + _pose.ToYAMLString() + "}";
            }
        }
    }
}