using SimpleJSON;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.geometry_msgs;

namespace ROSBridgeLib
{
    namespace nav_msgs
    {
        public class MapMetaDataMsg : ROSBridgeMsg
        {
            
            private TimeMsg _map_load_time;
            private float _resolution;
            private uint _width;
            private uint _height;
            private PoseMsg _origin;

            public MapMetaDataMsg(JSONNode msg)
            {
                _map_load_time = new TimeMsg(msg["map_load_time"]);
                _resolution = float.Parse(msg["resolution"], System.Globalization.CultureInfo.InvariantCulture);
                _width = uint.Parse(msg["width"], System.Globalization.CultureInfo.InvariantCulture);
                _height = uint.Parse(msg["height"], System.Globalization.CultureInfo.InvariantCulture);
                _origin = new PoseMsg(msg["origin"]);
            }

            public MapMetaDataMsg(TimeMsg map_load_time, float resolution, uint width , uint height, PoseMsg origin)
            {
                _map_load_time = map_load_time;
                _resolution = resolution;
                _width = width;
                _height = height;
                _origin = origin;
            }

            public static string GetMessageType()
            {
                return "nav_msgs/MapMetaData";
            }

            public TimeMsg GetMap_load_time()
            {
                return _map_load_time;
            }

            public float GetResolution()
            {
                return _resolution;
            }

            public uint Getwidth()
            {
                return _width;
            }

            public uint Getheight()
            {
                return _height;
            }

            public PoseMsg GetOrigin()
            {
                return _origin;
            }

            public override string ToString()
            {
                return "MapMetaData [map_load_time=" + _map_load_time.ToString() + ",  resolution=" + _resolution.ToString("F", System.Globalization.CultureInfo.InvariantCulture) + ",  width=" + _width + ",  height=" + _height + ",  origin=" + _origin.ToString() + "]";
            }

            public override string ToYAMLString()
            {
                return "{\"map_load_time\" : " + _map_load_time.ToYAMLString() + ", \"resolution\" : " + _resolution.ToString("F", System.Globalization.CultureInfo.InvariantCulture) + ", \"width\" : " + _width + ", \"height\" : " + _height + ", \"origin\" : " + _origin.ToYAMLString() + "}";
            }

        }
    }
}
