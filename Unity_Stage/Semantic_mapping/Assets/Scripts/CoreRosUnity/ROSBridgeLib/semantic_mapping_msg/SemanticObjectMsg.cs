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
            private HeaderMsg _header;
            private String _id;
            private double _accuracyEstimation;
            private PointCloudMsg _pointCloud;
            private PoseStampedMsg _pose;
            private Vector3Msg _scale;


            public SemanticObjectMsg(JSONNode msg)
            {
                _header = new HeaderMsg(msg["header"]);
                _id = msg["id"];
                _accuracyEstimation = double.Parse(msg["accuracy_estimation"], System.Globalization.CultureInfo.InvariantCulture);
                _pointCloud = new PointCloudMsg(msg["pointCloud"]);
                _pose = new PoseStampedMsg(msg["pose"]);
                _scale = new Vector3Msg(msg["scale"]);
            }

            public SemanticObjectMsg(HeaderMsg header, String Class, float probability, PointCloudMsg pointCloud, PoseStampedMsg position, Vector3Msg scale)
            {
                _header = header;
                _id = Class;
                _accuracyEstimation = probability;
                _pointCloud = pointCloud;
                _pose = position;
                _scale = scale;
            }

            public static string GetMessageType()
            {
                return "semantic_mapping/SemanticObject";
            }

            public HeaderMsg GetHeader()
            {
                return _header;
            }
            public String GetId()
            {
                return _id;
            }

            public double GetAccuracyEstimation()
            {
                return _accuracyEstimation;
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
                return "Detection [header=" + _header.ToString() + ", id=" + _id + ", accuracy_estimation="+ _accuracyEstimation + ", pointCloud="+ _pointCloud.ToString() + ", pose=" + _pose.ToString() + ", scale=" + _scale.ToString() + "]";
            }

            public override string ToYAMLString()
            {
                return "{\"header\" : " + _header.ToYAMLString() + ", \"id\" : " + _id + ", \"accuracy_estimation\" : " + _accuracyEstimation.ToString("N", System.Globalization.CultureInfo.InvariantCulture) + ", \"pointCloud\" : "+ _pointCloud.ToYAMLString() + ", \"pose\" : " + _pose.ToYAMLString() + ", \"scale\" : " + _scale.ToYAMLString() + "}";
            }
        }
    }
}
