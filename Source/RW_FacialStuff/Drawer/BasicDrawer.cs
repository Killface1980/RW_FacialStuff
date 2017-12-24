using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff.Drawer
{
    using UnityEngine;

    using Verse;

    public abstract class BasicDrawer
    {
        protected BasicDrawer()
        {
        }

        protected JointLister GetJointPositions(Rot4 rot, Vector3 vector)
        {

            JointLister joints = new JointLister();
            if (rot.IsHorizontal)
            {
                joints.rightJoint = new Vector3(vector.x, vector.y, vector.z);
                joints.leftJoint = new Vector3(vector.x, -vector.y, vector.z);

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
