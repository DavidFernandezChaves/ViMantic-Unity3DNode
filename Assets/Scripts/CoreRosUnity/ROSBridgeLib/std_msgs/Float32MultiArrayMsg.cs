using System.Collections;
using System.Text;
using SimpleJSON;

namespace ROSBridgeLib {
	namespace std_msgs {
		public class Float32MultiArrayMsg : ROSBridgeMsg {
			private MultiArrayLayoutMsg _layout;
			private float[] _data;

			public Float32MultiArrayMsg(JSONNode msg) {
				_layout = new MultiArrayLayoutMsg(msg["layout"]);
				_data = new float[msg["data"].Count];
				for (int i = 0; i < _data.Length; i++) {
					_data[i] = float.Parse(msg["data"][i]);
				}
			}

			public Float32MultiArrayMsg(MultiArrayLayoutMsg layout, float[] data) {
				_layout = layout;
				_data = data;
			}

			public static string getMessageType() {
				return "std_msgs/Float32MultiArray";
			}

			public float[] GetData() {
				return _data;
			}

			public MultiArrayLayoutMsg GetLayout() {
				return _layout;
			}

			public override string ToString() {
				string array = "[";
				for (int i = 0; i < _data.Length; i++) {
					array = array + _data [i].ToString ("N", System.Globalization.CultureInfo.InvariantCulture);
					if (i<_data.Length-1)
						array += ",";
				}
				array += "]";
				return "Float32MultiArray [layout=" + _layout.ToString() + ", data=" + array + "]";
			}

			public override string ToYAMLString() {
				string array = "[";
				for (int i = 0; i < _data.Length; i++) {
					array = array + _data[i].ToString("N",System.Globalization.CultureInfo.InvariantCulture);
					if (i<_data.Length-1)
						array += ",";
				}
				array += "]";
				return "{\"layout\" : " + _layout.ToYAMLString() + ", \"data\" : " + array + "}";
			}
		}
	}
}