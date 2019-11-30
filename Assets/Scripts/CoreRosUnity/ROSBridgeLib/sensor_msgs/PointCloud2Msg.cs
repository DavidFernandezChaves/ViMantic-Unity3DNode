using SimpleJSON;
using ROSBridgeLib.std_msgs;
using System;

namespace ROSBridgeLib
{
    namespace sensor_msgs
    {
        public class PointCloud2Msg : ROSBridgeMsg
        {
            private HeaderMsg _header;
            private UInt32Msg _height;
            private UInt32Msg _width;
            private PointFieldMsg[] _fields;
            private bool _is_bigendian;
            private UInt32Msg _point_step;
            private UInt32Msg _row_step;
            private UInt8Msg[] _data;
            private bool _is_dense;


            public PointCloud2Msg(JSONNode msg)
            {
                _header = new HeaderMsg(msg["header"]);
                _height = new UInt32Msg(msg["height"]);
                _width = new UInt32Msg(msg["width"]);
                string[] str_field = msg["fields"].ToString().Split(',');
                _fields = new PointFieldMsg[str_field.Length];
                for (int i = 0; i < _fields.Length; i++) {
                    _fields[i] = new PointFieldMsg(str_field[i]);
                }
                _is_bigendian = bool.Parse(msg["is_begendian"]);
                _point_step = new UInt32Msg(msg["point_step"]);
                _row_step = new UInt32Msg(msg["row_step"]);
                string[] str_data = msg["data"].ToString().Split(',');
                _data = new UInt8Msg[str_data.Length];
                for (int i = 0; i < _data.Length; i++)
                {
                    _data[i] = new UInt8Msg(str_data[i]);
                }
                _is_dense = bool.Parse(msg["is_dense"]);
            }

            public PointCloud2Msg(HeaderMsg header, UInt32Msg height, UInt32Msg width, PointFieldMsg[] fields, bool is_bigendian, UInt32Msg point_step, UInt32Msg row_step, UInt8Msg[] data, bool is_dense)
            {
                _header = header;
                _height = height;
                _width = width;
                _fields = fields;
                _is_bigendian = is_bigendian;
                _point_step = point_step;
                _row_step = row_step;
                _data = data;
                _is_dense = is_dense;
            }

            public static string GetMessageType()
            {
                return "sensor_msgs/PointCloud2";
            }

            public override string ToString()
            {
                string resultado = "PointCloud2 [header=" + _header.ToString()
                                + ",  height=" + _height.ToString()
                                + ",  width=" + _width.ToString()
                                + ",  fields=[";
                                for (int i = 0; i < _fields.Length; i++)
                                {
                                    resultado += _fields[i].ToString();
                                    if (i < (_fields.Length - 1))
                                        resultado += ",";
                                }
                resultado += "], is_bigendian=" + _is_bigendian.ToString()
                                + ",  point_step=" + _point_step.ToString()
                                + ",  row_step=" + _row_step.ToString()
                                + ",  data=[";
                                for (int i = 0; i < _data.Length; i++)
                                {
                                    resultado += _data[i].ToString();
                                    if (i < (_data.Length - 1))
                                        resultado += ",";
                                }
                resultado += "], is_dense=" + _is_dense.ToString()+"]";

                return resultado;
            }

            public override string ToYAMLString()
            {
                string resultado = "{\"header\" : " + _header.ToYAMLString()
                + ",  \"height\" : " + _height.ToYAMLString()
                + ",  \"width\" : " + _width.ToYAMLString()
                + ",  \"fields\" : [";
                for (int i = 0; i < _fields.Length; i++)
                {
                    resultado += _fields[i].ToYAMLString();
                    if (i < (_fields.Length - 1))
                        resultado += ",";
                }
                resultado += "], \"is_bigendian\" : " + _is_bigendian.ToString()
                                + ",  \"point_step\" : " + _point_step.ToYAMLString()
                                + ",  \"row_step\" : " + _row_step.ToYAMLString()
                                + ",  \"data\" : [";
                for (int i = 0; i < _data.Length; i++)
                {
                    resultado += _data[i].ToYAMLString();
                    if (i < (_data.Length - 1))
                        resultado += ",";
                }
                resultado += "], \"is_dense\" : " + _is_dense.ToString()+"}";

                return resultado;
            }
        }
    }
}
