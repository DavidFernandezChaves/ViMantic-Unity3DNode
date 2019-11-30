using UnityEngine;

namespace ROSBridgeLib
{
    public class CloseUpService : ROSBridgeServiceResponse
    {

        public new static void ServiceCallBack(string service, string yaml)
        {
            Debug.Log("Service:" + service + " yaml:" + yaml);
            switch (service)
            {
                case "/bt_manager/add_new_task":
                    Debug.Log("TaskManaguer dice: " + yaml);
                    break;
                default:
                    Debug.Log(service + "/" + yaml);
                    break;
            }

        }
    }
}