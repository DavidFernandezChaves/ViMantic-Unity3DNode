using SimpleJSON;
using ROSBridgeLib.sensor_msgs;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.geometry_msgs;
using System;

namespace ROSBridgeLib
{
    namespace semantic_mapping
    {
        public class SemanticObjectsMsg : ROSBridgeMsg
        {
            private HeaderMsg _header;
            private SemanticObjectMsg[] _semanticObjects;


            public SemanticObjectsMsg(JSONNode msg)
            {
                _header = new HeaderMsg(msg["header"]);
                _semanticObjects = new SemanticObjectMsg[msg["semanticObjects"].Count];
                for (int i = 0; i < _semanticObjects.Length; i++)
                {
                    _semanticObjects[i] = new SemanticObjectMsg(msg["semanticObjects"][i]);
                }
            }

            public SemanticObjectsMsg(HeaderMsg header, SemanticObjectMsg[] semanticObjects)
            {
                _header = header;
                _semanticObjects = semanticObjects;
            }

            public static string GetMessageType()
            {
                return "semantic_mapping/SemanticObjects";
            }

            public HeaderMsg GetHeader()
            {
                return _header;
            }

            public SemanticObjectMsg[] GetSemanticObjects() {
                return _semanticObjects;
            }

            public override string ToString()
            {
                string resultado = ", semanticObjects=[";
                for (int i = 0; i < _semanticObjects.Length; i++)
                {
                    resultado += _semanticObjects[i].ToString();
                    if (i < (_semanticObjects.Length - 1))
                        resultado += ",";
                }
                return "Detection [header=" + _header.ToString() + resultado +"]]";
            }

            public override string ToYAMLString()
            {
                string resultado = ",  \"semanticObjects\" : [";
                for (int i = 0; i < _semanticObjects.Length; i++)
                {
                    resultado += _semanticObjects[i].ToYAMLString();
                    if (i < (_semanticObjects.Length - 1))
                        resultado += ",";
                }
                return "{\"header\" : " + _header.ToYAMLString() + resultado + "]}";
            }
        }
    }
}
