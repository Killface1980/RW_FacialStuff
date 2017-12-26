using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff.Drawer
{
    using RimWorld;

    using UnityEngine;

    using Verse;

    public abstract class BasicDrawer
    {
        protected BasicDrawer()
        {
        }

        protected JointLister GetJointPositions(Rot4 rot, Vector3 vector, float jointWidth)
        {
            JointLister joints = new JointLister();
            float leftZ = vector.z;
            float rightZ = vector.z;
            float leftX = vector.x;
            float rightX = vector.x;

            float rightY = vector.y;
            float leftY = vector.y;
            if (rot.IsHorizontal)
            {
                if (rot == Rot4.East)
                {
                leftX -= jointWidth / 8;
                    leftZ += +jointWidth / 3;
                }
                else
                {
                rightX -= jointWidth / 8;
                    rightZ += +jointWidth / 3;
                }

                leftY *= -1;
            }
            else
            {
                leftX *= -1;
            }
            joints.rightJoint = new Vector3(rightX, rightY, rightZ);
            joints.leftJoint = new Vector3(leftX, leftY, leftZ);

            return joints;
        }

    }

    public struct JointLister
    {
        public Vector3 leftJoint;

        public Vector3 rightJoint;
    }
}
