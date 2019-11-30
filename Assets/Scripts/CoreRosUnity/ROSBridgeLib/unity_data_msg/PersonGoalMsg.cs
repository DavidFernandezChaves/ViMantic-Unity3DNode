using SimpleJSON;
using ROSBridgeLib.geometry_msgs;
using System;

namespace ROSBridgeLib
{
    namespace unity_data
    {
        public class PersonGoalMsg : ROSBridgeMsg
        {
            private String _name;
            private PoseMsg _interaction_pose;        


            public PersonGoalMsg(JSONNode msg)
            {
                _name = msg["name"];
                _interaction_pose = new PoseMsg(msg["interaction_pose"]);
            }

            public PersonGoalMsg(string name, PoseMsg interaction_pose)
            {
                _name = name;
                _interaction_pose = interaction_pose;
            }

            public static string GetMessageType()
            {
                return "PersonGoal";
            }

            public string GetName()
            {
                return _name;
            }

            public PoseMsg GetInteractionPose()
            {
                return _interaction_pose;
            }

            public void SetInteractionPose(PoseMsg pose)
            {
                _interaction_pose = pose;
            }

            public override string ToString()
            {
                return "User [name=" + _name.ToString()                                                      
                            + "\" ,  interaction_pose=" + _interaction_pose.ToString()                                                        
                            + "]";
            }

            public override string ToYAMLString()
            {
                return "{\"name\" : \"" + _name + "\""                    
                    + ", \"interaction_pose\" : " + _interaction_pose.ToYAMLString()                    
                    + "}";
            }
        }
    }
}
