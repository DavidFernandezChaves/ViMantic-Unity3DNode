using SimpleJSON;
using UnityEngine;

namespace ROSBridgeLib
{
    namespace sensor_msgs
    {
        public class ChannelFloat32 : ROSBridgeMsg
        {
            private string _name;
            private float[] _values;

            public ChannelFloat32(JSONNode msg)
            {
                _name = msg["name"];
                _values = new float[msg["values"].Count];
                for (int i = 0; i < _values.Length; i++)
                {
                    _values[i] = float.Parse(msg["values"][i], System.Globalization.CultureInfo.InvariantCulture);
                }
            }

            public ChannelFloat32(string name, float[] values)
            {
                _name = name;
                _values = values;
            }

            public static string GetMessageType()
            {
                return "sensors_msgs/ChannelFloat32";
            }

            public string GetName()
            {
                return _name;
            }

            public float[] GetValues()
            {
                return _values;
            }

            public override string ToString()
            {
                string resultado = "ChannelFloat32 [name=" + _name + ",  values=[";
                    for (int i = 0; i < _values.Length; i++)
                    {
                        resultado += _values[i].ToString("N", System.Globalization.CultureInfo.InvariantCulture);
                        if (i < (_values.Length - 1))
                            resultado += ",";
                    }

                resultado += "]]";
                return resultado;
            }

            public override string ToYAMLString()
            {
                string resultado = "{\"name\" : " + _name + ",  \"values\" : [";
                for (int i = 0; i < _values.Length; i++)
                {
                    resultado += _values[i].ToString("N", System.Globalization.CultureInfo.InvariantCulture);
                    if (i < (_values.Length - 1))
                        resultado += ",";
                }
                resultado += "]}";

                return resultado;                
            }
        }
    }
}