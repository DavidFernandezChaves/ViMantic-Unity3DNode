using SimpleJSON;
using ROSBridgeLib.geometry_msgs;

namespace ROSBridgeLib
{
    namespace Spencer
    {
        public class DetectedPersonMsg : ROSBridgeMsg
        {

            private float _confidence;
            private uint _detection_id;
            private string _modality;
            private PoseWithCovarianceMsg _pose;

            public DetectedPersonMsg(JSONNode msg)
            {
                _confidence = msg["confidence"].AsFloat;
                _detection_id = uint.Parse(msg["detection_id"]);
                _modality = msg["modality"];
                _pose = new PoseWithCovarianceMsg(msg["pose"]);
            }

            public DetectedPersonMsg(float confidence, uint detection_id, string modality, PoseWithCovarianceMsg pose)
            {
                _confidence = confidence;
                _detection_id = detection_id;
                _modality = modality;
                _pose = pose;
            }

            public static string GetMessageType()
            {
                return "spencer_tracking_msgs/DetectedPerson";
            }

            public float GetConfidence()
            {
                return _confidence;
            }

            public uint GetDetection_id()
            {
                return _detection_id;
            }

            public string GetModality()
            {
                return _modality;
            }

            public PoseWithCovarianceMsg GetPose()
            {
                return _pose;
            }

            public override string ToString()
            {
                return "DetectedPerson [confidence=" + _confidence.ToString("N", System.Globalization.CultureInfo.InvariantCulture) + ",  detection_id=" + _detection_id + ",  modality=" + _modality + ",  pose=" + _pose.ToString() +"]";
            }

            public override string ToYAMLString()
            {
                return "{\"confidence\" : " + _confidence.ToString("N", System.Globalization.CultureInfo.InvariantCulture) + ", \"detection_id\" : " + _detection_id + ", \"modality\" : " + _modality + ", \"pose\" : " + _pose.ToYAMLString() + "}";
            }

        }
    }
}
