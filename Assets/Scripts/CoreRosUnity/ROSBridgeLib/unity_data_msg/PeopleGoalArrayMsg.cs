using SimpleJSON;
using ROSBridgeLib.std_msgs;

namespace ROSBridgeLib
{
    namespace unity_data
    {
        public class PeopleGoalArrayMsg : ROSBridgeMsg
        {
            private HeaderMsg _header;
            private PersonGoalMsg[] _users;


            public PeopleGoalArrayMsg(JSONNode msg)
            {
                _header = new HeaderMsg(msg["header"]);
                _users = new PersonGoalMsg[msg["users"].Count];
                for (int i = 0; i < _users.Length; i++)
                {
                    _users[i] = new PersonGoalMsg(msg["users"][i]);
                }
            }

            public PeopleGoalArrayMsg(HeaderMsg header, PersonGoalMsg[] users)
            {
                _header = header;
                _users = users;
            }

            public static string GetMessageType()
            {
                return "PeopleGoalArray";
            }

            public HeaderMsg GetHeader()
            {
                return _header;
            }

            public PersonGoalMsg[] GetUsers()
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
