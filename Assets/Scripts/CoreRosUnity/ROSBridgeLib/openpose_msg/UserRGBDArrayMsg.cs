using SimpleJSON;
using ROSBridgeLib.std_msgs;

namespace ROSBridgeLib
{
    namespace openpose_msgs
    {
        public class UserRGBDArrayMsg : ROSBridgeMsg
        {
            private HeaderMsg _header;
            private UserRGBDMsg[] _users;


            public UserRGBDArrayMsg(JSONNode msg)
            {
                _header = new HeaderMsg(msg["header"]);
                _users = new UserRGBDMsg[msg["users"].Count];
                for (int i = 0; i < _users.Length; i++)
                {
                    _users[i] = new UserRGBDMsg(msg["users"][i]);
                }
            }

            public UserRGBDArrayMsg(HeaderMsg header, UserRGBDMsg[] users)
            {
                _header = header;
                _users = users;
            }

            public static string GetMessageType()
            {
                return "UserRGBDArray";
            }

            public HeaderMsg GetHeader()
            {
                return _header;
            }            

            public UserRGBDMsg[] GetUsers()
            {
                return _users;
            }

            public override string ToString()
            {
                string array = "[";
                for (int i = 0; i < _users.Length; i++)
                {
                    array = array + _users[i].ToString();
                    if (i < _users.Length - 1)
                        array += ",";
                }
                array += "]";
                return "UserArray [header=" + _header.ToString() + ",  users=" + array + "]";
            }

            public override string ToYAMLString()
            {
                string array = "[";
                for (int i = 0; i < _users.Length; i++)
                {
                    array = array + _users[i].ToYAMLString();
                    if (i < _users.Length - 1)
                        array += ",";
                }
                array += "]";
                return "{\"header\" : " + _header.ToYAMLString() + ", \"users\" : " + array + "}";
            }
        }
    }
}
