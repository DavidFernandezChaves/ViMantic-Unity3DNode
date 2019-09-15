using ROSBridgeLib;
using ROSBridgeLib.tf2_msgs;
using SimpleJSON;
using UnityEngine;


public class Tf_sub : ROSBridgeSubscriber
{

    public new static string GetMessageTopic()
    {
        return "/tf";
    }

    public new static string GetMessageType()
    {
        return "tf2_msgs/TFMessage";
    }

    public new static ROSBridgeMsg ParseMessage(JSONNode msg)
    {
        return new TFMsg(msg);
    }

    public new static void CallBack(ROSBridgeMsg msg, string host)
    {
        Object.FindObjectOfType<TfManaguer>().Newtf((TFMsg) msg);
    }
}
