using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Expension
{
    public static class ExpensionController
    {
        public static Vector3 NormalizeAngle(this Vector3 eulerAngle)
        {
            var delta = eulerAngle;

            if (delta.x > 180)
            {
                delta.x -= 360;
            }
            else if (delta.x < -180)
            {
                delta.x += 360;
            }

            if (delta.y > 180)
            {
                delta.y -= 360;
            }
            else if (delta.y < -180)
            {
                delta.y += 360;
            }

            if (delta.z > 180)
            {
                delta.z -= 360;
            }
            else if (delta.z < -180)
            {
                delta.z += 360;
            }

            return new Vector3(delta.x, delta.y, delta.z);//round values to angle;
        }
    }


}


