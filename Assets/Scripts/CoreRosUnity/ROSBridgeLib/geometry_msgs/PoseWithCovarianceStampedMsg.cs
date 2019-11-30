using SimpleJSON;
using ROSBridgeLib.std_msgs;

namespace ROSBridgeLib
{
	namespace geometry_msgs
	{
		public class PoseWithCovarianceStampedMsg : ROSBridgeMsg
		{
			private HeaderMsg _header;
			private PoseWithCovarianceMsg _pose;

			public PoseWithCovarianceStampedMsg(JSONNode msg)
			{
				_header = new HeaderMsg (msg ["header"]);
				_pose = new PoseWithCovarianceMsg(msg["pose"]);
			}

			public PoseWithCovarianceStampedMsg(HeaderMsg header, PoseWithCovarianceMsg pose)
			{
				_header = header;
				_pose = pose;
			}

			public static string GetMessageType()
			{
				return "geometry_msgs/PoseWithCovarianceStamped";
			}

			public HeaderMsg GetHeader()
			{
				return _header;
			}

			public PoseWithCovarianceMsg GetPose()
			{
				return _pose;
			}

			public override string ToString()
			{
				return "PoseWithCovarianceStamped [header=" + _header.ToString() + ",  pose=" + _pose.ToString() + "]";
			}

			public override string ToYAMLString()
			{
				return "{\"header\" : " + _header.ToYAMLString() + ", \"pose\" : " + _pose.ToYAMLString() + "}";
			}
		}
	}
}