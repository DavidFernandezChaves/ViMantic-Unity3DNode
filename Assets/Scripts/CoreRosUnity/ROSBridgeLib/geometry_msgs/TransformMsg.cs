using SimpleJSON;
using UnityEngine;

namespace ROSBridgeLib
{
    namespace geometry_msgs
    {
        public class TransformMsg : ROSBridgeMsg
        {
            private Vector3Msg _translation;
            private QuaternionMsg _rotation;

            public TransformMsg(JSONNode msg)
            {
                _translation = new Vector3Msg(msg["translation"]);
                _rotation = new QuaternionMsg(msg["rotation"]);
            }

            public TransformMsg(Vector3Msg translation, QuaternionMsg rotation)
            {
                _translation = translation;
                _rotation = rotation;
            }

            public static string GetMessageType()
            {
                return "geometry_msgs/Transform";
            }

            public Vector3Msg GetTranslation()
            {
                return _translation;
            }

            public QuaternionMsg GetRotation()
            {
                return _rotation;
            }

            public Matrix4x4 GetMatrix4x4() {
                return Matrix4x4.TRS(_translation.GetVector3Unity(), _rotation.GetQuaternionUnity(1), Vector3.one);
            }

            public override string ToString()
            {
                return "Transform [translation=" + _translation.ToString() + ",  rotation=" + _rotation.ToString() + "]";
            }

            public override string ToYAMLString()
            {
                return "{\"translation\" : " + _translation.ToYAMLString() + ", \"rotation\" : " + _rotation.ToYAMLString() + "}";
            }
        }
    }
}