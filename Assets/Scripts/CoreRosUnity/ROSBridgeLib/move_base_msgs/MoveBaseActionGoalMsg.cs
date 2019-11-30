using SimpleJSON;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.actionlib_msgs;
namespace ROSBridgeLib
{
    namespace move_base_msgs
    {
        public class MoveBaseActionGoalMsg : ROSBridgeMsg
        {
            private MoveBaseGoalMsg _goal;
            private GoalIDMsg _goal_id;
            private HeaderMsg _header;

            public MoveBaseActionGoalMsg(JSONNode msg)
            {
                _goal = new MoveBaseGoalMsg(msg["goal"]);
                _goal_id = new GoalIDMsg(msg["goal_id"]);
                _header = new HeaderMsg(msg["header"]);
            }

            public MoveBaseActionGoalMsg(MoveBaseGoalMsg goal, GoalIDMsg goal_id, HeaderMsg header)
            {
                _goal = goal;
                _goal_id = goal_id;
                _header = header;
            }

            public static string GetMessageType()
            {
                return "move_base_msgs/MoveBaseActionGoal";
            }

            public MoveBaseGoalMsg GetGoal()
            {
                return _goal;
            }

            public GoalIDMsg GetGoalId()
            {
                return _goal_id;
            }

            public HeaderMsg GetHeader()
            {
                return _header;
            }

            public override string ToString()
            {
                return "MoveBaseActionGoal [goal=" + _goal.ToString() + ",  goal_id=" + _goal_id.ToString() + ",  header=" + _header.ToString() + "]";
            }

            public override string ToYAMLString()
            {
                return "{\"goal\" : " + _goal.ToYAMLString() + ", \"goal_id\" : " + _goal_id.ToYAMLString() + ", \"header\" : " + _header.ToYAMLString() + "}";
            }
        }
    }
}