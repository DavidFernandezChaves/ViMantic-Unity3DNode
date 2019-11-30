using System.Collections;
using System.Collections.Generic;
using ROSBridgeLib.geometry_msgs;
using UnityEngine;

namespace ROSBridgeLib
{
    public class TfUtils
    {
        static public Vector3 GetEulerYPR(Quaternion q, int solution) {
            return GetEulerYPR(GetMatrix3x3Rotation(q), solution);
        }


        static public Vector3 GetEulerYPR(float[] matrix3x3, int solution) {
            Vector3 result1 = new Vector3();
            Vector3 result2 = new Vector3();

            if (matrix3x3[6] >= 1)
            {
                result1.x = 0f;
                result2.x = 0f;

                if (matrix3x3[6] < 0)
                {
                    float delta = Mathf.Atan2(matrix3x3[1], matrix3x3[2]);
                    result1.y = Mathf.PI / 2;
                    result2.y = Mathf.PI / 2;
                    result1.z = delta;
                    result2.z = delta;
                }
                else {
                    float delta = Mathf.Atan2(-matrix3x3[1], -matrix3x3[2]);
                    result1.y = -Mathf.PI / 2;
                    result2.y = -Mathf.PI / 2;
                    result1.z = delta;
                    result2.z = delta;
                }
            }
            else {
                result1.y = -Mathf.Asin(matrix3x3[6]);
                result2.y = Mathf.PI - result1.y;

                result1.z = Mathf.Atan2(matrix3x3[7] / Mathf.Cos(result1.y), matrix3x3[8] / Mathf.Cos(result1.y));
                result2.z = Mathf.Atan2(matrix3x3[7] / Mathf.Cos(result2.y), matrix3x3[8] / Mathf.Cos(result2.y));

                result1.x = Mathf.Atan2(matrix3x3[3] / Mathf.Cos(result1.y), matrix3x3[0] / Mathf.Cos(result1.y));
                result2.x = Mathf.Atan2(matrix3x3[3] / Mathf.Cos(result2.y), matrix3x3[0] / Mathf.Cos(result2.y));
            }

            if (solution == 1)
                return result1;
            else
                return result2;
        }

        static public float[] GetMatrix3x3Rotation(Quaternion q) {
            float[] result = new float[9];

            float xs = q.x * 2;
            float ys = q.y * 2;
            float zs = q.z * 2;
            float wx = q.w * xs;
            float wy = q.w * ys;
            float wz = q.w * zs;
            float xx = q.x * xs;
            float xy = q.x * ys;
            float xz = q.x * zs;
            float yy = q.y * ys;
            float yz = q.y * zs;
            float zz = q.z * zs;

            result[0] = 1 - (yy + zz);
            result[1] = xy - wz;
            result[2] = xz + wy;
            result[3] = xy + wz;
            result[4] = 1 - (xx + zz);
            result[5] = yz - wx;
            result[6] = xz - wy;
            result[7] = yz + wx;
            result[8] = 1 - (xx + yy);

            return result;
        }

        static public float Length2(Quaternion q) {
            return q.x * q.x + q.y * q.y + q.z * q.z + q.w + q.w;
        }


//        static public QuaternionMsg GetMatrix3x3Rotation(Vector3 YPR) { 


//        def quaternion_from_euler(ai, aj, ak, axes= 'sxyz'):

//try:
//firstaxis, parity, repetition, frame = _AXES2TUPLE[axes.lower()]
//except(AttributeError, KeyError):
//_TUPLE2AXES[axes] # validation
//firstaxis, parity, repetition, frame = axes

//i = firstaxis + 1
//j = _NEXT_AXIS[i + parity - 1] + 1
//k = _NEXT_AXIS[i - parity] + 1

//if frame:
//ai, ak = ak, ai
//if parity:
//aj = -aj

//ai /= 2.0
//aj /= 2.0
//ak /= 2.0
//ci = math.cos(ai)
//si = math.sin(ai)
//cj = math.cos(aj)
//sj = math.sin(aj)
//ck = math.cos(ak)
//sk = math.sin(ak)
//cc = ci* ck
//cs = ci* sk
//sc = si* ck
//ss = si* sk

//q = numpy.empty((4, ))
//if repetition:
//q[0] = cj* (cc - ss)
//q[i] = cj* (cs + sc)
//q[j] = sj* (cc + ss)
//q[k] = sj* (cs - sc)
//else:
//q[0] = cj* cc + sj* ss
//q[i] = cj* sc - sj* cs
//q[j] = cj* ss + sj* cc
//q[k] = cj* cs - sj* sc
//if parity:
//q[j] *= -1.0

//return q

//    }


    }
}