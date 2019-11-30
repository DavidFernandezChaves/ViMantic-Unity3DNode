using SimpleJSON;
using ROSBridgeLib.std_msgs;

namespace ROSBridgeLib
{
    namespace geometry_msgs
    {
        public class TransformStampedMsg : ROSBridgeMsg
        {
            private  HeaderMsg _header;
            private string _child_frame_id;
            private TransformMsg _transform;

            public TransformStampedMsg(JSONNode msg)
            {
                _header = new HeaderMsg(msg["header"]);
                _child_frame_id = msg["child_frame_id"];
                _transform = new TransformMsg(msg["transform"]);
            }

            public TransformStampedMsg(HeaderMsg header, string child_frame_id, TransformMsg transform)
            {
                _header = header;
                _child_frame_id = child_frame_id;
                _transform = transform;
            }

            public static string GetMessageType()
            {
                return "geometry_msgs/TransformStamped";
            }

            public HeaderMsg Getheader()
            {
                return _header;
            }

            public string GetChild_frame_id()
            {
                return _child_frame_id;
            }

            public TransformMsg Gettransform()
            {
                return _transform;
            }

            public override string ToString()
            {
                return "TransformStamped [header=" + _header.ToString() + ",  child_frame_id=" + _child_frame_id + ",  transform=" + _transform.ToString() + "]";
            }

            public override string ToYAMLString()
            {
                return "{\"header\" : " + _header.ToYAMLString() + ", \"child_frame_id\" : " + _child_frame_id + ", \"transform\" : " + _transform.ToYAMLString() + "}";
            }
        }
    }
}