using SimpleJSON;
using ROSBridgeLib.sensor_msgs;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.geometry_msgs;
using System;

namespace ROSBridgeLib
{
    namespace semantic_mapping
    {
        public class SemanticObjectMsg : ROSBridgeMsg
        {
            private String _id;
            private double _score;
            private PointCloudMsg _pointCloud;
            private PoseStampedMsg _pose;
            private Vector3Msg _scale;


            public SemanticObjectMsg(JSONNode msg)
            {
                _id = msg["id"];
                _score = double.Parse(msg["score"], System.Globalization.CultureInfo.InvariantCulture);
                _pointCloud = new PointCloudMsg(msg["pointCloud"]);
                _pose = new PoseStampedMsg(msg["pose"]);
                _scale = new Vector3Msg(msg["scale"]);
            }

            public SemanticObjectMsg(String Class, float score, PointCloudMsg pointCloud, PoseStampedMsg position, Vector3Msg scale)
            {
                _id = Class;
                _score = score;
                _pointCloud = pointCloud;
                _pose = position;
                _scale = scale;
            }

            public static string GetMessageType()
            {
                return "semantic_mapping/SemanticObject";
            }

            public String GetId()
            {
                return _id;
            }

            public double GetAccuracyEstimation()
            {
                return _score;
            }

            public PointCloudMsg GetPointCloud()
            {
                return _pointCloud;
            }

            public PoseStampedMsg GetPose() {
                return _pose;
            }

            public Vector3Msg GetScale()
            {
                return _scale;
            }

            public override string ToString()
            {
                return "Detection [id=" + _id + ", score="+ _score + ", pointCloud="+ _pointCloud.ToString() + ", pose=" + _pose.ToString() + ", scale=" + _scale.ToString() + "]";
            }

            public override string ToYAMLString()
            {
                return "{\"id\" : " + _id + ", \"score\" : " + _score.ToString("N", System.Globalization.CultureInfo.InvariantCulture) + ", \"pointCloud\" : "+ _pointCloud.ToYAMLString() + ", \"pose\" : " + _pose.ToYAMLString() + ", \"scale\" : " + _scale.ToYAMLString() + "}";
            }
        }
    }
}
