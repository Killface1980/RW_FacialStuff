namespace FacialStuff.Drawer
{
    using UnityEngine;

    using Verse;

    public abstract class BasicDrawer
    {
        protected BasicDrawer()
        {
        }
        protected Rot4 bodyFacing;

        protected JointLister GetJointPositions(Vector3 vector, float jointWidth, bool carrying = false)
        {
            var rot = this.bodyFacing;
            JointLister joints = new JointLister();
            float leftZ = vector.z;
            float rightZ = vector.z;
            float leftX = vector.x;
            float rightX = vector.x;

            float rightY = vector.y;
            float leftY = vector.y;
            if (carrying)
            {

                leftX -= jointWidth / 2;
                rightX += jointWidth / 2;
                leftZ = -0.025f;
                rightZ = -leftZ;
                if (rot == Rot4.North)
                {
                    leftY = rightY = -vector.y;
                }
            }
            else if (rot.IsHorizontal)
            {
                float offsetX = jointWidth / 8;
                float offsetZ = jointWidth / 2;

                if (rot == Rot4.East)
                {
                    leftX -= offsetX;
                    rightX += offsetX;
                    leftZ += +offsetZ;
                }
                else
                {
                    leftX += offsetX;
                    rightX -= offsetX;
                    rightZ += offsetZ;
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
