using SimpleJSON;
using ROSBridgeLib.geometry_msgs;
namespace ROSBridgeLib
{
    namespace move_base_msgs
    {
        public class MoveBaseGoalMsg : ROSBridgeMsg
        {
            private PoseStampedMsg _target_pose;

            public MoveBaseGoalMsg(JSONNode msg)
            {
                _target_pose = new PoseStampedMsg(msg["target_pose"]);
            }

            public MoveBaseGoalMsg(PoseStampedMsg target_pose)
            {
                _target_pose = target_pose;
            }

            public static string GetMessageType()
            {
                return "move_base_msgs/MoveBaseGoal";
            }

            public PoseStampedMsg GetTarget_pose()
            {
                return _target_pose;
            }

            public override string ToString()
            {
                return "MoveBaseGoal [target_pose=" + _target_pose.ToString() + "]";
            }

            public override string ToYAMLString()
            {
                return "{\"target_pose\" : " + _target_pose.ToYAMLString() + "}";
            }
        }
    }
}