namespace FacialStuff
{
    using UnityEngine;

    using Verse;

    public abstract class BasicDrawer
    {
        protected BasicDrawer()
        {
        }

        protected Rot4 bodyFacing;

        protected Rot4 headFacing;

        protected JointLister GetJointPositions(Vector3 vector, float jointWidth, float emptyHandsOffsetY = 0f, bool carrying = false)
        {
            Rot4 rot = this.bodyFacing;
            JointLister joints = new JointLister();
            float leftX = vector.x;
            float rightX = vector.x;
            float leftZ = vector.z;
            float rightZ = vector.z;

            float offsetY = (carrying ? 0f : emptyHandsOffsetY) + Offsets.YOffset_HandsFeet;
            float leftY = offsetY;
            float rightY = offsetY;

            if (carrying)
            {

                leftX -= jointWidth / 2;
                rightX += jointWidth / 2;
                leftZ = -0.025f;
                rightZ = -leftZ;
            }
            else if (rot.IsHorizontal)
            {
                float offsetX = jointWidth * 0.1f;
                float offsetZ = jointWidth * 0.2f;

                if (rot == Rot4.East)
                {

                    leftY = -Offsets.YOffset_Behind;
                    leftZ += +offsetZ;

                }
                else
                {

                    rightY = -Offsets.YOffset_Behind;
                    rightZ += offsetZ;

                }

                leftX += offsetX;
                rightX -= offsetX;


            }
            else
            {
                leftX = -rightX;
            }

            if (rot == Rot4.North)
            {
                leftY = rightY = -Offsets.YOffset_Behind;
            }


            joints.rightJoint = new Vector3(rightX, rightY, rightZ);
            joints.leftJoint = new Vector3(leftX, leftY, leftZ);

            return joints;
        }

        public virtual Mesh GetPawnMesh(bool wantsBody, bool portrait)
        {
            return wantsBody ? MeshPool.humanlikeBodySet.MeshAt(this.bodyFacing) : MeshPool.humanlikeHeadSet.MeshAt(this.headFacing);
        }
    }

    public struct JointLister
    {
        public Vector3 leftJoint;

        public Vector3 rightJoint;
    }
}
