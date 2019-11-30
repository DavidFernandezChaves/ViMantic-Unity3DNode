using SimpleJSON;
using ROSBridgeLib.std_msgs;
namespace ROSBridgeLib
{
    namespace actionlib_msgs
    {
        public class GoalIDMsg : ROSBridgeMsg
        {
            private string _id;
            private TimeMsg _stamp;

            public GoalIDMsg(JSONNode msg)
            {
                _id = msg["id"];
                _stamp = new TimeMsg(msg["stamp"]);
            }

            public GoalIDMsg(string id, TimeMsg stamp)
            {
                _id = id;
                _stamp = stamp;
            }

            public static string GetMessageType()
            {
                return "actionlib_msgs/GoalID";
            }

            public string GetID()
            {
                return _id;
            }

            public TimeMsg GetStamp()
            {
                return _stamp;
            }

            public override string ToString()
            {
                return "GoalID [id=" + _id + ",  stamp=" + _stamp + "]";
            }

            public override string ToYAMLString()
            {
                return "{\"id\" : \"" + _id + "\", \"stamp\" : " + _stamp.ToYAMLString() + "}";
            }
        }
    }
}