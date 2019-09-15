using SimpleJSON;
using UnityEngine;

namespace ROSBridgeLib{
	namespace geometry_msgs	{
		public class PointMsg : ROSBridgeMsg		{
			private double _x;
			private double _y;
			private double _z;

			public PointMsg(JSONNode msg) {
				_x = double.Parse(msg["x"],System.Globalization.CultureInfo.InvariantCulture);
				_y = double.Parse(msg["y"],System.Globalization.CultureInfo.InvariantCulture);
				_z = double.Parse(msg["z"],System.Globalization.CultureInfo.InvariantCulture);
			}

			public PointMsg(double x, double y, double z) {
				_x = x;
				_y = y;
				_z = z;
			}

            public PointMsg(Vector3 point) {
                _x = point.x;
                _y = point.y;
                _z = point.z;
            }

			public static string GetMessageType() {
				return "geometry_msgs/Point";
			}

			public double GetX() {
				return _x;
			}

			public double GetY() {
				return _y;
			}

			public double GetZ() {
				return _z;
			}

            public Vector3 GetPoint() {
                return new Vector3((float)_x, (float)_y, (float)_z);
            }

            public Vector3 GetPointPositionUnity() {
                return new Vector3((float)_x, (float)_z, (float)_y);
            }

			public override string ToString() {
				return "Point [x=" + _x.ToString("N",System.Globalization.CultureInfo.InvariantCulture)  + ",  y="+ _y.ToString("N",System.Globalization.CultureInfo.InvariantCulture)  + ",  z=" + _z.ToString("N",System.Globalization.CultureInfo.InvariantCulture)  + "]";
			}

			public override string ToYAMLString() {
				return "{\"x\" : " + _x.ToString("N",System.Globalization.CultureInfo.InvariantCulture)  + ", \"y\" : " + _y.ToString("N",System.Globalization.CultureInfo.InvariantCulture)  + ", \"z\" : " + _z.ToString("N",System.Globalization.CultureInfo.InvariantCulture)  + "}";
			}
		}
	}
}