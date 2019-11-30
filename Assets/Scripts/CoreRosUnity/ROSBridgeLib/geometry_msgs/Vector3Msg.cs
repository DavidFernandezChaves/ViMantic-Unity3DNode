using SimpleJSON;
using UnityEngine;

namespace ROSBridgeLib {
	namespace geometry_msgs {
		public class Vector3Msg : ROSBridgeMsg {
			private double _x;
			private double _y;
			private double _z;
			
			public Vector3Msg(JSONNode msg) {
				_x = double.Parse(msg["x"],System.Globalization.CultureInfo.InvariantCulture);
				_y = double.Parse(msg["y"],System.Globalization.CultureInfo.InvariantCulture);
				_z = double.Parse(msg["z"],System.Globalization.CultureInfo.InvariantCulture);
			}
			
			public Vector3Msg(double x, double y, double z) {
				_x = x;
				_y = y;
				_z = z;
			}
			
			public static string GetMessageType() {
				return "geometry_msgs/Vector3";
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

            public Vector3 GetVector3() {
                return new Vector3((float)_x, (float)_y, (float)_z);
            }

            public Vector3 GetVector3Unity() {
                return new Vector3((float)_x, (float)_z, (float)_y);
            }
			
			public override string ToString() {
				return "Vector3 [x=" + _x.ToString("N",System.Globalization.CultureInfo.InvariantCulture)  + ",  y="+ _y.ToString("N",System.Globalization.CultureInfo.InvariantCulture)  + ",  z=" + _z.ToString("N",System.Globalization.CultureInfo.InvariantCulture)  + "]";
			}
			
			public override string ToYAMLString() {
				return "{\"x\" : " + _x.ToString("N",System.Globalization.CultureInfo.InvariantCulture)  + ", \"y\" : " + _y.ToString("N",System.Globalization.CultureInfo.InvariantCulture)  + ", \"z\" : " + _z.ToString("N",System.Globalization.CultureInfo.InvariantCulture)  + "}";
			}
		}
	}
}