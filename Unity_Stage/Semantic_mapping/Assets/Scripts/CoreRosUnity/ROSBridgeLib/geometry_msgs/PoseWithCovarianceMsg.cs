using SimpleJSON;

namespace ROSBridgeLib
{
	namespace geometry_msgs
	{
		public class PoseWithCovarianceMsg : ROSBridgeMsg
		{
			private PoseMsg _pose;
			private double[] _covariance;

			public PoseWithCovarianceMsg(JSONNode msg)
			{
				_pose = new PoseMsg(msg["pose"]);
				_covariance = new double[msg["covariance"].Count];
				for (int i = 0; i < _covariance.Length; i++) {
					_covariance[i] = double.Parse(msg["covariance"][i], System.Globalization.CultureInfo.InvariantCulture);
                }
			}

			public PoseWithCovarianceMsg(PoseMsg pose, double[] covariance)
			{
				_pose = pose;
				_covariance = covariance;
			}

			public static string GetMessageType()
			{
				return "geometry_msgs/PoseWithCovariance";
			}

			public PoseMsg GetPose()
			{
				return _pose;
			}

			public double[] GetCovariance()
			{
				return _covariance;
			}

			public override string ToString()
			{
				string array = "[";
				for (int i = 0; i < _covariance.Length; i++) {
					array = array + _covariance [i].ToString ("N", System.Globalization.CultureInfo.InvariantCulture);
					if (i<_covariance.Length-1)
						array += ",";
				}
				array += "]";
				return "PoseWithCovariance [pose=" + _pose.ToString() + ", covariance=" + array + "]";
			}

			public override string ToYAMLString()
			{
				string array = "[";
				for (int i = 0; i < _covariance.Length; i++) {
					array = array + _covariance[i].ToString("N",System.Globalization.CultureInfo.InvariantCulture);
					if (i<_covariance.Length-1)
						array += ",";
				}
				array += "]";
				return "{\"pose\" : " + _pose.ToYAMLString()  + ", \"covariance\" : " + array + "}";
			}
		}
	}
}