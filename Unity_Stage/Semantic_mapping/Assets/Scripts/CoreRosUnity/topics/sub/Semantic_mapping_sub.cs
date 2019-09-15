using ROSBridgeLib;
using UnityEngine;
using SimpleJSON;
using ROSBridgeLib.semantic_mapping;

public class Semantic_mapping_sub : ROSBridgeSubscriber
{

    public new static string GetMessageTopic()
    {
        return "/semantic_mapping/SemanticObject";
    }

    public new static string GetMessageType()
    {
        return "semantic_mapping/SemanticObject";
    }

    public new static ROSBridgeMsg ParseMessage(JSONNode msg)
    {
        return new SemanticObjectMsg(msg);
    }

    public new static void CallBack(ROSBridgeMsg msg, string host)
    {
        Object.FindObjectOfType<SemanticMapping>().DetectedObject((SemanticObjectMsg)msg,host);
    }
}
