using SimpleJSON;
using ROSBridgeLib.std_msgs;

namespace ROSBridgeLib
{
    namespace nav_msgs
    {
        public class OccupancyGridMsg : ROSBridgeMsg
        {
            private HeaderMsg _header;
            private MapMetaDataMsg _info;
            private sbyte[] _data;

            public OccupancyGridMsg(JSONNode msg)
            {
                _header = new HeaderMsg(msg["header"]);
                _info = new MapMetaDataMsg(msg["info"]);
                _data = new sbyte[msg["data"].Count];
                for (int i = 0; i < _data.Length; i++)
                {
                    _data[i] = sbyte.Parse(msg["data"][i]);
                }
            }

            public OccupancyGridMsg(HeaderMsg header, MapMetaDataMsg info, sbyte[] data)
            {
                _header = header;
                _info = info;
                _data = data;
            }

            public static string GetMessageType()
            {
                return "nav_msgs/OccupancyGrid";
            }

            public HeaderMsg GetHeader()
            {
                return _header;
            }

            public MapMetaDataMsg GetInfo()
            {
                return _info;
            }

            public sbyte[] GetData()
            {
                return _data;
            }

            public override string ToString()
            {
                string array = "[";
                for (int i = 0; i < _data.Length; i++)
                {
                    array = array + _data[i];
                    if (_data.Length - i <= 1)
                        array += ",";
                }
                array += "]";
                return "OccupancyGrid [header=" + _header.ToString() + ",  info=" + _info.ToString() + ",  data=" + _data + "]";
            }

            public override string ToYAMLString()
            {
                string array = "[";
                for (int i = 0; i < _data.Length; i++)
                {
                    array = array + _data[i];
                    if (_data.Length - i <= 1)
                        array += ",";
                }
                array += "]";
                return "{\"header\" : " + _header.ToYAMLString() + ", \"info\" : " + _info.ToYAMLString() + ", \"data\" : " + array + "}";
            }
        }
    }
}
