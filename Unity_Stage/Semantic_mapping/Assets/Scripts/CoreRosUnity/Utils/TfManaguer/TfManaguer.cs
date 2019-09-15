using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib.tf2_msgs;
using ROSBridgeLib.geometry_msgs;

public class TfManaguer : MonoBehaviour {

    private char[] _specialChar = { '/', ' ' };

    public void Newtf(TFMsg msg) {
        TransformStampedMsg[] msgs = msg.Gettransforms();
        foreach (TransformStampedMsg tf in msgs)
        {
            
            string nameParent = tf.Getheader().GetFrameId().Trim(_specialChar);
            string nameTf = tf.GetChild_frame_id().Trim(_specialChar);

            if (nameTf != null)
            {
                GameObject go = GameObject.Find(nameTf);
                if (go == null)
                {
                    CreateTf(nameTf,nameParent, tf.Gettransform().GetMatrix4x4());
                }
                else
                {
                    if (go.transform.parent == null) {
                        GameObject parent = GameObject.Find(nameParent);
                        if (parent == null)
                        {
                            go.transform.parent = CreateTf(nameParent, "", Matrix4x4.identity);
                        }
                        else {
                            go.transform.parent = parent.transform;
                        }                        
                    }
                    go.transform.FromMatrix(tf.Gettransform().GetMatrix4x4());                    
                }
            }
        }
    }

    Transform CreateTf(string name, string nameParent, Matrix4x4 m) {
        Transform newTf = new GameObject().transform;

        if (nameParent != "") {
            GameObject parent = GameObject.Find(nameParent);
            if (parent == null)
            {
                newTf.parent = CreateTf(nameParent, "", Matrix4x4.identity);
            }else{
                newTf.parent = parent.transform;
            }            
        }            
        
        newTf.name = name;
        newTf.FromMatrix(m);

        return newTf;
    }
   


}
