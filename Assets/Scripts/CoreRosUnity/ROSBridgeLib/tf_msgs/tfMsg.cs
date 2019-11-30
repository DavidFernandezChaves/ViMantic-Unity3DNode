using SimpleJSON;
using ROSBridgeLib.geometry_msgs;

namespace ROSBridgeLib
{
    namespace tf_msgs
    {
        public class tfMsg : ROSBridgeMsg
        {
            private TransformStampedMsg[] _transforms;

            public tfMsg(JSONNode msg)
            {
                _transforms = new TransformStampedMsg[msg["transforms"].Count];
                for (int i = 0; i < _transforms.Length; i++)
                {
                    _transforms[i] = new TransformStampedMsg(msg["transforms"][i]);
                }
            }

            public tfMsg(TransformStampedMsg[] transforms)
            {
                _transforms = transforms;
            }

            public static string GetMessageType()
            {
                return "tf/tf";
            }

            public TransformStampedMsg[] Gettransforms()
            {
                return _transforms;
            }

            public override string ToString()
            {
                string array = "[";
                for (int i = 0; i < _transforms.Length; i++)
                {
                    array = array + _transforms[i].ToString();
                    if (i < _transforms.Length - 1)
                        array += ",";
                }
                array += "]";
                return "tf [transforms=" + array + "]";
            }

            public override string ToYAMLString()
            {
                string array = "[";
                for (int i = 0; i < _transforms.Length; i++)
                {
                    array = array + _transforms[i].ToYAMLString();
                    if (i < _transforms.Length - 1)
                        array += ",";
                }
                array += "]";
                return "{\"transforms\" : " + array + "}";
            }
        }
    }
}