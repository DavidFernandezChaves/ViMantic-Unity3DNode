using SimpleJSON;
using UnityEngine;

namespace ROSBridgeLib
{
	namespace geometry_msgs
	{
		public class PoseMsg : ROSBridgeMsg
		{
			private PointMsg _position;
			private QuaternionMsg _orientation;

			public PoseMsg(JSONNode msg)
			{
				_position = new PointMsg(msg["position"]);
				_orientation = new QuaternionMsg(msg["orientation"]);
			}

			public PoseMsg(PointMsg translation, QuaternionMsg rotation)
			{
				_position = translation;
				_orientation = rotation;
			}

            public PoseMsg(Transform tf) {
                Vector3 position = tf.position;
                Vector3 orientation = tf.rotation.eulerAngles;
                _position = new PointMsg(position.x, position.z, position.y);
                _orientation = new QuaternionMsg(orientation.x, orientation.y, orientation.z);
            }

			public static string GetMessageType()
			{
				return "geometry_msgs/Pose";
			}

			public PointMsg GetTranslation()
			{
				return _position;
			}


			public QuaternionMsg GetRotation()
			{
				return _orientation;
			}

            public Vector3 GetTranslationUnity() {
                Vector3 p = _position.GetPoint();
                return new Vector3(p.x, p.z, p.y);
            }

            public Quaternion GetRotationUnity(int solution)
            {
                return Quaternion.Euler(_orientation.GetRotationEulerUnity(solution));
            }

            public Vector3 GetRotationEulerUnity(int solution) {
                return _orientation.GetRotationEulerUnity(solution);
            }

			public override string ToString()
			{
				return "Pose [position=" + _position.ToString() + ",  orientation=" + _orientation.ToString() + "]";
			}

			public override string ToYAMLString()
			{
				return "{\"position\" : " + _position.ToYAMLString() + ", \"orientation\" : " + _orientation.ToYAMLString() + "}";
			}
		}
	}
}