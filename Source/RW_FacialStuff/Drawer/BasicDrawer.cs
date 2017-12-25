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
            if (rot.IsHorizontal)
            {
                float offset = (rot == Rot4.East ? -1f : 1f) * jointWidth / 3;
                joints.rightJoint = new Vector3(vector.x + offset, vector.y, vector.z);
                joints.leftJoint = new Vector3(vector.x - offset, -vector.y, vector.z);

            }
            else
            {
                joints.rightJoint = new Vector3(vector.x, vector.y, vector.z);
                joints.leftJoint = new Vector3(-vector.x, vector.y, vector.z);
            }

            return joints;
        }

    }

    public struct JointLister
    {
        public Vector3 leftJoint;

        public Vector3 rightJoint;
    }
}
