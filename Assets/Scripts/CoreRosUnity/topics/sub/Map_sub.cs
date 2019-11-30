using ROSBridgeLib;
using ROSBridgeLib.nav_msgs;
using SimpleJSON;
using UnityEngine;

class Map_sub : ROSBridgeSubscriber
{
    public new static string GetMessageTopic()
    {
        return "/map";
    }

    public new static string GetMessageType()
    {
        return "nav_msgs/OccupancyGrid";
    }

    public new static ROSBridgeMsg ParseMessage(JSONNode msg)
    {
        return new OccupancyGridMsg(msg);
    }

    public new static void CallBack(ROSBridgeMsg msg, string host)
    {
        Object.FindObjectOfType<TerrainConstructor>().NewOcupanceGridMsg((OccupancyGridMsg)msg);
    }
}

