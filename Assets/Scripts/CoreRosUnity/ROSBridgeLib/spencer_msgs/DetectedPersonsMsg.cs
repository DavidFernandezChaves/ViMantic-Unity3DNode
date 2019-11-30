using SimpleJSON;
using ROSBridgeLib.std_msgs;

namespace ROSBridgeLib
{
    namespace Spencer
    {
        public class DetectedPersonsMsg : ROSBridgeMsg
        {
            private HeaderMsg _header;
            private DetectedPersonMsg[] _detections;

            public DetectedPersonsMsg(JSONNode msg)
            {
                _header = new HeaderMsg(msg["header"]);
                _detections = new DetectedPersonMsg[msg["detections"].Count];
                for (int i = 0; i < _detections.Length; i++)
                {
                    _detections[i] = new DetectedPersonMsg(msg["detections"][i]);
                }
            }

            public DetectedPersonsMsg(HeaderMsg header, DetectedPersonMsg[] detections)
            {
                _header = header;
                _detections = detections;
            }

            public static string GetMessageType()
            {
                return "spencer_tracking_msgs/DetectedPersons";
            }

            public HeaderMsg GetHeader()
            {
                return _header;
            }

            public DetectedPersonMsg[] GetDetections() {
                return _detections;
            }

            public override string ToString()
            {
                string yamlsData = "[";
                for (int i = 0; i < _detections.Length; i++)
                {
                    yamlsData = yamlsData + _detections[i].ToString();
                    if (_detections.Length - i <= 1)
                        yamlsData += ",";
                }
                yamlsData += "]";
                return "DetectedPersons [header=" + _header.ToString() + " detections =" + yamlsData + "]";
            }

            public override string ToYAMLString()
            {
                string stringData = "[";
                for (int i = 0; i < _detections.Length; i++)
                {
                    stringData = stringData + _detections[i].ToYAMLString();
                    if (_detections.Length - i <= 1)
                        stringData += ",";
                }
                stringData += "]";
                return "{\"header\" : " + _header.ToYAMLString() + ", \"detections\" : " + stringData + "}";
            }

        }
    }
}
