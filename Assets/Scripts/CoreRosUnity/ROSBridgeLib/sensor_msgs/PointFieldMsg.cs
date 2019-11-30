using SimpleJSON;
using ROSBridgeLib.std_msgs;
using System;

namespace ROSBridgeLib
{
    namespace sensor_msgs
    {
        public class PointFieldMsg : ROSBridgeMsg
        {
            private Int8Msg _int8 = new Int8Msg(1);
            private Int8Msg _uint8 = new Int8Msg(2);
            private Int8Msg _int16 = new Int8Msg(3);
            private Int8Msg _uint16 = new Int8Msg(4);
            private Int8Msg _int32 = new Int8Msg(5);
            private Int8Msg _uint32 = new Int8Msg(6);
            private Int8Msg _float32 = new Int8Msg(7);
            private Int8Msg _fluat64 = new Int8Msg(8);
            private string _name;
            private UInt32Msg _offset;
            private UInt8Msg _datatype;
            private UInt32Msg _count;


            public PointFieldMsg(JSONNode msg)
            {
                _int8 = new Int8Msg(msg["INT8"]);
                _uint8 = new Int8Msg(msg["UINT8"]);
                _int16 = new Int8Msg(msg["INT16"]);
                _uint16 = new Int8Msg(msg["UINT16"]);
                _int32 = new Int8Msg(msg["INT32"]);
                _uint32 = new Int8Msg(msg["UINT32"]);
                _float32 = new Int8Msg(msg["FLOAT32"]);
                _fluat64 = new Int8Msg(msg["FLOAT64"]);

                _name = msg["name"].ToString();
                _offset = new UInt32Msg(msg["offset"]);
                _datatype = new UInt8Msg(msg["datatype"]);
                _count = new UInt32Msg(msg["count"]);
            }

            public PointFieldMsg(Int8Msg int8, Int8Msg uint8, Int8Msg int16, Int8Msg uint16, Int8Msg int32, Int8Msg uint32, Int8Msg float32, Int8Msg float64, string name, UInt32Msg offset, UInt8Msg datatype, UInt32Msg count)
            {
                _int8 = int8;
                _uint8 = uint8;
                _int16 = int16;
                _uint16 = uint16;
                _int32 = int32;
                _uint32 = uint32;
                _float32 = float32;
                _fluat64 = float64;

                _name = name;
                _offset = offset;
                _datatype = datatype;
                _count = count;
            }

            public static string GetMessageType()
            {
                return "sensor_msgs/PointField";
            }

            public override string ToString()
            {
                return "PointField [INT8=" + _int8.ToString()
                                + ",  UINT8=" + _uint8.ToString()
                                + ",  INT16=" + _int16.ToString()
                                + ",  UINT16=" + _uint16.ToString()
                                + ",  INT32=" + _int32.ToString()
                                + ",  UINT32=" + _uint32.ToString()
                                + ",  FLOAT32=" + _float32.ToString()
                                + ",  FLOAT64=" + _fluat64.ToString()
                                + ",  name=" + _name.ToString()
                                + ",  offset=" + _offset.ToString()
                                + ",  datatype=" + _datatype.ToString()
                                + ",  count=" + _count.ToString() + "]";

            }

            public override string ToYAMLString()
            {
                return "{\"INT8\" : " + _int8.ToString()
                            + ",  \"UINT8\" : " + _uint8.ToYAMLString()
                            + ",  \"INT16\" : " + _int16.ToYAMLString()
                            + ",  \"UINT16\" : " + _uint16.ToYAMLString()
                            + ",  \"INT32\" : " + _int32.ToYAMLString()
                            + ",  \"UINT32\" : " + _uint32.ToYAMLString()
                            + ",  \"FLOAT32\" : " + _float32.ToYAMLString()
                            + ",  \"FLOAT64\" : " + _fluat64.ToYAMLString()
                            + ",  \"name\" : " + _name.ToString()
                            + ",  \"offset\" : " + _offset.ToYAMLString()
                            + ",  \"datatype\" : " + _datatype.ToYAMLString()
                            + ",  \"count\" : " + _count.ToYAMLString() + "}";
            }
        }
    }
}
