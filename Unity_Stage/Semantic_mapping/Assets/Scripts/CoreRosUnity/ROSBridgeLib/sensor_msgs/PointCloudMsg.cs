using SimpleJSON;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.geometry_msgs;
using System;

namespace ROSBridgeLib
{
    namespace sensor_msgs
    {
        public class PointCloudMsg : ROSBridgeMsg
        {
            private HeaderMsg _header;
            private Point32Msg[] _points;
            private ChannelFloat32[] _channels;


            public PointCloudMsg(JSONNode msg)
            {
                _header = new HeaderMsg(msg["header"]);

                _points = new Point32Msg[msg["points"].Count];
                for (int i = 0; i < _points.Length; i++)
                {
                    _points[i] = new Point32Msg(msg["points"][i]);
                }

                _channels = new ChannelFloat32[msg["clannels"].Count];
                for (int i = 0; i < _channels.Length; i++)
                {
                    _channels[i] = new ChannelFloat32(msg["clannels"][i]);
                }
            }

            public PointCloudMsg(HeaderMsg header, Point32Msg[] points, ChannelFloat32[] channels)
            {
                _header = header;
                _points = points;
                _channels = channels;
            }

            public static string GetMessageType()
            {
                return "sensor_msgs/PointCloud";
            }

            public HeaderMsg GetHeader()
            {
                return _header;
            }

            public Point32Msg[] GetPoints()
            {
                return _points;
            }

            public ChannelFloat32[] GetChannels()
            {
                return _channels;
            }

            public override string ToString()
            {
                string resultado = "PointCloud [header=" + _header.ToString()
                                + ",  points=[";
                for (int i = 0; i < _points.Length; i++)
                {
                    resultado += _points[i].ToString();
                    if (i < (_points.Length - 1))
                        resultado += ",";
                }
                resultado += "], channels=[";
                for (int i = 0; i < _channels.Length; i++)
                {
                    resultado += _channels[i].ToString();
                    if (i < (_channels.Length - 1))
                        resultado += ",";
                }
                resultado += "]]";

                return resultado;
            }

            public override string ToYAMLString()
            {
                string resultado = "{\"header\" : " + _header.ToYAMLString()
                + ",  \"points\" : [";
                for (int i = 0; i < _points.Length; i++)
                {
                    resultado += _points[i].ToYAMLString();
                    if (i < (_points.Length - 1))
                        resultado += ",";
                }
                resultado += "], \"channels\" : [";
                for (int i = 0; i < _channels.Length; i++)
                {
                    resultado += _channels[i].ToYAMLString();
                    if (i < (_channels.Length - 1))
                        resultado += ",";
                }
                resultado += "]}";

                return resultado;
            }
        }
    }
}
