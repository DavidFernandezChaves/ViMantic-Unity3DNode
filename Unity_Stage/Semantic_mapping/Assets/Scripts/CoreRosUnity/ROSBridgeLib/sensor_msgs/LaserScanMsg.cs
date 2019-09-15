using SimpleJSON;
using ROSBridgeLib.std_msgs;
using System;

namespace ROSBridgeLib {
	namespace sensor_msgs {
		public class LaserScanMsg : ROSBridgeMsg {
			private HeaderMsg _header;
			private float _angle_min;
			private float _angle_max;
			private float _angle_increment;
			private float _time_increment;
			private float _scan_time;
			private float _range_min;
			private float _range_max;
			private float[] _ranges;
			private float[] _intensities;


			public LaserScanMsg(JSONNode msg) {
				_header = new HeaderMsg (msg ["header"]);
				_angle_min = float.Parse(msg["angle_min"]);
				_angle_max =float.Parse(msg ["angle_max"]);
				_angle_increment = float.Parse(msg ["angle_increment"]);
				_time_increment = float.Parse(msg ["time_increment"]);
				_scan_time = float.Parse(msg ["scan_time"]);
				_range_min = float.Parse(msg ["range_min"]);
				_range_max = float.Parse(msg ["range_max"]);
				string datos = msg ["ranges"].ToString ();
				_ranges = Array.ConvertAll(datos.Split(','), float.Parse);
				datos = msg ["intensities"].ToString ();
				_intensities = Array.ConvertAll(datos.Split(','), float.Parse);
			}

			public LaserScanMsg(HeaderMsg header,float angle_min, float angle_max, float angle_increment, float time_increment, float scan_time, float range_min, float range_max, float[] ranges, float[] intensities) {
				_header = header;
				_angle_min = angle_min;
				_angle_max = angle_max;
				_angle_increment = angle_increment;
				_time_increment = time_increment;
				_scan_time = scan_time;
				_range_min = range_min;
				_range_max = range_max;
				_ranges = ranges;
				_intensities = intensities;
			}

			public static string GetMessageType() {
				return "sensor_msgs/LaserScan";
			}

			public override string ToString() {
				string resultado = "LaserScan [header=" + _header.ToString ()
								+ ",  angle_min=" + _angle_min.ToString ("N",System.Globalization.CultureInfo.InvariantCulture)
								+ ",  angle_max=" + _angle_max.ToString ("N",System.Globalization.CultureInfo.InvariantCulture)
								+ ",  angle_increment=" + _angle_increment.ToString ("N",System.Globalization.CultureInfo.InvariantCulture)
								+ ",  time_increment=" + _time_increment.ToString ("N",System.Globalization.CultureInfo.InvariantCulture)
								+ ",  scan_time=" + _scan_time.ToString ("N",System.Globalization.CultureInfo.InvariantCulture)
								+ ",  range_min=" + _range_min.ToString ("N",System.Globalization.CultureInfo.InvariantCulture)
								+ ",  range_max=" + _range_max.ToString ("N",System.Globalization.CultureInfo.InvariantCulture)
				                   + ",  ranges=[";
									for(int i=0;i<_ranges.Length;i++){
										resultado += _ranges[i].ToString("N",System.Globalization.CultureInfo.InvariantCulture);
										if (i < (_ranges.Length - 1))
											resultado += ",";
									}
									resultado += "],intensities=[";
									for(int i=0;i<_intensities.Length;i++){
										resultado += _ranges[i].ToString("N",System.Globalization.CultureInfo.InvariantCulture);
										if (i < (_intensities.Length - 1))
											resultado += ",";
									}
									resultado+="]]";

				return resultado;
			}

			public override string ToYAMLString() {
				string resultado = "{\"header\" : " + _header.ToYAMLString ()
								+ ", \"angle_min\" : " + _angle_min.ToString("N",System.Globalization.CultureInfo.InvariantCulture)
								+ ", \"angle_max\" : " + _angle_max.ToString("N",System.Globalization.CultureInfo.InvariantCulture)
								+ ", \"angle_increment\" : " + _angle_increment.ToString("N",System.Globalization.CultureInfo.InvariantCulture)
								+ ", \"time_increment\" : " + _time_increment.ToString("N",System.Globalization.CultureInfo.InvariantCulture)
								+ ", \"scan_time\" : " + _scan_time.ToString("N",System.Globalization.CultureInfo.InvariantCulture)
								+ ", \"range_min\" : " + _range_min.ToString("N",System.Globalization.CultureInfo.InvariantCulture)
								+ ", \"range_max\" : " + _range_max.ToString("N",System.Globalization.CultureInfo.InvariantCulture)
				                   + ", \"ranges\" : [";
				
									for(int i=0;i<_ranges.Length;i++){
										resultado += _ranges[i].ToString("N",System.Globalization.CultureInfo.InvariantCulture);
										if (i < (_ranges.Length - 1))
											resultado += ",";
									}
									resultado += "], \"intensities\" : [";
									for(int i=0;i<_intensities.Length;i++){
										resultado += _intensities[i].ToString("N",System.Globalization.CultureInfo.InvariantCulture);
										if (i < (_intensities.Length - 1))
											resultado += ",";
									}
									resultado+="]}";
				return resultado;
			}
		}
	}
}
