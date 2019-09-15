using ROSBridgeLib;
using ROSBridgeLib.move_base_msgs;

public class Goal_pub : ROSBridgePublisher
{

    public new static string GetMessageTopic()
    {
        return "/move_base/goal";
    }

    public new static string GetMessageType()
    {
        return "move_base_msgs/MoveBaseActionGoal";
    }

    public static string ToYAMLString(MoveBaseActionGoalMsg msg)
    {
        return msg.ToYAMLString();
    }

}

